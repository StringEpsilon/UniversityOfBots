/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Gauss.Scheduling {

	public class Scheduler {
		private readonly List<IScheduledTask> _tasks;

		private readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();
		private readonly object _lock = new object();

		public Scheduler() {
			this._tasks = new List<IScheduledTask>();

			this.RunSchedule();
		}

		~Scheduler() {
			this._cancellationToken.Cancel();
		}

		public void AddTask(IScheduledTask newTask) {
			lock (this._lock) {
				this._tasks.Add(newTask);
			}
		}

		private void RunSchedule() {
			var tokenSource = new CancellationTokenSource();
			Task.Run(() => {
				while (!_cancellationToken.IsCancellationRequested) {
					Thread.Sleep(1000);
					if (this._tasks.Count > 0) {
						lock (this._lock) {
							foreach (var task in _tasks.Where(y => y.ShouldRun())) {
								try {
									task.Run();
								} catch (Exception) {
								}
							}
							_tasks.RemoveAll(y => y.IsComplete);
						}
					}
				}
			}, tokenSource.Token);
		}
	}
}