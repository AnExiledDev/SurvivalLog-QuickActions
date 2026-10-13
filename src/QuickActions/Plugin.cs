using System.IO;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using QuickActions.Adapters;

namespace QuickActions;

[BepInPlugin(Guid, "Quick Actions", Version)]
public sealed class Plugin : BasePlugin
{
    private const string Guid = "com.dev.survivallog.quickactions";
    private const string Version = "0.1.2";

    public override void Load()
    {
        var isEnabled = Config.Bind("General", "Enabled", true,
            "Show every action the game suggests in one row, one chip per action. Scroll the row sideways with the mouse wheel. The game on its own shows at most three. Set to false to keep the game's own row.\n" +
            "在一行里显示游戏推荐的所有行为，每个行为一个推荐项，鼠标放在这一行上用滚轮左右滚动。游戏自己最多只显示三个。设为 false 则保留游戏自带的那一行。").Value;
        var maxChips = Config.Bind("General", "MaxChips", 0,
            "The most chips the row shows, in the game's order, best first. 0 shows them all. Numbers below 0 count as 0.\n" +
            "这一行最多显示几个推荐项，按游戏的推荐顺序排列，最好的在最前。0 表示全部显示，小于 0 的数按 0 处理。").Value;
        var log = new QuickActionsLog(Log, Path.Combine(Paths.BepInExRootPath, "quick-actions.log"));

        PatchKit.Log = log.Write;
        log.TrimIfLarge();
        log.Write($"===== Quick Actions {Version} started: {(isEnabled ? "on" : "off")}, max chips {(maxChips > 0 ? maxChips.ToString() : "no limit")}");

        if (!isEnabled) return;

        // Without the hook the game's row keeps its three chips, which fit without scrolling, so the page script stays out.
        if (!RecommendPatch.Apply(new Harmony(Guid), maxChips)) return;

        PageTicker.Page = new WebPage(log.Write);
        AddComponent<PageTicker>();
    }
}
