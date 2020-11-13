/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Gauss.CommandAttributes;
using Gauss.Database;
using Gauss.Models;
using Gauss.Utilities;
using NodaTime;

namespace Gauss.Commands {
	[NotBot]
	[CheckDisabled]
	public class RemindMeCommands : BaseCommandModule {

		private readonly ReminderRepository _repository;

		public RemindMeCommands(ReminderRepository repository) {
			this._repository = repository;
		}

		[Command("settimezone")]
		public async Task SetUserTimezone(CommandContext context, string timezoneName) {
			var timezone = TimezoneHelper.Find(timezoneName);
			if (timezone != null) {
				var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
				this._repository.SetUserTimezone(context.User.Id, timezone);
				await context.RespondAsync($"Saved {timezone.Id}: {timezone.GetUtcOffset(now).ToTimeSpan()}");
			} else {
				await context.RespondAsync("Use a timezone from this list: <https://en.wikipedia.org/wiki/List_of_tz_database_time_zones>");
			}
			return;
		}

		[Command("remindme")]
		public async Task SetReminder(CommandContext context, DateTime datetime, [RemainingText] string message = "") {
			var zonedDateTime = datetime.InTimeZone(this._repository.GetUserTimezone(context.User.Id));
			Reminder reminder = new Reminder(zonedDateTime, message, context.User.Id);
			this._repository.AddReminder(reminder);
			await context.RespondAsync(embed: reminder.CreateEmbed());
			return;
		}

		[Command("remindme")]
		public async Task SetReminder(CommandContext context, DateTime date, DateTime time, [RemainingText] string message = "") {
			var datetime = date.Date + time.TimeOfDay;
			var zonedDateTime = datetime.InTimeZone(this._repository.GetUserTimezone(context.User.Id));
			Reminder reminder = new Reminder(zonedDateTime, message, context.User.Id);
			this._repository.AddReminder(reminder);
			await context.RespondAsync(embed: reminder.CreateEmbed());
			return;
		}

		[Command("remindme")]
		public async Task SetReminder(CommandContext context, string day, DateTime time, [RemainingText] string message = "") {
			ZonedDateTime zonedDateTime;
			var timezone = this._repository.GetUserTimezone(context.User.Id);
			var today = Instant.FromDateTimeUtc(DateTime.UtcNow).InZone(timezone);
			zonedDateTime = (today.Date.ToDateTimeUnspecified() + time.TimeOfDay).InTimeZone(timezone);
			switch (day) {
				case "today": {
						break;
					}
				case "tomorrow": {
						zonedDateTime = zonedDateTime.PlusHours(24);
						break;
					}
				default: {
						await context.RespondAsync($"'{day}' not supported. Only 'today' or 'tomorrow'");
						return;
					}
			}
			Reminder reminder = new Reminder(zonedDateTime, message, context.User.Id);
			this._repository.AddReminder(reminder);
			await context.RespondAsync(embed: reminder.CreateEmbed());
			return;
		}


		[Command("remindme")]
		public async Task SetReminder(CommandContext context, int time, string unit, [RemainingText] string message = "") {
			if (time < 0) {
				await context.RespondAsync("Reminder can't be set for the past.");
				return;
			}
			DateTime dueAt = DateTime.UtcNow;

			switch (unit.ToLower()) {
				case "minute":
				case "minutes": {
						dueAt = dueAt.AddMinutes(time);
						break;
					}
				case "hour":
				case "hours": {
						dueAt = dueAt.AddHours(time);
						break;
					}
				case "day":
				case "days": {
						dueAt = dueAt.AddDays(time);
						break;
					}
				case "week":
				case "weeks": {
						dueAt = dueAt.AddDays(time * 7);
						break;
					}
				case "month":
				case "months": {
						dueAt = dueAt.AddMonths(time);
						break;
					}
				case "year":
				case "years": {
						dueAt = dueAt.AddYears(time);
						break;
					}
				default: {
						await context.RespondAsync("Unknown unit of time. Supported: minute, hour, day, week, month, year");
						return;
					}
			}
			if (dueAt > DateTime.UtcNow.AddYears(5)) {
				await context.RespondAsync("Reminders set more than 5 years in the future are not supported");
				return;
			}

			Reminder reminder = new Reminder(dueAt, message, context.User.Id);
			this._repository.AddReminder(reminder);
			await context.RespondAsync(embed: reminder.CreateEmbed());
		}
	}
}