/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

using System.Threading.Tasks;

namespace Gauss.Scheduling {
    public interface IScheduledTask {
        /// <summary>
        /// Indicates that the task is complete and should be taken out of the schedule.
        /// </summary>
		bool IsComplete { get; }

        /// <summary>
        /// Checks whether or not the tasks run method should be invoked
        /// </summary>
		bool ShouldRun();

        /// <summary>
        /// Implements the actual work done in the task.
        /// </summary>
        Task Run();
    }
}