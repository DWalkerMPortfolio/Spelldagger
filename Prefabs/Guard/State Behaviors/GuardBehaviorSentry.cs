using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class GuardBehaviorSentry : GuardStateBehavior
{
    [Export] bool MakeSounds = true;
    [Export] float SoundRadius;
    [Export] float SoundInterval;
    [Export] float Speed;
    [Export] float HighAlertSpeed;
    [Export] float TurnSpeed;
    [Export] float HighAlertTurnSpeed;
    [Export] float HighAlertLookInterval;
    [Export] float RepositionDistance;

    ulong lastSoundTick;
    Tween highAlertTween;
    bool sentryInPosition;
    Vector3 sentryForwardVector;
    Vector3 sentryPosition;

    public override void Initialize(GuardController controller)
    {
        base.Initialize(controller);

        sentryPosition = owner.Body.GlobalPosition;
        sentryForwardVector = -owner.Body.GlobalBasis.Z;
    }

    public override void RestoreCustomTemporalState(Dictionary<string, Variant> data)
    {
        sentryInPosition = false;
        highAlertTween?.Kill();
    }

    public override void EnterState(int previousState)
    {
        base.EnterState(previousState);

        // Create sound
        if (MakeSounds && !TemporalController.RestoringSnapshots)
        {
            SoundManager.Instance.CreateSound(owner, owner.Foot.GlobalPosition, SoundRadius, Sound.Messages.None);
            lastSoundTick = ScaledTime.TicksMsec;
        }

        owner.ClearNavigationPath();

        MoveToSentryPosition();
    }

    public override void ExitState(int nextState)
    {
        base.ExitState(nextState);

        highAlertTween?.Kill();
    }

    public override void PhysicsProcessState(double delta)
    {
        base.PhysicsProcessState(delta);

        // Create sound
        if (MakeSounds && ScaledTime.TicksMsec - lastSoundTick > SoundInterval * 1000)
        {
            SoundManager.Instance.CreateSound(owner, owner.Foot.GlobalPosition, SoundRadius, Sound.Messages.None);
            lastSoundTick = ScaledTime.TicksMsec;
        }

        if (!owner.IsNavigationFinished())
        {
            // Navigate back to sentry position
            if (owner.highAlert)
                owner.FollowPath(HighAlertSpeed, HighAlertTurnSpeed, delta);
            else
                owner.FollowPath(Speed, TurnSpeed, delta);
        }
        else if (!sentryInPosition)
        {
            // Turn to face sentry direction
            float turnSpeed = owner.highAlert ? HighAlertTurnSpeed : TurnSpeed;
            if (owner.RotateToFacePosition(owner.Body.GlobalPosition + sentryForwardVector, turnSpeed, delta))
            {
                sentryInPosition = true;
                owner.Body.Velocity *= Vector3.Up; // Zero out X and Z velocity

                if (owner.highAlert)
                {
                    // Initialize looping high alert tween
                    highAlertTween?.Kill();
                    highAlertTween = owner.CreateTween().SetLoops();
                    highAlertTween.TweenInterval(HighAlertLookInterval);
                    owner.CreateLookAroundTween(highAlertTween, HighAlertTurnSpeed, endingDelay: 0);
                }
            }
        }
        else
        {
            // Check if moved too far from sentry position and reposition
            if (owner.Body.GlobalPosition.DistanceSquaredTo(sentryPosition) > RepositionDistance * RepositionDistance)
            {
                highAlertTween?.Kill();
                MoveToSentryPosition();
            }
        }
    }

    void MoveToSentryPosition()
    {
        if (!owner.navigationServerInitialized)
            return;

        owner.CreateNavigationPath(sentryPosition);
        sentryInPosition = false;
    }
}
