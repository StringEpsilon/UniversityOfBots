/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

using Gauss.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Gauss.Database {
	public class CalendarAccessor {
		static readonly string[] _scopes = { CalendarService.Scope.CalendarEvents };
		static readonly string _applicationName = "Gauss";
		private readonly Dictionary<ulong, CalendarService> _services = new();
		private readonly GaussConfig _config;
		private readonly ILogger _logger;

		public CalendarAccessor(GaussConfig config, ILogger logger) {
			this._config = config;
			this._logger = logger;

			foreach (var guildConfig in config.GuildConfigs.Where(y => !string.IsNullOrEmpty(y.Value.CalendarId))) {
				var guildId = guildConfig.Key;
				UserCredential credential;

				try {
					using (var stream = new FileStream(Path.Join(this._config.ConfigDirectory, "google", $"{guildId}.json"), FileMode.Open, FileAccess.Read)) {
						// The file token.json stores the user's access and refresh tokens, and is created
						// automatically when the authorization flow completes for the first time.
						string credPath = Path.Join(this._config.ConfigDirectory, "google", guildId.ToString());
						credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
							GoogleClientSecrets.Load(stream).Secrets,
							_scopes,
							"user",
							CancellationToken.None,
							new FileDataStore(credPath, true)).Result;
					}

					// Create Google Calendar API service.
					this._services.Add(
						guildId,
						new CalendarService(new BaseClientService.Initializer() {
							HttpClientInitializer = credential,
							ApplicationName = _applicationName,
						})
					);
					this._logger.LogInformation($"[Calendar] Enabled calendar integration for guild {guildId}.");
				} catch (Exception ex) {
					this._logger.LogError(ex, $"Error setting up client for guild calendar integration. Guild ID: {guildId}");
				}
			}
		}

		public async Task AddEvent(ulong guildId, Event newEvent) {
			if (!this._services.ContainsKey(guildId)) {
				return;
			}
			try {
				var request = this._services[guildId].Events.Insert(
					newEvent,
					this._config.GuildConfigs[guildId].CalendarId
				);
				await request.ExecuteAsync();
				this._logger.LogInformation($"[Calendar] Added calendar event for {guildId}: {newEvent.Summary}.");
				return;
			} catch (Exception ex) {
				this._logger.LogError(ex, $"Error while creating a new calendar event. Guild ID: {guildId}");
			}
		}

		public async Task<Event> GetNextEvent(ulong guildId) {
			if (!this._services.ContainsKey(guildId)) {
				return null;
			}
			try {
				EventsResource.ListRequest request = this._services[guildId].Events.List(
					this._config.GuildConfigs[guildId].CalendarId
				);

				request.TimeMin = DateTime.Now;
				request.ShowDeleted = false;
				request.SingleEvents = true;
				request.MaxResults = 1;
				request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

				Events events = await request.ExecuteAsync();
				if (events.Items != null && events.Items.Count > 0) {
					return events.Items.First();
				}
			} catch (Exception ex) {
				this._logger.LogError(ex, $"Error retrieving calendar event. Guild ID: {guildId}");
			}
			return null;
		}
	}
}