# Quick Actions settings

Settings live in `BepInEx\config\com.dev.survivallog.quickactions.cfg`, created the first time the game starts with the mod. Edit the file while the game is closed; changes apply at the next game start. Delete the file to get the defaults back.

## General

| Setting | Default | What it does |
| --- | --- | --- |
| `Enabled` | `true` | Show every action the game suggests in one row that scrolls sideways, one chip per action, instead of at most three, one of each kind. `false` leaves the game's own row. |
| `MaxChips` | `0` | The most chips the row shows, best first. `0` shows every suggested action. A number below 0 counts as 0. |

With `MaxChips` at `3` the row is close to the game's own, except that two chips of the same kind can show at once.

There are no keys or buttons to set.
