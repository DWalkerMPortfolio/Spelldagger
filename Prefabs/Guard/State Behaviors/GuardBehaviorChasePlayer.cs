using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class GuardBehaviorChasePlayer : GuardStateBehavior
{
    enum ChaseSubStates { None, PlayerVisible, MovingToLastSpottedPosition, FollowingLastSpottedDirection }

    readonly string[] TEMPORAL_PROPERTIES =
    {
        PropertyName.lastSpottedPlayerPosition,
        PropertyName.lastSpottedPlayerDirection,
        PropertyName.lastSpottedPlayerDirectionTarget,
        PropertyName.lastTargetedPlayerPosition
    };

    [Export] bool JustGetLineOfSight;
    [Export] float Speed;
    [Export] float TurnSpeed;
    [Export] float StartChaseSoundRadius;
    [Export] float MinMovingToLastSeenPositionTime;
    [Export] float ChaseStopRadius;
    [Export] float RecalculatePathDistance;
    [Export] float FollowLastSpottedPlayerDirectionDistance;

    StateMachine chaseStateMachine;
    Vector3 lastSpottedPlayerPosition;
    Vector3 lastSpottedPlayerDirection;
    Vector3 lastSpottedPlayerDirectionTarget;
    Vector3 lastTargetedPlayerPosition;
    ulong chaseStartedMovingToLastSeenPositionTick;

    public override void Initialize(GuardController controller)
    {
        base.Initialize(controller);

        // Initialize chase sub-states
        chaseStateMachine = new StateMachine();
        owner.StateMachine.AddChild(chaseStateMachine);

        chaseStateMachine.RegisterState((int)ChaseSubStates.None);
        chaseStateMachine.RegisterState((int)ChaseSubStates.PlayerVisible, enter: EnterChasingPlayerVisible, physicsProcess: PhysicsProcessChasingPlayerVisible);
        chaseStateMachine.RegisterState((int)ChaseSubStates.MovingToLastSpottedPosition, enter: EnterChasingMovingToLastSpottedPosition, physicsProcess: PhysicsProcessChasingMovingToLastSpottedPosition);
        chaseStateMachine.RegisterState((int)ChaseSubStates.FollowingLastSpottedDirection, enter: EnterChasingFollowingLastSpottedDirection, physicsProcess: PhysicsProcessChasingFollowingLastSpottedDirection);
        chaseStateMachine.SwitchState((int)ChaseSubStates.None);
    }

    public override string[] GetTemporalProperties()
    {
        return TEMPORAL_PROPERTIES;
    }

    public override Dictionary<string, Variant> SaveCustomTemporalState()
    {
        Dictionary<string, Variant> data = new Dictionary<string, Variant>();
        data.Add(StateMachine.PropertyName.CurrentState, chaseStateMachine.CurrentState);
        return data;
    }

    public override void RestoreCustomTemporalState(Dictionary<string, Variant> customData)
    {
        if (customData == null)
            return;

        chaseStateMachine.SwitchState((int)customData[StateMachine.PropertyName.CurrentState]);
    }

    public override void EnterState(int previousState)
    {
        owner.SetPerceptionVisibility(false);
        owner.updateAwareness = false;

        if (!TemporalController.RestoringSnapshots)
        {
            lastSpottedPlayerPosition = PlayerController.Instance.GlobalPosition;
            owner.CreateNavigationPath(lastSpottedPlayerPosition);

            owner.SetHighAlert(true);
            chaseStateMachine.SwitchState((int)ChaseSubStates.PlayerVisible);

            SoundManager.Instance.CreateSound(owner, owner.Foot.GlobalPosition, StartChaseSoundRadius, Sound.Messages.Alert, targetPosition: lastSpottedPlayerPosition, duration: 2, screenShakeAmplitude: 3, screenShakeDuration: 1);
        }
    }

    public override void ExitState(int nextState)
    {
        chaseStateMachine.SwitchState((int)ChaseSubStates.None);
        owner.updateAwareness = true;

        owner.SetPerceptionVisibility(true);
    }

    public override void PhysicsProcessState(double delta)
    {
        // Go back to chasing player if they enter line of sight
        if (chaseStateMachine.CurrentState != (int)ChaseSubStates.PlayerVisible)
        {
            if (owner.IsPlayerInLineOfSight())
            {
                if (chaseStateMachine.CurrentState != (int)ChaseSubStates.MovingToLastSpottedPosition
                    || ScaledTime.TicksMsec - chaseStartedMovingToLastSeenPositionTick > MinMovingToLastSeenPositionTime * 1000) // Prevent switching rapidly between chase states
                {
                    chaseStateMachine.SwitchState((int)ChaseSubStates.PlayerVisible);
                }
            }
        }
    }

    #region Player Visible Sub-State
    void EnterChasingPlayerVisible(int previousState)
    {
        owner.ClearNavigationPath();
    }

    void PhysicsProcessChasingPlayerVisible(double delta)
    {
        // Check if player is in line of sight
        if (owner.IsPlayerInLineOfSight())
        {
            lastSpottedPlayerPosition = PlayerController.Instance.GlobalPosition;
            lastSpottedPlayerDirection = PlayerController.Instance.Velocity.Normalized();
            
            if (JustGetLineOfSight)
            {
                owner.LookAt(lastSpottedPlayerPosition);
            }
            else
            {
                // Move
                if (owner.IsDirectPathToPlayer())
                {
                    // Move directly towards player
                    if (owner.Body.GlobalPosition.DistanceSquaredTo(PlayerController.Instance.GlobalPosition) > ChaseStopRadius * ChaseStopRadius)
                    {
                        Vector3 chaseVelocity = owner.Body.GlobalPosition.DirectionTo(PlayerController.Instance.GlobalPosition) with { Y = 0 } * Speed;
                        owner.Body.Velocity = chaseVelocity;
                        owner.Body.MoveAndSlide();
                        owner.LookAt(owner.Body.GlobalPosition + owner.Body.Velocity with { Y = 0 });
                        owner.Body.Velocity = Vector3.Zero;
                    }
                    else
                        owner.Body.Velocity = Vector3.Zero;
                }
                else
                {
                    // Follow path towards player
                    if (owner.IsNavigationFinished() || PlayerController.Instance.GlobalPosition.DistanceTo(lastTargetedPlayerPosition) > RecalculatePathDistance)
                    {
                        lastTargetedPlayerPosition = PlayerController.Instance.GlobalPosition;
                        owner.CreateNavigationPath(PlayerController.Instance.GlobalPosition);
                    }

                    owner.FollowPath(Speed, TurnSpeed, delta, false);
                }
            }
        }
        else
            chaseStateMachine.SwitchState((int)ChaseSubStates.MovingToLastSpottedPosition);
    }
    #endregion

    #region Moving To Last Spotted Position Sub-State
    void EnterChasingMovingToLastSpottedPosition(int previousState)
    {
        owner.CreateNavigationPath(lastSpottedPlayerPosition);

        if (!TemporalController.RestoringSnapshots)
            chaseStartedMovingToLastSeenPositionTick = ScaledTime.TicksMsec;
    }

    void PhysicsProcessChasingMovingToLastSpottedPosition(double delta)
    {
        if (!owner.IsNavigationFinished())
            owner.FollowPath(Speed, TurnSpeed, delta, false);
        else
            chaseStateMachine.SwitchState((int)ChaseSubStates.FollowingLastSpottedDirection);
    }
    #endregion

    #region Following Last Spotted Direction Sub-State
    void EnterChasingFollowingLastSpottedDirection(int previousState)
    {
        if (!TemporalController.RestoringSnapshots)
        {
            lastSpottedPlayerDirectionTarget = lastSpottedPlayerPosition + lastSpottedPlayerDirection * FollowLastSpottedPlayerDirectionDistance;
            float[] shapecastResult = owner.Shapecast(lastSpottedPlayerDirectionTarget);
            lastSpottedPlayerDirectionTarget = lastSpottedPlayerPosition + lastSpottedPlayerDirection * FollowLastSpottedPlayerDirectionDistance * shapecastResult[0];
        }
    }

    void PhysicsProcessChasingFollowingLastSpottedDirection(double delta)
    {
        if (owner.Body.GlobalPosition.DistanceTo(lastSpottedPlayerDirectionTarget) >= Speed * delta)
        {
            owner.Body.Velocity = owner.Body.GlobalPosition.DirectionTo(lastSpottedPlayerDirectionTarget) * Speed;
            owner.LookAt(owner.Body.GlobalPosition + owner.Body.Velocity);
            owner.Body.MoveAndSlide();
        }
        else
        {
            owner.StateMachine.SwitchState((int)GuardController.States.Idle);
        }
    }
    #endregion
}
