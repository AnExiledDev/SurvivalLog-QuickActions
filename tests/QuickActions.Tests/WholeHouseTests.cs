using QuickActions.Core;
using Xunit;

namespace QuickActions.Tests;

public class HouseFloorsTests
{
    /// <summary>The game's map table: the home (1, 101, 102 in group 1), the office (1003, 10031, 10032) and outdoor maps (group 0).</summary>
    private static HouseFloors GameMaps()
    {
        var floors = new HouseFloors();

        foreach (var (mapId, group) in new[] { (1, 1), (2, 0), (3, 0), (101, 1), (102, 1), (1003, 1003), (10031, 1003), (10032, 1003), (1004, 0) })
            floors.Add(mapId, group);

        return floors;
    }

    [Fact]
    public void On_the_ground_floor_the_other_floors_are_the_basement_and_upstairs()
    {
        Assert.Equal(new[] { 101, 102 }, GameMaps().OtherFloors(1).ToArray());
    }

    [Fact]
    public void Upstairs_the_other_floors_are_the_ground_floor_and_the_basement()
    {
        Assert.Equal(new[] { 1, 101 }, GameMaps().OtherFloors(102).ToArray());
    }

    [Fact]
    public void Another_house_does_not_mix_with_the_home()
    {
        Assert.Equal(new[] { 10031, 10032 }, GameMaps().OtherFloors(1003).ToArray());
    }

    [Fact]
    public void An_outdoor_map_has_no_other_floors()
    {
        Assert.Empty(GameMaps().OtherFloors(2));
    }

    [Fact]
    public void A_map_the_table_does_not_have_has_no_other_floors()
    {
        Assert.Empty(GameMaps().OtherFloors(999));
    }

    [Fact]
    public void A_cleared_table_is_read_again_from_scratch()
    {
        var floors = GameMaps();

        floors.Clear();
        floors.Add(1, 1);
        floors.Add(102, 1);

        Assert.Equal(new[] { 102 }, floors.OtherFloors(1).ToArray());
    }
}

public class OtherFloorChipsTests
{
    [Fact]
    public void A_suggestion_from_another_floor_joins_the_list()
    {
        var picks = new OtherFloorChips();

        picks.Start();
        picks.Listed(10, 1);

        Assert.True(picks.Adds(ChipKind.Morale, 20, 1));
    }

    [Fact]
    public void Sleep_from_another_floor_is_never_added_so_the_game_keeps_its_best_bed()
    {
        var picks = new OtherFloorChips();

        picks.Start();

        Assert.False(picks.Adds(ChipKind.Sleep, 30, 5));
    }

    [Fact]
    public void Furniture_the_game_already_suggested_is_not_added_again()
    {
        var picks = new OtherFloorChips();

        picks.Start();
        picks.Listed(40, 2);

        Assert.False(picks.Adds(ChipKind.Repair, 40, 2));
    }

    [Fact]
    public void The_same_furniture_found_from_two_floors_is_added_once()
    {
        var picks = new OtherFloorChips();

        picks.Start();

        Assert.True(picks.Adds(ChipKind.Repair, 50, 3));
        Assert.False(picks.Adds(ChipKind.Repair, 50, 3));
    }

    [Fact]
    public void A_second_function_of_the_same_furniture_is_its_own_suggestion()
    {
        var picks = new OtherFloorChips();

        picks.Start();
        picks.Listed(60, 1);

        Assert.True(picks.Adds(ChipKind.Morale, 60, 2));
    }

    [Fact]
    public void A_new_row_forgets_what_the_last_one_listed()
    {
        var picks = new OtherFloorChips();

        picks.Start();
        picks.Listed(70, 1);
        picks.Start();

        Assert.True(picks.Adds(ChipKind.Morale, 70, 1));
    }
}
