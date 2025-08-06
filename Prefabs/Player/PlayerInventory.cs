using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerInventory : Node, ITemporalControl
{
    readonly string[] TEMPORAL_PROPERTIES = { PropertyName.temporaryItems };

    [Export] Godot.Collections.Array<InventoryItem> StartingItems;

    Godot.Collections.Array<InventoryItem> permanentItems = new Godot.Collections.Array<InventoryItem>();
    Godot.Collections.Array<InventoryItem> temporaryItems = new Godot.Collections.Array<InventoryItem>();

    public override void _Ready()
    {
        base._Ready();

        foreach (InventoryItem item in StartingItems)
        {
            AddItem(item);
        }
    }

    public void AddItem(InventoryItem item)
    {
        if (item.IsPermanent())
        {
            if (!item.IsUnique() || !permanentItems.Contains(item))
                permanentItems.Add(item);
        }
        else
        {
            if (!item.IsUnique() || !temporaryItems.Contains(item))
                temporaryItems.Add(item);
        }
    }

    public void RemoveItem(InventoryItem item)
    {
        if (item.IsPermanent())
            permanentItems.Remove(item);
        else
            temporaryItems.Remove(item);
    }

    public bool HasItem(InventoryItem item)
    {
        return permanentItems.Contains(item) || temporaryItems.Contains(item);
    }

    public string[] GetTemporalProperties()
    {
        return TEMPORAL_PROPERTIES;
    }
}
