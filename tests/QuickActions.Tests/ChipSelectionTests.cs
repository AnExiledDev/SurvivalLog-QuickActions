using QuickActions.Core;
using Xunit;

namespace QuickActions.Tests;

/// <summary>Suggestions shaped like the ones the game makes.</summary>
internal static class Chips
{
    public static Chip Furniture(ChipKind kind, string name, float score, string icon = "icon/default", int actionId = 100, int funcId = 1) =>
        new(kind, name, icon, actionId, funcId, 0, score, IsDance: false, IsAvailable: true);

    public static Chip Food(string name, float score, int itemConfigId, int actionId = 200) =>
        new(ChipKind.Eat, name, "icon/" + itemConfigId, actionId, 0, itemConfigId, score, IsDance: false, IsAvailable: true);

    public static Chip Dance(float score) =>
        new(ChipKind.Morale, "Dance", "icon/dance", 900, 0, 0, score, IsDance: true, IsAvailable: true);

    public static Chip Unavailable(this Chip chip) => chip with { IsAvailable = false };

    /// <summary>The names of the chosen chips, in row order.</summary>
    public static string[] Row(IReadOnlyList<Chip> candidates, int maxChips = 0) =>
        new ChipSelection().Choose(candidates, maxChips).Select(i => candidates[i].ActionName).ToArray();
}

public class ChipSelectionTests
{
    [Fact]
    public void Every_suggested_action_is_shown_not_just_three_of_different_kinds()
    {
        var candidates = new[]
        {
            Chips.Furniture(ChipKind.Morale, "Play guitar", 9f, "icon/guitar"),
            Chips.Furniture(ChipKind.Morale, "Listen to music", 8f, "icon/music"),
            Chips.Food("Eat bread", 7f, 11),
            Chips.Food("Eat apple", 6f, 12),
            Chips.Furniture(ChipKind.Sleep, "Sleep", 5f, "icon/bed"),
            Chips.Furniture(ChipKind.Repair, "Repair door", 4f, "icon/door")
        };

        Assert.Equal(new[] { "Play guitar", "Listen to music", "Eat bread", "Eat apple", "Sleep", "Repair door" }, Chips.Row(candidates));
    }

    [Fact]
    public void The_same_action_on_two_objects_gives_one_chip_from_the_best_scored_object()
    {
        var weakerPlayer = Chips.Furniture(ChipKind.Morale, "Listen to music", 3f, "icon/music", funcId: 7);
        var betterPlayer = Chips.Furniture(ChipKind.Morale, "Listen to music", 6f, "icon/music", funcId: 8);
        var candidates = new[] { weakerPlayer, betterPlayer };

        Assert.Equal(new[] { 1 }, new ChipSelection().Choose(candidates, 0).ToArray());
    }

    [Fact]
    public void The_same_food_in_the_fridge_and_in_the_bag_gives_one_chip()
    {
        var candidates = new[]
        {
            Chips.Food("Eat bread", 7f, 11, actionId: 201),
            Chips.Food("Eat bread", 5f, 11, actionId: 202)
        };

        Assert.Equal(new[] { "Eat bread" }, Chips.Row(candidates));
    }

    [Fact]
    public void Different_foods_stay_separate_chips()
    {
        var candidates = new[] { Chips.Food("Eat bread", 7f, 11), Chips.Food("Eat apple", 6f, 12) };

        Assert.Equal(new[] { "Eat bread", "Eat apple" }, Chips.Row(candidates));
    }

    [Fact]
    public void Same_name_with_a_different_icon_is_a_different_action()
    {
        var candidates = new[]
        {
            Chips.Furniture(ChipKind.Morale, "Play", 5f, "icon/guitar"),
            Chips.Furniture(ChipKind.Morale, "Play", 4f, "icon/piano")
        };

        Assert.Equal(2, new ChipSelection().Choose(candidates, 0).Count);
    }

    [Fact]
    public void The_row_runs_best_first()
    {
        var candidates = new[]
        {
            Chips.Furniture(ChipKind.Sleep, "Sleep", 2f),
            Chips.Food("Eat bread", 9f, 11),
            Chips.Furniture(ChipKind.Morale, "Listen to music", 5f, "icon/music")
        };

        Assert.Equal(new[] { "Eat bread", "Listen to music", "Sleep" }, Chips.Row(candidates));
    }

    [Fact]
    public void Chips_with_equal_scores_keep_the_games_order()
    {
        var candidates = new[]
        {
            Chips.Food("Eat bread", 5f, 11),
            Chips.Food("Eat apple", 5f, 12),
            Chips.Food("Eat soup", 5f, 13)
        };

        Assert.Equal(new[] { "Eat bread", "Eat apple", "Eat soup" }, Chips.Row(candidates));
    }

    [Fact]
    public void A_chip_the_game_would_hide_stays_hidden()
    {
        var candidates = new[]
        {
            Chips.Food("Eat bread", 9f, 11).Unavailable(),
            Chips.Food("Eat apple", 6f, 12)
        };

        Assert.Equal(new[] { "Eat apple" }, Chips.Row(candidates));
    }

