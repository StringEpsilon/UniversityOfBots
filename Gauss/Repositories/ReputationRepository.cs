/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gauss.Utilities;

namespace Gauss.Database {
	public class GuildReputations {
		// Key: year-month
		// Value: Dictionary with user rep.
		public Dictionary<string, Dictionary<ulong, int>> Reputation = new();

		public Dictionary<ulong, int> GetMonthData(DateTime dateTime, bool create = false) {
			var key = dateTime.ToString("yyyy-MM");
			if (!this.Reputation.ContainsKey(key)) {
				if (create) {
					this.Reputation.Add(key, new Dictionary<ulong, int>());
				} else {
					return null;
				}
			}
			return this.Reputation[key];
		}

		public void AddReputation(ulong userId, int amount) {
			var currentData = this.GetMonthData(DateTime.UtcNow, create: true);
			if (!currentData.ContainsKey(userId)) {
				currentData.Add(userId, 0);
			}
			currentData[userId] += amount;
		}

		public int this[ulong userId] {
			get {
				var result = 0;
				var currentData = this.GetMonthData(DateTime.UtcNow, create: false);
				if (currentData?.ContainsKey(userId) == true) {
					result = currentData[userId];
				}
				return result;
			}
		}
	}


	public class ReputationRepository {
		private readonly string _reputationFile;
		private readonly Dictionary<ulong, GuildReputations> _reputation;

		public ReputationRepository(string configDirectory) {
			this._reputationFile = Path.Join(configDirectory, "reputation.json");
			try {
				this._reputation = JsonUtility.Deserialize<Dictionary<ulong, GuildReputations>>(this._reputationFile);
			} catch (Exception) {
				this._reputation = new Dictionary<ulong, GuildReputations>();
			}
		}

		public void GiveRep(ulong guildId, ulong userId, int amount = 1) {
			if (amount == 0) {
				return;
			}
			lock (this._reputation) {
				if (!this._reputation.ContainsKey(guildId)) {
					this._reputation.Add(guildId, new GuildReputations());
				}
				this._reputation[guildId].AddReputation(userId, amount);
				this.SaveChanges();
			}
		}

		public int GetRep(ulong guildId, ulong userId) {
			lock (this._reputation) {
				if (this._reputation.ContainsKey(guildId)) {
					return this._reputation[guildId][userId];
				}
				return 0;
			}
		}

		public (int points, int rank) GetRank(ulong guildId, ulong userId) {
			var result = (0, 0);
			lock (this._reputation) {
				if (this._reputation.ContainsKey(guildId)) {
					var sortedScores = this.GetCurrentScores(guildId).OrderByDescending(y => y.Value);

					List<string> lines = new();
					int rank = 0;
					int rankbuffer = 1;
					int previousScore = 0;
					foreach (var entry in sortedScores) {
						if (entry.Value != previousScore) {
							rank += rankbuffer;
							rankbuffer = 1;
						} else {
							rankbuffer++;
						}
						previousScore = entry.Value;
						if (entry.Key == userId) {
							result = (entry.Value, rank);
							break;
						}
					}
				}
			}
			return result;
		}

		private void SaveChanges() {
			lock (this._reputation) {
				JsonUtility.Serialize(
					this._reputationFile,
					this._reputation
				);
			}
		}

		public void TakeRep(ulong guildId, ulong userId, int amount = 1) {
			this.GiveRep(guildId, userId, amount * -1);
		}

		public Dictionary<ulong, int> GetCurrentScores(ulong id) {
			if (this._reputation.ContainsKey(id)) {
				return this._reputation[id].GetMonthData(DateTime.UtcNow);
			}
			return null;
		}

		public Dictionary<ulong, int> GetMonthlyScores(ulong id, DateTime month) {
			if (this._reputation.ContainsKey(id)) {
				return this._reputation[id].GetMonthData(month);
			}
			return null;
		}
	}
}