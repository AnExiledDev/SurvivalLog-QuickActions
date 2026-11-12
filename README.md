# Quick Actions

Shows every action the game suggests in one row you can scroll sideways, instead of only three.

Survival Log suggests actions in a small row of chips above your action queue: eat this, sleep there, listen to music. It only ever shows three of them, and only one of each kind, so a second morale action or a second food never makes it onto the row. Quick Actions shows every suggested action, best first, in one row that scrolls sideways when it gets long. It only changes which chips show up. Clicking a chip does the same thing it always did, and nothing is added to your save.

## Features

- **Every suggestion**: every action the game suggests shows up, and several of the same kind can show at once (two foods, two ways to cheer up).
- **One chip per action**: the same action on two objects is one chip. Two music players give one "listen to music" chip, and the same food in the fridge and in your bag gives one chip. The chip uses whichever object the game rates higher.
- **Every floor of the house**: set `WholeHouse` to `true` and suggestions come from the basement, the ground floor and upstairs, instead of only the floor your character is on. Clicking one walks your character there, the way the game already does for a bed on another floor. Sleep still goes to the best bed in the house. `WholeHouse` is off when you first install the mod.
- **Best first**: the row keeps the game's order, with the best suggestion on the left.
- **Scrolls sideways**: when there are more chips than fit, move the mouse over the row and use the scroll wheel.
- **Status bars don't block the row**: the empty space beside the status bars in the bottom right no longer takes your clicks, so you can click the chips at the right end of the row. The bars and their buffs still respond as before.
- **Hidden chips stay hidden**: a chip you just clicked, or an action already waiting in your queue, is left out like before. When you are starving or exhausted, the game still hides everything but eating and sleeping.
- **Limit the number of chips**: set `MaxChips` to show at most that many chips.
- **A log file**: the mod writes what it set up to `BepInEx\quick-actions.log`.

There are no new keys or buttons.

Quick Actions adds no data to your save and does not edit game files. You can remove it at any time.

## Known issues and limits

- "The same action" means the same name and the same icon on the chip. Two different things that happen to share both become one chip.
- When a chip is scrolled halfway out of sight, its hidden half still blocks clicks on the world just past the edge of the row, up to about one chip wide. A chip scrolled all the way out of sight blocks nothing.
- Clicks in the small gaps between chips, and a few pixels above and below them, go to the row instead of the world, so the scroll wheel works there too.
- The scroll wheel over the row scrolls the row. Over the rest of the screen it zooms the camera as usual.
- The row stops growing at about a third of the screen width (less on small screens) so it stays clear of the panels in the middle of the bottom edge.

Works in English and Chinese. The chips show the game's action names in your game language, the mod adds no words of its own on screen, and every config setting is explained in English and Simplified Chinese.

## Requirements

- Survival Log 1.1.20719. It was built for this version, and a game update can break it until the mod is updated.
- The [BepInEx Pack for Survival Log](https://www.nexusmods.com/survivallog/mods/12), the BepInEx 6 (IL2CPP) build for the game, version `6.0.0-be.788`.
- [Exiled's Toolkit](https://github.com/AnExiledDev/SurvivalLog-ExiledToolkit) 0.20.0 or newer. Quick Actions does not load without it. It applies settings changed on the Mod Manager's Mods screen without restarting the game.
- Optional: [Mod Manager](https://www.nexusmods.com/survivallog/mods/73). With it, you can change every setting in game on its Mods screen and see what each one does, without opening the config file.

## Install

1. Install the [BepInEx Pack for Survival Log](https://www.nexusmods.com/survivallog/mods/12) (if no other mods were installed before, start the game once so BepInEx finishes setup, then quit).
2. Install Exiled's Toolkit. Download `ExiledToolkit-x.y.z.zip` from [its latest release](https://github.com/AnExiledDev/SurvivalLog-ExiledToolkit/releases/latest) or from [Nexus Mods](https://www.nexusmods.com/survivallog/mods/61), and extract it into the game folder (the folder with `SurvivalLog.exe`).
3. Download `QuickActions-x.y.z.zip` from the [latest release](https://github.com/AnExiledDev/SurvivalLog-QuickActions/releases/latest) or from [Nexus Mods](https://www.nexusmods.com/survivallog/mods/80).
4. Extract the zip into the game folder (the folder with `SurvivalLog.exe`). It holds a `BepInEx` folder, so the DLL lands in the right place. When asked, merge the folders. The result should be:
   `C:\Program Files (x86)\Steam\steamapps\common\Survival Log\BepInEx\plugins\QuickActions.dll`

## Uninstall

Delete `QuickActions.dll` from the `BepInEx\plugins` folder. For a clean slate, also delete `BepInEx\config\com.dev.survivallog.quickactions.cfg` and `BepInEx\quick-actions.log`.

## Configuration

Settings live in `BepInEx\config\com.dev.survivallog.quickactions.cfg`, created the first time the game starts with the mod. Change them in game on the Mod Manager's Mods screen, where they take effect at once, or edit the file while the game is closed. Enabled takes effect at the next game start. See [CONFIG.md](CONFIG.md) for every setting.

## Troubleshooting

Built against BepInEx `6.0.0-be.788`.

Check `BepInEx\LogOutput.log` for a line starting `===== Quick Actions`, followed by the mod's version and `started`, then `on` or `off`. Next should come a line starting `hooked` and a `max chips` line, which shows up again each time you change that setting. A moment later, a line says the page script was sent to the web UI.

The same lines are in `BepInEx\quick-actions.log`, which survives restarts.

If the row still shows only three chips, look in the log for a line starting `could not` or `failed`. When something goes wrong, the mod leaves the game's normal row of three in place.

## Feedback and bug reports

Found a bug, have an idea, or something feels off? Come say so on the Core Directive Discord: https://discord.gg/trTsyxUFqg . You can also post in the Posts tab of the mod's Nexus Mods page. When reporting a bug, attach `BepInEx\LogOutput.log` and `BepInEx\quick-actions.log`.

To reach me directly, message `exiled.dev` on Discord. You can find me in the [official Survival Log Discord](https://discord.gg/MZGg4gQAUe). I'm also on X as [@AnExiledDev](https://x.com/AnExiledDev).

A Survival Log wiki with detailed documentation for all of my mods is coming soon.

## Build

This is a BepInEx 6 IL2CPP plugin. It compiles against BepInEx and the game's IL2CPP interop assemblies, so a game install with BepInEx set up and started once is required. Those assemblies come from the game and are not part of this repo. The .NET 8 SDK is required.

```
dotnet build src/QuickActions -c Release -p:GameDir="C:\path\to\Survival Log"
```

`GameDir` is the game folder. The default in the project file is a local path you will almost certainly need to override. Compiling also needs Exiled's Toolkit 0.20.0 or newer: point `-p:ToolkitDll="C:\path\to\ExiledToolkit.dll"` at its DLL. The output DLL is at `src\QuickActions\bin\Release\QuickActions.dll`.

The unit tests cover the pure rules for picking the row (one chip per action, best first, hidden chips, the dance rule and the cap) and need nothing from the game:

```
dotnet test tests/QuickActions.Tests
```

## License

MIT. Copyright (c) 2026 AnExiledDev. See [LICENSE](LICENSE).
