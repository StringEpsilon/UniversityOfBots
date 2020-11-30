/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Gauss.CommandAttributes;
using Gauss.Commands;
using Gauss.Converters;
using Gauss.Database;
using Gauss.Logging;
using Gauss.Models;
using Gauss.Modules;
using Gauss.Scheduling;
using Gauss.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Gauss {
	public class GaussBot {
		private readonly DiscordClient _client;
		private readonly GaussConfig _config;
		private readonly List<object> _modules = new();
		private readonly CommandsNextExtension _commands;
		private readonly Scheduler _scheduler;


		public void RegisterModule(TypeInfo type, IServiceProvider services) {
			if (type.CustomAttributes.OfType<ModuleInactiveAttribute>().Any()) {
				this._client.Logger.LogInformation(LogEvent.Module, $"Module '{type.Name}' is marked inactive.");
				return;
			}
			this._modules.Add(
				ActivatorUtilities.CreateInstance(services, type)
			);
			this._client.Logger.LogInformation(LogEvent.Module, $"Module '{type.Name}' registered and active.");
		}

		public void RegisterModules(Assembly assembly, IServiceProvider services) {
			var modules = assembly.DefinedTypes.Where(type => type.IsSubclassOf(typeof(BaseModule)));
			foreach (var module in modules) {
				this.RegisterModule(module, services);
			}
			_client.Logger.LogInformation(LogEvent.Module, $"Found {modules.Count()} non-command modules");
		}

		public GaussBot(GaussConfig config) {
			this._config = config;
			this._client = new DiscordClient(
				new DiscordConfiguration {
					LoggerFactory = new GaussLoggerFactory(config.LogConfig),
					Intents = DiscordIntents.AllUnprivileged | DiscordIntents.GuildMembers,
					Token = config.DiscordToken,
				}
			);

			this._scheduler = new Scheduler(this._client);
			// Register stuff for dependency injection.
			// DSharpPlus will supply the commandServices collection to any class instanciated internally.
			var commandServices = new ServiceCollection()
				.AddDbContext<UserSettingsContext>(ServiceLifetime.Singleton)
				.AddDbContext<GuildSettingsContext>(ServiceLifetime.Singleton)
				.AddSingleton(new CalendarAccessor(this._config, this._client.Logger))
				.AddSingleton(new ReputationRepository(config.ConfigDirectory))
				.AddSingleton(new ReminderRepository(config.ConfigDirectory))
				.AddSingleton(new ElectionRepository(config.ConfigDirectory))
				.AddSingleton(this._scheduler)
				.AddSingleton(this._config)
				.AddSingleton(this._client)
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
			this._client.GuildAvailable += this.OnGuildAvailable;
			this._client.UseInteractivity(new InteractivityConfiguration { });

			// Register the command modules:
			this._commands = this._client.GetCommandsNext();
			this._commands.RegisterConverter(new DateTimeConverter());
			this._commands.RegisterCommands<SendMessageCommands>();
			this._commands.RegisterCommands<AdminCommands>();
			this._commands.RegisterCommands<MiscCommands>();
			this._commands.RegisterCommands<FoldingCommands>();
			this._commands.RegisterCommands<ElectionCommands>();
			this._commands.RegisterCommands<RemindMeCommands>();
			this._commands.RegisterCommands<TimeCommands>();

			// Register all internal "modules" that handle non-command tasks:
			this.RegisterModules(Assembly.GetExecutingAssembly(), commandServices);

			// setup event handlers:
			this._client.Ready += this.OnClientReady;
			this._commands.CommandErrored += this.Commands_CommandErrored;
		}

		private Task OnGuildAvailable(DiscordClient client, GuildCreateEventArgs e) {
			// Get the member list cached, otherwise certain features don't work correctly.
			return Task.Run(async () => await e.Guild.RequestMembersAsync());
		}

		private Task OnClientReady(DiscordClient client, ReadyEventArgs e) {
			// Set status text after client ready:
			return Task.Run(async () => {
				if (string.IsNullOrEmpty(this._config.StatusText)) {
					return;
				}
				await this._client.UpdateStatusAsync(new DiscordActivity(this._config.StatusText, ActivityType.Playing));
			});
		}

		private Task Commands_CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e) {
			switch (e.Exception) {
				case Checks​Failed​Exception checkException: {
						if (checkException.FailedChecks.Any(ex => ex is CheckDisabledAttribute || ex is RequireAdminAttribute)) {
							e.Context.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("🚫"));
						}
						if (checkException.FailedChecks.Any(ex => ex is NeedsGuildAttribute)) {
							e.Context.Message.RespondAsync("Could not determine a server to execute this command for.");
						}
						break;
					}
				case ArgumentException argumentException: {
						if (e.Command != null) {
							var command = e.Command;
							var sb = new StringBuilder();

							foreach (var ovl in command.Overloads.OrderByDescending(x => x.Priority)) {
								sb.Append('`')
									.Append(command.QualifiedName);

								foreach (var arg in ovl.Arguments) {
									sb.Append(arg.IsOptional || arg.IsCatchAll
										? " ["
										: " <"
									)
									.Append(arg.Name)
									.Append(arg.IsCatchAll ? "..." : "")
									.Append(arg.IsOptional || arg.IsCatchAll ? ']' : '>');
								}
								sb.Append('`');
							}
							if (e.Context.Channel.IsPrivate) {
								e.Context.RespondAsync($"Invalid syntax for `{e.Command.QualifiedName}`. Syntax:\n{sb}");
							} else {
								e.Context.Member.SendMessageAsync($"Invalid syntax for `{e.Command.QualifiedName}`. Syntax:\n{sb}");
							}
						}
						break;
					}
				case CommandNotFoundException ex: {
						this._client.Logger.LogError(
							LogEvent.Module,
							e.Exception,
							$"Someone tried executing an unknown command. {ex.CommandName}"
						);
						e.Context.GuessCommand(ex.CommandName);
						break;
					}
				case InvalidOperationException invalidOperationException: {
						this._client.Logger.LogError(
							LogEvent.Module,
							e.Exception,
							$"Someone tried executing a command, but it failed."
						);
						if (e.Context.Command is CommandGroup group) {
							var probableName = e.Context.Command.QualifiedName + " " + e.Context.RawArgumentString.Split(" ").First();
							e.Context.GuessCommand(probableName);
						}
						break;
					}
				default: {
						this._client.Logger.LogError(
							LogEvent.Module,
							e.Exception,
							$"Someone tried executing a command, but it failed."
						);
						break;
					}
			}

			return Task.CompletedTask;
		}

		public void Connect() {
			Task.Run(() => this._client.ConnectAsync());
		}

		public void Disconnect() {
			this._client.Logger.LogInformation("Disconnecting bot and shutting down.");
			this._client.DisconnectAsync().GetAwaiter().GetResult();
			this._client.Dispose();
		}
	}
}