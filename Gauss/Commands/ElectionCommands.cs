/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Gauss.Utilities;
using Gauss.CommandAttributes;
using Gauss.Models.Elections;
using System;
using Gauss.Database;
using DSharpPlus.Entities;

namespace Gauss.Commands {
	[Group("election")]
	[Description("Commands to participate in elections")]
	[RequireGuild]
	[CheckDisabled]
	public class ElectionCommands : BaseCommandModule {
		private readonly ElectionRepository _pollRepository;

		public ElectionCommands(ElectionRepository pollRepository) {
			_pollRepository = pollRepository;
		}

		[Command("create")]
		[Description("Create a new election.")]
		[RequireAdmin]
		public async Task CreateElection(
			CommandContext context,
			[Description("Title that is being elected.")]
		 	string title,
			[Description("Planned start of the election (UTC).")]
			DateTime start,
			[Description("Planned end of the election (UTC).")]
			DateTime end,
			[Description("List of candidates.")]
			params DiscordUser[] candidateNames
		) {
			if (start <= DateTime.UtcNow){
				await context.RespondAsync($"`start` must not be in the past.");
				return;
			}
			if (start > end){
				await context.RespondAsync($"`start` must be before `end`.");
				return;
			}
			if (candidateNames.Distinct().Count() != candidateNames.Count()){
				await context.RespondAsync("You have duplicate names in the candidate list.");
				return;
			}
			
			var guild = context.GetGuild();

			List<Candidate> candidates = new List<Candidate>();
			candidates = candidateNames.Select((item, index) => new Candidate{
				Option = char.ConvertFromUtf32(65+index),
				UserId = item.Id, 
				Username = item.Username + "#" + item.Discriminator, 
				Votes = 0,
			}).ToList();
			var election = new Election() {
				Candidates = candidates,
				Title = title,
				Start = start.ToUniversalTime(),
				End = end.ToUniversalTime(),
			};
			_pollRepository.AddElection(guild.Id, election);

			await context.RespondAsync(embed: election.GetEmbed());

			await context.CreateConfirmation(
				"Schedule the above election poll?",
				() => {
					this._pollRepository.CommitElection(context.GetGuild().Id, election.ID);
					context.RespondAsync("Election committed and scheduled.");
				},
				() => {
					this._pollRepository.RemoveElection(context.GetGuild().Id, election.ID);
					context.RespondAsync("Draft withdrawn.");
				}
			);
		}

		[Command("vote")]
		[Description("Vote for one or more candidates. The voting system is approval voting.")]
		public async Task VoteFor(
			CommandContext context, 
			[Description("ID of the election you want to vote in")]
			ulong electionId, 
			[Description("The candidates you approve. Either by their full username or their assigned letter.")]
			params string[] approvals
		) {
			var guild = context.GetGuild();
			var members = await guild.GetAllMembersAsync();
			var voterStatus = _pollRepository.CanVote(guild.Id, electionId, context.User.Id);
			switch (voterStatus) {
				case VoteStatus.ElectionNotFound: {
						await context.RespondAsync($"Could not find the election with ID `{electionId}`.");
						return;
					}
				case VoteStatus.ElectionNotStarted: {
						await context.RespondAsync($"Election #{electionId} has not started yet.");
						return;
					}
				case VoteStatus.ElectionOver: {
						await context.RespondAsync($"Election #{electionId} is already over and decided.");
						return;
					}
				case VoteStatus.AlreadyVoted: {
						await context.RespondAsync($"You already voted in this election. Unfortunately, you can not change your vote.");
						return;
					}
			}
			if (approvals.Distinct().Count() != approvals.Count()){
				await context.RespondAsync("You must not list any candidate more than once.");
				return;
			}

			var election = _pollRepository.GetElection(guild.Id, electionId);
			List<Candidate> candidates = new List<Candidate>();
			foreach (var item in approvals) {
				Candidate foundCandidate = null;
				if (item.Length == 1){
					foundCandidate = election.Candidates.Find(y => y.Option.ToLower() == item.ToLower());
				}else{
					foundCandidate = election.Candidates.Find(y => y.Username.ToLower() == item.ToLower());
				}
				if (foundCandidate == null) {
					await context.RespondAsync($"Could not find `{item}` on the list of candidates.");
					return;
				}
				candidates.Add(foundCandidate);
			}

			var confirmationMessage = $"Do you approve of the following candidate(s) for {election.Title} and want to give them your vote?:\n\n"
				+ $"{string.Join("\n", candidates.Select(y => " - " + y.Username))}\n\n"
				+ $"**Your vote can not be changed after confirmation. Make sure you selected all your approved candidates.**";


			await context.CreateConfirmation(
				confirmationMessage,
				async () => {
					var (hashBefore, hashafter) = _pollRepository.SaveVotes(guild.Id, election.ID, context.User.Id, candidates, context.Client);
					await guild.Channels[637646862798553108].SendMessageAsync(
						$"A user cast a their vote for election #{electionId}.\nHash before: {hashBefore}\nHash after: {hashafter}"
					);
					await context.RespondAsync("Your vote has been cast.");
				}
			);
		}
	}
}
