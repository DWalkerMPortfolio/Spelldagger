using Godot;
using System;

public partial class FPSDisplay : Label
{

    public override void _Process(double delta)
    {
        base._Process(delta);

        Text = "FPS: " + Engine.GetFramesPerSecond().ToString();
    }
}
