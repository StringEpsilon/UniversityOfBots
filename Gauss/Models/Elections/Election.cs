/**
This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this
file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace Gauss.Models.Elections {
	public class Election {
		public ulong GuildId { get; set; }

		public ulong ID { get; set; }

		/// <summary>
		/// Current status of the election.
		/// </summary>
		public ElectionStatus Status { get; set; }
		/// <summary>
		/// Title of the office to elect for.
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// Description of the election / office.
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Schedule start time and date of the election poll.
		/// </summary>
		public DateTime Start { get; set; }

		/// <summary>
		/// Schedule end time and date of the election poll.
		/// </summary>
		public DateTime End { get; set; }

		/// <summary>
		/// List of candidates that can be voted for in the election.
		/// </summary>
		public List<Candidate> Candidates { get; set; }

		/// <summary>
		/// List of users (by ID) who voted in the election.
		/// </summary>
		public List<ulong> Voters { get; set; } = new List<ulong>();

		/// <summary>
		/// Reference to the message informing users of the election. Message is updated in various steps.
		/// </summary>
		public MessageReference Message { get; set; }

		/// <summary>
		/// Computes a hash of the current election state.
		/// </summary>
		/// <returns>
		/// The computed hash as a hexadecimal string.
		/// </returns>
		public string GetHash() {
			var json = JsonConvert.SerializeObject(this);
			var hash = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(json));
			var stringBuilder = new StringBuilder();
			for (int i = 0; i < hash.Length; i++) {
				stringBuilder.Append(hash[i].ToString("x2"));
			}
			// Return the hexadecimal string.
			return stringBuilder.ToString();
		}

		/// <summary>
		/// Generate an embed to present the election in a nice way to the users.
		/// </summary>
		/// <returns>
		/// The generated embed.
		/// </returns>
		internal DiscordEmbed GetEmbed() {
			var optionsText = string.Join(
				"\n",
				this.Candidates.Select((y) => $"{y.Option}) {y.Username}")
			);
			DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
				.WithTitle($"Election for {this.Title} (ID: {this.ID})")
				.WithColor(DiscordColor.Blurple)
				.WithTimestamp(this.End);

			if (this.Status != ElectionStatus.Decided) {
				embedBuilder.Description = this.Description + "\n" +
					$"**Start:** {this.Start:yyyy-MM-dd HH:mm} UTC\n" +
					$"**End:** {this.End:yyyy-MM-dd HH:mm} UTC\n" +
					$"**Candidates:**\n{optionsText}\n\n" +
					$"Vote via `!g election vote {this.ID} username [username ...]`";
			} else {
				embedBuilder.Description = this.Description + "\n" +
					$"**Period:** {this.Start:yyyy-MM-dd HH:mm} - {this.End:yyyy-MM-dd HH:mm} UTC\n" +
					$"**Results:**\n{this.GetResults()}";
			}

			if (this.Message != null) {
				embedBuilder.WithFooter(this.GetHash());
			}
			return embedBuilder.Build();
		}

		public string GetResults() {
			var stringBuilder = new StringBuilder();
			var sortedCandidates = this.Candidates.OrderByDescending(y => y.Votes);
			int place = 1;
			foreach (var candidate in sortedCandidates) {
				stringBuilder
					.Append(place)
					.Append(". ")
					.Append(candidate.Username)
					.Append(": ")
					.Append(candidate.Votes)
					.Append('\n');

				place++;
			}
			stringBuilder
				.Append("Number of voters: ")
				.Append(this.Voters.Count);

			return stringBuilder.ToString();
		}
	}
}