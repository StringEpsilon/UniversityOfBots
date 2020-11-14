[Back to overview](../README.md)

---

## time commands:

- [time now](#time-now1)
- [time now [timezone]](#time-now-[timezone])
- [time convert [datetime] [timezone]](#time-convert-[datetime])
- [time convert [datetime] [timezone]](#time-convert-[datetime]-[timezone])

---

### time now

**Description**: Posts the current date and time in the configured timezone of the user.

**Note:** If no timezone is configured, the command will not do anything.

**Parameters**: _none_

**Aliases**: `!g time`

**Examples**: `!g time now` - posts the current time.

---

### time now [timezone]

**Description**: Posts the current date and time in given timezone.

**Note:** [Valid timezone names can be found here.](https://en.wikipedia.org/wiki/List_of_tz_database_time_zones)

**Parameters**: 

| name         | format | default | description          |
|--------------|--------|---------|----------------------|
| timezoneName | text   | UTC     | Name of the timezone |


**Aliases**: `!g time <timezone>`

**Examples**: `!g time now Europe/Berlin` - posts the current time in the Europe/Berlin timezone.

---

### time convert [datetime]

**Description**: Convert a UTC (date and) time to your configured timezone.

**Parameters**:

| name     | format   | default | description              |
|----------|----------|---------|--------------------------|
| datetime | HH:MM:SS | -       | Time you want to convert |

`datetime` can also include a date:

- `1970-01-01T00:00:00`
- `1970-01-01 00:00:00`
- `1970-01-01 00:00`

**Aliases**: _none_

**Examples**: `!g time convert 20:00` - Converts 20:00 UTC to your configured timezone.

---

### time convert [datetime] [timezone]

**Description**: Convert a UTC (date and) time to your configured timezone.

**Parameters**:

| name         | format   | default | description                                   |
|--------------|----------|---------|-----------------------------------------------|
| datetime     | HH:MM:SS | -       | Time you want to convert                      |
| timezoneName | text     | -       | Name of the timezone you want to convert from |

`datetime` can also include a date:

- `1970-01-01T00:00:00`
- `1970-01-01 00:00:00`
- `1970-01-01 00:00`

**Note:** [Valid timezone names can be found here.](https://en.wikipedia.org/wiki/List_of_tz_database_time_zones)

**Aliases**: _none_

**Examples**: `!g time convert 20:00 Europe/Berlin` - Converts 20:00 Europe/Berlin (CET / CEST) to your configured timezone.
