using Godot;
using System;

public partial class ScaledTime : Node
{
    public static ulong TicksMsec { get; private set; } = 0;

    public override void _Ready()
    {
        base._Ready();

        ProcessPriority = -128;
        ProcessMode = ProcessModeEnum.Pausable;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        TicksMsec += (ulong)Mathf.FloorToInt(delta * 1000);
    }
}
