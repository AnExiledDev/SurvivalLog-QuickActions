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
    private const string Version = "0.1.0";

    public override void Load()
    {
        var isEnabled = Config.Bind("General", "Enabled", true,
            "Show every action the game suggests in one row that scrolls sideways, one chip per action, instead of at most three, one of each kind. Off leaves the game's own row.").Value;
        var maxChips = Config.Bind("General", "MaxChips", 0,
            "The most chips the row shows, best first. 0 shows every suggested action.").Value;
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
