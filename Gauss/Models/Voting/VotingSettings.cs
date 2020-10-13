/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

using System.ComponentModel.DataAnnotations;

namespace Gauss.Models.Voting {
    /// <summary>
    /// Settings governing the voting system for each Guild.
    /// </summary>
    public class VotingSettings {
        /// <summary>
		/// Unique ID of the guild these settings belong to.
		/// </summary>
        [Key]      
        public ulong GuildId {get;set;}

        /// <summary>
        /// Number of voting tokens per user and cycle.
        /// </summary>
        public int TokenPool {get;set;}

        /// <summary>
        /// Length of the polling cycle in length.
        /// </summary>
        public int PollingLength {get;set;}

        /// <summary>
        /// ID of the channel in which polls will be posted.
        /// </summary>
        public ulong PollingChannel {get;set;}
    }
}