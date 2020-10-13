
/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;

namespace Gauss.Utilities {
	public static class ContextExtensions {
		static private readonly DiscordEmoji _confirmEmoji = DiscordEmoji.FromUnicode("✅");
		static private readonly DiscordEmoji _abortEmoji = DiscordEmoji.FromUnicode("❌");

		public static DiscordGuild GetGuild(this CommandContext context) {
			if (context.Channel.IsPrivate) {
				return context.Client.Guilds.Values
					.ToList()
					.Find(
						x => x.Members.Values.Any(m => m.Id == context.Message.Author.Id)
					);
			}
			return context.Guild;
		}

		public static async Task<DiscordChannel> GetDMChannel(this CommandContext context) {
			if (context.Channel.IsPrivate) {
				return context.Channel;
			}
			var guild = context.GetGuild();
			return await guild.Members[context.User.Id]?.CreateDmChannelAsync();
		}

		public static async Task CreateConfirmation(this CommandContext context, string message, Action onConfirm = null, Action onAbort = null) {
			DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder {
				Title = "Confirm:",
				Description = message,

			};
			embedBuilder.Color = DiscordColor.Red;
			embedBuilder.Footer = new DiscordEmbedBuilder.EmbedFooter() { Text = $"{_confirmEmoji} to confirm, {_abortEmoji} to abort." };

			DiscordMessage botMessage = await context.RespondAsync(null, false, embedBuilder.Build());

			await botMessage.CreateReactionAsync(_confirmEmoji);
			await Task.Delay(250);
			await botMessage.CreateReactionAsync(_abortEmoji);

			var interactivity = context.Client.GetInteractivity();
			var reaction = await interactivity.WaitForReactionAsync(
				(MessageReactionAddEventArgs x) => {
					if (x.User.IsBot) {
						return false;
					}
					if (x.User.Id != context.User.Id) {
						return false;
					}
					return x.Message == botMessage &&
				 		(x.Emoji == _confirmEmoji || x.Emoji == _abortEmoji);
				},
				TimeSpan.FromMinutes(1)
			);

			if (!reaction.TimedOut) {
				if (reaction.Result.Emoji == _confirmEmoji) {
					onConfirm?.Invoke();
					await botMessage.DeleteOwnReactionAsync(_abortEmoji);
				} else {
					onAbort?.Invoke();
					await botMessage.DeleteOwnReactionAsync(_confirmEmoji);
				}
			} else {
				await botMessage.DeleteOwnReactionAsync(_abortEmoji);
				await Task.Delay(250);
				await botMessage.DeleteOwnReactionAsync(_confirmEmoji);
				await Task.Delay(250);
				await botMessage.CreateReactionAsync(DiscordEmoji.FromUnicode("⌛"));
			}
		}
	}
}