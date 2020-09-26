/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

using System;
using System.IO;
using System.Threading.Tasks;
using Gauss.Models;
using Gauss.Utilities;
using static System.Environment;

namespace Gauss {
	public class Program {
		private static GaussBot BotInstance;
		public async static Task Main(string[] args) {
			string configDirectory = Path.Join(GetFolderPath(SpecialFolder.UserProfile), "GaussBot");

			if (args.Length > 0 && args[0] == "--configDir") {
				configDirectory = args[1];
			}
			if (!Directory.Exists(configDirectory)) {
				throw new Exception($"Config directory '{configDirectory}' does not exist.");
			}
			if (!File.Exists(Path.Join(configDirectory, "config.json"))) {
				throw new Exception($"'config.json' was not found in '{configDirectory}'.");
			}
			GaussConfig config = JsonUtility.Deserialize<GaussConfig>(Path.Join(configDirectory, "config.json"));

			BotInstance = new GaussBot(config);
			BotInstance.Connect();
			await Task.Delay(-1);
		}


		private static void HandleExit(object sender, EventArgs e) {
			BotInstance.Disconnect();
		}
	}
}
