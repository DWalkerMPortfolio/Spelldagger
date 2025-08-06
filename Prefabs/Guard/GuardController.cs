using Godot;
using System;
using System.Collections.Generic;

public partial class GuardController : Node, ITemporalControl
{
    #region Variables
    readonly string[] TEMPORAL_PROPERTIES =
    {
        PropertyName.awareness,
        PropertyName.investigationPosition,
        PropertyName.investigationHighAlert,
        PropertyName.navigationPath,
        PropertyName.navigationPathPoint
    };

    public enum States { Idle, Investigating, Alerted, Damaged }

    public delegate void DamageSourcesRemovedDelegate();
    public DamageSourcesRemovedDelegate WeakpointDamageSourcesRemoved;

    [ExportGroup("References")]
    [Export] NodePath[] PerceptionSourcePaths;
    GuardPerception[] perceptionSources;
    [Export] NodePath[] WeakPointPaths;
    GuardWeakPoint[] weakPoints;
    [Export] NodePath[] WeaponPaths;
    GuardWeapon[] weapons;
    [Export] public GuardEditor Editor { get; private set; }
    [Export] public CharacterBody3D Body { get; private set; }
    [Export] public StateMachine StateMachine { get; private set; }
    [Export] public Node3D Foot { get; private set; }
    [Export] Node3D NavigationPathReferenceTransform;
    [Export] Label3D DebugLabel;

    [ExportGroup("Values")]
    [Export] float GravityScale;
    [Export] float HighAlertMinAwareness;
    [Export] float AwarenessDecayDelta;
    [Export] float DirectPathCheckingStep;
    [Export] float ShapecastRadius;
    [Export] float ShapecastHeight;
    [Export] float PathSimplificationEpsilon;
    [Export] bool Immovable;
    [Export] bool Debug;

    public float minAwareness;
    public bool updateAwareness = true;

    public bool navigationServerInitialized { get; private set; } = false;
    public float awareness { get; private set; }
    public bool highAlert { get; private set; }
    public Vector3 investigationPosition { get; private set; }
    public bool investigationHighAlert {  get; private set; }
    public bool perceptionVisible { get; private set; } = true;

    float gravity;
    CapsuleShape3D shapecastShape;
    int damagedWeakPoints;
    bool immovableDefault;
    Vector3[] navigationPath = null;
    int navigationPathPoint;
    NavigationRegion3D navigationRegion;
    #endregion

    #region Godot Functions
    public override void _Ready()
    {
        base._Ready();

        // Initialize node arrays
        perceptionSources = Globals.ConvertNodePathArray<GuardPerception>(this, PerceptionSourcePaths);
        weakPoints = Globals.ConvertNodePathArray<GuardWeakPoint>(this, WeakPointPaths);
        weapons = Globals.ConvertNodePathArray<GuardWeapon>(this, WeaponPaths);

        // Initialize debug
        if (Debug)
            DebugLabel.Visible = true;

        // Initialize default values for overrideable variables
        immovableDefault = Immovable;

        // Initialize states
        InitializeState(States.Idle, Editor.IdleBehavior);
        InitializeState(States.Investigating, Editor.InvestigatingBehavior);
        InitializeState(States.Alerted, Editor.AlertedBehavior);
        InitializeState(States.Damaged, Editor.DamagedBehavior);
        StateMachine.StateChanged += OnStateChanged;
        StateMachine.SwitchState((int)States.Idle);

        // Initialize perception sources
        foreach (GuardPerception perceptionSource in perceptionSources)
            perceptionSource.Initialize(this);

        // Initialize weak points
        foreach (GuardWeakPoint weakPoint in weakPoints)
            weakPoint.Initialize(this);

        // Initialize weapons
        foreach (GuardWeapon weapon in weapons)
            weapon.Initialize(this);

        // Wait for navigation server to initialize
        CallDeferred(MethodName.WaitForNavigationServerInitialization);

        // Initialize navigation region
        navigationRegion = Editor.Floor.LevelEditor.NavigationRegion;

        // Initialize gravity
        gravity = (float)ProjectSettings.GetSetting(Globals.GravitySetting);

        // Initialize shapecast shape
        shapecastShape = new CapsuleShape3D();
        shapecastShape.Radius = ShapecastRadius;
        shapecastShape.Height = ShapecastHeight;
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        StateMachine.StateChanged -= OnStateChanged;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (updateAwareness)
            UpdateAwareness(delta);
        
        if (Debug)
            UpdateDebug();
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        // Move
        if (!Immovable)
        {
            if (!Body.IsOnFloor())
                Body.Velocity = Body.Velocity with { Y = Body.Velocity.Y - gravity * GravityScale * (float)GetPhysicsProcessDeltaTime() };

        }
        else
            Body.Velocity = Vector3.Zero;
        
        Body.MoveAndSlide();
        Body.Velocity *= Vector3.Up;
    }
    #endregion

