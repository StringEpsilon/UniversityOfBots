
/**
This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this
file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

using System.Collections.Generic;

namespace Gauss.Models.Elections {
	public class Ballot {
		/// <summary>
		/// List of approved candidates.
		/// </summary>
		public List<Candidate> Approvals { get; set; } = new();
	}
}