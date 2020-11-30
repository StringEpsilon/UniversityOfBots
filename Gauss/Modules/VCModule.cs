/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Gauss.Database;
using Gauss.Models;
using Gauss.Utilities;

namespace Gauss.Modules {
	public class VCModule : BaseModule {
		private readonly GaussConfig _config;
		private readonly UserSettingsContext _settings;

		public VCModule(DiscordClient client, GaussConfig config, UserSettingsContext settings) {
			this._config = config;
			this._settings = settings;
			client.VoiceStateUpdated += this.HandleVoiceStateEvent;
		}

		private Task HandleJoinVoiceChat(DiscordGuild guild, DiscordUser user, DiscordChannel channel) {

			return Task.Run(async () => {
				await Task.Delay(TimeSpan.FromSeconds(10));
				if (!channel.Users.Any()) {
					return;
				}

				// Check only certain categories:
				if (!channel.ParentId.HasValue || this._config.VoiceNotificationCategories.Contains(channel.ParentId.Value)) {
					return;
				}
				// Only alert for users who have VC notifications enabled themselves:
				var userConfig = this._settings.GetVoiceSettings(guild.Id, user.Id);
				if (userConfig == null) {
					return;
				}
				userConfig.IsInTimeout = true;
				var voiceUsers = this._settings.GetVoiceUsers(guild.Id);
				foreach (var user in voiceUsers.Where(y => y.UserId != user.Id && y.CheckFilter(user.Id))) {
					try {
						var member = guild.Members[user.UserId];
						var matchingStatus = member?.Presence != null && member.Presence.Status.MatchesAvailability(user.TargetStatus);
						if (!matchingStatus) {
							continue;
						}
						await member.SendMessageAsync($"A user just joined the {channel.Name} voice channel in ${guild.Name}!");
						user.IsInTimeout = true;
					} catch (Exception ex) {
						throw new Exception($"Exception while trying to notify {user.UserId} about voice chat. {ex}");
					}
				}
				_settings.SaveChanges();
			});
		}


		private void HandleLeaveVoiceChat(DiscordGuild guild, DiscordUser user, DiscordChannel channel) {
			Task.Run(() => {
				if (channel?.Users?.Count() == 0) {
					Task.Run(async () => {
						await Task.Delay(TimeSpan.FromSeconds(15));
						if (channel?.Users?.Count() > 0) {
							return;
						}

						foreach (var config in _settings.UserVoiceSettings.Where(x => x.GuildId == guild.Id)) {
							config.IsInTimeout = false;
						}
						_settings.SaveChanges();
					});
				}
			});
		}

		private Task HandleVoiceStateEvent(DiscordClient client, VoiceStateUpdateEventArgs e) {
			/* 	3 possible events:
				A: User joins a voice channel.
					-> Dispatch one JoinedVoiceChannel event.
				B: User switches from one channel to another.
					-> Dispatch one JoinedVoiceChannel event.
					-> Dispatch one LeftVoiceChannel event.
				C: User disconnects from voice.
					-> Dispatch one LeftVoiceChannel event.
			*/
			if (e.Before?.Channel == null && e.After?.Channel != null) {
				this.HandleJoinVoiceChat(e.Guild, e.User, e.Channel);
			}
			if (e.Before?.Channel != null && e.After?.Channel == null) {
				this.HandleLeaveVoiceChat(e.Guild, e.User, e.Before.Channel);
			}
			return Task.CompletedTask;
			/*
				Exmaple:  Xorander joins General Voice 01 -> Notify IonSprite.
						Ripple joins General Voice 02 -> No additional notification.
						And only when all voice channels in #General are empty again can a new notification be send.
			*/
		}

	}
}