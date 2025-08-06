using Godot;
using System;

[Tool]
public partial class GuardBehaviorAlertedMagicSensor : GuardStateBehavior
{
    [Export] float ShoutRadius;
    [Export] float ShoutInterval;

    ulong lastShoutTick;

    public override void EnterState(int previousState)
    {
        base.EnterState(previousState);

        owner.Body.Velocity = Vector3.Zero;
    }

    public override void PhysicsProcessState(double delta)
    {
        base.PhysicsProcessState(delta);

        // Shout
        if (ScaledTime.TicksMsec - lastShoutTick > ShoutInterval * 1000)
        {
            SoundManager.Instance.CreateSound(owner, owner.Foot.GlobalPosition, ShoutRadius, Sound.Messages.Alert, owner.Foot.GlobalPosition);
            lastShoutTick = ScaledTime.TicksMsec;
        }

        // Exit state
        if (owner.awareness < 1)
            owner.StateMachine.SwitchState((int)GuardController.States.Idle);
    }
}
