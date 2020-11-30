/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/


using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Gauss.Database;
using Google.Apis.Calendar.v3.Data;

namespace Gauss.Modules {
	public class YAGPDBToCalendarModule : BaseModule {
		private readonly CalendarAccessor _calendar;
		private readonly List<ulong> _pendingEvents = new List<ulong>();

		public YAGPDBToCalendarModule(CalendarAccessor calendar, DiscordClient client) {
			this._calendar = calendar;
			client.MessageCreated += this.HandleNewMessage;
			client.MessageUpdated += this.HandleMessageUpdate;
		}

		private Task HandleNewMessage(DiscordClient sender, MessageCreateEventArgs e) {
			if (e.Message.Embeds.Count != 1) {
				return Task.CompletedTask;
			}
			// Only react to the bot:
			if (e.Message.Author.Id != 204255221017214977) {
				return Task.CompletedTask;
			}
			var embed = e.Message.Embeds[0];

			// This is a bit flaky, but so is the entire idea of bridging one bot to google calendar using another bot... ;-)
			if (embed.Description.StartsWith("Setting up RSVP Event")) {
				this._pendingEvents.Add(e.Message.Id);
			}

			return Task.CompletedTask;
		}

		private Task HandleMessageUpdate(DiscordClient sender, MessageUpdateEventArgs e) {
			if (this._pendingEvents.Contains(e.Message.Id)) {
				Task.Run(async () => {
					var embed = e.Message.Embeds[0];
					// Also flaky:
					if (embed.Footer?.Text == "Event starts" && !string.IsNullOrEmpty(embed.Title)) {
						Event newEvent = new Event() {
							Summary = embed.Title,
							Start = new EventDateTime() {
								DateTime = embed.Timestamp.Value.UtcDateTime,
							},
							End = new EventDateTime() {
								DateTime = embed.Timestamp.Value.UtcDateTime.AddHours(1),
							},
						};
						await this._calendar.AddEvent(e.Guild.Id, newEvent);
						this._pendingEvents.Remove(e.Message.Id);
						System.Threading.Thread.Sleep(1000); // Wait with the reaction to keep YAGPDBs reactions in order.
						await e.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("📅"));
					}
				});
			}
			return Task.CompletedTask;
		}

	}
}