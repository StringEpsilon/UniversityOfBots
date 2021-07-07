/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DSharpPlus.Entities;
using Gauss.Models.Elections;
using Gauss.Utilities;
using Newtonsoft.Json;
using DSharpPlus;

namespace Gauss.Database {
	public class ElectionRepository {
		private readonly Dictionary<ulong, List<Election>> _elections;
		private readonly string _configDirectory;

		private void WriteAuditLog(Election election, string action, bool includeJson = false) {

			lock (_elections) {
				string hash = null;
				string json = null;
				hash = election.GetHash();
				json = JsonConvert.SerializeObject(election);
				string logPath = Path.Join(this._configDirectory, "auditlogs", $"{election.GuildId}-{election.ID}.log");


				var logText = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm}] [{action}] Hash: {hash}\n";
				if (includeJson) {
					logText += $"JSON: \n{json}\n\n";
				}
				File.AppendAllText(
					logPath,
					logText
				);
			}
		}

		public string GetAuditLogPath(Election election) {
			return Path.Join(this._configDirectory, "auditlogs", $"{election.GuildId}-{election.ID}.log");
		}

		public ElectionRepository(string configDirectory) {
			this._configDirectory = configDirectory;
			if (!Directory.Exists(Path.Join(this._configDirectory, "auditlogs"))) {
				Directory.CreateDirectory(Path.Join(this._configDirectory, "auditlogs"));
			}
			try {
				this._elections = JsonUtility.Deserialize<Dictionary<ulong, List<Election>>>(
					Path.Join(this._configDirectory, "elections.json")
				);
			} catch (Exception) {
				this._elections = new Dictionary<ulong, List<Election>>();
			}
		}

		public List<Election> GetActiveElections() {
			var results = new List<Election>();
			lock (this._elections) {
				foreach (var electionsList in this._elections.Values) {
					foreach (var election in electionsList.Where(y => y.Status == ElectionStatus.Active)) {
						results.Add(election);
					}
				}
			}

			return results;
		}

		public List<Election> GetCurrentElections(ulong guildId) {
			var results = new List<Election>();
			lock (this._elections) {
				foreach (var electionsList in this._elections.Values) {
					foreach (var election in electionsList.Where(y => {
						if (y.GuildId != guildId) {
							return false;
						}
						return y.Status == ElectionStatus.Active && y.Start <= DateTime.UtcNow && y.End >= DateTime.UtcNow;
					})) {
						results.Add(election);
					}
				}
			}

			return results;
		}

		internal void SetMessage(ulong guildId, ulong electionId, DiscordMessage message) {
			var election = GetElection(guildId, electionId);
			this.WriteAuditLog(election, "Post election poll message");
			lock (_elections) {
				election.Message = new MessageReference() {
					GuildId = guildId,
					ChannelId = message.ChannelId,
					MessageId = message.Id,
				};
			}
			this.WriteAuditLog(election, "Saved election poll message", true);
		}

		public ulong AddElection(ulong guildId, Election election) {
			ulong nextId = 0;
			lock (this._elections) {
				if (!this._elections.ContainsKey(guildId)) {
					this._elections.Add(guildId, new List<Election>());
				}
				nextId = this._elections[guildId].Count > 0 ? this._elections[guildId].Max(y => y.ID) + 1 : 1;
				election.ID = nextId;
				election.GuildId = guildId;
				election.Status = ElectionStatus.Draft;
				this._elections[guildId].Add(election);
			}
			this.SaveChanges();
			return nextId;
		}

		public Election GetElection(ulong guildId, ulong id) {
			Election result = null;
			lock (this._elections) {
				if (!this._elections.ContainsKey(guildId)) {
					return null;
				}
				result = this._elections[guildId].Find(y => y.ID == id);
			}
			return result;
		}

		public bool CommitElection(ulong guildId, ulong electionId) {
			var election = this.GetElection(guildId, electionId);
			if (election == null || election.Status != ElectionStatus.Draft) {
				return false;
			}
			lock (_elections) {
				election.Status = ElectionStatus.Active;
			}
			this.WriteAuditLog(election, "Added election poll", true);
			this.SaveChanges();
			return true;
		}

		public void RemoveElection(ulong guildId, ulong electionId) {
			var election = this.GetElection(guildId, electionId);
			if (election == null || election.Status != ElectionStatus.Draft) {
				return;
			}
			lock (_elections) {
				this._elections[guildId].Remove(election);
			}
			this.SaveChanges();
		}

		public VoteStatus CanVote(ulong guildId, ulong electionId, ulong userId) {
			var election = this.GetElection(guildId, electionId);
			if (election == null) {
				return VoteStatus.ElectionNotFound;
			}
			if (election.Voters.Contains(userId)) {
				return VoteStatus.AlreadyVoted;
			}
			if (election.Start >= DateTime.UtcNow || election.Status == ElectionStatus.Draft) {
				return VoteStatus.ElectionNotStarted;
			}
			if (election.End <= DateTime.UtcNow) {
				return VoteStatus.ElectionOver;
			}

			return VoteStatus.CanVote;
		}

		public void SaveChanges() {
			lock (this._elections) {
				JsonUtility.Serialize(Path.Join(this._configDirectory, "elections.json"), this._elections);
			}
		}

		public (string, string) SaveBallot(ulong guildId, ulong electionId, ulong voterId, Ballot ballot, DiscordClient client) {
			var election = this.GetElection(guildId, electionId);
			this.WriteAuditLog(election, "Add ballot - before");
			string hashBefore = null;
			string hashAfter = null;
			lock (_elections) {
				hashBefore = election.GetHash();
				if (!election.Voters.Contains(voterId)) {
					election.Voters.Add(voterId);
					election.Ballots.Add(ballot);
				}
				hashAfter = election.GetHash();
			}
			this.SaveChanges();
			_ = election.Message.UpdateMessage(client, election.GetEmbed());
			this.WriteAuditLog(election, "Add ballot - after");
			return (hashBefore, hashAfter);
		}

		public bool CloseElection(Election election, DiscordClient client = null) {
			if (election.End >= DateTime.UtcNow) {
				return false;
			}
			this.WriteAuditLog(election, "Close election - before");
			lock (this._elections) {
				election.Status = ElectionStatus.Decided;
				if (client != null) {
					_ = election.Message.UpdateMessage(client, election.GetEmbed());
				}
			}
			this.SaveChanges();

			this.WriteAuditLog(election, "Close election- after", true);
			return true;
		}
	}
}