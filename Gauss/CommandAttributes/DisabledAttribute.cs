/*!
This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this
file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace Gauss.CommandAttributes {
	public class DisabledAttribute : CheckBaseAttribute {
		public override Task<bool> ExecuteCheckAsync(CommandContext context, bool help) {
			return Task.FromResult(false);
		}
	}
}