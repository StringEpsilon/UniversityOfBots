/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

using System;
using Gauss.Models;
using Microsoft.Extensions.Logging;

namespace Gauss.Logging {
	public class GaussLoggerFactory : ILoggerFactory {
		private readonly LogConfig _logConfig;

		public GaussLoggerFactory(LogConfig logConfig) {
			if (logConfig != null) {

				this._logConfig = logConfig;
			} else {
				this._logConfig = new LogConfig {
					LogLevel = LogLevel.Information,
					LogToConsole = true,
					Filename = "log.txt",
				};
			}
		}

		public void AddProvider(ILoggerProvider provider) {
			throw new NotImplementedException();
		}

		public ILogger CreateLogger(string categoryName) {
			return new GaussLogger(this._logConfig);
		}

		public void Dispose() {
			GC.SuppressFinalize(this);
		}
	}
}