using Godot;
using System;

public partial class FarsightVisionLight : OmniLight3D
{
    [Export] Node3D Parent;

    public override void _Process(double delta)
    {
        GlobalPosition = Parent.GlobalPosition;

        int floor = Mathf.FloorToInt(GlobalPosition.Y / Globals.FloorHeight);
        int playerFloor = Mathf.FloorToInt(PlayerController.Instance.GlobalPosition.Y / Globals.FloorHeight);
        if (floor != playerFloor)
            LightEnergy = 0;
        else
            LightEnergy = 1;
    }
}
