namespace QuickActions.Core;

/// <summary>
/// What makes two suggestions the same action. A chip shows only its action name and icon, so two chips with the same
/// name and icon look identical to the player: the same music on two players, or the same food in the fridge and in the
/// bag. Those are one action. A chip without a name falls back to the ids that say what it does. A value, so telling
/// chips apart builds no string.
/// </summary>
public readonly record struct ActionKey(string Name, string IconPath, int Kind, int ActionId, int FurnitureFuncId, int ItemConfigId)
{
    // A named key keeps no ids and an unnamed one keeps no name, so the two can never be equal.
    private const int NamedKind = -1;

    public static ActionKey Of(Chip chip) =>
        string.IsNullOrWhiteSpace(chip.ActionName)
            ? new ActionKey("", "", (int)chip.Kind, chip.ActionId, chip.FurnitureFuncId, chip.ItemConfigId)
            : new ActionKey(chip.ActionName.Trim(), chip.IconPath, NamedKind, 0, 0, 0);
}