    #region Interface Functions
    public string[] GetTemporalProperties()
    {
        return TEMPORAL_PROPERTIES;
    }

    public Dictionary<string, Variant> SaveCustomTemporalState()
    {
        Dictionary<string, Variant> data = new Dictionary<string, Variant>();

        data.Add(Node3D.PropertyName.GlobalPosition, Body.GlobalPosition);
        data.Add(Node3D.PropertyName.GlobalRotation, Body.GlobalRotation);
        data.Add(StateMachine.PropertyName.CurrentState, StateMachine.CurrentState);
        data.Add(PropertyName.highAlert, highAlert);

        return data;
    }

    public void RestoreCustomTemporalState(Dictionary<string, Variant> data)
    {
        if (data == null)
            return;

        StateMachine.SwitchState((int)data[StateMachine.PropertyName.CurrentState]);

        SetHighAlert((bool)data[PropertyName.highAlert]);

        Body.GlobalPosition = (Vector3)data[Node3D.PropertyName.GlobalPosition];
        Body.GlobalRotation = (Vector3)data[Node3D.PropertyName.GlobalRotation];

        AwarenessUpdated();
    }

    public void OnWeakpointDamaged(GuardWeakPoint weakPoint)
    {
        damagedWeakPoints++;
        StateMachine.SwitchState((int)States.Damaged);
    }

    public void OnWeakpointDamageSourceRemoved(GuardWeakPoint weakPoint)
    {
        damagedWeakPoints--;
        if (damagedWeakPoints == 0)
            WeakpointDamageSourcesRemoved?.Invoke();
    }
    #endregion

    #region Public Functions
    public void SetPerceptionVisibility(bool value)
    {
        if (value == perceptionVisible)
            return;

        foreach (GuardPerception perceptionSource in perceptionSources)
            perceptionSource.SetVisibility(value);

        perceptionVisible = value;
    }

    public void OverrideImmovable(bool value)
    {
        Immovable = value;
    }

    public void ResetImmovable()
    {
        Immovable = immovableDefault;
    }

    public void InvestigatePosition(Vector3 position, bool highAlert)
    {
        if (highAlert)
            SetHighAlert(true);

        if (StateMachine.CurrentState != (int)States.Alerted && StateMachine.CurrentState != (int)States.Damaged)
        {
            investigationPosition = position;
            investigationHighAlert = highAlert;
            StateMachine.SwitchState((int)States.Investigating);
        }
    }

    public void AllClear()
    {
        if (StateMachine.CurrentState == (int)States.Investigating)
            StateMachine.SwitchState((int)States.Idle);
    }

    public void SetHighAlert(bool value)
    {
        if (highAlert == value)
            return;

        if (value)
            minAwareness += HighAlertMinAwareness;
        else
            minAwareness -= HighAlertMinAwareness;

        highAlert = value;
    }

    public Vector3 GetPatrolPathPoint(int index)
    {
        if (index >= Editor.Points.Length)
            GD.Print(Editor.Name + " tried to access path point (" + index + ") outside bounds of array (" + Editor.Points.Length + ")");

        Vector2 position2D = Editor.ToGlobal(Editor.Points[index]);
        return new Vector3(position2D.X / Globals.PixelsPerUnit, Editor.Height, position2D.Y / Globals.PixelsPerUnit);
    }

    public bool CreateNavigationPath(Vector3 destination)
    {
        navigationPath = NavigationServer3D.MapGetPath(Body.GetWorld3D().NavigationMap, Body.GlobalPosition, destination, true);
        navigationPath = NavigationServer3D.SimplifyPath(navigationPath, PathSimplificationEpsilon);

        if (navigationPath.Length < 2)
        {
            //GD.Print("Guard generated invalid path with length: " + navigationPath.Length);
            navigationPath = null;
            return false;
        }
        
        navigationPathPoint = 1;
        NavigationPathReferenceTransform.GlobalTransform = navigationRegion.GlobalTransform;
        //GD.Print("------------------New Path. Length: " + navigationPath.Length);
        return true;
    }

    public void ClearNavigationPath()
    {
        navigationPath = null;
    }

    public bool IsNavigationFinished()
    {
        return navigationPath == null;
    }

