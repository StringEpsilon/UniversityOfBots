/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using System.IO;
using Gauss.Utilities;
using Microsoft.Extensions.Logging;

namespace Gauss.Models {
	public class LogConfig {
		[JsonProperty("filename")]
		public string Filename { get; set; }

		[JsonProperty("level")]
		public LogLevel LogLevel { get; set; }

		[JsonProperty("console")]
		public bool LogToConsole { get; set; }
	}

	public class GuildConfig {

		[JsonProperty("folding_team")]
		public string FoldingTeam { get; set; }
		[JsonProperty("folding_channel")]
		public ulong FoldingChannel { get; set; }

		[JsonProperty("welcome_message")]
		public string WelcomeMessage { get; set; }

		[JsonProperty("welcome_channel")]
		public ulong WelcomeChannel { get; set; }

		[JsonProperty("vote_channel")]
		public ulong VoteChannel { get; set; }

		[JsonProperty("calendar_id")]
		public string CalendarId { get; set; }

		[JsonProperty("vc_names")]
		public Dictionary<ulong, string> VoiceChannelNames { get; set; }
	}

	public class GaussConfig {
		private static string _directory;

		[JsonProperty("logging")]
		public LogConfig LogConfig { get; set; }

		[JsonProperty("discord_token")]
		public string DiscordToken { get; set; }

		[JsonProperty("guild_configs")]
		public Dictionary<ulong, GuildConfig> GuildConfigs { get; set; } = new Dictionary<ulong, GuildConfig>();

		[JsonProperty("command_prefix")]
		public string CommandPrefix { get; set; }

		[JsonProperty("assign_roles")]
		public ulong AutoAssignedRole { get; set; }

		[JsonProperty("voice_notification_categories")]
		public List<ulong> VoiceNotificationCategories { get; set; }

		[JsonProperty("status_text")]
		public string StatusText { get; set; }

		public List<ulong> RedditEnabledChannels { get; set; }

		[JsonIgnore]
		public string ConfigDirectory { get; set; }

		public GaussConfig() {

		}

		public static GaussConfig ReadConfig(string configDirectory) {
			_directory = configDirectory;
			if (!Directory.Exists(configDirectory)) {
				throw new Exception($"Config directory '{configDirectory}' does not exist.");
			}
			if (!File.Exists(Path.Join(configDirectory, "config.json"))) {
				throw new Exception($"'config.json' was not found in '{configDirectory}'.");
			}

			var config = JsonUtility.Deserialize<GaussConfig>(Path.Join(configDirectory, "config.json"));
			config.ConfigDirectory = configDirectory;
			return config;
		}

		public void Save() {
			JsonUtility.Serialize<GaussConfig>(Path.Join(_directory, "config.json"), this);
		}
	}
}