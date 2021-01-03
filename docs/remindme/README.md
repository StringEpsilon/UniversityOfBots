[Back to overview](../README.md)

---

## time commands:

- [settimezone](#settimezone)
- [remindme [datetime] [message]](#remindme-[datetime]-[message])
- [remindme [date] [time] [message]](#remindme-[date]-[time]-[message])
- [remindme [amount] [unit] [message]](#remindme-[amount]-[unit]-[message])

---

### settimezone

**Description**: Sets your timezone for conversions and handling your local time for reminders.

**Note:** [Valid timezone names can be found here.](https://en.wikipedia.org/wiki/List_of_tz_database_time_zones)

**Parameters**:

| name         | format | default | description          |
| ------------ | ------ | ------- | -------------------- |
| timezoneName | text   | UTC     | Name of the timezone |

**Aliases**: _None_

**Examples**: `!g settimezone Europe/Berlin` - Sets your timezone as Europe/Berlin.

---

### remindme [datetime] [message]

**Description**: Set a reminder for a give date and time and message.

**Note:** `datetime` is a single item. If only a date is specified, 00:00 is assumed. If only a time is specified, today is assumed.

**Parameters**:

| name     | format       | default  | description                            |
| -------- | ------------ | -------- | -------------------------------------- |
| datetime | See examples | -        | The date and or time for your reminder |
| message  | text         | No text. | The text for your reminder.            |

**Aliases**: _None_

**Examples**:

- `!g remindme 2021-04-01T08:00 Beware of april fools` - Reminder for the first of april 2021, 08:00.

---

### remindme [date] [time] [message]

**Description**: Set a reminder for a give date and time and message.

**Parameters**:

| name    | format     | default | description                 |
| ------- | ---------- | ------- | --------------------------- |
| date    | YYYY-MM-DD | -       | The date for your reminder  |
| time    | HH:MM[:SS] | -       | The time for your reminder  |
| message | text       | No text | The text for your reminder. |

**Aliases**: _None_

**Examples**:

- `!g remindme 2021-04-01 08:00 Beware of april fools` - Reminder for the first of april 2021, 08:00.

---

### remindme [amount] [unit] [message]

**Description**: Let gauss notify you after a specified amount of time elapsed.

**Notes:**

- Beware that you might receive the reminder up to a minute late, depending on circumstances.
- The bot will not accept reminders set more than 5 years in the future.

**Parameters**:

| name    | format | default | description                                                        |
| ------- | ------ | ------- | ------------------------------------------------------------------ |
| amount  | number | -       | The span of time to be reminded in.                                |
| unit    | text   | -       | The unit of time: `minute`, `hour`, `day`, `week`, `month`, `year` |
| message | text   | No text | The text for your reminder.                                        |

`unit` can also be the respective plural.

**Aliases**: _None_

**Examples**:

- `!g remindme 10 minute "Pizza is done!"` - Get a DM 10 minutes after sending the command.