    public void FollowPath(float speed, float turnSpeed, double delta, bool pauseOnTurn = true)
    {
        if (navigationPath == null)
            return;

        //GD.Print("Navigating to path point: " + navigationPathPoint);

        // Account for sublevel moving since path was created
        Vector3 navigationPathPosition = TransformNavigationPosition(navigationPath[navigationPathPoint]);

        bool arrived = MoveToPosition(navigationPathPosition, speed, turnSpeed, delta, pauseOnTurn);
        if (arrived)
        {
            navigationPathPoint++;
            if (navigationPathPoint == navigationPath.Length)
                navigationPath = null;
        }
    }

    /// <summary>
    /// Move towards a target position at a specific speed
    /// </summary>
    /// <param name="position">The target position to move towards</param>
    /// <param name="speed">The speed to move</param>
    /// <param name="turnSpeed">The speed to turn</param>
    /// <param name="delta">Delta time</param>
    /// <param name="pauseOnTurn">Whether to stop moving when turning</param>
    /// <returns>Whether the target position was reached</returns>
    public bool MoveToPosition(Vector3 position, float speed, float turnSpeed, double delta, bool pauseOnTurn = true)
    {
        position = position with { Y = Body.GlobalPosition.Y };

        bool rotated = RotateToFacePosition(position, turnSpeed, delta);
        if (rotated || !pauseOnTurn)
        {
            if (Body.GlobalPosition.DistanceSquaredTo(position) <= (speed * delta) * (speed * delta))
            {
                Body.GlobalPosition = position;
                Body.Velocity *= Vector3.Up;
                return true;
            }
            else
                Body.Velocity = Body.GlobalPosition.DirectionTo(position) * speed;
        }
        else
        {
            Body.Velocity *= Vector3.Up; // Zero out X and Z velocity
        }
        return false;
    }

    /// <summary>
    /// Turn to face a target position at a specific speed
    /// </summary>
    /// <param name="targetPosition">The global position to face</param>
    /// <param name="turnSpeed">The degrees to turn per second</param>
    /// <param name="delta">Delta time</param>
    /// <returns>Whether the rotation has completed</returns>
    public bool RotateToFacePosition(Vector3 targetPosition, float turnSpeed, double delta)
    {
        Vector3 targetDirection = Body.GlobalPosition.DirectionTo(targetPosition) with { Y = 0 };
        Vector3 currentDirection = -Body.GlobalBasis.Z;
        float angleDifference = currentDirection.SignedAngleTo(targetDirection, Vector3.Up);
        float turnSpeedRadians = Mathf.DegToRad(turnSpeed);
        if (Mathf.Abs(angleDifference) > turnSpeedRadians * delta)
        {
            Body.RotateY(turnSpeedRadians * Mathf.Sign(angleDifference) * (float)delta);
            return false;
        }
        else
        {
            Body.RotateY(angleDifference);
            return true;
        }
    }

    public void CreateLookAroundTween(Tween tween, float turnSpeed, float endingDelay = 0.5f)
    {
        // Full 360 degree turn
        Vector3 startingRotation = Body.GlobalRotation;
        tween.TweenMethod(Callable.From((float value) => { Body.RotateY(Mathf.DegToRad(turnSpeed * (float)GetProcessDeltaTime())); }),
            0, 0, 360 / turnSpeed);
        tween.TweenProperty(Body, (string)Node3D.PropertyName.GlobalRotation, startingRotation, 0);

        // Look back and forth
        /*tween.TweenMethod(Callable.From((float value) => { RotateY(Mathf.DegToRad(PatrolHighAlertTurnSpeed * (float)GetProcessDeltaTime())); }),
            0, 0, PatrolHighAlertTurnAngle / PatrolHighAlertTurnSpeed);
        tween.TweenMethod(Callable.From((float value) => { RotateY(Mathf.DegToRad(-PatrolHighAlertTurnSpeed * (float)GetProcessDeltaTime())); }),
            0, 0, PatrolHighAlertTurnAngle * 2 / PatrolHighAlertTurnSpeed);
        tween.TweenMethod(Callable.From((float value) => { RotateY(Mathf.DegToRad(PatrolHighAlertTurnSpeed * (float)GetProcessDeltaTime())); }),
            0, 0, PatrolHighAlertTurnAngle / PatrolHighAlertTurnSpeed);*/

        tween.TweenInterval(endingDelay);
    }

    public void LookAt(Vector3 position)
    {
        Body.LookAt(position with { Y = Body.GlobalPosition.Y });
    }

