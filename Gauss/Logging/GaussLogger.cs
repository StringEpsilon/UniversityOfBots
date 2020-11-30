/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

using System;
using System.IO;
using System.Text;
using DSharpPlus;
using Gauss.Models;
using Microsoft.Extensions.Logging;

namespace Gauss.Logging {
	internal class GaussLogger : ILogger {
		private readonly object _lock = new object();
		private readonly LogConfig _logConfig;

		public GaussLogger(LogConfig logConfig) {
			this._logConfig = logConfig;
		}

		public IDisposable BeginScope<TState>(TState state) {
			throw new NotImplementedException();
		}

		public bool IsEnabled(LogLevel logLevel) {
			return logLevel >= this._logConfig.LogLevel;
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) {
			if (!this.IsEnabled(logLevel)) {
				return;
			}
			StringBuilder entryBuilder = new StringBuilder();
			entryBuilder.Append('[')
				.Append(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"))
				.Append("] [")
				.Append(string.IsNullOrEmpty(eventId.Name) ? "     " : eventId.Name)
				.Append("] ")
				.Append(
					logLevel switch {
						LogLevel.Trace => "[Trace]",
						LogLevel.Debug => "[Debug]",
						LogLevel.Information => "[Info ]",
						LogLevel.Warning => "[Warn ]",
						LogLevel.Error => "[Error]",
						LogLevel.Critical => "[Crit ]",
						LogLevel.None => "[None ]",
						_ => "[Other]"
					}
				)
				.Append(' ')
				.Append(formatter(state, exception))
				.Append('\n');

			if (exception != null) {
				entryBuilder.Append(exception);
				entryBuilder.Append("\n\n");
			}

			lock (_lock) {
				if (this._logConfig.LogToConsole) {
					Console.WriteLine(entryBuilder.ToString().Trim());
				}
				File.AppendAllText(this._logConfig.Filename, entryBuilder.ToString());
			}
		}
	}
}