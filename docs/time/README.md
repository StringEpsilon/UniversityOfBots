[Back to overview](../README.md)

---

## time commands:

- [time now](#time-now1)
- [time now [timezone]](#time-now-[timezone])

---

### time now

**Description**: Posts the current date and time in the configured timezone of the user.

**Note:** If no timezone is configured, the command will not do anything.

**Parameters**: _none_

**Aliases**: _none_

**Examples**: `!g now` - posts the current time.


### time now [timezone]

**Description**: Posts the current date and time in given timezone.

**Note:** [Valid timezone names can be found here.](https://en.wikipedia.org/wiki/List_of_tz_database_time_zones)

**Parameters**: 

| name         | format | default | description           |
| ------------ | ------ | ------- | --------------------- |
| timezoneName | text   | UTC     | Name of the timezone  |


**Aliases**: _none_

**Examples**: `!g now Europe/Berlin` - posts the current time in the Europe/Berlin timezone.