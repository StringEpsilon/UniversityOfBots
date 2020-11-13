# University of Bayes Bot

## References:

### Basic technologies

- Programming language: [C#](<https://en.wikipedia.org/wiki/C_Sharp_(programming_language)>)
- Framework: [.NET core](https://en.wikipedia.org/wiki/.NET_Core)
- Discord Library: [DSharpPlus](https://github.com/DSharpPlus/DSharpPlus)
  - For easy interfacing with Discord's APIs and an easy to use command system.
- ef core: [MSDN documentation](https://docs.microsoft.com/en-US/ef/core/)
  - For database abstraction.

### Documentation for used tech:

- [The MSDN on C#](https://docs.microsoft.com/en-us/dotnet/csharp/)
- [.NET docs in general](https://docs.microsoft.com/en-us/dotnet/)
- [DSharpPlus documentation](https://dsharpplus.github.io/)
- [The discord API reference](https://discord.com/developers/docs/reference) might be helpful too.

### Used tools:

- GitHub - obviously
- [git](https://git-scm.com/) for version control
- [Visual Studio Code](https://code.visualstudio.com/) as the IDE of choice.
  - [C# extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp)
  - [GitLens](https://marketplace.visualstudio.com/items?itemName=eamodio.gitlens)
  - [EditorConfig for VS Code](https://marketplace.visualstudio.com/items?itemName=EditorConfig.EditorConfig)

### Suggested reading:

- If you want a book on C#, "C# in Depth" by Jon Skeet is probably worth looking into. Though i haven't read it myself.

### How to run the bot from source

0. Install dotnet core 3.1 ([infos here](https://dotnet.microsoft.com/download))
1. Clone the repository: `git clone https://github.com/StringEpsilon/UniversityOfBots.git`
2. Run `dotnet restore` to install all the require dependencies.
3. Create a folder (preferably in your home directory) called "GaussBot"
4. Create a file called "config.json" in that new folder. (You can use the example_config.json as a template)
5. Go to https://discord.com/developers/applications and create a new application
6. In the new application:
   6a. Enable "Presence Intent" and "Server Members intent" (under "Bot")
   6b. Click on "Token" -> "Copy" and add that to the config.json (as "discord_token") (also under "Bot")
   6c. Invite the bot to a discord server you can test it on.
7. Execute `dotnet run`. You might have to specify the folder your config file sits in using `--configDir`

Example:

`dotnet run --configDir /home/user/coding/botconfig`
