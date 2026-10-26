using System.Collections.Generic;

namespace QuickActions.Core;

/// <summary>
/// Picks the row of suggestions. The game keeps the best three, one of each kind; this keeps every action, best first,
/// and holds on to the game's other rules: chips it would hide stay hidden, and a dance suggestion pushes out the
/// once-a-day furniture ones. It keeps its working lists between rows, so building a row allocates nothing once they
/// have grown to the game's longest list; use one instance from one thread.
/// </summary>
public sealed class ChipSelection
{
    private readonly List<int> _bestFirst = new();
    private readonly List<int> _chosen = new();
    private readonly HashSet<ActionKey> _shownActions = new();

    /// <param name="candidates">Every suggestion the game made, in the game's order.</param>
    /// <param name="maxChips">The most chips to show; zero or below shows them all.</param>
    /// <returns>
    /// Positions in <paramref name="candidates"/>, in the order the row shows them. The list is this instance's own and
    /// is rewritten by the next call.
    /// </returns>
    public IReadOnlyList<int> Choose(IReadOnlyList<Chip> candidates, int maxChips)
    {
        var hasDance = false;

        _chosen.Clear();
        _shownActions.Clear();
        SortBestFirst(candidates);

        foreach (var index in _bestFirst)
        {
            var chip = candidates[index];

            if (maxChips > 0 && _chosen.Count >= maxChips) break;
            if (!chip.IsAvailable) continue;
            if (hasDance && chip.Kind == ChipKind.DailyFurniture) continue;
            if (!_shownActions.Add(ActionKey.Of(chip))) continue;

            _chosen.Add(index);

            if (!chip.IsDance) continue;

            hasDance = true;

            DropDailyFurniture(candidates);
        }

        return _chosen;
    }

    /// <summary>Highest score first; equal scores keep the game's order. A score that is not a number sorts last.</summary>
    private void SortBestFirst(IReadOnlyList<Chip> candidates)
    {
        _bestFirst.Clear();

        for (var index = 0; index < candidates.Count; index++)
        {
            var score = candidates[index].Score;
            var at = _bestFirst.Count;

            while (at > 0 && candidates[_bestFirst[at - 1]].Score.CompareTo(score) < 0) at--;
            _bestFirst.Insert(at, index);
        }
    }

    private void DropDailyFurniture(IReadOnlyList<Chip> candidates)
    {
        for (var at = _chosen.Count - 1; at >= 0; at--)
        {
            if (candidates[_chosen[at]].Kind == ChipKind.DailyFurniture) _chosen.RemoveAt(at);
        }
    }
}
