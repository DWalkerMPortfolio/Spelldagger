using Godot;
using System;

public partial class Pocketwatch : Node
{
    const string REWIND_INPUT = "time_rewind";
    const string REWIND_BACK_INPUT = "move_left";
    const string REWIND_FORWARD_INPUT = "move_right";
    const string FAST_FORWARD_INPUT = "time_fast_forward";
    const string RELOAD_INPUT = "time_reload";
    const string CONFIRM_INPUT = "interact";

    const string FAST_FORWARD_INPUT_LOCK = "PocketwatchFastForwarding";

    public delegate void StartedRewindDelegate();
    public static event StartedRewindDelegate StartedRewind;
    public delegate void StoppedRewindDelegate();
    public static event StoppedRewindDelegate StoppedRewind;

    [Export] ClockOverlay ClockOverlay;
    [Export] Label RewindsRemainingLabel;
    [Export] float FastForwardAcceleration;
    [Export] float FastForwardMaxSpeed;
    [Export] float RewindMinSpeed;
    [Export] float RewindAcceleration;
    [Export] float RewindMaxSpeed;
    [Export] int MaxRewinds;

    bool rewinding = false;
    bool fastForwarding = false;
    double rewindAmount; // The amount currently rewinded
    int displayedSnapshotIndex; // The index of the currently displayed snapshot
    int rewindsRemaining;
    float rewindSpeed = 1;

    public override void _Ready()
    {
        base._Ready();

        StopRewind();
        StopFastForward();
        rewindsRemaining = MaxRewinds;
        RewindsRemainingLabel.Text = "Rewinds Remaining: " + rewindsRemaining;
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        StopRewind();
        StopFastForward();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (fastForwarding)
            FastForward(delta);
        else if (rewinding)
            Rewind(delta);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!rewinding && !fastForwarding)
        {
            if (@event.IsActionPressed(REWIND_INPUT))
                StartRewind();
            else if (@event.IsActionPressed(FAST_FORWARD_INPUT))
                StartFastForward();
        }
        else if (rewinding)
        {
            if (@event.IsActionPressed(REWIND_INPUT) || @event.IsActionPressed(CONFIRM_INPUT))
                StopRewind();
        }

        if (@event.IsActionPressed(RELOAD_INPUT))
            GetTree().ReloadCurrentScene();
    }

    #region Rewind
    public void StartRewind()
    {
        if (rewindsRemaining <= 0)
            return;

        rewinding = true;
        displayedSnapshotIndex = 0;
        rewindAmount = 0;
        rewindSpeed = RewindMinSpeed;
        ClockOverlay.FadeInOut(true);
        GetTree().Paused = true;
        TemporalController.RestoringSnapshots = true;
        StartedRewind?.Invoke();
    }

    void StopRewind()
    {
        if (!rewinding || PlayerController.Instance.IsDead())
            return;

        rewinding = false;
        ClockOverlay.FadeInOut(false);
        GetTree().Paused = false;
        TemporalController.RestoringSnapshots = false;

        // Clear all rewound snapshots
        TemporalController.ClearSnapshotsFromIndex(displayedSnapshotIndex);

        rewindsRemaining--;
        RewindsRemainingLabel.Text = "Rewinds Remaining: " + rewindsRemaining;
        StoppedRewind?.Invoke();
    }

    void Rewind(double delta)
    {
        if (Input.IsActionPressed(REWIND_FORWARD_INPUT))
        {
            rewindSpeed = Mathf.Min(RewindMaxSpeed, rewindSpeed + RewindAcceleration * (float)delta);
            rewindAmount -= delta * rewindSpeed;
        }
        else if (Input.IsActionPressed(REWIND_BACK_INPUT))
        {
            rewindSpeed = Mathf.Min(RewindMaxSpeed, rewindSpeed + RewindAcceleration * (float)delta);
            rewindAmount += delta * rewindSpeed;
        }
        else
            rewindSpeed = RewindMinSpeed;

        rewindAmount = Mathf.Clamp(rewindAmount, 0, TemporalController.SnapshotCount * TemporalController.SnapshotDelta);

        int targetSnapshotIndex = Mathf.FloorToInt(rewindAmount / TemporalController.SnapshotDelta);
        if (targetSnapshotIndex != displayedSnapshotIndex)
        {
            TemporalController.RestoreSnapshot(targetSnapshotIndex);
            ClockOverlay.RotateTo(displayedSnapshotIndex * 360 / TemporalController.MaxSnapshots, false);

            displayedSnapshotIndex = targetSnapshotIndex;
        }
    }
    #endregion

    #region Fast Forward
    void StartFastForward()
    {
        InputManager.Instance.AddInputLock(FAST_FORWARD_INPUT_LOCK);
        ClockOverlay.FadeInOut(true);
        fastForwarding = true;
    }
    
    public void StopFastForward()
    {
        if (!fastForwarding)
            return;

        Engine.TimeScale = 1;
        InputManager.Instance.RemoveInputLock(FAST_FORWARD_INPUT_LOCK);
        ClockOverlay.FadeInOut(false);
        fastForwarding = false;
    }

    void FastForward(double delta)
    {
        Engine.TimeScale = Mathf.Min(FastForwardMaxSpeed, Engine.TimeScale + FastForwardAcceleration * delta / Engine.TimeScale);
        ClockOverlay.Rotate(-(float)Engine.TimeScale);

        if (!Input.IsActionPressed(FAST_FORWARD_INPUT))
            StopFastForward();
    }
    #endregion
}
