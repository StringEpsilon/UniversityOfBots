/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/


namespace Gauss.Models.Voting {

	public class UserTokenPool {
		public ulong UserId { get; set; }
		public ulong GuildId { get; set; }
		public int Total { get; set; }
		public int Current { get; set; }
	}
}