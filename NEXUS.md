Name: Quick Actions
Summary: See every action the game suggests, not just three, in one row of chips you can scroll sideways with the mouse wheel. Best suggestion first, one chip per action even when several objects offer it, and the game's own rules stay. Set MaxChips if you want a cap. No new keys, and nothing added to your save.

Description (BBCode):
Survival Log suggests actions in a small row of chips above your action queue, but it only ever shows three of them, and only one of each kind. Quick Actions shows every suggested action, best first, in one row that scrolls sideways when it gets long. It only changes which chips the row shows; it never changes what a chip does, your character, the world or a save.

[size=5]Main features[/size]
[list]
[*][b]Every suggestion[/b]: all the actions the game suggests show up, and several of the same kind can show at once (two foods, two ways to cheer up).[/*]
[*][b]One chip per action[/b]: the same action on two objects is one chip. Two music players give one chip, and the same food in the fridge and in your bag gives one chip.[/*]
[*][b]Best first[/b]: the row keeps the game's order, best suggestion on the left.[/*]
[*][b]Scrolls sideways[/b]: when there are more chips than fit, move the mouse over the row and use the scroll wheel.[/*]
[*][b]The game's own rules stay[/b]: a chip you just clicked, or an action already in your queue, is left out as before.[/*]
[*][b]A cap if you want one[/b]: set MaxChips to show at most that many chips.[/*]
[/list]
No new keys or buttons. Quick Actions adds no data to your save and does not edit game files. You can remove it at any time.

Works in English and Chinese.

[size=5]Known issues[/size]
[list]
[*]"The same action" means the same name and icon on the chip, so two different things that share both become one chip.[/*]
[*]A chip scrolled fully out of sight cannot be clicked until you scroll it back.[/*]
[*]The row stops growing at about a third of the screen width so it stays clear of the middle of the bottom edge.[/*]
[/list]

[size=5]Requirements[/size]
[list]
[*]Survival Log 1.1.19131. A game update can break it until the mod is updated.[/*]
[*][url=https://www.nexusmods.com/survivallog/mods/12]BepInEx for Survival Log[/url] (BepInEx 6.0.0-be.788, IL2CPP). Install it first.[/*]
[*]Optional: [url=https://www.nexusmods.com/survivallog/mods/61]Exiled's Toolkit[/url] 0.4.0 or newer. With it, settings changed on the Mod Manager's Mods screen take effect without restarting the game. Without it, they take effect at the next game start.[/*]
[/list]

[size=5]Installation[/size]
[list=1]
[*]Install [url=https://www.nexusmods.com/survivallog/mods/12]BepInEx[/url] [i](if no other mods were installed before, start the game once so BepInEx finishes setup, then quit)[/i].[/*]
[*]Optional, to change settings without a restart: install Exiled's Toolkit the same way. Download its zip and extract it into the game folder.[/*]
[*]Download the zip from the Files tab.[/*]
[*]Extract it into the game folder (the folder with [font=Courier New]SurvivalLog.exe[/font]). The zip holds a BepInEx folder, so the DLL lands in the right place. When asked, merge the folders. The result should be:
[code]C:\Program Files (x86)\Steam\steamapps\common\Survival Log\BepInEx\plugins\QuickActions.dll[/code][/*]
[/list]

[size=5]Uninstall[/size]
Delete [font=Courier New]QuickActions.dll[/font] from the [font=Courier New]BepInEx\plugins[/font] folder.

[size=5]Configuration[/size]
To turn the mod off or cap the number of chips, edit [font=Courier New]BepInEx\config\com.dev.survivallog.quickactions.cfg[/font] while the game is closed, or do it in game on the Mod Manager's Mods screen, where changes take effect at once (see [url=https://github.com/AnExiledDev/SurvivalLog-QuickActions/blob/main/CONFIG.md]CONFIG.md[/url] for every setting). Enabled takes effect at the next game start. Without Exiled's Toolkit installed, changes made on the Mods screen take effect at the next game start instead.

[size=5]Feedback[/size]
Found a bug or have an idea? Come say hi on the [url=https://discord.gg/trTsyxUFqg]Core Directive Discord[/url], or post in this page's Posts tab. For bugs, please include [font=Courier New]BepInEx\LogOutput.log[/font] and [font=Courier New]BepInEx\quick-actions.log[/font].
To reach me directly, message [b]exiled.dev[/b] on Discord; you can find me in the [url=https://discord.gg/MZGg4gQAUe]official Survival Log Discord[/url]. I'm also on X as [url=https://x.com/AnExiledDev]@AnExiledDev[/url].
Check out my [url=https://sldb.core-directive.com/en/]Survival Log wiki[/url] for items, crafting, maps and more.

[size=5]Source and license[/size]
Open source under MIT: [url=https://github.com/AnExiledDev/SurvivalLog-QuickActions]github.com/AnExiledDev/SurvivalLog-QuickActions[/url]

[line]
[size=5]简体中文[/size]

Survival Log 会在行动队列上方用一小行推荐项提示你可以做的行为，但最多只显示三个，而且同一类只显示一个。Quick Actions 会把游戏推荐的所有行为按从好到差的顺序放在同一行里，太长时可以左右滚动。它只改变这一行显示哪些推荐项，不会改变推荐项的效果、你的角色、世界或存档。

[size=5]主要功能[/size]
[list]
[*][b]所有推荐[/b]：游戏推荐的行为全部显示，同一类也可以同时出现多个（两种食物、两种提升心情的方式）。[/*]
[*][b]一个行为一个推荐项[/b]：对两个物体的同一行为只算一个推荐项。两台音乐播放器只显示一个，冰箱和背包里的同一种食物也只显示一个。[/*]
[*][b]最好的在最前[/b]：保持游戏的推荐顺序，最好的推荐在最左边。[/*]
[*][b]左右滚动[/b]：推荐项放不下时，把鼠标放在这一行上用滚轮滚动。[/*]
[*][b]保留游戏原有规则[/b]：刚点过的推荐项，或已经在队列中的行为，照旧不显示。[/*]
[*][b]可设上限[/b]：设置 MaxChips 后最多只显示这么多个推荐项。[/*]
[/list]
没有新增按键或按钮。Quick Actions 不会往存档里添加任何内容，也不会修改游戏文件，随时可以移除。

支持英文和中文。

[size=5]已知问题[/size]
[list]
[*]「同一行为」指推荐项上的名称和图标都相同，所以两件名称和图标都相同的不同事物会合并成一个推荐项。[/*]
[*]完全滚出视野的推荐项要滚回来才能点击。[/*]
[*]这一行最多长到大约屏幕宽度的三分之一，以免挡住屏幕底部中间的内容。[/*]
[/list]

[size=5]前置要求[/size]
[list]
[*]Survival Log 1.1.19131。游戏更新后模组可能暂时失效，直到模组更新为止。[/*]
[*][url=https://www.nexusmods.com/survivallog/mods/12]BepInEx for Survival Log[/url]（BepInEx 6.0.0-be.788，IL2CPP），请先安装。[/*]
[*]可选：[url=https://www.nexusmods.com/survivallog/mods/61]Exiled's Toolkit[/url] 0.4.0 或更新版本。装了它，在 Mod Manager 的「模组」界面改动的设置无需重启游戏即可生效；没装的话，改动在下次启动游戏时生效。[/*]
[/list]

[size=5]安装[/size]
[list=1]
[*]安装 [url=https://www.nexusmods.com/survivallog/mods/12]BepInEx[/url] [i]（如果之前没有装过其他模组，先启动一次游戏，让 BepInEx 完成初始化，然后退出）[/i]。[/*]
[*]可选（改动设置无需重启游戏）：用同样的方式安装 Exiled's Toolkit，下载它的压缩包并解压到游戏目录。[/*]
[*]在「文件」页下载压缩包。[/*]
[*]把压缩包解压到游戏目录（[font=Courier New]SurvivalLog.exe[/font] 所在的文件夹）。压缩包里带有 BepInEx 文件夹，DLL 会自动放到正确位置。如果提示合并文件夹，选择合并。解压后应为：
[code]C:\Program Files (x86)\Steam\steamapps\common\Survival Log\BepInEx\plugins\QuickActions.dll[/code][/*]
[/list]

[size=5]卸载[/size]
从 [font=Courier New]BepInEx\plugins[/font] 文件夹中删除 [font=Courier New]QuickActions.dll[/font]。

[size=5]配置[/size]
如需关闭模组或限制推荐项数量，可以在游戏内 Mod Manager 的「模组」界面修改（改动立即生效），也可以在游戏关闭时编辑 [font=Courier New]BepInEx\config\com.dev.survivallog.quickactions.cfg[/font]。配置文件中每项设置都有中文说明，完整说明见 [url=https://github.com/AnExiledDev/SurvivalLog-QuickActions/blob/main/CONFIG.md]CONFIG.md[/url]（英文）。Enabled 会在下次启动游戏时生效。没装 Exiled's Toolkit 的话，在「模组」界面的改动改为在下次启动游戏时生效。

[size=5]反馈[/size]
发现 bug 或有好点子？欢迎来 [url=https://discord.gg/trTsyxUFqg]Core Directive Discord[/url] 打个招呼，或在本页的「Posts」标签里发帖。报告 bug 时，请附上 [font=Courier New]BepInEx\LogOutput.log[/font] 和 [font=Courier New]BepInEx\quick-actions.log[/font]。
想直接联系我，可以在 Discord 上给 [b]exiled.dev[/b] 发消息，我在 [url=https://discord.gg/MZGg4gQAUe]Survival Log 官方 Discord[/url] 里。也可以在 X 上找我：[url=https://x.com/AnExiledDev]@AnExiledDev[/url]。
也欢迎看看我做的 [url=https://sldb.core-directive.com/zh/]Survival Log 维基[/url]，收录物品、制作、地图等资料。

[size=5]源代码与许可[/size]
基于 MIT 许可开源：[url=https://github.com/AnExiledDev/SurvivalLog-QuickActions]github.com/AnExiledDev/SurvivalLog-QuickActions[/url]
