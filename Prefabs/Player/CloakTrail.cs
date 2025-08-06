using Godot;
using System;

public partial class CloakTrail : MeshInstance3D
{
    const string TRAIL_OFFSET_PARAMETER = "trail_offset";

    [Export] Node3D TrailPosition;
    [Export] float SpringConstant;
    [Export] float SpringDamping;
    [Export] float TrailOffsetMaxDistance;

    Vector3 trailVelocity;

    public override void _Process(double delta)
    {
        base._Process(delta);

        trailVelocity += SpringConstant * (GlobalPosition - TrailPosition.GlobalPosition) * (float)delta; // Spring force
        trailVelocity -= SpringDamping * trailVelocity * (float)delta; // Damping
        TrailPosition.GlobalPosition += trailVelocity * (float)delta;

        Vector3 localTrailOffset = ToLocal(TrailPosition.GlobalPosition);
        localTrailOffset = localTrailOffset.Normalized() * Mathf.Min(TrailOffsetMaxDistance, localTrailOffset.Length());
        TrailPosition.GlobalPosition = ToGlobal(localTrailOffset);

        SetInstanceShaderParameter(TRAIL_OFFSET_PARAMETER, localTrailOffset);
    }
}
