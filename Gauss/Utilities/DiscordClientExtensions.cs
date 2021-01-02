/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace Gauss.Utilities {
	public static class DiscordClientExtensions {

		public static void ExecuteCommand(
			this DiscordClient client,
			DiscordChannel channel,
			DiscordUser user,
			string commandName,
			params string[] parameters
		) {
			var commandsNext = client.GetCommandsNext();
			var context = commandsNext.CreateFakeContext(
				user,
				channel,
				commandName,
				"",
				commandsNext.FindCommand(commandName, out _),
				string.Join(" ", parameters)
			);

			commandsNext.ExecuteCommandAsync(context);
		}
	}
}