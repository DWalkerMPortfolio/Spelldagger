using Godot;
using System;

[Tool]
public partial class InventoryItem : Resource
{
    [Export] public string Name;
    [Export] public Mesh Mesh;
    
    public virtual void OnPickedUp(Pickup pickup)
    {
        PlayerController.Instance.Inventory.AddItem(this);

        Tween pickupTween = pickup.CreateTween();
        pickupTween.TweenProperty(pickup.MeshInstance, (string)MeshInstance3D.PropertyName.Scale, pickup.MeshInstance.Scale * 1.5f, 0.5f);
        pickupTween.Parallel().TweenProperty(pickup.MeshInstance, (string)MeshInstance3D.PropertyName.Transparency, 1, 0.5f);
        pickupTween.TweenCallback(Callable.From(pickup.OnPickupComplete));
    }

    public virtual bool IsPermanent()
    {
        return true;
    }

    public virtual bool IsUnique()
    {
        return true;
    }
}
