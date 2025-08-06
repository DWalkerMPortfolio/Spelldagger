using Godot;
using System;

[Tool]
public partial class GuardBehaviorDebugPathfinding : GuardStateBehavior
{
    public override void PhysicsProcessState(double delta)
    {
        base.PhysicsProcessState(delta);

        if (owner.navigationServerInitialized)
        {
            if (owner.IsNavigationFinished())
                owner.CreateNavigationPath(Vector3.Zero);
            else
                owner.FollowPath(10, 360, delta);
        }
    }
}
