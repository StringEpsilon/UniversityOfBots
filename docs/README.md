# Gauss - University of Bayes Discord Bot

[See recent changes here.](./WhatsNew.md)

## Privacy

Up to date explanations of all record keeping as it related to personal information is [explained here](./PRIVACY.md).

## Basic usage

You can use any command in one of three ways:

1. Send a message starting with the `!g` prefix, followed by the command name:
   `!g help`
2. @mention the bot, directly followed by the command name: `@Gauss help`
3. DM the bot directly with the command name: `help`

## Available commands

Click on the headlines to get to detailed documentation of all available commands in the category.

### [send](./send/README.md)

You can send anonymous messages through Gauss with the `send` command.

### [election](./election/README.md)

You can participate in elections on the server. The system uses [approval voting](https://en.wikipedia.org/wiki/Approval_voting).

The bot will post polls in a configured channels with the information you need. **You can vote by sending the command in a private message.**

### [admin](./admin/README.md)

You can access moderation options through various `admin` commands.

### [election](./election/README.md)

Creating and voting in elections.

### [reputation](./reputation/README.md)

Tracking, giving and taking away Bayes Points.

### [Miscellaneous commands](./misc/README.md)

Other commands that don't fit neatly into their own grouping.

---

## Ideas, feedback and bug reports

If you have ideas for new commands, feedback on existing commands, or any other feedback, please open a ticket on GitHub here:

[GitHub issue tracker](https://github.com/StringEpsilon/UniversityOfBots/issues)

If you don't have a GitHub account, just leave your feedback in the appropriate channel on the UofB server (#caretaking-uofbayes).

---

## Other Features

### Posting welcome texts

The Bot will respond to any message containing the phrases "met gauss" or "meet gauss" by posting a configured message.

If that same message has been posted previously, the bot will edit it to contain just:

> [This previously contained the welcome message]

To avoid a long trail of potentially very large messages clogging the channel.

The message can be configured with `!g admin welcome message <text>` and `!g admin welcome channel <channelId>` by a moderator. [See here for more information.](./admin/README.md#admin-welcome-message)

### Clickable subreddit links

Gauss will, in certain channels, automatically generate clickable links to mentioned subreddits.

For example, if a user mentions /r/University_of_Bayes, Gauss will respond like this:

> Here is a clickable link: https://www.reddit.com/r/University_of_Bayes

Gauss will also warn about subreddits marked as NSFW and not provide clickable links for those subreddits.

### Automatic time conversion

If you configured your timezone using the `settimezone` command, Gaussbot can provide a conversion of your local time to the readers timezone.

To do so, simply put whatever time you want converted into square brackets. For example `I will host a game night at [20:00].`

<img src="./img/time_conversion.png" />

Other users will see their respective local time on the right hand side, where the screenshot says "Today at 20:00".
