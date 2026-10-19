# Quick Actions settings

Settings live in `BepInEx\config\com.dev.survivallog.quickactions.cfg`, created the first time the game starts with the mod. Change them in game on the Mod Manager's Mods screen, where they take effect at once, or edit the file while the game is closed. Enabled takes effect at the next game start. Delete the file to get the defaults back.

## General

| Setting | Default | What it does |
| --- | --- | --- |
| `Enabled` | `true` | Show every action the game suggests in one row, one chip per action. Scroll the row sideways with the mouse wheel. The game on its own shows at most three, one of each kind. `false` keeps the game's own row. |
| `MaxChips` | `0` | The most chips the row shows, in the game's order, best first. `0` shows them all. A number below 0 counts as 0. |

With `MaxChips` at `3` the row is close to the game's own, except that two chips of the same kind can show at once.

There are no keys or buttons to set.
