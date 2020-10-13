/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

namespace Gauss.Models.Voting {
    public enum PollState {
        /// <summary>
		/// Poll is currently in proposal state.
		/// </summary>
        Proposal,
    
        /// <summary>
		/// Poll has been proposed and subsequently retracted by the author or a moderator.
		/// </summary>
        Retracted,

        /// <summary>
		/// Poll is currently running and users can vote on it.
		/// </summary>
        Active,

        /// <summary>
		/// Poll has been closed.
		/// </summary>
        Finished,
    }
}