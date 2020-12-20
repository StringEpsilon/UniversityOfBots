/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

using System.Linq;
using Microsoft.EntityFrameworkCore;
using Gauss.Models;
using System.Collections.Generic;
using System.IO;

namespace Gauss.Database {
	public class UserSettingsContext : DbContext {
		public DbSet<UserMessageSettings> UserMessageSettings { get; set; }
		public DbSet<UserVoiceSettings> UserVoiceSettings { get; set; }
		private readonly GaussConfig _config;

		private readonly object _lock = new();

		public UserSettingsContext(GaussConfig config) {
			this._config = config;
			this.Database.EnsureCreated();
		}

		protected override void OnConfiguring(DbContextOptionsBuilder options)
			=> options.UseSqlite("Data Source=" + Path.Join(this._config.ConfigDirectory, "usersettings.db"));

		protected override void OnModelCreating(ModelBuilder modelBuilder) {
			modelBuilder.Entity<UserVoiceSettings>()
				.HasKey(e => new { e.GuildId, e.UserId });

			modelBuilder.Entity<UserMessageSettings>()
				.HasKey(e => new { e.GuildId, e.UserId });
		}



		public UserMessageSettings GetMessageSettings(ulong guildId, ulong userId) {
			UserMessageSettings result;
			lock (_lock) {
				result = (from settings in this.UserMessageSettings
						  where settings.GuildId == guildId
							  && settings.UserId == userId
						  select settings).SingleOrDefault();
			}
			return result;
		}

		public void SetMessageSettings(UserMessageSettings settings) {
			lock (_lock) {
				if (this.UserMessageSettings.Any(y => y.GuildId == settings.GuildId && y.UserId == settings.UserId)) {
					this.UserMessageSettings.Update(settings);
				} else {
					this.UserMessageSettings.Add(settings);
				}
				this.SaveChanges();
			}
		}

		public UserVoiceSettings GetVoiceSettings(ulong guildId, ulong userId) {
			UserVoiceSettings result = null;
			lock (_lock) {
				result = this.UserVoiceSettings.Find(guildId, userId);
			}
			return result;
		}

		public List<UserVoiceSettings> GetVoiceUsers(ulong guildId) {
			List<UserVoiceSettings> results = null;
			lock (_lock) {
				results = (from vcnsettings in this.UserVoiceSettings
						   where vcnsettings.GuildId == guildId
							   && vcnsettings.IsActive
							   && !vcnsettings.IsInTimeout
						   select vcnsettings).ToList();
			}
			return results;
		}

		public void SetVoiceSettings(UserVoiceSettings settings) {
			lock (_lock) {
				if (this.UserVoiceSettings.Find(settings.GuildId, settings.UserId) == null) {
					this.UserVoiceSettings.Add(settings);
				} else {
					this.UserVoiceSettings.Update(settings);
				}
				this.SaveChanges();
			}
		}
	}
}