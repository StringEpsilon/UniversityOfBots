/**
This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this
file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

namespace Gauss.Models.Elections {
	public class Candidate {
		public string Option { get; set; }

		/// <summary>
		/// Username of the candidate.
		/// </summary>
		public string Username { get; set; }

		/// <summary>
		/// User ID of the candidate.
		/// </summary>
		public ulong UserId { get; set; }
	}
}