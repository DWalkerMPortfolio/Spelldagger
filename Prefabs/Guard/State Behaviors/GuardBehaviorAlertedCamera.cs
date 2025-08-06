using Godot;
using System;

[Tool]
public partial class GuardBehaviorAlertedCamera : GuardStateBehavior
{
    [Export] bool Shout = true;
    [Export] bool FacePlayer = true;
    [Export] float ShoutRadius;

    public override void EnterState(int previousState)
    {
        base.EnterState(previousState);

        owner.SetPerceptionVisibility(false);
        owner.updateAwareness = false;
        owner.SetHighAlert(true);

        if (!TemporalController.RestoringSnapshots)
        {
            if (Shout)
                SoundManager.Instance.CreateSound(owner, owner.Foot.GlobalPosition, ShoutRadius, Sound.Messages.Alert, targetPosition: PlayerController.Instance.GlobalPosition);
        }
    }

    public override void ExitState(int nextState)
    {
        base.ExitState(nextState);

        owner.SetPerceptionVisibility(true);
        owner.updateAwareness = true;
    }

    public override void PhysicsProcessState(double delta)
    {
        base.PhysicsProcessState(delta);

        if (!owner.IsPlayerInLineOfSight())
        {
            owner.StateMachine.SwitchState((int)GuardController.States.Idle);
        }
        else if (FacePlayer)
        {
            owner.LookAt(PlayerController.Instance.GlobalPosition);
        }
    }
}
