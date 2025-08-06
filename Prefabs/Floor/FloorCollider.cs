using Godot;
using System;

public partial class FloorCollider : AnimatableBody3D
{
    ProcessModeEnum initialProcessMode;

    public override void _EnterTree()
    {
        base._EnterTree();

        Pocketwatch.StartedRewind += OnStartedRewind;
        Pocketwatch.StoppedRewind += OnStoppedRewind;
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        Pocketwatch.StartedRewind -= OnStartedRewind;
        Pocketwatch.StoppedRewind -= OnStoppedRewind;
    }

    private void OnStartedRewind()
    {
        initialProcessMode = ProcessMode;
        ProcessMode = ProcessModeEnum.Disabled;
    }

    private void OnStoppedRewind()
    {
        ProcessMode = initialProcessMode;
    }
}
