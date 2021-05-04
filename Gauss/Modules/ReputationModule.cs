/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

using System.Collections.Generic;
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
		private readonly Regex _thanksRegex = new(@"(?<!^>)\b(thank|thanks|thx|merci|gracias|ty|tyvm)\b", RegexOptions.IgnoreCase);
		private readonly Regex _amountRegex = new(@"/s(<amount>(\+|-)?\d+)$", RegexOptions.IgnoreCase);
		private readonly Regex _takeRepExpression = new(@"^-(takerep|-|tr|trep)", RegexOptions.IgnoreCase);
		private readonly Regex _giveRepExpression = new(@"^-(giverep|gr|grep)", RegexOptions.IgnoreCase);
		private readonly Regex _topRepExpression = new(@"^-(toprep)", RegexOptions.IgnoreCase);

		public ReputationModule(DiscordClient client, ReputationRepository repository) {
			this._repository = repository;
			client.MessageCreated += this.HandleNewMessage;
		}

		private static List<ulong> GetMentionedUsers(string message) {
			var regex = new Regex(@"(?:<@!?)(\d+)(?:>)");
			var matches = regex.Matches(message);
			return matches.Select(match => ulong.Parse(match.Groups[1].Value)).ToList();
		}

		private (DiscordUser, int) GetParameters(MessageCreateEventArgs e) {
			var username = e.Message.Content.Substring(e.Message.Content.IndexOf(" ") + 1);
			var amountRaw = _amountRegex.Match(username)?.Groups["amount"].Value;
			if (!int.TryParse(amountRaw, out int amount)) {
				amount = 1;
			}
			if (amount > 5) {
				return (null, 0);
			}
			var member = (DiscordUser)e.Guild.FindMember(username);
			if (member == null || member.Id == e.Author.Id) {
				if (e.MentionedUsers.Count == 1) {
					member = e.MentionedUsers[0];
				} else {
					return (null, 0);
				}
			}
			return (member, amount);
		}

		public Task HandleNewMessage(DiscordClient client, MessageCreateEventArgs e) {
			if (e.Channel.IsPrivate) {
				return Task.CompletedTask;
			}
			return Task.Run(() => {
				var message = new Regex(@"^>.+").Replace(e.Message.Content, "");
				if (_takeRepExpression.IsMatch(message)) {
					var (member, amount) = this.GetParameters(e);
					client.ExecuteCommand(e.Channel, e.Author, "takereputation", member?.Mention, amount.ToString());
					return;
				} else if (_giveRepExpression.IsMatch(message)) {
					var (member, amount) = this.GetParameters(e);
					client.ExecuteCommand(e.Channel, e.Author, "givereputation", member?.Mention, amount.ToString());
					return;
				} else if (_topRepExpression.IsMatch(message)) {
					client.ExecuteCommand(e.Channel, e.Author, "leaderboard");
				}

				if (_thanksRegex.IsMatch(message)) {
					var mentionedUsers = GetMentionedUsers(message).Distinct();
					if (e.Message.Reference != null) {
						mentionedUsers = mentionedUsers.Append(e.Message.Reference.Message.Author.Id);
					}
					IEnumerable<DiscordMember> awardedMembers = mentionedUsers
						.Where(y => y != e.Author.Id)
						.Distinct()
						.Select(y => e.Guild.Members[y]);

					foreach (var member in awardedMembers) {
						this._repository.GiveRep(e.Guild.Id, member.Id);
					}
					if (awardedMembers.Count() <= 4) {
						int points;
						int rank;
						var replyLines = awardedMembers.Select(y => {
							(points, rank) = _repository.GetRank(e.Guild.Id, y.Id);

							return $"Gave `1` Bayes Point to **{y.DisplayName}**. Currently `#{rank}: {points}`.";
						});
						e.Channel.SendMessageAsync(string.Join("\n", replyLines));
					} else {
						e.Channel.SendMessageAsync($"Gave `1` Bayes Point each to: {string.Join(", ", awardedMembers.Select(y => y.DisplayName))}");
					}
				}
			});
		}
	}
}