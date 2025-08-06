using Godot;
using System;

[Tool]
public partial class DocumentItem : InventoryItem
{
    [Export] public PackedScene Template { get; private set; }
    [Export(PropertyHint.MultilineText)] public string Text { get; private set; }

    Pickup pickup;

    public override void OnPickedUp(Pickup pickup)
    {
        this.pickup = pickup;
        Hud.Instance.DisplayDocument(this, OnDoneReading);
    }

    void OnDoneReading()
    {
        base.OnPickedUp(pickup);
    }
}
