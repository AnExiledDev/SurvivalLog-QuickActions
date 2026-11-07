using System;
using GameCore.HotUpdate;
using GameCore.HotUpdate.Battle.Logic;
using HarmonyLib;
using QuickActions.Core;
using GameChips = Il2CppSystem.Collections.Generic.List<GameCore.HotUpdate.SmartRecommendItemData>;

namespace QuickActions.Adapters;

/// <summary>
/// Adds the furniture suggestions of every floor of the house. SmartRecommendManager.CollectFurnitureFuncCandidates
/// gathers them for the character's floor only, plus beds and urgent doors from the other floors. The hook runs it
/// again for each other floor and keeps what it finds there, except sleep and anything already gathered. Whatever is
/// added is checked with the game's own home-scope rule (floor and area unlocked), the one it uses for off-floor beds.
/// A click on such a chip walks the character there, as the game already does for an off-floor bed. Static because
/// Harmony hooks are; the game gathers on its main thread, so the working lists are reused from one row to the next.
/// </summary>
internal static class WholeHousePatch
{
    private const string Doing = "adding suggestions from the other floors, the row shows the character's floor only";

    private static readonly HouseFloors Floors = new();
    private static readonly OtherFloorChips Picks = new();
    private static readonly GameChips Gathered = new();

    private static Il2CppSystem.Collections.Generic.Dictionary<int, Config_MapPoint>? _floorTable;
    private static bool _isOn;
    private static bool _isGathering;

    /// <returns>Whether the hook was placed.</returns>
    public static bool Apply(Harmony harmony) =>
        PatchKit.Patch(harmony, typeof(WholeHousePatch), typeof(SmartRecommendManager), nameof(SmartRecommendManager.CollectFurnitureFuncCandidates),
            nameof(AddOtherFloors),
            "hooked SmartRecommendManager.CollectFurnitureFuncCandidates: the row can show every floor of the house",
            "the row shows the character's floor only");

    /// <summary>Used from the next time the game builds the row. Set on the main thread, where the row is built.</summary>
    public static void UseWholeHouse(bool isOn) => _isOn = isOn;

    private static void AddOtherFloors(SmartRecommendManager __instance, BattleLogicWorld world, LeadingRole leadingRole,
        AttributeComponent attrComp, int currentFloorMapConfigId, GameChips candidates)
    {
        // The hook calls the game's method again for each floor, which comes back through here.
        if (!_isOn || _isGathering) return;

        try
        {
            var agents = world?._AgentManager;

            if (__instance == null || agents == null || candidates == null) return;
            if (!agents.IsHomeMap(currentFloorMapConfigId)) return;

            var otherFloors = HouseFloorsNow().OtherFloors(currentFloorMapConfigId);

            if (otherFloors.Count == 0) return;

            _isGathering = true;

            for (var i = 0; i < otherFloors.Count; i++)
                __instance.CollectFurnitureFuncCandidates(world, leadingRole, attrComp, otherFloors[i], Gathered);

            AddNew(agents, candidates);
        }
        catch (Exception ex)
        {
            PatchKit.Fail(Doing, ex);
        }
        finally
        {
            _isGathering = false;
            Gathered.Clear();
        }
    }

    private static void AddNew(AgentManager agents, GameChips candidates)
    {
        Picks.Start();

        for (var i = 0; i < candidates.Count; i++)
        {
            var chip = candidates[i];

            if (chip != null) Picks.Listed(chip._FurnitureLogicId_k__BackingField, chip._FurnitureFuncId_k__BackingField);
        }

        for (var i = 0; i < Gathered.Count; i++)
        {
            var chip = Gathered[i];

            if (chip == null || !IsInHomeScope(agents, chip._FurnitureLogicId_k__BackingField)) continue;
            if (!Picks.Adds((ChipKind)(int)chip._RecommendType_k__BackingField, chip._FurnitureLogicId_k__BackingField, chip._FurnitureFuncId_k__BackingField)) continue;

            candidates.Add(chip);
        }
    }

    /// <summary>
    /// The game skips its home-scope check for furniture on the floor it gathers for, since the character stands
    /// there. Gathered for another floor, the furniture still has to pass it: a locked basement or a locked area.
    /// </summary>
    private static bool IsInHomeScope(AgentManager agents, long furnitureId) =>
        agents.GetAgent(furnitureId)?.TryCast<Furniture>() is { } furniture && HomeScopeHelper.IsReachableContainer(furniture);

    /// <summary>Read again only when the game swaps its config tables, which a config reload does.</summary>
    private static HouseFloors HouseFloorsNow()
    {
        var table = GameSingleton.Existing<ConfigManager>()?._Config_MapPoint_Dict;

        if (table == null || (_floorTable != null && table.Pointer == _floorTable.Pointer)) return Floors;

        Floors.Clear();

        foreach (var row in table)
        {
            var map = row.Value;

            if (map != null) Floors.Add(map._ID_k__BackingField, map._HomeGroup_k__BackingField);
        }

        _floorTable = table;

        return Floors;
    }
}
