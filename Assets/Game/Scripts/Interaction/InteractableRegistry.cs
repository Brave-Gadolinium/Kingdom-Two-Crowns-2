using System.Collections.Generic;

public static class InteractableRegistry
{
    private static readonly HashSet<IPlayerInteractable> Items = new HashSet<IPlayerInteractable>();

    public static IEnumerable<IPlayerInteractable> All => Items;

    public static void Register(IPlayerInteractable item)
    {
        if (item != null)
            Items.Add(item);
    }

    public static void Unregister(IPlayerInteractable item)
    {
        if (item != null)
            Items.Remove(item);
    }
}
