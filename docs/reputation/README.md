[Back to overview](../README.md)

---

## Commands for using Bayes Points:

- [leaderboard](#leaderboard)
- [givereputation](#givereputation)
- [takereputation](#takereputation)
- [getreputation](#getreputation)

### leaderboard

**Description**: Shows the reputation point leaderboard for a given month (defaults to current month)

**Parameters**:

| name      | format  | default       | description                  |
| --------- | ------- | ------------- | ---------------------------- |
| yearMonth | yyyy-mm | current month | Specify a moovercember 2020. |

---

## givereputation

**Description**: Give Bayes Points to another server member.

**Parameters**:

| name   | format   | default | description                       |
| ------ | -------- | ------- | --------------------------------- |
| member | @Mention | -       | The user you want to give points. |
| amount | number   | 1       | Number of points to give.         |

**Aliases**: `giverep`

**Examples**:
`!g giverep @Gauss 1` - Gives Gauss 1 Bayes Point.

---

## takereputation

**Description**: Reduce a users points total.

**Parameters**:

| name   | format   | default | description                         |
| ------ | -------- | ------- | ----------------------------------- |
| member | @Mention | -       | The user you want take points from. |
| amount | number   | 1       | Number of points to take away.      |

**Aliases**: `takerep`

**Examples**:
`!g takerep @Gauss 1` - Undo the giverep example from above.

---

## getreputation

**Description**: Show the points total and current ranking of a user.

**Parameters**:

| name   | format   | default | description                                         |
| ------ | -------- | ------- | --------------------------------------------------- |
| member | @Mention | You     | Member you want to check the points and ranking of. |

**Aliases**: `getrep`

**Examples**:
`!g getreputation @Gauss` - Check what reputation Gauss currently has.
`!g getrep` - Check your own reputation.

---
