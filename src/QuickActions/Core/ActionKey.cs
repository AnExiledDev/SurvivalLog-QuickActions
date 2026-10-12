namespace QuickActions.Core;

/// <summary>
/// What makes two suggestions the same action. A chip shows only its action name and icon, so two chips with the same
/// name and icon look identical to the player: the same music on two players, or the same food in the fridge and in the
/// bag. Those are one action. A chip without a name falls back to the ids that say what it does.
/// </summary>
public static class ActionKey
{
    public static string Of(Chip chip) =>
        string.IsNullOrWhiteSpace(chip.ActionName)
            ? $"ids:{(int)chip.Kind}:{chip.ActionId}:{chip.FurnitureFuncId}:{chip.ItemConfigId}"
            : $"name:{chip.ActionName.Trim()}|{chip.IconPath}";
}