    public bool IsPlayerInLineOfSight()
    {
        if (PlayerController.Instance.IsDead())
            return false;

        foreach (GuardPerception perceptionSource in perceptionSources)
        {
            if (perceptionSource.IsPlayerInLineOfSight())
                return true;
        }
        return false;
    }

    public bool IsDirectPathToPlayer()
    {
        Vector3 testPosition = Foot.GlobalPosition + Vector3.Up * 0.25f;
        Vector3 targetPosition = PlayerController.Instance.Foot.GlobalPosition + Vector3.Up * 0.25f;
        while (testPosition.DistanceSquaredTo(targetPosition) > DirectPathCheckingStep * DirectPathCheckingStep)
        {
            if (testPosition.DistanceSquaredTo(NavigationServer3D.MapGetClosestPoint(Body.GetWorld3D().NavigationMap, testPosition)) > DirectPathCheckingStep * DirectPathCheckingStep)
            {
                return false;
            }
            testPosition = testPosition.MoveToward(targetPosition, DirectPathCheckingStep / 2);
        }
        return true;
    }

    public float[] Shapecast(Vector3 targetPosition)
    {
        PhysicsDirectSpaceState3D spaceState = Body.GetWorld3D().DirectSpaceState;
        PhysicsShapeQueryParameters3D query = new PhysicsShapeQueryParameters3D();
        query.Transform = Body.GlobalTransform;
        query.Shape = shapecastShape;
        query.Motion = targetPosition - Body.GlobalPosition;
        query.CollisionMask = 1;
        return spaceState.CastMotion(query);
    }
    #endregion

    #region Private Functions
    void InitializeState(States state, GuardStateBehavior behavior)
    {
        if (behavior != null)
        {
            behavior.Initialize(this);
            StateMachine.RegisterState((int)state, behavior.EnterState, behavior.ExitState, behavior.ProcessState, behavior.PhysicsProcessState);

            // Initialize a temporal controller for the behavior
            TemporalController behaviorTemporalController = new TemporalController();
            AddChild(behaviorTemporalController);
            behaviorTemporalController.Initialize(behavior);
        }
        else
            StateMachine.RegisterState((int)state);
    }

    void OnStateChanged(int newState, int previousState)
    {
        // Power up and down weapons
        if (newState == (int)States.Alerted)
        {
            foreach (GuardWeapon weapon in weapons)
                weapon.EnteredAlert(previousState);
        }
        else if (previousState == (int)States.Alerted)
        {
            foreach (GuardWeapon weapon in weapons)
                weapon.ExitedAlert(newState);
        }
    }

    async void WaitForNavigationServerInitialization()
    {
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
        navigationServerInitialized = true;
    }

    /// <summary>
    /// Transforms a navigation position to account for moving platforms
    /// </summary>
    /// <param name="position">The position to transform</param>
    /// <returns>The position, transformed</returns>
    Vector3 TransformNavigationPosition(Vector3 position)
    {
        position = NavigationPathReferenceTransform.ToLocal(position);
        position = navigationRegion.ToGlobal(position);
        return position;
    }

    void UpdateAwareness(double delta)
    {
        // Awareness delta
        float awarenessDelta = 0;
        foreach (GuardPerception perceptionSource in perceptionSources)
        {
            awarenessDelta = Mathf.Max(awarenessDelta, perceptionSource.UpdateAwareness());
        }
        awareness += awarenessDelta * (float)delta;

        // Clamp awareness
        if (awarenessDelta == 0)
            awareness = (float)Mathf.MoveToward(awareness, minAwareness, delta);
        awareness = Mathf.Clamp(awareness, 0, 1);

        // If awareness reaches 1, enter alerted state
        if (awareness == 1)
            StateMachine.SwitchState((int)States.Alerted);

        AwarenessUpdated();
    }

    void UpdateDebug()
    {
        DebugLabel.Text = ((States)StateMachine.CurrentState).ToString();
        DebugLabel.GlobalRotation = DebugLabel.GlobalRotation with { Y = MainCamera.Instance.GlobalRotation.Y };

        if (navigationPath != null)
        {
            for (int i=0; i<navigationPath.Length - 1; i++)
            {
                DebugDraw3D.DrawSphere(TransformNavigationPosition(navigationPath[i]), 0.25f, Colors.Blue);
            }
            DebugDraw3D.DrawSphere(TransformNavigationPosition(navigationPath[navigationPath.Length - 1]), 0.25f, Colors.Green);
        }
    }

    void AwarenessUpdated()
    {
        // Update perception source visuals to new awareness level
        foreach (GuardPerception perceptionSource in perceptionSources)
        {
            perceptionSource.AwarenessUpdated();
        }
    }
    #endregion
}
