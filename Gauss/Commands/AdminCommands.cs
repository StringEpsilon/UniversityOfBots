/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Gauss.Utilities;
using Gauss.Database;
using Gauss.CommandAttributes;
using Gauss.Models;

namespace Gauss.Commands {
	[Group("admin")]
	[NeedsGuild]
	[RequireAdmin]
	[NotBot]
	[Description("Group of commands to manage your voice chat notification settings.")]
	public class AdminCommands : BaseCommandModule {
		private readonly GuildSettingsContext _context;

		public AdminCommands(GuildSettingsContext dbContext) {
			this._context = dbContext;
		}

		[Command("test")]
		public async Task AdminPermissionCheck(CommandContext context) {
			await context.RespondAsync("You have permissions to execute admin commands.");
		}

		[Command("addrole")]
		public async Task AddAdminRole(CommandContext context, ulong roleId) {
			var guild = context.GetGuild();
			var role = guild.Roles[roleId];
			if (role == null) {
				await context.RespondAsync("Could not find role with that ID.");
				return;
			}

			await context.CreateConfirmation(
				$"Give bot administration capabilities to `{role.Name}`?",
				() => {
					this._context.AddAdminRole(guild.Id, roleId);
					context.RespondAsync($"Users with the `{role.Name}` role can now use `admin` commands.");
				}
			);
		}

		[Command("removerole")]
		public async Task RemoveAdminRole(CommandContext context, ulong roleId) {
			var guild = context.GetGuild();
			var role = guild.Roles[roleId];

			if (!_context.IsAdminRole(guild.Id, roleId)) {
				await context.RespondAsync("The role with this ID does not have priviliges.");
				return;
			}

			await context.CreateConfirmation(
				$"Give bot administration capabilities to `{role.Name}`?",
				() => {
					this._context.RemoveAdminRole(guild.Id, roleId);
					context.RespondAsync($"The role `{(role != null ? role.Name : roleId.ToString())}` can no longer use `admin` commands.");
				}
			);
		}


		[Group("welcome")]
		public class WelcomeAdminCommands : BaseCommandModule {
			private readonly GaussConfig _config;

			public WelcomeAdminCommands(GaussConfig config) {
				this._config = config;
			}

			public override Task BeforeExecutionAsync(CommandContext context) {
				var guildId = context.GetGuild().Id;
				if (this._config.GuildConfigs.ContainsKey(guildId)) {

					this._config.GuildConfigs.Add(guildId, new GuildConfig());
				}

				return base.BeforeExecutionAsync(context);
			}

			[Command("message")]
			public async Task SetWelcomeMessage(CommandContext context, [RemainingText] string message) {
				ulong guildId = context.GetGuild().Id;

				this._config.GuildConfigs[guildId].WelcomeMessage = message;
				await context.RespondAsync("New message saved.");
				this._config.Save();
			}

			[Command("channel")]
			public async Task SetWelcomeMessageChannel(CommandContext context, ulong channelId) {
				ulong guildId = context.GetGuild().Id;

				this._config.GuildConfigs[guildId].WelcomeChannel = channelId;
				this._config.Save();
				await context.RespondAsync($"Target channel (<#{channelId}>) saved.");
			}
		}

		[Group("command")]
		public class CommandAdminCommands : BaseCommandModule {
			private readonly GuildSettingsContext _context;

			public CommandAdminCommands(GuildSettingsContext dbContext) {
				this._context = dbContext;
			}

			[Command("listdisabled")]
			[GroupCommand]
			[Aliases("list_disabled", "list-disabled")]
			public async Task ListDisabledCommands(CommandContext context) {
				var disabledCommands = this._context.GetDisabledCommands(context.GetGuild().Id);

				if (disabledCommands == null) {
					await context.RespondAsync("No commands are currently disabled.");
					return;
				} else {
					await context.RespondAsync("The following commands are currently disabled:\n " + string.Join(", ", disabledCommands));
				}
			}

