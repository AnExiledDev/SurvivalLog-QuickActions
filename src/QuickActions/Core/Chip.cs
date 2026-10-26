namespace QuickActions.Core;

/// <summary>The kinds the game sorts its suggestions into. The numbers are the game's own.</summary>
public enum ChipKind
{
    Eat = 0,
    Sleep = 1,
    Morale = 2,
    DailyFurniture = 3,
    PlantCare = 4,
    Cooking = 5,
    Repair = 6
}

/// <summary>
/// One suggestion the game made, reduced to what choosing the row needs. <see cref="IsAvailable"/> is the game's own
/// verdict: false while the chip was just clicked or its action already waits in the queue. A value, so reading the
/// game's suggestions allocates nothing per chip.
/// </summary>
public readonly record struct Chip(
    ChipKind Kind,
    string ActionName,
    string IconPath,
    int ActionId,
    int FurnitureFuncId,
    int ItemConfigId,
    float Score,
    bool IsDance,
    bool IsAvailable);
