/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Gauss.Models.Voting;

namespace Gauss.Database {
	public class PollRepository : DbContext {
		private readonly object _lock = new object();
		public DbSet<Poll> Polls { get; set; }
		public DbSet<VotingSettings> VotingSettings { get; set; }
		public DbSet<UserTokenPool> UserTokenPools { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder options)
			=> options.UseSqlite("Data Source=polldata.db");

		protected override void OnModelCreating(ModelBuilder modelBuilder) {
			modelBuilder.Entity<UserTokenPool>()
				.HasKey(e => new { e.GuildId, e.UserId });

			modelBuilder.Entity<Poll>().HasIndex(e => new { e.GuildId, e.Id });
			modelBuilder.Entity<Poll>()
				.Property(f => f.Id)
				.ValueGeneratedOnAdd();
		}
		public PollRepository() {
			this.Database.EnsureCreated();
		}

		#region Manipulate polls
		public int ProposePoll(Poll newPoll) {
			lock (this._lock) {
				newPoll.State = PollState.Proposal;
				this.Polls.Add(newPoll);
				this.SaveChanges();
			}
			return newPoll.Id;
		}

		public IEnumerable<Poll> ListPolls(ulong guildId, PollState state = PollState.Proposal) {
			IEnumerable<Poll> result = null;
			lock (_lock) {
				result = from poll in this.Polls
						 where poll.GuildId == guildId && poll.State == state
						 select poll;

			}

			return result;
		}

		public Poll GetPoll(int pollId) {
			Poll result = null;
			lock (_lock) {
				result = this.Polls.Find(pollId);
			}
			return result;
		}

		internal void UpdatePoll(Poll poll) {
			lock (_lock) {
				this.Polls.Update(poll);
				this.SaveChanges();
			}
		}
		#endregion
	}
}