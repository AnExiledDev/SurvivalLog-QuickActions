using System.Collections.Generic;
using System.Linq;

namespace QuickActions.Core;

/// <summary>
/// Picks the row of suggestions. The game keeps the best three, one of each kind; this keeps every action, best first,
/// and holds on to the game's other rules: chips it would hide stay hidden, and a dance suggestion pushes out the
/// once-a-day furniture ones.
/// </summary>
public static class ChipSelection
{
    /// <param name="candidates">Every suggestion the game made, in the game's order.</param>
    /// <param name="maxChips">The most chips to show; zero or below shows them all.</param>
    /// <returns>Positions in <paramref name="candidates"/>, in the order the row shows them.</returns>
    public static IReadOnlyList<int> Choose(IReadOnlyList<Chip> candidates, int maxChips)
    {
        var bestFirst = Enumerable.Range(0, candidates.Count).OrderByDescending(i => candidates[i].Score);
        var chosen = new List<int>();
        var shownActions = new HashSet<string>();
        var hasDance = false;

        foreach (var index in bestFirst)
        {
            var chip = candidates[index];

            if (maxChips > 0 && chosen.Count >= maxChips) break;
            if (!chip.IsAvailable) continue;
            if (hasDance && chip.Kind == ChipKind.DailyFurniture) continue;
            if (!shownActions.Add(ActionKey.Of(chip))) continue;

            chosen.Add(index);

            if (!chip.IsDance) continue;

            hasDance = true;

            chosen.RemoveAll(i => candidates[i].Kind == ChipKind.DailyFurniture);
        }

        return chosen;
    }
}
