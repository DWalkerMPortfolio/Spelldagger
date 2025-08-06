using Godot;
using System;

[Tool]
public partial class GuardBehaviorIdleCamera : GuardStateBehavior
{
    [Export] float TurnSpeed;

    Vector3 idleForwardDirection;

    public override void Initialize(GuardController controller)
    {
        base.Initialize(controller);

        idleForwardDirection = -owner.Body.GlobalBasis.Z;
    }

    public override void ProcessState(double delta)
    {
        base.ProcessState(delta);

        owner.RotateToFacePosition(owner.Body.GlobalPosition + idleForwardDirection, TurnSpeed, delta);
    }
}
