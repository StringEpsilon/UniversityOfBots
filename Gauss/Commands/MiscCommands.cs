/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/
using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Gauss.CommandAttributes;
using Gauss.Database;
using Gauss.Utilities;

namespace Gauss.Commands {
	[NotBot]
	[CheckDisabled]
	public class MiscCommands : BaseCommandModule {
		private readonly CalendarAccessor _calendar;

		public MiscCommands(CalendarAccessor calendar) {
			this._calendar = calendar;
		}

		[Command("whatsnew")]
		[Description("Link to the changelog, to see what functionality was recently changed or added.")]
		public async Task WhatsNew(CommandContext context) {
			await context.RespondAsync("<https://stringepsilon.github.io/UniversityOfBots/WhatsNew.html>");
		}

		[Command("upcoming")]
		[Aliases("nextevent")]
		[Description("Get the next scheduled server event from the calendar.")]
		public async Task GetEvents(CommandContext context) {
			var nextEvent = await this._calendar.GetNextEvent(context.GetGuild().Id);
			if (nextEvent == null) {
				await context.RespondAsync("No upcoming event found");
				return;
			} else {
				await context.RespondAsync(
					embed: new DiscordEmbedBuilder()
						.WithTitle(nextEvent.Summary)
						.WithDescription(nextEvent.Description)
						.WithTimestamp(nextEvent.Start.DateTime)
						.Build()
				);
			}
		}

		[Description("Get a link to detailed documentation")]
		[Command("docs")]
		[Aliases("documentation")]
		public async Task GetDocumentation(CommandContext context) {
			await context.RespondAsync("<https://stringepsilon.github.io/UniversityOfBots/>");
		}

		[Description("Get information about your privacy and this Bot.")]
		[Command("privacy")]
		public async Task GetPrivacyInfo(CommandContext context) {
			await context.RespondAsync("You can find up to date privacy information in the link below.\n" +
				"<https://stringepsilon.github.io/UniversityOfBots/PRIVACY>\n");
		}

		[Description("Get an invite link to the current server.")]
		[Command("invite")]
		public async Task GetInvite(CommandContext context) {
			try {
				var invites = await context.Channel.Guild.GetInvitesAsync();
				var invite = invites
					.OrderByDescending(y => y.Uses)
					.FirstOrDefault(y => y.IsTemporary == false && y.IsRevoked == false && y.MaxAge == 0 && y.MaxUses == 0);
				if (invite != null) {
					await context.RespondAsync($"Here is your invite link: <https://discord.gg/{invite.Code}>");
				}
			} catch (Exception) {
			}
		}
	}
}