using Godot;
using System;

public partial class MainCamera : Camera3D
{
    public static MainCamera Instance { get; private set; }

    public delegate void FloorChangedDelegate(int newFloor);
    public FloorChangedDelegate FloorChanged;

    [Export] Node3D Root;
    [Export] ColorRect Fadeout;
    [Export] Node3D LightCaptureCamera;
    [Export] float FloorCheckHeightOffset;
    [Export] float Easing;
    [Export] float TransitionDuration;
    [Export] float FadeTransitionDistance;

    public int CurrentFloor { get; private set; }
    
    CameraTarget target;
    CameraTarget previousTarget;
    Vector3 position;
    Tween transitionTween;
    bool initialized;
    bool transitioning;

    public override void _EnterTree()
    {
        base._EnterTree();

        if (Instance == null)
            Instance = this;
        else
        {
            GD.PushWarning("Duplicate main camera in scene: " + Name);
            QueueFree();
        }
    }

    public override void _Ready()
    {
        base._Ready();

        TargetHighestPriority();
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        CallDeferred(MethodName.UnregisterSingleton);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        // Follow target
        if (target != null && !transitioning)
        {
            Root.GlobalPosition = target.GlobalPosition.Lerp(Root.GlobalPosition, Easing);
            
            // Update floor
            int newFloor = Mathf.FloorToInt((target.GlobalPosition.Y + FloorCheckHeightOffset) / Globals.FloorHeight);
            if (newFloor != CurrentFloor)
            {
                CurrentFloor = newFloor;
                FloorChanged?.Invoke(newFloor);
            }
        }

        LightCaptureCamera.GlobalPosition = GlobalPosition;
    }

    public void TargetActivationChanged(CameraTarget target)
    {
        if (this.target == target)
        {
            if (!target.Active)
                TargetHighestPriority();
        }
        else if (target.Active)
        {
            if (this.target == null || target.Priority > this.target.Priority)
                SwitchTarget(target);
        }
    }

    void TargetHighestPriority()
    {
        int highestPriority = -1000000000;
        CameraTarget highestPriorityTarget = null;
        foreach (CameraTarget target in CameraTarget.Instances)
        {
            if (target.Active && target.Priority > highestPriority)
            {
                highestPriority = target.Priority;
                highestPriorityTarget = target;
            }
        }

        if (highestPriorityTarget != null)
            SwitchTarget(highestPriorityTarget);
    }

    void SwitchTarget(CameraTarget newTarget)
    {
        if (!initialized || !newTarget.EaseTransition)
        {
            Root.GlobalPosition = newTarget.GlobalPosition;
            if (!initialized)
                CurrentFloor = Mathf.FloorToInt((newTarget.GlobalPosition.Y + FloorCheckHeightOffset) / Globals.FloorHeight);

            if (transitioning)
            {
                transitionTween?.Kill();
                transitioning = false;
            }

            initialized = true;
        }
        else
        {
            transitionTween?.Kill();
            transitionTween = CreateTween();
            Vector3 startingPosition = Root.GlobalPosition;
            
            if (Root.GlobalPosition.DistanceSquaredTo(newTarget.GlobalPosition) < FadeTransitionDistance * FadeTransitionDistance)
            {
                transitionTween.TweenMethod(Callable.From((float t) => {
                        Root.GlobalPosition = startingPosition.Lerp(target.GlobalPosition, t);
                        LightCaptureCamera.GlobalPosition = GlobalPosition;
                    }), 0.0, 1.0, TransitionDuration).
                    SetTrans(Tween.TransitionType.Sine);
                transitionTween.Parallel().TweenProperty(Fadeout, (string)ColorRect.PropertyName.Modulate, Colors.Transparent, TransitionDuration / 2);
            }
            else
            {
                transitionTween.TweenMethod(Callable.From((float t) => { 
                        Root.GlobalPosition = startingPosition.Lerp(startingPosition + startingPosition.DirectionTo(target.GlobalPosition) * FadeTransitionDistance / 2, t);
                        LightCaptureCamera.GlobalPosition = GlobalPosition; 
                    }), 0.0, 1.0, TransitionDuration / 2)
                    .SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Sine);
                transitionTween.Parallel().TweenProperty(Fadeout, (string)ColorRect.PropertyName.Modulate, Colors.Black, TransitionDuration / 2);
                transitionTween.TweenMethod(Callable.From((float t) => { 
                        Root.GlobalPosition = (target.GlobalPosition + target.GlobalPosition.DirectionTo(startingPosition) * FadeTransitionDistance / 2).Lerp(target.GlobalPosition, t);
                        LightCaptureCamera.GlobalPosition = GlobalPosition; 
                    }), 0.0, 1.0, TransitionDuration / 2)
                    .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
                transitionTween.Parallel().TweenProperty(Fadeout, (string)ColorRect.PropertyName.Modulate, Colors.Transparent, TransitionDuration / 2);
            }
            transitionTween.TweenCallback(Callable.From(() => { transitioning = false; }));

            transitioning = true;
        }

        previousTarget = target;
        target = newTarget;
    }

    void UnregisterSingleton()
    {
        if (Instance == this)
            Instance = null;
    }
}