    [Fact]
    public void When_the_best_object_for_an_action_is_hidden_the_next_one_offers_that_action()
    {
        var queuedPlayer = Chips.Furniture(ChipKind.Morale, "Listen to music", 6f, "icon/music", funcId: 8).Unavailable();
        var otherPlayer = Chips.Furniture(ChipKind.Morale, "Listen to music", 3f, "icon/music", funcId: 7);
        var candidates = new[] { queuedPlayer, otherPlayer };

        Assert.Equal(new[] { 1 }, new ChipSelection().Choose(candidates, 0).ToArray());
    }

    [Fact]
    public void A_dance_suggestion_pushes_out_once_a_day_furniture_ranked_above_it()
    {
        var candidates = new[]
        {
            Chips.Furniture(ChipKind.DailyFurniture, "Exercise", 9f, "icon/bike"),
            Chips.Dance(8f),
            Chips.Food("Eat bread", 7f, 11)
        };

        Assert.Equal(new[] { "Dance", "Eat bread" }, Chips.Row(candidates));
    }

    [Fact]
    public void A_dance_suggestion_pushes_out_once_a_day_furniture_ranked_below_it()
    {
        var candidates = new[]
        {
            Chips.Dance(8f),
            Chips.Furniture(ChipKind.DailyFurniture, "Exercise", 6f, "icon/bike"),
            Chips.Food("Eat bread", 5f, 11)
        };

        Assert.Equal(new[] { "Dance", "Eat bread" }, Chips.Row(candidates));
    }

    [Fact]
    public void Once_a_day_furniture_shows_when_there_is_no_dance_suggestion()
    {
        var candidates = new[]
        {
            Chips.Furniture(ChipKind.DailyFurniture, "Exercise", 9f, "icon/bike"),
            Chips.Furniture(ChipKind.DailyFurniture, "Read", 8f, "icon/book")
        };

        Assert.Equal(new[] { "Exercise", "Read" }, Chips.Row(candidates));
    }

    [Fact]
    public void The_cap_keeps_the_best_chips()
    {
        var candidates = new[]
        {
            Chips.Food("Eat apple", 3f, 12),
            Chips.Food("Eat bread", 9f, 11),
            Chips.Food("Eat soup", 6f, 13),
            Chips.Food("Eat meat", 1f, 14)
        };

        Assert.Equal(new[] { "Eat bread", "Eat soup" }, Chips.Row(candidates, maxChips: 2));
    }

    [Fact]
    public void A_cap_of_zero_or_below_shows_every_action()
    {
        var candidates = Enumerable.Range(1, 12).Select(i => Chips.Food("Eat food " + i, 20 - i, i)).ToArray();

        Assert.Equal(12, new ChipSelection().Choose(candidates, 0).Count);
        Assert.Equal(12, new ChipSelection().Choose(candidates, -3).Count);
    }

    [Fact]
    public void A_duplicate_does_not_use_up_a_place_under_the_cap()
    {
        var candidates = new[]
        {
            Chips.Food("Eat bread", 9f, 11, actionId: 201),
            Chips.Food("Eat bread", 8f, 11, actionId: 202),
            Chips.Food("Eat apple", 7f, 12)
        };

        Assert.Equal(new[] { "Eat bread", "Eat apple" }, Chips.Row(candidates, maxChips: 2));
    }

    [Fact]
    public void Nothing_suggested_gives_an_empty_row()
    {
        Assert.Empty(new ChipSelection().Choose(Array.Empty<Chip>(), 0));
    }

    [Fact]
    public void The_next_row_is_built_fresh_when_the_working_lists_are_reused()
    {
        var selection = new ChipSelection();
        var first = new[]
        {
            Chips.Dance(9f),
            Chips.Food("Eat bread", 8f, 11),
            Chips.Food("Eat apple", 7f, 12)
        };
        var second = new[]
        {
            Chips.Food("Eat bread", 2f, 11),
            Chips.Furniture(ChipKind.DailyFurniture, "Exercise", 5f, "icon/bike")
        };

        selection.Choose(first, 0);

        var row = selection.Choose(second, 0).Select(i => second[i].ActionName).ToArray();

        // Bread was shown last row and a dance hid daily furniture last row: neither carries over.
        Assert.Equal(new[] { "Exercise", "Eat bread" }, row);
    }

    [Fact]
    public void A_shorter_row_after_a_longer_one_holds_only_its_own_chips()
    {
        var selection = new ChipSelection();
        var longRow = Enumerable.Range(1, 8).Select(i => Chips.Food("Eat food " + i, 20 - i, i)).ToArray();
        var shortRow = new[] { Chips.Food("Eat soup", 1f, 30) };

        selection.Choose(longRow, 0);

        Assert.Equal(new[] { 0 }, selection.Choose(shortRow, 0).ToArray());
    }
}

public class ActionKeyTests
{
    [Fact]
    public void Chips_without_a_name_are_told_apart_by_what_they_do()
    {
        var water = new Chip(ChipKind.PlantCare, "", "", 1613, 3, 0, 5f, IsDance: false, IsAvailable: true);
        var weed = water with { ActionId = 1614 };

        Assert.NotEqual(ActionKey.Of(water), ActionKey.Of(weed));
    }

    [Fact]
    public void Chips_without_a_name_that_do_the_same_thing_are_one_action()
    {
        var water = new Chip(ChipKind.PlantCare, " ", "icon/a", 1613, 3, 0, 5f, IsDance: false, IsAvailable: true);
        var waterElsewhere = water with { IconPath = "icon/b", Score = 2f };

        Assert.Equal(ActionKey.Of(water), ActionKey.Of(waterElsewhere));
    }
}
