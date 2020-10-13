/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using Gauss.CommandAttributes;
using Gauss.Commands;
using Gauss.Database;
using Gauss.Models;
using Gauss.Modules;
using Gauss.Scheduling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Gauss {
	public class GaussBot {
		private readonly DiscordClient _client;
		private readonly Scheduler _scheduler;
		private readonly GaussConfig _config;
		private readonly List<object> _modules = new List<object>();
		private readonly CommandsNextExtension _commands;

		public GaussBot(GaussConfig config) {
			this._config = config;
			this._client = new DiscordClient(new DiscordConfiguration {
				Token = config.DiscordToken,
			});

			this._scheduler = new Scheduler();
			
			var commandServices = new ServiceCollection()
				.AddDbContext<UserSettingsContext>(ServiceLifetime.Singleton)
				.AddDbContext<GuildSettingsContext>(ServiceLifetime.Singleton)
				.AddDbContext<PollRepository>(ServiceLifetime.Singleton)
				.AddSingleton(this._config)
				.AddSingleton(this._scheduler)
				.BuildServiceProvider();

			var commandConfig = new CommandsNextConfiguration {
				StringPrefixes = new List<string>() { _config.CommandPrefix },
				EnableDms = true,
				PrefixResolver = (message) => {
					if (message.Channel.IsPrivate) {
						return Task.FromResult(0);
					}
					return Task.FromResult(-1);
				},
				Services = commandServices,
				EnableMentionPrefix = true,
			};


			this._client.UseCommandsNext(commandConfig);
			this._client.UseInteractivity(new InteractivityConfiguration { });
			this._commands = this._client.GetCommandsNext();
			this._commands.RegisterCommands<SendMessageCommands>();
			this._commands.RegisterCommands<AdminCommands>();
			this._commands.RegisterCommands<MiscCommands>();
			this._commands.RegisterCommands<VoteCommands>();
			
			// this._modules.Add(new RoleAssign(this._client, _config));
			this._modules.Add(new WelcomeModule(this._client, this._config));

			this._modules.Add(new RedditLinker(this._client, this._config));
			this._modules.Add(new VCModule(this._client, this._config, commandServices));
			this._commands.CommandErrored += this.Commands_CommandErrored;

			this._client.Ready += this.OnClientReady;
		}

		private Task OnClientReady(ReadyEventArgs e) {
			return Task.Run(async () => {
				if (string.IsNullOrEmpty(this._config.StatusText)) {
					return;
				}
				await this._client.UpdateStatusAsync(new DiscordActivity(this._config.StatusText, ActivityType.Playing));
			});
		}

		private Task Commands_CommandErrored(CommandErrorEventArgs e) {
			if (string.IsNullOrEmpty(e.Command?.QualifiedName)) {
				return Task.CompletedTask;
			}

			if (e.Exception is Checks​Failed​Exception checkException) {
				if (checkException.FailedChecks.Any(ex => ex is CheckDisabledAttribute || ex is RequireAdminAttribute)) {
					e.Context.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("🚫"));
				}
			} else {
				e.Context.Client.Logger.Log(
					LogLevel.Error,
					$"Someone tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception}",

					DateTime.Now
				);
			}
			return Task.CompletedTask;
		}

		public void Connect() {
			Task.Run(() => this._client.ConnectAsync());
		}

		public void Disconnect() {
			this._client.DisconnectAsync().GetAwaiter().GetResult();
			this._client.Dispose();
		}
	}
}