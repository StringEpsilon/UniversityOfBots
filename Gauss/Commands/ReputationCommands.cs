/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Gauss.CommandAttributes;
using Gauss.Database;
using Gauss.Utilities;

namespace Gauss.Commands {
	[NotBot]
	[CheckDisabled]
	public class ReputationCommands : BaseCommandModule {
		private readonly ReputationRepository _reputationRepository;
		public ReputationCommands(ReputationRepository reputationRepository) {
			this._reputationRepository = reputationRepository;
		}

		[Command("leaderboard")]
		[Description("Gives a leaderboard of bayes points for the given month.")]
		public async Task GetLeaderBoardByMonth(
			CommandContext context,
			[Description("Month you want to see, in format YYYY-MM (e.g. 2021-01).")]
			DateTime yearMonth
		) {
			var scores = this._reputationRepository.GetMonthlyScores(context.Guild.Id, yearMonth);
			if (scores == null) {
				await context.RespondAsync("No reputation data available this month.");
				return;
			}
			var guild = context.GetGuild();
			var sortedScores = scores.OrderByDescending(y => y.Value);

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
				if (guild.Members.ContainsKey(entry.Key)) {
					var member = guild.Members[entry.Key];
					lines.Add($"#{rank,3}: {entry.Value,6} - {member.Username}#{member.Discriminator}");
				} else {
					lines.Add($"#{rank,3}: {entry.Value,6} - {entry.Key}");
				}
			}

			var embedBuilder = new DiscordEmbedBuilder()
				.WithTitle($"Leaderboard {yearMonth:yyyy-MM}");

			var interactivity = context.Client.GetInteractivity();
			var foo = interactivity.GeneratePagesInEmbed(string.Join("\n", lines), SplitType.Line, embedBuilder);
			await interactivity.SendPaginatedMessageAsync(
				context.Channel,
				context.User,
				foo.Select(x => new Page(
					"",
					new DiscordEmbedBuilder(x.Embed).WithDescription(
						$"```\nRank: Points - User\n{x.Embed.Description}\n```"
					)
				)),
				deletion: PaginationDeletion.DeleteEmojis,
				timeoutoverride: TimeSpan.FromMinutes(5)
			);
		}

		[Command("leaderboard")]
		[Description("Gives a leaderboard of bayes points for current month.")]
		public async Task GetCurrentLeaderboard(CommandContext context) {
			await this.GetLeaderBoardByMonth(context, DateTime.UtcNow);
		}
	}
}