using Godot;
using System;

[Tool]
public partial class GuardBehaviorInvestigate : GuardStateBehavior
{
    [Export] float Speed;
    [Export] float HighAlertSpeed;
    [Export] float TurnSpeed;
    [Export] float HighAlertTurnSpeed;
    [Export] float AllClearSoundRadius;
    [Export] float MinAwareness;

    Tween lookAroundTween;
    Vector3 startInvestigationPosition;
    float originalTargetDesiredDistance;

    public override void EnterState(int previousState)
    {
        owner.CreateNavigationPath(owner.investigationPosition);
        owner.minAwareness += MinAwareness;
        startInvestigationPosition = owner.Body.GlobalPosition;
    }
    
    public override void ExitState(int nextState)
    {
        lookAroundTween?.Kill();
        owner.minAwareness -= MinAwareness;
    }

    public override void PhysicsProcessState(double delta)
    {
        if (!owner.IsNavigationFinished())
        {
            if (owner.highAlert)
                owner.FollowPath(HighAlertSpeed, HighAlertTurnSpeed, delta, false);
            else
                owner.FollowPath(Speed, TurnSpeed, delta);
        }
        else
        {
            if (lookAroundTween == null || !lookAroundTween.IsValid())
            {
                // Reached point: shout all clear, look around, then return to patrolling
                if (startInvestigationPosition != owner.Body.GlobalPosition) // Only shout all clear if had to move
                    SoundManager.Instance.CreateSound(owner, owner.Foot.GlobalPosition, AllClearSoundRadius, Sound.Messages.AllClear);

                float turnSpeed = TurnSpeed;
                if (owner.highAlert)
                    turnSpeed = HighAlertTurnSpeed;

                lookAroundTween = owner.CreateTween();
                owner.CreateLookAroundTween(lookAroundTween, turnSpeed);
                lookAroundTween.TweenCallback(Callable.From(() => { owner.StateMachine.SwitchState((int)GuardController.States.Idle); }));
            }
        }
    }
}
