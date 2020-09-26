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
using Gauss.Models;
using Gauss.Modules;
using Gauss.Utilities;

namespace Gauss.Commands {
	[Group("voice")]
	[Description("Group of commands to manage your voice chat notification settings.")]
	public class VoiceCommands : BaseCommandModule {
		[Command("settings")]
		[GroupCommand]
		public async Task GetSettings(CommandContext context) {
			var config = VCModule.GetUserConfig(context);
			if (config == null) {
				await context.RespondAsync(
					"Notifications are inactive and or not configured. Use `!vox notify` to enable them."
				);
				return;
			}

			await context.RespondAsync(
				$"Notifications {(config.IsActive ? "active" : "inactive")}. " +
				$"Minimum discord status: {config.TargetStatus}. " +
				$"Filter: {config.FilterMode}."
			);
		}

		[Command("notify")]
		[Aliases("enable")]
		[Description("Enable notifications (through a DM) about people joining a voice chat channel.")]
		public async Task AddVoxNotification(
			CommandContext context,
			[Description("The status you have to be at to be notified. Values: Online, Idle, Busy, DoNotDisturb, Offline")]
			string status = "Idle"
		) {
			if (!Enum.TryParse(status, true, out UserStatus statusEnum)) {
				statusEnum = UserStatus.Idle;
			};

			var guild = context.GetGuild();
			var config = VCModule.GetUserConfig(context);
			if (config == null) {
				config = new VCNotificationConfig() {
					UserId = context.User.Id,
					GuildId = guild.Id,
					TargetStatus = statusEnum,
					IsActive = true,
				};
				VCModule.AddUserConfig(config);
			} else {
				config.TargetStatus = statusEnum;
				config.IsActive = true;
			}
			VCModule.SaveConfig();

			Console.WriteLine("AddVoxNotification(), calling GetSetings");
			await context.RespondAsync(
				$"Notifications {(config.IsActive ? "active" : "inactive")}. " +
				$"Minimum discord status: {config.TargetStatus}. " +
				$"Filter: {config.FilterMode}."
			);
			return;
		}

		[Group("filter")]
		[Description("Commands to modify your filter settings for notifications.")]
		public class VoxList : BaseCommandModule {
			public override Task BeforeExecutionAsync(CommandContext context) {
				var config = VCModule.GetUserConfig(context);
				if (config == null) {
					config = new VCNotificationConfig() {
						UserId = context.User.Id,
						GuildId = context.GetGuild().Id,
						IsActive = false,
					};
					VCModule.AddUserConfig(config);
					VCModule.SaveConfig();
				}
				return Task.CompletedTask;
			}

			[Command("whitelist")]
			[Description("Only receive notifications currently on your filter list.")]
			public async Task VoxSetWhitelist(CommandContext context) {
				var config = VCModule.GetUserConfig(context);
				config.FilterMode = FilterMode.Whitelist;
				VCModule.SaveConfig();

				var dmChannel = await context.GetDMChannel();
				await dmChannel.SendMessageAsync(
					config.TargetUsers.Count > 0
						? $"I will now only send you notifications for these people: " + string.Join(" ", config.TargetUsers.Select(y => y.Username))
						: "I will now only send you notifications for people you add to the list."
				);
			}

			[Command("blacklist")]
			[Description("You won't receive notifications currently on your filter list.")]
			public async Task VoxSetBlacklist(CommandContext context) {
				var config = VCModule.GetUserConfig(context);
				config.FilterMode = FilterMode.Blacklist;
				VCModule.SaveConfig();

				var dmChannel = await context.GetDMChannel();
				await dmChannel.SendMessageAsync(
					$"I won't bother you about these people starting a voice chat: " + string.Join(" ", config.TargetUsers)
				);
			}

			[Command("disable")]
			[Description("Disable filtering, but keeps the list intact for later reactivation")]
			public async Task VoxDisableFilter(CommandContext context) {
				var config = VCModule.GetUserConfig(context);
				config.FilterMode = FilterMode.Disabled;
				VCModule.SaveConfig();

				var dmChannel = await context.GetDMChannel();
				await dmChannel.SendMessageAsync($"I will ignore your filter list from now on, but I'll remember it for later.");
			}


			[Command("remove")]
			[Description("Remove people from the notification blacklist / whitelist.")]
			public async Task VoxRemoveFromList(
				CommandContext context,
				[Description("The users you want to remove from the list. No @Mentions!")]
				params string[] users
			) {
				var config = VCModule.GetUserConfig(context);
				var guild = context.GetGuild();
				var member = guild.Members[context.User.Id];
				foreach (var username in users) {
					var user = guild.Members.Values.FirstOrDefault(member => member.Username.ToLower() == username.ToLower());
					if (user != null) {
						config.TargetUsers.RemoveAll(y => y.UserId == user.Id);
					}
				}
				VCModule.SaveConfig();

				var dmChannel = await member.CreateDmChannelAsync();
				if (config.TargetUsers.Count() > 0) {
					string userList = string.Join(", ", config.TargetUsers.Select(y => y.Username));
					await dmChannel.SendMessageAsync($"Your {config.FilterMode} filter list consists of: {userList}");
				} else {
					await dmChannel.SendMessageAsync($"Your {config.FilterMode} filter is now cleared.");
				}
			}


			[Command("add")]
			[Description("Add people to the whistlist / blacklist of voice chat notifications.")]
			public async Task VoxAddToList(
				CommandContext context,
				[Description("The users you want to be notified about, as plain text. No @Mentions!")]
				params string[] users
			) {
				var config = VCModule.GetUserConfig(context);
				var guild = context.GetGuild();
				var member = guild.Members[context.User.Id];
				foreach (var username in users) {
					var user = guild.Members.Values.FirstOrDefault(member => member.Username.ToLower() == username.ToLower());
					if (user != null && !config.TargetUsers.Any(y => y.UserId == user.Id)) {
						config.TargetUsers.Add( new FilterEntry(){
							UserId = user.Id,
							Username = user.Username
						});
					}
				}
				VCModule.SaveConfig();

				string userList = string.Join(", ", config.TargetUsers.Select(y => y.Username));
				var dmChannel = await member.CreateDmChannelAsync();

				await dmChannel.SendMessageAsync($"Your filter list consists of: {userList}");
			}
		}


		[Command("unnotify")]
		[Aliases("disable")]
		[Description("Deactivate voice chat notifications entirely. This will keep your settings intact for later reactivation")]
		public async Task RemoveVoxNotification(CommandContext context) {
			var config = VCModule.GetUserConfig(context);
			var dmChannel = await context.GetDMChannel();
			if (config == null) {
				await dmChannel.SendMessageAsync("You don't have voice chat notiifcations active.");
				return;
			}
			config.IsActive = false;
			VCModule.SaveConfig();

			await context.RespondAsync($"I cleared your notification settings. I will no longer bother you about voice chats.");
		}
	}
}