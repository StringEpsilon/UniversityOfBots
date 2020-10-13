/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

using System;
using System.Threading.Tasks;

namespace Gauss.Scheduling {
	public class DelayedTask : IScheduledTask {
		private readonly Action _action;
		private readonly DateTime _runAt;
		private bool _taskRan = false;

		public bool IsComplete { get => this._taskRan; }
		public bool HasRun { get; set; }

		public DelayedTask(Action onRun, DateTime runAt) {
            this._action = onRun;
            this._runAt = runAt;
        }

		public Task Run() {
            
			return Task.Run( () => {
                this._action();
				this._taskRan = true;
            });
		}

		public bool ShouldRun() {
			return !this._taskRan && DateTime.Now >= this._runAt;
		}
	}
}