using Godot;
using System;

[Tool]
public partial class KeyItem : InventoryItem
{
    public override bool IsPermanent()
    {
        return false;
    }
}
