/*!
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus;
using Gauss.Models;
using Gauss.Scheduling;
using Gauss.Utilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Gauss.Modules {
	public class FoldingModule : BaseModule {
		private readonly Scheduler _scheduler;
		private readonly DiscordClient _client;
		private readonly GaussConfig _config;
		private readonly TimeSpan _timeSpan = TimeSpan.FromHours(24);

		private readonly Dictionary<int, FoldingStatus> _previousStats;

		public FoldingModule(Scheduler scheduler, DiscordClient client, GaussConfig config) {
			this._scheduler = scheduler;
			this._client = client;
			this._config = config;
			try {
				this._previousStats = JsonUtility.Deserialize<Dictionary<int, FoldingStatus>>(
					Path.Join(this._config.ConfigDirectory, "foldingdata.json")
				);
			} catch (Exception) {
				this._client.Logger.LogInformation(LogEvent.Folding, $"Could not restore F@H data. Starting new record.");
				_previousStats = new Dictionary<int, FoldingStatus>();
			}

			var now = DateTime.UtcNow;

			var scheduleStart = new DateTime(now.Year, now.Month, now.Day, 12, 0, 0, kind: DateTimeKind.Utc);

			while (now > scheduleStart) {
				scheduleStart += _timeSpan;
			}
			this._client.Logger.LogInformation(LogEvent.Folding, $"F@H updates start at {scheduleStart:yyyy-MM-dd HH:mm}");

			this._scheduler.AddTask(
				new CyclicTask(
					_timeSpan,
					scheduleStart,
					this.PostUpdate
				)
			);
		}

		public async Task PostUpdate() {
			foreach (var item in this._config.GuildConfigs.Where(y => !string.IsNullOrEmpty(y.Value.FoldingTeam))) {
				FoldingStatus stats = null;
				try {
					stats = await GetFoldingStats(item.Value.FoldingTeam);
				} catch (Exception ex) {
					await this._client.Guilds[item.Key].Channels[item.Value.FoldingChannel].SendMessageAsync(
						"[Encountered an error trying to get statistics.]"
					);
					this._client.Logger.LogError(LogEvent.Folding, ex, $"Could not retrieve F@H data.");
					return;
				}
				bool hasPreviousValue = false;

				var header = $"```{stats.Name} F@H Statistics {stats.Last:yyyy-MM-dd HH:mm:ss}";
				var teamCredit = $"Team credit: {stats.Credit:N0}";
				var currentRank = $"Current rank: {stats.Rank:N0} overall";
				var monthlyRank = $"Monthly rank: {stats.MonthlyRank:N0}";
				var workUnits = $"WUs folded: {stats.WorkUnits:N0}";
				var members = $"Team members: {stats.Donors.Count:N0}";
				var creditsPerWU = $"Average credit per WU: {stats.Credit / stats.WorkUnits:N0}";
				var footer = $"```<https://stats.foldingathome.org/team/{item.Value.FoldingTeam}>";

				if (this._previousStats.ContainsKey(stats.Team)) {
					var prevStatus = this._previousStats[stats.Team];
					hasPreviousValue = true;
					teamCredit += $" ({stats.Credit - prevStatus.Credit:+#;-#;+0})";
					currentRank += $" ({stats.Rank - prevStatus.Rank:+#;-#;+0})";
					monthlyRank += $" ({stats.MonthlyRank - prevStatus.MonthlyRank:+#;-#;+0})";
					workUnits += $" ({stats.WorkUnits - prevStatus.WorkUnits:+#;-#;+0})";
					members += $" ({stats.Donors.Count - prevStatus.Donors.Count:+#;-#;+0})";
				}

				try {
					await this._client.Guilds[item.Key].Channels[item.Value.FoldingChannel].SendMessageAsync(
						header + "\n" +
						teamCredit + "\n" +
						currentRank + "\n" +
						monthlyRank + "\n" +
						workUnits + "\n" +
						members + "\n" +
						creditsPerWU + "\n" +
						footer
					);
				} catch (Exception ex) {
					this._client.Logger.LogError(LogEvent.Folding, ex, $"Could not post F@H update.");
				}
				if (!hasPreviousValue) {
					this._previousStats.Add(stats.Team, stats);
				} else {
					this._previousStats[stats.Team] = stats;
				}
			}
			JsonUtility.Serialize(
				Path.Join(this._config.ConfigDirectory, "foldingdata.json"),
				this._previousStats
			);
		}

		public static async Task<FoldingStatus> GetFoldingStats(string teamId) {
			FoldingStatus stats = null;
			var client = new HttpClient();
			try {
				var response = await client.GetAsync($"https://stats.foldingathome.org/api/team/{teamId}");
				var currentDate = DateTime.UtcNow;
				var monthlyStatsURI = $"https://stats.foldingathome.org/api/teams-monthly?search_type=exact&team={teamId}&month={currentDate.Month}&year={currentDate.Year}";

				var statsJson = await response.Content.ReadAsStringAsync();
				response.Dispose();
				stats = JsonConvert.DeserializeObject<FoldingStatus>(statsJson);

				var monthlyStatsResponse = await client.GetAsync(monthlyStatsURI);
				string monthlyStatsJson = await monthlyStatsResponse.Content.ReadAsStringAsync();
				monthlyStatsResponse.Dispose();

				var monthlyStats = JObject.Parse(monthlyStatsJson);
				stats.MonthlyRank = (int)monthlyStats.SelectToken("results[0].rank");
				stats.RetrievalTime = DateTime.UtcNow;
			} catch (Exception) {
				throw;
			} finally {
				client.Dispose();
			}
			return stats;
		}
	}
}