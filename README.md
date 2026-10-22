# Quick Actions

See every action the game suggests, not just three, in one row you can scroll sideways.

Survival Log suggests actions in a small row of chips above your action queue: eat this, sleep there, listen to music. It only ever shows three of them, and only one of each kind, so a second morale action or a second food never makes it onto the row. Quick Actions shows every suggested action, best first, in one row that scrolls sideways when it gets long. It only changes which chips the row shows. It never changes what a chip does, your character, the world or a save.

## Features

- **Every suggestion**: all the actions the game suggests show up, not only the best three, and several of the same kind can show at once (two foods, two ways to cheer up).
- **One chip per action**: the same action on two objects is one chip. Two music players give one "listen to music" chip, and the same food in the fridge and in your bag gives one chip. The chip uses the better-scored object, the one the game itself would have picked.
- **Best first**: the row keeps the game's order, best suggestion on the left.
- **Scrolls sideways**: when there are more chips than fit, the row scrolls. Move the mouse over it and use the scroll wheel.
- **Status bars don't block the row**: the empty space beside the status bars in the bottom right no longer takes your clicks, so chips at the right end of the row can be clicked. The bars and their buffs still respond as before.
- **The game's own rules stay**: a chip you just clicked, or an action already waiting in your queue, is left out as before, and the game still hides everything but eating and sleeping when you are starving or exhausted.
- **A cap if you want one**: set `MaxChips` to show at most that many chips.
- **A log file**: what the mod set up is written to `BepInEx\quick-actions.log`.

There are no new keys or buttons.

Quick Actions adds no data to your save and does not edit game files. You can remove it at any time.

## Known issues and limits

- "The same action" means the same name and the same icon on the chip. Two different things that happen to share both become one chip.
- A chip scrolled half out of sight can still take a click on the world just past the row's edge, up to about one chip wide. A chip scrolled fully out of sight takes no clicks. The row also takes clicks in the small gaps between chips and a few pixels above and below them, so the scroll wheel works there.
- The scroll wheel over the row scrolls the row. Over the rest of the screen it zooms the camera as usual.
- The row stops growing at about a third of the screen width (less on small screens) so it stays clear of the panels in the middle of the bottom edge.

Works in English and Chinese: the chips show the game's own action names in your game language, the mod adds no words of its own on screen, and every config setting is explained in English and Simplified Chinese.

## Requirements

- Survival Log 1.1.19131. Built for this version; a game update can break it until the mod is updated.
- The [BepInEx Pack for Survival Log](https://www.nexusmods.com/survivallog/mods/12), the BepInEx 6 (IL2CPP) build for the game, version `6.0.0-be.788`.
- Optional: [Exiled's Toolkit](https://github.com/AnExiledDev/SurvivalLog-ExiledToolkit) 0.4.0 or newer. With it, settings changed on the Mod Manager's Mods screen take effect without restarting the game. Without it, they take effect at the next game start.

## Install

1. Install the [BepInEx Pack for Survival Log](https://www.nexusmods.com/survivallog/mods/12) (if no other mods were installed before, start the game once so BepInEx finishes setup, then quit).
2. Optional, to change settings without a restart: install Exiled's Toolkit. Download `ExiledToolkit-x.y.z.zip` from [its latest release](https://github.com/AnExiledDev/SurvivalLog-ExiledToolkit/releases/latest) or from [Nexus Mods](https://www.nexusmods.com/survivallog/mods/61), and extract it into the game folder (the folder with `SurvivalLog.exe`).
3. Download `QuickActions-x.y.z.zip` from the [latest release](https://github.com/AnExiledDev/SurvivalLog-QuickActions/releases/latest) or from [Nexus Mods](https://www.nexusmods.com/survivallog/mods/80).
4. Extract the zip into the game folder (the folder with `SurvivalLog.exe`). It holds a `BepInEx` folder, so the DLL lands in the right place. When asked, merge the folders. The result should be:
   `C:\Program Files (x86)\Steam\steamapps\common\Survival Log\BepInEx\plugins\QuickActions.dll`

## Uninstall

Delete `QuickActions.dll` from the `BepInEx\plugins` folder. For a clean slate, also delete `BepInEx\config\com.dev.survivallog.quickactions.cfg` and `BepInEx\quick-actions.log`.

## Configuration

Settings live in `BepInEx\config\com.dev.survivallog.quickactions.cfg`, created the first time the game starts with the mod. Change them in game on the Mod Manager's Mods screen, where they take effect at once, or edit the file while the game is closed. Enabled takes effect at the next game start. See [CONFIG.md](CONFIG.md) for every setting. Without Exiled's Toolkit installed, changes made on the Mods screen take effect at the next game start instead.

## Troubleshooting

Built against BepInEx `6.0.0-be.788`.

Check `BepInEx\LogOutput.log` for a line starting `===== Quick Actions`, followed by the mod's version and `started`. It says whether the mod is on. The next lines should say the suggestion hook went in and give the chip limit (`max chips`, again each time you change it), and a moment later, that the page script was sent to the web UI.

The same lines are in `BepInEx\quick-actions.log`, which survives restarts.

If the row still shows only three chips, look in the log for a line starting `could not` or `failed`. When the mod cannot do its part, it leaves the game's own row in place.

## Feedback and bug reports

Found a bug, have an idea, or something feels off? Come say so on the Core Directive Discord: https://discord.gg/trTsyxUFqg . You can also post in the Posts tab of the mod's Nexus Mods page. When reporting a bug, attach `BepInEx\LogOutput.log` and `BepInEx\quick-actions.log`.

To reach me directly, message `exiled.dev` on Discord; you can find me in the [official Survival Log Discord](https://discord.gg/MZGg4gQAUe). I'm also on X as [@AnExiledDev](https://x.com/AnExiledDev).

A Survival Log wiki with detailed documentation for all of my mods is coming soon.

## Build

This is a BepInEx 6 IL2CPP plugin. It compiles against BepInEx and the game's IL2CPP interop assemblies, so a game install with BepInEx set up and started once is required. Those assemblies come from the game and are not part of this repo. The .NET 8 SDK is required.

```
dotnet build src/QuickActions -c Release -p:GameDir="C:\path\to\Survival Log"
```

`GameDir` is the game folder. The default in the project file is a local path you will almost certainly need to override. The output DLL is at `src\QuickActions\bin\Release\QuickActions.dll`.

The unit tests cover the pure rules for picking the row (one chip per action, best first, hidden chips, the dance rule and the cap) and need nothing from the game:

```
dotnet test tests/QuickActions.Tests
```

## License

MIT. Copyright (c) 2026 AnExiledDev. See [LICENSE](LICENSE).
