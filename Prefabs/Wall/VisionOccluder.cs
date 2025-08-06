using Godot;
using System;

public partial class VisionOccluder : StaticBody3D
{
    // TODO: Remove or uncomment

    [Export] CollisionShape3D CollisionShape;
    [Export] float HideDelay = 0.2f;

    int floor;

    /*
    public override void _Ready()
    {
        base._Ready();

        floor = Mathf.FloorToInt(GlobalPosition.Y / GlobalValues.FloorHeight);
        Visible = MainCamera.Instance.CurrentFloor == floor;
        CollisionShape.Disabled = !Visible;

        MainCamera.Instance.FloorChanged += OnCameraFloorChanged;
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        MainCamera.Instance.FloorChanged -= OnCameraFloorChanged;
    }

    async void OnCameraFloorChanged(int newFloor)
    {
        if (newFloor < floor)
        {
            if (!TemporalController.RestoringSnapshots)
                await ToSignal(GetTree().CreateTimer(HideDelay), Timer.SignalName.Timeout);
            Visible = false;
        }
        else
            Visible = true;

        CollisionShape.Disabled = !Visible;
    }*/
}
