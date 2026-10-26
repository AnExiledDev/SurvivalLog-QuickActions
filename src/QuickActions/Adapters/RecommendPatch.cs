using System;
using System.Collections.Generic;
using GameCore.HotUpdate;
using GameCore.HotUpdate.Battle.Logic;
using HarmonyLib;
using QuickActions.Core;
using GameChips = Il2CppSystem.Collections.Generic.List<GameCore.HotUpdate.SmartRecommendItemData>;

namespace QuickActions.Adapters;

/// <summary>
/// Replaces the row the game picks from its suggestions. SmartRecommendManager.SelectTopDiversified sorts every
/// candidate best first, then keeps at most three, one of each kind. The hook runs after it and builds the row again
/// from the same sorted candidates. The game stores the returned list and indexes it when a chip is clicked, so the
/// row the player sees and the chip the game runs stay in step. On any failure the game's own three are kept.
/// Static because Harmony hooks are. The game builds the row on its main thread, after a change to the action queue
/// or the bag and at least every ten seconds, so the working lists below are reused from one row to the next.
/// </summary>
internal static class RecommendPatch
{
    private const string Doing = "building the row of every suggested action, the game's own three are shown instead";

    private static readonly List<SmartRecommendItemData> GameChipsRead = new();
    private static readonly List<Chip> ChipsRead = new();
    private static readonly ChipSelection Selection = new();

    private static int _maxChips;

    /// <returns>Whether the hook was placed.</returns>
    public static bool Apply(Harmony harmony) =>
        PatchKit.Patch(harmony, typeof(RecommendPatch), typeof(SmartRecommendManager), nameof(SmartRecommendManager.SelectTopDiversified),
            nameof(ShowEveryAction),
            "hooked SmartRecommendManager.SelectTopDiversified: the row shows every suggested action",
            "the game's row of three suggestions stays");

    /// <summary>Used from the next time the game builds the row. Set on the main thread, where the row is built.</summary>
    public static void UseMaxChips(int maxChips) => _maxChips = maxChips;

    private static void ShowEveryAction(SmartRecommendManager __instance, GameChips candidates, ref GameChips __result)
    {
        try
        {
            // With nothing suggested the game's own row is already the empty one this would build.
            if (__instance == null || candidates == null || candidates.Count == 0) return;

            ReadCandidates(__instance, candidates);

            __result = BuildRow();
        }
        catch (Exception ex)
        {
            PatchKit.Fail(Doing, ex);
        }
        finally
        {
            // The game's objects are let go of between rows.
            GameChipsRead.Clear();
        }
    }

    private static void ReadCandidates(SmartRecommendManager manager, GameChips candidates)
    {
        var queue = LeaderQueue.Read();
        var cooldowns = manager._clickedCooldown;
        var hasCooldowns = cooldowns != null && cooldowns.Count > 0;

        GameChipsRead.Clear();
        ChipsRead.Clear();

        for (var i = 0; i < candidates.Count; i++)
        {
            var gameChip = candidates[i];

            if (gameChip == null) continue;

            GameChipsRead.Add(gameChip);
            ChipsRead.Add(ToChip(manager, gameChip, queue, hasCooldowns ? cooldowns : null));
        }
    }

    private static GameChips BuildRow()
    {
        var chosen = Selection.Choose(ChipsRead, _maxChips);
        var row = new GameChips();

        // By index: a foreach over the read-only list would allocate its enumerator.
        for (var i = 0; i < chosen.Count; i++) row.Add(GameChipsRead[chosen[i]]);

        return row;
    }

    /// <summary>
    /// Availability is asked of the game's own checks, the same two its row uses. The chip's fields are read straight
    /// from their backing fields; the properties would each be a call into the game. The cooldown key is only made when
    /// some chip is cooling down: making it is a game call that returns a string.
    /// </summary>
    private static Chip ToChip(SmartRecommendManager manager, SmartRecommendItemData chip, LeaderQueue queue,
        Il2CppSystem.Collections.Generic.Dictionary<string, float>? cooldowns)
    {
        var isCoolingDown = cooldowns != null && cooldowns.ContainsKey(manager.MakeChipKey(chip));
        var isQueued = manager.IsChipAlreadyInActionQueue(queue.Actions, queue.AgentId, chip);

        return new Chip(
            (ChipKind)(int)chip._RecommendType_k__BackingField,
            chip._ActionName_k__BackingField ?? "",
            chip._IconPath_k__BackingField ?? "",
            chip._ActionId_k__BackingField,
            chip._FurnitureFuncId_k__BackingField,
            chip._ItemConfigId_k__BackingField,
            chip._Score_k__BackingField,
            manager.IsDanceChip(chip),
            !isCoolingDown && !isQueued);
    }

    /// <summary>
    /// The queue the game checks a chip against: the world's action manager and the leading character's id. Either
    /// can be missing, and then the game's check answers "not queued", as it does in the game's own row.
    /// </summary>
    private readonly record struct LeaderQueue(ActionManager? Actions, long AgentId)
    {
        public static LeaderQueue Read()
        {
            var world = GameSingleton.Existing<BattleLogicWorld>();
            var leader = world?._AgentManager?.GetLeadingRole();

            return new LeaderQueue(world?._ActionManager, leader?.InstanceId ?? 0);
        }
    }
}
