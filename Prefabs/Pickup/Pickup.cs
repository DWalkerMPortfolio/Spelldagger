using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class Pickup : Node2D, ILevelEditorElement, ITemporalControl
{
    readonly string[] TEMPORAL_PROPERTIES = { PropertyName.pickedUp };

    [Export] InventoryItem InventoryItem 
    { 
        get { return _inventoryItem; }
        set { _inventoryItem = value; InitializeItem(); }
    }
    InventoryItem _inventoryItem;
    [Export] float InteractionRadius;

    [ExportGroup("Internal")]
    [Export] public MeshInstance3D MeshInstance { get; private set; }
    [Export] Follow2DParent Root;
    [Export] Area3D Interactable;
    [Export] CollisionShape3D InteractionShape;

    bool pickedUp;
    Transform3D meshInstanceStartingTransform;

    public override void _Ready()
    {
        base._Ready();

        if (Engine.IsEditorHint())
        {
            GetParent()?.SetEditableInstance(this, true);
            SetDisplayFolded(true);
        }
        else
        {
            SphereShape3D sphereShape = new SphereShape3D();
            sphereShape.Radius = InteractionRadius;
            InteractionShape.Shape = sphereShape;
            ((Interactable)Interactable).Interacted += OnInteracted;

            meshInstanceStartingTransform = MeshInstance.Transform;
        }
    }

    public void SetFloor(LevelEditorFloor floor)
    {
        Root.SetFloor(floor.Floor);
    }

    public string[] GetTemporalProperties()
    {
        return TEMPORAL_PROPERTIES;
    }

    public void RestoreCustomTemporalState(Dictionary<string, Variant> customData)
    {
        //if (!InventoryItem.IsPermanent())
            UpdateMeshInstance();
    }

    void InitializeItem()
    {
        if (InventoryItem != null)
        {
            Name = InventoryItem.Name;
            if (MeshInstance != null)
                MeshInstance.Mesh = InventoryItem.Mesh;
        }
        else
        {
            Name = "Pickup";
            if (MeshInstance != null)
                MeshInstance.Mesh = new PlaceholderMesh();
        }
    }

    void UpdateMeshInstance()
    {
        if (pickedUp)
        {
            MeshInstance.Transparency = 1;
        }
        else
        {
            MeshInstance.Transform = meshInstanceStartingTransform;
            MeshInstance.Transparency = 0;
        }
    }

    // Called when the interactable is interacted with
    private void OnInteracted()
    {
        if (pickedUp)
            return;

        if (InventoryItem != null)
            InventoryItem.OnPickedUp(this);
        else
            OnPickupComplete();

        pickedUp = true;
    }

    // Called after the inventory item has finished its OnPickup interaction
    public void OnPickupComplete() { }
}