			[Command("disable")]
			public async Task DisableCommand(CommandContext context, [RemainingText] string commandName) {
				var guild = context.GetGuild();
				var nameFragments = commandName.Split(".");
				Command command = context.CommandsNext.FindCommand(commandName, out _);

				if (command == null) {
					await context.RespondAsync("Could not find specified command.");
					return;
				}
				if (this._context.IsCommandDisabled(guild.Id, command.QualifiedName)) {
					await context.RespondAsync("Command is already disabled.");
					return;
				}

				string botMessage = null;
				if (command is CommandGroup group) {
					string subcommands = string.Join(", ", group.Children.Select(x => $"`{x.QualifiedName}`"));
					botMessage = $"Restrict command group `{group.Name}`?\nIt has these sub commands: {subcommands}'.";
				} else {
					botMessage = $"Restrict command `{command.Name}`";
				}
				botMessage += $"\nThe command(s) will be disabled **for everyone** on the server.";

				await context.CreateConfirmation(
					botMessage,
					() => {
						this._context.DisableCommand(guild.Id, command.QualifiedName);
						context.RespondAsync($"Command `{command.QualifiedName}`(including all subcommands) is now disabled.");
					}
				);
			}

			[Command("enable")]
			public async Task EnableCommand(CommandContext context, [RemainingText] string commandName) {
				var guild = context.GetGuild();
				var nameFragments = commandName.Split(".");
				Command command = context.CommandsNext.FindCommand(commandName, out _);

				if (command == null) {
					await context.RespondAsync("Could not find specified command.");
					return;
				}
				if (!this._context.IsCommandDisabled(guild.Id, command.QualifiedName)) {
					await context.RespondAsync("Command isn't disabled.");
					return;
				}

				string botMessage = null;
				if (command is CommandGroup group) {
					string subcommands = string.Join(", ", group.Children.Select(x => $"`{x.QualifiedName}`"));
					botMessage = $"Enable command group `{group.Name}`?\nIt has these sub commands: {subcommands}'.";
				} else {
					botMessage = $"Enable command `{command.Name}`?";
				}
				botMessage += $"\nThe command(s) will be enabled for everyone on the server.";

				await context.CreateConfirmation(
					botMessage,
					() => {
						this._context.EnableCommand(guild.Id, command.QualifiedName);
						context.RespondAsync($"Command `{command.QualifiedName}` is now enabled.");
					}
				);
			}

			[Command("disableFor")]
			[Aliases("disable_For", "disable-for")]
			public async Task DisableCommandFor(CommandContext context, string userName, [RemainingText] string commandName) {
				var guild = context.GetGuild();
				var nameFragments = commandName.Split(".");
				var user = guild.FindMember(userName);

				if (user == null) {
					await context.RespondAsync($"Can not find the user '{userName}'.");
					return;
				}

				Command command = context.CommandsNext.FindCommand(commandName, out _);
				if (command == null) {
					await context.RespondAsync("Could not find specified command.");
					return;
				}
				string botMessage = null;
				if (command is CommandGroup group) {
					string subcommands = string.Join(", ", group.Children.Select(x => $"`{x.QualifiedName}`"));
					botMessage = $"Disable command group `{group.Name}`` for {user.Username}?\nIt has these sub commands: {subcommands}'.";
				} else {
					botMessage = $"Disable command `{command.QualifiedName}` for {user.Username}?";
				}
				botMessage += $"\nThe user '{userName}' won't be able to use the command(s).";

				await context.CreateConfirmation(
					botMessage,
					async () => {
						this._context.SetRestrictionForUser(
							guild.Id,
							user.Id,
							new CommandRestriction { CommandName = command.QualifiedName }
						);
						await context.RespondAsync($"Disabled `{command.QualifiedName}` for {user.Username}?.");
					},
					async () => {
						await context.RespondAsync("Action aborted. No settings changed.");
					}
				);
			}

			[Command("enableFor")]
			[Aliases("enable_For", "enable-for")]
			public async Task EnableCommandFor(CommandContext context, string userName, [RemainingText] string commandName) {
				var guild = context.GetGuild();
				var nameFragments = commandName.Split(".");
				var user = guild.FindMember(userName);
				if (user == null) {
					await context.RespondAsync($"Can not find the user '{userName}'.");
					return;
				}

				CommandRestriction restriction = _context.GetUserRestriction(guild.Id, user.Id)?.FindCommandRestriction(commandName);

				if (restriction == null) {
					await context.RespondAsync("The user is already allowed to use this command.");
					return;
				}
				string botMessage = $"Re-enable command `{restriction.CommandName}` for {user.Username}?";

				await context.CreateConfirmation(
					botMessage,
					async () => {
						this._context.RemoveRestrictionForUser(
							guild.Id,
							user.Id,
							restriction
						);
						await context.RespondAsync($"Re-enabled `{restriction.CommandName}` for {user.Username}?.");
					},
					async () => {
						await context.RespondAsync("Action aborted. No settings changed.");
					}
				);
			}
		}
	}
}