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
	[Group("time")]
	public class TimeCommands : BaseCommandModule {
		[GroupCommand]
		[Command("now")]
		[Description("Get the current time in your configured timezone (or UTC).")]
		public async Task ConvertTime(CommandContext context) {
			var timezone = this._repository.GetUserTimezone(context.User.Id);
			if (timezone == null) {
				return;
			}

			var zonedDateTime = SystemClock.Instance.GetCurrentInstant().InZone(timezone);
			var embed = new DiscordEmbedBuilder()
				.WithColor(DiscordColor.None)
				.WithFooter($"Current time: {zonedDateTime:yyyy-MM-dd HH:mm} {timezone.Id}")
				.WithTimestamp(zonedDateTime.ToDateTimeUtc())
				.Build();

			await context.RespondAsync(embed: embed);
		}


		[Command("now")]
		[Description("Get the current time in a given timezone.")]
		public async Task ConvertTime(
			CommandContext context, 
			[Description("Name of the timezone")]
			string timezoneName
		) {
			var timezone = TimezoneHelper.Find(timezoneName);
			if (timezone == null) {
				return;
			}

			var zonedDateTime = Instant.FromDateTimeUtc(DateTime.UtcNow).InZone(timezone);
			var embed = new DiscordEmbedBuilder()
				.WithColor(DiscordColor.None)
				.WithFooter($"Current time: {zonedDateTime:yyyy-MM-dd HH:mm} {timezone.Id}")
				.WithTimestamp(zonedDateTime.ToDateTimeUtc())
				.Build();

			await context.RespondAsync(embed: embed);
		}


		[Command("convert")]
		[Description("Convert a UTC (date and) time to your configured timezone.")]
		public async Task ConvertTime(
			CommandContext context, 
			[Description("Time (or date and time) you want to convert.")]
			DateTime datetime
		) {
			var timezone = this._repository.GetUserTimezone(context.User.Id);
			var zonedDateTime = datetime.InTimeZone(timezone);

			await context.RespondAsync(
				embed: new DiscordEmbedBuilder()
					.WithColor(DiscordColor.None)
					.WithFooter($"{zonedDateTime:yyyy-MM-dd HH:mm} {timezone.Id} in your local time:")
					.WithTimestamp(zonedDateTime.ToDateTimeUtc())
					.Build()
			);
		}

		[Command("convert")]
		[Description("Convert a UTC (date and) time to a given timezone.")]
		public async Task ConvertTime(CommandContext context, 
			[Description("Time (or date and time) you want to convert.")]
			DateTime datetime, 
			[Description("Name of the timezone.")]
			string timezoneName
		) {
			var timezone = TimezoneHelper.Find(timezoneName);
			if (timezone == null) {
				return;
			}

			var zonedDateTime = datetime.InTimeZone(timezone);
			await context.RespondAsync(
				embed: new DiscordEmbedBuilder()
					.WithColor(DiscordColor.None)
					.WithFooter($"{zonedDateTime:yyyy-MM-dd HH:mm} {timezone.Id} in your local time:")
					.WithTimestamp(zonedDateTime.ToDateTimeUtc())
					.Build()
			);
		}

		[Command("convert")]
		public async Task ConvertTime(CommandContext context, DateTime date, DateTime time) {
			var datetime = date.Date + time.TimeOfDay;
			await this.ConvertTime(context, datetime);
		}

		[Command("convert")]
		public async Task ConvertTime(CommandContext context, DateTime date, DateTime time, string timezoneName) {
			var datetime = date.Date + time.TimeOfDay;
			await this.ConvertTime(context, datetime, timezoneName);
		}
	}
}