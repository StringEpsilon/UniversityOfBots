/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Gauss.Database;
using Gauss.Utilities;
using NodaTime;

namespace Gauss.Modules {
	public class AutoTimeModule : BaseModule {
		private readonly ReminderRepository _repository;
		private readonly Regex _triggerExpression = new(
			@"\[(?<hour>[0-2]\d):(?<minute>[0-6]\d):?(?<second>[0-6]\d)?(?<timezone>\s?[^\]]+)?\]",
			RegexOptions.IgnoreCase
		);

		public AutoTimeModule(DiscordClient client, ReminderRepository repository) {
			this._repository = repository;
			client.MessageCreated += this.HandleNewMessage;
		}

		public Task HandleNewMessage(DiscordClient client, MessageCreateEventArgs e) {
			if (e.Channel.IsPrivate || e.Author.IsBot) {
				return Task.CompletedTask;
			}

			return Task.Run(async () => {
				var counter = 0;
				var matches = _triggerExpression.Matches(e.Message.Content);
				foreach (Match match in matches) {
					counter++;
					if (counter == 3) {
						return;
					}
					var hourRaw = match.Groups["hour"].Value.Trim();
					var minuteRaw = match.Groups["minute"].Value.Trim();
					var secondRaw = match.Groups["second"].Value.Trim();
					var timezoneRaw = match.Groups["timezone"].Value.Trim();

					DateTimeZone timezone = null;
					if (!string.IsNullOrEmpty(timezoneRaw)) {
						timezone = TimezoneHelper.Find(timezoneRaw.Trim());
					} else {
						timezone = this._repository.GetUserTimezone(e.Message.Author.Id);
						if (timezone.Id == "UTC") {
							timezone = null;
						}
					}

					if (timezone == null) {
						await e.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("❌"));
						return;
					}
					try {
						var today = Instant.FromDateTimeUtc(DateTime.UtcNow).InZone(timezone);
						var date = new LocalDateTime(
							today.Year,
							today.Month,
							today.Day,
							int.Parse(hourRaw),
							int.Parse(minuteRaw),
							string.IsNullOrEmpty(secondRaw) ? 0 : int.Parse(secondRaw)
						).InZoneLeniently(timezone);
						await e.Message.Channel.SendMessageAsync(
							embed: new DiscordEmbedBuilder()
								.WithFooter($"Above time ({date:HH:mm} {timezone.Id}) in your local time")
								.WithTimestamp(date.ToDateTimeUtc())
							 	.Build()
						);
					} catch (Exception) {
					}
					System.Threading.Thread.Sleep(100);
				}
			});
		}
	}
}
