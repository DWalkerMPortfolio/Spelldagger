using Godot;
using System;

[Tool]
public partial class GuardBehaviorPatrol : GuardStateBehavior
{
    enum PathEndBehaviors { Loop, Invert}

    readonly string[] TEMPORAL_PROPERTIES =
    {
        PropertyName.pathIndex,
        PropertyName.pathDirection,
    };

    [Export] PathEndBehaviors PathEndBehavior;
    [Export] float Speed;
    [Export] float HighAlertSpeed;
    [Export] float TurnSpeed;
    [Export] float HighAlertTurnSpeed;
    [Export] bool CreateSounds = true;
    [Export] float SoundRadius;
    [Export] float SoundInterval;

    ulong lastSoundTick;
    bool skipNextLookAround;
    Tween highAlertTween;
    int pathIndex = 1;
    int pathDirection = 1;

    public override string[] GetTemporalProperties()
    {
        return TEMPORAL_PROPERTIES;
    }

    public override void EnterState(int previousState)
    {
        base.EnterState(previousState);

        //GD.Print("Path index: " + pathIndex);
        
        // Create sound
        if (!TemporalController.RestoringSnapshots)
        {
            SoundManager.Instance.CreateSound(owner, owner.Foot.GlobalPosition, SoundRadius, Sound.Messages.None);
            lastSoundTick = ScaledTime.TicksMsec;
        }

        owner.ClearNavigationPath();

        if (owner.highAlert)
            skipNextLookAround = true;


        pathIndex-=pathDirection;
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
        if (CreateSounds && ScaledTime.TicksMsec - lastSoundTick > SoundInterval * 1000)
        {
            SoundManager.Instance.CreateSound(owner, owner.Foot.GlobalPosition, SoundRadius, Sound.Messages.None);
            lastSoundTick = ScaledTime.TicksMsec;
        }

        // Target the next point on the path
        if (owner.IsNavigationFinished())
        {
            //GD.Print("Navigation finished");

            if (!owner.navigationServerInitialized)
            {
                //GD.Print("Navigation server not initialized");
                return;
            }

            // Get next path point
            pathIndex += pathDirection;
            if (pathIndex >= owner.Editor.Points.Length)
            {
                if (PathEndBehavior == PathEndBehaviors.Loop)
                    pathIndex = 0;
                else if (PathEndBehavior == PathEndBehaviors.Invert)
                {
                    pathDirection = -1;
                    pathIndex = owner.Editor.Points.Length - 2;
                }
            }
            else if (pathIndex < 0)
            {
                pathDirection = 1;
                pathIndex = 1;
            }
            //GD.Print("Creating navigation path to: " + pathIndex);
            if (!owner.CreateNavigationPath(owner.GetPatrolPathPoint(pathIndex)))
                pathIndex -= pathDirection;

            // High alert tween
            if (owner.highAlert)
            {
                owner.Body.Velocity = Vector3.Zero;
                if (skipNextLookAround)
                    skipNextLookAround = false;
                else
                {
                    highAlertTween?.Kill();
                    highAlertTween = owner.CreateTween();
                    owner.CreateLookAroundTween(highAlertTween, HighAlertTurnSpeed);
                }
            }
        }

        if (highAlertTween == null || !highAlertTween.IsValid())
        {
            // Move along path
            if (owner.highAlert)
                owner.FollowPath(HighAlertSpeed, HighAlertTurnSpeed, delta);
            else
                owner.FollowPath(Speed, TurnSpeed, delta);
        }
    }
}
