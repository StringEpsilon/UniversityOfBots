[Back to overview](./README.md)

---

# Privacy

## Data records

The Bot keeps track of the following data:

**0. Exception and error handling.**

In order to diagnose problems and malfunctions, the Bot writes a log with [exceptions](https://en.wikipedia.org/wiki/Exception_handling) into a logfile. User specific information and messages are kept out of these log files as best as possible.

Logs are regularly deleted, if they are no longer needed.

**1. User specific settings.**

If you use a command to adjust some settings, like `send blockDMs`, the bot will save your discord user ID along with the specified settings.

**2. Elections**

For elections, the bot will store your user ID and tally your votes. But it does not store who you voted for in detail. Only the vote totals for each candidate is stored, alongside a list of who voted.

**3. Reminders**

For reminders, the bot will store your user ID, the desired time of the reminder and the associated message.

If you set your timezone via `!g settimezone`, the bot will also store that.

**4. Reputation**

Also known as Bayes Points. The Bot stores a simple table of month, user ID and score. Access to the whole of that information is possible through the `!g leaderboard` command.

## Messages and command usage

Gauss will not keep any record of messages send to it via DMs or in channels it has access to.
The usage of commands is not logged or kept track of in any way.

There is also no analytics or anonymized / pseudonymized data collection.

## Requests for information and deletion.

There is currently no command to retrieve all stored information and or delete it. Ask the bot maintainer in a Discord DM.
