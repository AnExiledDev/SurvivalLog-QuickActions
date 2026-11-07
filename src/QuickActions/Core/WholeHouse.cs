using System.Collections.Generic;

namespace QuickActions.Core;

/// <summary>
/// The floors of the house, from the game's map table. Maps that share a home group above 0 are floors of one house
/// (the home's ground floor, basement and upstairs are 1, 101 and 102, all in group 1). Filled once per table.
/// </summary>
public sealed class HouseFloors
{
    private readonly Dictionary<int, int> _groupOf = new();
    private readonly List<int> _mapIds = new();
    private readonly List<int> _others = new();

    public int Count => _mapIds.Count;

    public void Clear()
    {
        _groupOf.Clear();
        _mapIds.Clear();
    }

    public void Add(int mapId, int homeGroup)
    {
        if (homeGroup <= 0 || !_groupOf.TryAdd(mapId, homeGroup)) return;

        _mapIds.Add(mapId);
    }

    /// <summary>
    /// Every other floor of the house <paramref name="floor"/> is in, in table order. Empty when that floor is in no
    /// house. The list is reused by the next call.
    /// </summary>
    public IReadOnlyList<int> OtherFloors(int floor)
    {
        _others.Clear();

        if (!_groupOf.TryGetValue(floor, out var group)) return _others;

        for (var i = 0; i < _mapIds.Count; i++)
        {
            var mapId = _mapIds[i];

            if (mapId != floor && _groupOf[mapId] == group) _others.Add(mapId);
        }

        return _others;
    }
}

/// <summary>
/// Which suggestions gathered on the other floors join the ones the game gathered on the character's floor. The game
/// already offers beds and doors from every floor, so the same furniture and function is never added twice. Sleep is
/// never added: the game keeps one sleep suggestion, the best bed in the house, and a second one from another floor
/// would send the character to a worse bed.
/// </summary>
public sealed class OtherFloorChips
{
    private readonly HashSet<(long FurnitureId, int FuncId)> _listed = new();

    /// <summary>Starts a new row: forgets what the last one listed.</summary>
    public void Start() => _listed.Clear();

    /// <summary>A suggestion the game already gathered.</summary>
    public void Listed(long furnitureId, int funcId) => _listed.Add((furnitureId, funcId));

    /// <summary>Whether a suggestion from another floor joins the list. Once it does, the same one is not added again.</summary>
    public bool Adds(ChipKind kind, long furnitureId, int funcId) =>
        kind != ChipKind.Sleep && _listed.Add((furnitureId, funcId));
}
