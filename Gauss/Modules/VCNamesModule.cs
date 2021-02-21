/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Gauss.Models;

namespace Gauss.Modules {
	public class VCNamesModule : BaseModule {
		private readonly GaussConfig _config;

		public VCNamesModule(DiscordClient client, GaussConfig config) {
			this._config = config;
			client.VoiceStateUpdated += this.HandleVoiceStateEvent;
		}

		private void HandleLeaveVoiceChat(DiscordGuild guild, DiscordChannel channel) {
			Task.Run(async () => {
				var guildConfig = this._config.GuildConfigs[guild.Id];
				if (guildConfig.VoiceChannelNames?.ContainsKey(channel.Id) == true) {
					await channel.ModifyAsync(model => model.Name = guildConfig.VoiceChannelNames[channel.Id]);
				}
			});
		}

		private Task HandleVoiceStateEvent(DiscordClient client, VoiceStateUpdateEventArgs e) {
			if (e.Before?.Channel != null && e.After?.Channel == null) {
				this.HandleLeaveVoiceChat(e.Guild, e.Before.Channel);
			}
			return Task.CompletedTask;
		}

	}
}