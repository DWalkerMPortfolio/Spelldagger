using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class Door : Node3D, ITemporalControl
{
    readonly string[] TEMPORAL_PROPERTIES =
    {
        PropertyName.Open,
    };

	[Export] Area3D InteractionZone;
    [Export] Area3D GuardDetector;
    [Export] Node3D DoorRoot;
    [Export] Node3D DisableWhenOpen;
    [Export] MeshInstance3D MeshInstance;
    [Export] public bool Open;

    [ExportGroup("Dynamic")]
    [Export] public KeyItem[] Keys;
    [Export] public bool openBackwards;
    
    bool interactionOpen; // The state the last interaction left this door in

    public override void _Ready()
    {
        base._Ready();

        if (Engine.IsEditorHint())
            return;

        ((Interactable)InteractionZone).Interacted += OnInteracted;
        GuardDetector.BodyEntered += OnGuardDetectorBodyEntered;
        GuardDetector.BodyExited += OnGuardDetectorBodyExited;
    }

    public string[] GetTemporalProperties()
    {
        return TEMPORAL_PROPERTIES;
    }

    public void RestoreCustomTemporalState(Dictionary<string, Variant> customData)
    {
        SetOpen(Open);
    }

    private void OnGuardDetectorBodyEntered(Node3D body)
    {
        SetOpen(true);
    }

    private void OnGuardDetectorBodyExited(Node3D body)
    {
        if (GuardDetector.GetOverlappingBodies().Count == 0)
        {
            SetOpen(interactionOpen);
        }
    }

    private void OnInteracted()
    {
        bool canOpen = true;
        if (Keys.Length > 0)
        {
            canOpen = false;
            foreach (KeyItem key in Keys)
            {
                if (PlayerController.Instance.Inventory.HasItem(key))
                {
                    canOpen = true;
                    break;
                }
            }
        }

        if (canOpen)
        {
            interactionOpen = !Open;
            SetOpen(interactionOpen);
        }
    }

    public void SetOpen(bool value)
    {
        Open = value;

        if (Open)
        {
            if (openBackwards)
                DoorRoot.RotationDegrees = new Vector3(0, -90, 0);
            else
                DoorRoot.RotationDegrees = new Vector3(0, 90, 0);

        }
        else
            DoorRoot.RotationDegrees = Vector3.Zero;
        
        DisableWhenOpen.Visible = !Open;
        DisableWhenOpen.ProcessMode = Open ? ProcessModeEnum.Disabled : ProcessModeEnum.Inherit;
    }

    public void SetMesh(Mesh mesh)
    {
        MeshInstance.Mesh = mesh;
    }
}
