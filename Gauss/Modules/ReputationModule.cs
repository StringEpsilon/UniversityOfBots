/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Gauss.Database;
using Gauss.Utilities;

namespace Gauss.Modules {
	public class ReputationModule : BaseModule {
		private readonly ReputationRepository _repository;
		private readonly Regex _thanksRegex = new Regex(@"\b(thank|thanks|thx|merci|gracias|ty|tyvm)\b", RegexOptions.IgnoreCase);
		private readonly Regex _amountRegex = new Regex(@"/s(<amount>(\+|-)?\d+)$", RegexOptions.IgnoreCase);
		private readonly Regex _takeRepExpression = new Regex(@"^-(takerep|-|tr|trep)", RegexOptions.IgnoreCase);
		private readonly Regex _giveRepExpression = new Regex(@"^-(giverep|gr|grep)", RegexOptions.IgnoreCase);



		public ReputationModule(DiscordClient client, ReputationRepository repository) {
			this._repository = repository;
			client.MessageCreated += this.HandleNewMessage;
		}

		private (DiscordMember, int) GetParameters(MessageCreateEventArgs e) {
			var username = e.Message.Content.Substring(e.Message.Content.IndexOf(" ") + 1);
			var amountRaw = _amountRegex.Match(username)?.Groups["amount"].Value;
			if (!int.TryParse(amountRaw, out int amount)) {
				amount = 1;
			}
			if (amount > 5) {
				return (null, 0);
			}
			var member = e.Guild.FindMember(username);
			if (member == null || member.Id == e.Author.Id) {
				return (null, 0);
			}
			return (member, amount);
		}

		public Task HandleNewMessage(DiscordClient client, MessageCreateEventArgs e) {
			if (e.Channel.IsPrivate) {
				return Task.CompletedTask;
			}
			return Task.Run(() => {
				var message = e.Message.Content;
				if (_takeRepExpression.IsMatch(e.Message.Content)) {
					var (member, amount) = this.GetParameters(e);
					if (member != null) {
						this._repository.TakeRep(e.Guild.Id, member.Id, amount);
					}
					return;
				}
				if (_giveRepExpression.IsMatch(e.Message.Content)) {
					var (member, amount) = this.GetParameters(e);
					if (member != null) {
						this._repository.GiveRep(e.Guild.Id, member.Id, amount);
					}
					return;
				}

				if (_thanksRegex.IsMatch(e.Message.Content)) {
					int count = 0;
					int limit = 5; // emulate the CLYDE limit on mentions processed for each give rep.
					foreach (var user in e.Message.MentionedUsers.Distinct()) {
						this._repository.GiveRep(e.Guild.Id, user.Id);
						count++;
						if (count >= limit) {
							return;
						}
					}
				}
			});
		}
	}
}
