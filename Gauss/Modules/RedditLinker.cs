/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace Gauss.Modules {

	class RedditLinker : BaseModule {
		public RedditLinker(DiscordClient client) {
			client.MessageCreated += this.HandleNewMessage;
		}

		public Task HandleNewMessage(DiscordClient client, MessageCreateEventArgs e) {
			return Task.Run(async () => {
				var redditRegex = new Regex(@"(?<!^>)((?:^|\s)\/?r\/\w{2,21}\b)", RegexOptions.Multiline);
				var redditMatches = redditRegex.Matches(e.Message.Content);
				List<string> validSubs = new();
				List<string> nsfwSubs = new();
				bool addShrug = false;
				foreach (Match match in redditMatches) {
					if (match.Groups.Count >= 1) {
						string subreddit = match.Groups[1].Value.Trim();
						subreddit = subreddit.StartsWith("/") ? subreddit : "/" + subreddit;
						string content = "";
						try {
							var client = new HttpClient();
							client.DefaultRequestHeaders.Add("User-Agent", "Gauss discord bot");
							var result = await client.GetAsync($"https://api.reddit.com/{subreddit}/about");
							content = await result.Content.ReadAsStringAsync();
							var json = JsonDocument.Parse(content);
							if (!json.RootElement.GetProperty("data").GetProperty("over18").GetBoolean()) {
								validSubs.Add(subreddit);
							} else {
								nsfwSubs.Add(subreddit);
							}
						} catch (Exception) {
							addShrug = true;
						}
					}
				}
				string answer = "";

				if (validSubs.Count > 0) {
					answer = validSubs.Count == 1
						? $"Here is a clickable link: <https://www.reddit.com{validSubs[0]}>"
						: "Here are clickable links:\n" + string.Join("\n", validSubs.Select(y => $"<https://www.reddit.com{y}>"));
				}
				if (nsfwSubs.Count > 0) {
					answer += nsfwSubs.Count == 1
						? $"Warning: {nsfwSubs[0]} is NSFW!"
						: $"Warning! These subs are NSFW:" + string.Join(", ", validSubs);
				}
				if (addShrug && nsfwSubs.Count == 0 && validSubs.Count == 0) {
					await e.Message.CreateReactionAsync(DiscordEmoji.FromName(client, ":person_shrugging:"));
				} else if (!string.IsNullOrEmpty(answer)) {
					await e.Message.RespondAsync(answer);
				}
			});
		}
	}
}
