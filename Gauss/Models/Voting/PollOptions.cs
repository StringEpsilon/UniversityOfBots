/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

using Microsoft.EntityFrameworkCore;

namespace Gauss.Models.Voting {

	[Owned]
	public class PollOption {
		public string Name { get; set; }
		public int Id { get; set; }
		public uint Votes { get; set; }
	}
}