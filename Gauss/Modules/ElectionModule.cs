using System;
using System.Threading.Tasks;
using DSharpPlus;
using Gauss.Models;
using Gauss.Database;
using Gauss.Scheduling;
using Microsoft.Extensions.Logging;
using DSharpPlus.Entities;

namespace Gauss.Modules {
	/// <summary>
	/// Module to keep track of all active elections to post and update their respective messages.
	/// Also closes the election polls on scheduled end date/time.
	/// </summary>
	class ElectionModule : BaseModule {
		private readonly DiscordClient _client;
		private readonly GaussConfig _config;
		private readonly Scheduler _scheduler;
		private readonly ElectionRepository _repository;
		private DateTime _nextCheck = DateTime.MinValue;

		public ElectionModule(
			DiscordClient client,
			Scheduler scheduler,
			GaussConfig config,
			ElectionRepository repository
		) {
			this._client = client;
			this._config = config;
			this._scheduler = scheduler;
			this._repository = repository;

			// Setup the UpdateElections function to run in the general scheduler:
			this._scheduler.AddTask(
				new TaskThunk(
					this.ShouldUpdateElections,
					this.UpdateElections
				)
			);
		}

		/// <summary>
		/// Election update logic.
		/// </summary>
		private async Task UpdateElections() {
			var activeElections = _repository.GetActiveElections();
			if (activeElections.Count == 0) {
				return;
			}

			foreach (var election in activeElections) {
				var voteChannelId = this._config.GuildConfigs[election.GuildId].VoteChannel;
				if (voteChannelId == 0) {
					this._client.Logger.LogError("Posting election failed: No vote channel specified.");
					break;
				}
				if (election.Message == null) {
					if (election.Start <= DateTime.UtcNow) {
						var voteChannel = this._client
							.Guilds[election.GuildId]
							.Channels[voteChannelId];

						var message = await voteChannel.SendMessageAsync(embed: election.GetEmbed());
						this._repository.SetMessage(election.GuildId, election.ID, message);
					}
				} else {
					if (election.End <= DateTime.UtcNow) {
						if (_repository.CloseElection(election, this._client)) {
							DiscordChannel voteChannel = this._client
								.Guilds[election.GuildId]
								.Channels[voteChannelId];

							DiscordMessageBuilder builder = new DiscordMessageBuilder()
								.WithFile(_repository.GetAuditLogPath(election))
								.WithContent($"Election #{election.ID} for {election.Title} has concluded. Results:\n{election.GetResults()}");

							await voteChannel.SendMessageAsync(builder);
						}
					}
				}
			}
			this._repository.SaveChanges();
			return;
		}

		/// <summary>
		/// Determines if the main task should run.
		/// </summary>
		/// <returns>
		/// True if the task should run, otherwise false.
		/// </returns>
		private bool ShouldUpdateElections() {
			// Run the main logic every 5 minutes:
			if (this._nextCheck < DateTime.UtcNow) {
				this._nextCheck = DateTime.UtcNow + TimeSpan.FromMinutes(1);
				return true;
			}
			return false;
		}
	}
}