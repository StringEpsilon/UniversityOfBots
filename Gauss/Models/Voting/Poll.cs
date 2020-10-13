/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

using System.Collections.Generic;
using System;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Gauss.Models.Voting {

	[Owned]
	public class PollParticipant {
		public ulong UserId {get;set;}
	}

	public class Poll {
		/// <summary>
		/// Running ID of the poll.
		/// </summary>
		[Key]
		// [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		/// <summary>
		/// Unique ID of the guild the poll is proposed for.
		/// </summary>
		public ulong GuildId { get; set; }

		/// <summary>
		/// State of the poll. Defaults to <see cref="PollState.Proposal"/>.
		/// </summary>
		public PollState State { get; set; } = PollState.Proposal;

		/// <summary>
		/// Date and time of when the polling will close.
		/// </summary>
		public DateTime EndDate { get; set; }

		public DateTime ProposedAt {get;set;}

		/// <summary>
		/// Description of what will be decided by the poll.
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// List of available options users can vote for.
		/// </summary>
		public List<PollOption> Options { get; set; }

		/// <summary>
		/// The list of users IDs who participated in the poll.
		/// </summary>
		public List<PollParticipant> ParticipantIds { get; set; }

		/// <summary>
		/// The ID of the user who proposed the poll.
		/// </summary>
		public ulong ProposingUser { get; set; }

		/// <summary>
		/// ID of the discord message the poll has been posted in.
		/// </summary>
		public ulong MessageId{ get; set;}
	}
}