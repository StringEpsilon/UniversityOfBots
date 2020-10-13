/*!
This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this
file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Gauss.Database;
using Gauss.Utilities;

namespace Gauss.CommandAttributes {
	public class CheckDisabledAttribute : CheckBaseAttribute {
		public override Task<bool> ExecuteCheckAsync(CommandContext context, bool help) {
			GuildSettingsContext settingsContext = (GuildSettingsContext)context.Services.GetService(typeof(GuildSettingsContext));
			if (settingsContext == null) {
				return Task.FromResult(true);
			}
			bool result = true;
			var command = context.Command;
			ulong guildId = context.GetGuild().Id;
			while (command != null) {
				result = !settingsContext.IsCommandDisabled(guildId, command.QualifiedName);
				command = result ? command.Parent : null;
			}

			if (result) {
				var userRestrictions = settingsContext.GetUserRestriction(guildId, context.User.Id);
				if (userRestrictions?.RestrictedCommands != null) {
					result = userRestrictions.FindCommandRestriction(context.Command.QualifiedName) == null;
				}
			}
			return Task.FromResult(result || help);
		}
	}
}