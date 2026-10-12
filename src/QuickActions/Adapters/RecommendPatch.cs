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
/// Static because Harmony hooks are.
/// </summary>
internal static class RecommendPatch
{
    private const string Doing = "building the row of every suggested action, the game's own three are shown instead";

    private static int _maxChips;

    /// <returns>Whether the hook was placed.</returns>
    public static bool Apply(Harmony harmony, int maxChips)
    {
        _maxChips = maxChips;

        return PatchKit.Patch(harmony, typeof(RecommendPatch), typeof(SmartRecommendManager), nameof(SmartRecommendManager.SelectTopDiversified),
            nameof(ShowEveryAction),
            "hooked SmartRecommendManager.SelectTopDiversified: the row shows every suggested action",
            "the game's row of three suggestions stays");
    }

    private static void ShowEveryAction(SmartRecommendManager __instance, GameChips candidates, ref GameChips __result)
    {
        try
        {
            if (__instance == null || candidates == null) return;

            var queue = LeaderQueue.Read();
            var gameChips = new List<SmartRecommendItemData>(candidates.Count);
            var chips = new List<Chip>(candidates.Count);

            for (var i = 0; i < candidates.Count; i++)
            {
                var gameChip = candidates[i];

                if (gameChip == null) continue;

                gameChips.Add(gameChip);
                chips.Add(ToChip(__instance, gameChip, queue));
            }

            var row = new GameChips();

            foreach (var index in ChipSelection.Choose(chips, _maxChips)) row.Add(gameChips[index]);

            __result = row;
        }
        catch (Exception ex)
        {
            PatchKit.Fail(Doing, ex);
        }
    }

    /// <summary>Availability is asked of the game's own checks, the same two its row uses.</summary>
    private static Chip ToChip(SmartRecommendManager manager, SmartRecommendItemData chip, LeaderQueue queue)
    {
        var cooldowns = manager._clickedCooldown;
        var isCoolingDown = cooldowns != null && cooldowns.ContainsKey(manager.MakeChipKey(chip));
        var isQueued = manager.IsChipAlreadyInActionQueue(queue.Actions,queue.AgentId, chip);

        return new Chip(
            (ChipKind)(int)chip.RecommendType,
            chip.ActionName ?? "",
            chip.IconPath ?? "",
            chip.ActionId,
            chip.FurnitureFuncId,
            chip.ItemConfigId,
            chip.Score,
            manager.IsDanceChip(chip),
            !isCoolingDown && !isQueued);
    }

    /// <summary>
    /// The queue the game checks a chip against: the world's action manager and the leading character's id. Either
    /// can be missing, and then the game's check answers "not queued", as it does in the game's own row.
    /// </summary>
    private sealed record LeaderQueue(ActionManager? Actions, long AgentId)
    {
        public static LeaderQueue Read()
        {
            var world = GameSingleton.Existing<BattleLogicWorld>();
            var leader = world?._AgentManager?.GetLeadingRole();

            return new LeaderQueue(world?._ActionManager, leader?.InstanceId ?? 0);
        }
    }
}
