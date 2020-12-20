/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Gauss.Models;

namespace Gauss.Modules {
	public class WelcomeModule : BaseModule {
		private readonly GaussConfig _config;
		private DiscordMessage _lastMessage;
		private readonly Regex _triggerExpression = new("(met gauss|meet gauss)", RegexOptions.IgnoreCase);

		public WelcomeModule(DiscordClient client, GaussConfig config) {
			client.MessageCreated += this.HandleNewMessage;
			client.MessageDeleted += this.HandleMessageDeletion;
			this._config = config;
		}

		private Task HandleMessageDeletion(DiscordClient client, MessageDeleteEventArgs e) {
			if (e.Message == this._lastMessage) {
				this._lastMessage = null;
			}
			return Task.CompletedTask;
		}

		public Task HandleNewMessage(DiscordClient client, MessageCreateEventArgs e) {
			if (e.Channel.IsPrivate || e.Author.IsBot) {
				return Task.CompletedTask;
			}

			return Task.Run(async () => {
				if (!this._config.GuildConfigs.TryGetValue(e.Guild.Id, out GuildConfig guildConfig)) {
					return;
				}
				if (guildConfig.WelcomeChannel == 0 || guildConfig.WelcomeMessage == null) {
					return;
				}
				if (e.Channel.Id != guildConfig.WelcomeChannel) {
					return;
				}

				if (_triggerExpression.IsMatch(e.Message.Content)) {
					if (this._lastMessage != null) {
						await this._lastMessage.ModifyAsync("[This previously contained the welcome message]");
					}
					this._lastMessage = await e.Channel.SendMessageAsync(guildConfig.WelcomeMessage);
				}
			});
		}
	}
}
