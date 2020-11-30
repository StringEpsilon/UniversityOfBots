/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

using System.Linq;
using Microsoft.EntityFrameworkCore;
using Gauss.Models;
using System.Collections.Generic;
using DSharpPlus.Entities;
using System.IO;

namespace Gauss.Database {


	public class GuildSettingsContext : DbContext {
		public DbSet<DisabledCommand> DisabledCommands { get; set; }
		public DbSet<UserRestriction> UserRestrictions { get; set; }

		public DbSet<GuildRole> AdminRoles { get; set; }

		private readonly object _lock = new object();
		private readonly GaussConfig _config;

		public GuildSettingsContext(GaussConfig config) {
			this._config = config;

			this.Database.EnsureCreated();
		}

		protected override void OnConfiguring(DbContextOptionsBuilder options) {
			options.UseSqlite("Data Source=" + Path.Join(this._config.ConfigDirectory, "guildsettings.db"));
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder) {
			modelBuilder.Entity<GuildRole>()
				.HasKey(e => new { e.GuildId, e.RoleId });

			modelBuilder.Entity<DisabledCommand>()
				.HasKey(e => new { e.GuildId, e.CommandName });

			modelBuilder.Entity<UserRestriction>()
				.HasKey(e => new { e.GuildId, e.UserId });

			modelBuilder.Entity<UserRestriction>().OwnsMany(
				p => p.RestrictedCommands,
				a => {
					a.WithOwner().HasForeignKey("GuildId", "UserId");
					a.Property<int>("Id");
					a.HasKey("Id");
				}
			);
		}

		#region Admin roles
		public bool IsAdminRole(ulong guildId, IEnumerable<DiscordRole> roles) {
			if (roles == null || !roles.Any()) {
				return false;
			}
			lock (_lock) {
				var guildAdminRoles = from guildRole in this.AdminRoles
									  where guildRole.GuildId == guildId
									  select guildRole.RoleId;

				if (guildAdminRoles != null) {
					foreach (var role in guildAdminRoles) {
						if (roles.Any(y => y.Id == role)) {
							return true;
						}
					}
				}
			}
			return false;
		}

		public bool IsAdminRole(ulong guildId, ulong roleId) {
			var result = false;
			lock (_lock) {
				result = this.AdminRoles.Find(guildId, roleId) != null;
			}
			return result;
		}

		internal void RemoveAdminRole(ulong guildId, ulong roleId) {
			lock (_lock) {
				var adminRole = this.AdminRoles.Find(guildId, roleId);
				if (adminRole != null) {
					this.AdminRoles.Remove(adminRole);
					this.SaveChanges();
				}
			}
		}

		public void AddAdminRole(ulong guildId, ulong roleId) {
			lock (_lock) {
				this.AdminRoles.Add(new GuildRole() { GuildId = guildId, RoleId = roleId });
				this.SaveChanges();
			}
		}
		#endregion


		#region User restrictions
		public UserRestriction GetUserRestriction(ulong guildId, ulong userId) {
			UserRestriction result = null;

			lock (_lock) {
				result = this.UserRestrictions.Find(guildId, userId);
			}

			return result;
		}

		public void SetRestrictionForUser(ulong guildId, ulong userId, CommandRestriction restriction) {
			lock (_lock) {
				UserRestriction user = this.UserRestrictions.Find(guildId, userId);
				if (user == null) {
					user = new UserRestriction() {
						GuildId = guildId,
						UserId = userId,
					};
					this.UserRestrictions.Add(user);
					this.SaveChanges();
				}
				if (user.RestrictedCommands == null) {
					user.RestrictedCommands = new List<CommandRestriction>();
				} else {
					if (user.RestrictedCommands.Any(x => x.CommandName == restriction.CommandName)) {
						return;
					}
				}

				user.RestrictedCommands.Add(restriction);
				this.UserRestrictions.Update(user);
				this.SaveChanges();
			}
		}


		public void RemoveRestrictionForUser(ulong guildId, ulong userId, CommandRestriction restriction) {
			lock (_lock) {
				UserRestriction user = this.UserRestrictions.Find(guildId, userId);
				if (user?.RestrictedCommands == null) {
					return;
				}
				if (user.RestrictedCommands.Contains(restriction)) {
					user.RestrictedCommands.Remove(restriction);
				}
				this.UserRestrictions.Update(user);
				this.SaveChanges();
			}
		}
		#endregion

		#region Enable/Disable commands
		public bool IsCommandDisabled(ulong guildId, string CommandName) {
			bool result = false;
			lock (_lock) {
				if (!DisabledCommands.Any()) {
					result = false;
				} else {
					result = this.DisabledCommands.Find(guildId, CommandName) != null;
				}
			}
			return result;
		}

		public List<string> GetDisabledCommands(ulong guildId) {
			List<string> result;

			lock (_lock) {
				result = (from command in this.DisabledCommands
						  where command.GuildId == guildId
						  select command.CommandName).ToList();
			}

			return result;
		}

		public void DisableCommand(ulong guildId, string CommandName) {
			lock (_lock) {
				this.DisabledCommands.Add(new DisabledCommand { GuildId = guildId, CommandName = CommandName });
				this.SaveChanges();
			}
		}

		public void EnableCommand(ulong guildId, string CommandName) {
			lock (_lock) {
				var entity = this.DisabledCommands.Find(guildId, CommandName);
				if (entity != null) {
					this.DisabledCommands.Remove(entity);
					this.SaveChanges();
				}
			}
		}
	}
	#endregion
}