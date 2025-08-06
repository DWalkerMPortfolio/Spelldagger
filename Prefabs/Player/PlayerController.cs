using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerController : CharacterBody3D, ITemporalControl, IDamageable
{
	public static PlayerController Instance;

	enum States { Walking, Dashing, Dead }

	readonly string[] MOVE_INPUTS = { "move_left", "move_right", "move_up", "move_down" };
	const string INTERACTION_INPUT = "interact";
	const string SPRINT_INPUT = "sprint";
	const string WALK_ANIMATION = "Run";
	const string IDLE_ANIMATION = "Idle";
	readonly string[] TEMPORAL_PROPERTIES = { PropertyName.GlobalPosition, PropertyName.GlobalRotation, PropertyName.Velocity, PropertyName.dashDirection, PropertyName.dashStartPosition };

	[Export] public Node3D Foot { get; private set; }
	[Export] public LightDetector LightDetector { get; private set; }
	[Export] public PlayerInventory Inventory { get; private set; }
	[Export] public DaggerHolder DaggerHolder { get; private set; }
	[Export] StateMachine StateMachine;
	[Export] AnimationPlayer AnimationPlayer;
	[Export] MeshInstance3D MeshInstance;
	[Export] RemoteTransform3D RemoteTransform;
	[Export] Pocketwatch Pocketwatch;
	[Export] Interactor Interactor;
	[Export] float TopSpeed;
	[Export] float SprintTopSpeed;
	[Export] float Acceleration;
	[Export] float Deceleration;
	[Export] float GravityScale = 1;
	[Export] float SprintSoundInterval;
	[Export] float SprintSoundRadius;
	[Export] float DashDistance;
	[Export] float DashSpeed;
	[Export] float DashCooldown;
	[Export] float AnimationBlendTime;
	[Export] float RotationSpeed;
	[Export] float PushForce;

	float gravity;
	float currentSpeed;
	Vector3 currentDirection;
	ulong lastSprintSoundTick;
	Vector3 dashDirection;
	Vector3 dashStartPosition;
	ulong lastDashTick;
	Vector3 preCollisionVelocity;

	#region Godot Functions
	public override void _Ready()
	{
		base._Ready();

		// Register singleton
		if (Instance == null)
			Instance = this;
		else
		{
			GD.PushError("Duplicate player in scene: " + Name);
			QueueFree();
		}

		// Initialize gravity
		gravity = (float)ProjectSettings.GetSetting(Globals.GravitySetting);

		// Register states
		StateMachine.RegisterState((int)States.Walking, physicsProcess: PhysicsProcessWalking);
		StateMachine.RegisterState((int)States.Dashing, enter: EnterDashing, exit: ExitDashing, physicsProcess: PhysicsProcessDashing);
		StateMachine.RegisterState((int)States.Dead, enter: EnterDead, exit: ExitDead);

		StateMachine.SwitchState((int)States.Walking);
	}

	public override void _ExitTree()
	{
		base._ExitTree();

		if (Instance == this)
			Instance = null;
	}

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

		if (!InputManager.Instance.IsInputUnlocked())
			return;

		if (@event.IsActionPressed(INTERACTION_INPUT))
			Interactor.Interact();
		else if (@event.IsActionPressed(SPRINT_INPUT))
		{
            if (StateMachine.CurrentState == (int)States.Walking && ScaledTime.TicksMsec - lastDashTick > DashCooldown * 1000)
			{
				StateMachine.SwitchState((int)States.Dashing);
			}
		}
    }

    public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		// Add gravity
		if (!IsOnFloor() && StateMachine.CurrentState != (int)States.Dashing)
			SetVelocity(Velocity with { Y = Velocity.Y - gravity * GravityScale * (float)delta });
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
		data.Add(StateMachine.PropertyName.CurrentState, StateMachine.CurrentState);
		return data;
	}

	public void RestoreCustomTemporalState(Dictionary<string, Variant> data)
	{
		if (data == null)
			return;

		int dataState = (int)data[StateMachine.PropertyName.CurrentState];
		StateMachine.SwitchState(dataState);

		currentSpeed = 0;
		RemoteTransform.ForceUpdateTransform();
	}

	public void TakeDamage(IDamageable.Teams team, Node3D source)
	{
		if (team != IDamageable.Teams.Player)
			StateMachine.SwitchState((int)States.Dead);
	}
	#endregion

	#region Public Functions
	public bool IsDead()
	{
		return StateMachine.CurrentState == (int)States.Dead;
	}
    #endregion

    #region Private Functions
	private new void SetVelocity(Vector3 velocity)
	{
		Velocity = velocity;
		preCollisionVelocity = velocity;
	}

    // From: https://kidscancode.org/godot_recipes/4.x/physics/character_vs_rigid/index.html
    private void PushRigidbodies()
	{
		for (int i=0; i<GetSlideCollisionCount(); i++)
		{
			KinematicCollision3D collision = GetSlideCollision(i);
			RigidBody3D collisionRigidbody = collision.GetCollider() as RigidBody3D;
			if (collisionRigidbody != null)
				collisionRigidbody.ApplyCentralImpulse(preCollisionVelocity.Project(collision.GetNormal()) * PushForce);
		}
	}
    #endregion

    #region Walking State
    void PhysicsProcessWalking(double delta)
	{
		// Get the input direction and handle movement
		Vector2 inputVector = Input.GetVector(MOVE_INPUTS[0], MOVE_INPUTS[1], MOVE_INPUTS[2], MOVE_INPUTS[3]);
		if (!inputVector.IsZeroApprox() && InputManager.Instance.IsInputUnlocked())
		{
			currentDirection = new Vector3(inputVector.X, 0, inputVector.Y).Normalized();

			bool sprinting = Input.IsActionPressed(SPRINT_INPUT);

            float currentTopSpeed = TopSpeed;
			if (sprinting)
				currentTopSpeed = SprintTopSpeed;

			float speedDelta = Acceleration;
			if (currentSpeed > currentTopSpeed)
				speedDelta = Deceleration;

			currentSpeed = (float)Mathf.MoveToward(currentSpeed, currentTopSpeed, speedDelta * delta);

            // Sprint sound
			if (sprinting)
			{
				if (ScaledTime.TicksMsec - lastSprintSoundTick >= SprintSoundInterval * 1000)
				{
					SoundManager.Instance.CreateSound(this, Foot.GlobalPosition, SprintSoundRadius, Sound.Messages.Investigate, duration: 0.5f, opacity: 0.5f);
					lastSprintSoundTick = ScaledTime.TicksMsec;
				}
			}

			// Animation
			AnimationPlayer?.Play(WALK_ANIMATION, AnimationBlendTime);
        }
		else
		{
			currentSpeed = (float)Mathf.MoveToward(currentSpeed, 0, Deceleration * delta);

			// Animation
			AnimationPlayer?.Play(IDLE_ANIMATION, AnimationBlendTime);
		}

		// Turn to face current direction
		float angleToDirection = (-GlobalBasis.Z).SignedAngleTo(currentDirection, Vector3.Up);
		GlobalRotate(Vector3.Up, Mathf.Min(Mathf.Abs(angleToDirection), Mathf.DegToRad(RotationSpeed) * (float)delta) * Mathf.Sign(angleToDirection));
        //LookAt(GlobalPosition + currentDirection); // Face current direction

        SetVelocity(Velocity with { X = currentDirection.X * currentSpeed, Z = currentDirection.Z * currentSpeed });
		MoveAndSlide();
		PushRigidbodies();
		RemoteTransform.ForceUpdateTransform();
	}
    #endregion

    #region Dashing State
	void EnterDashing(int previousState)
	{
		dashDirection = currentDirection;
		dashStartPosition = GlobalPosition;
	}

	void PhysicsProcessDashing(double delta)
	{
		float dashDistance = ((GlobalPosition - dashStartPosition) * new Vector3(1, 0, 1)).Length();
		if (dashDistance > DashDistance)
		{
			StateMachine.SwitchState((int)States.Walking);
			return;
		}

		SetVelocity(dashDirection * DashSpeed);

		if (MoveAndSlide())
		{
			StateMachine.SwitchState((int)States.Walking);
			PushRigidbodies();
		}
        
		RemoteTransform.ForceUpdateTransform();
		DaggerHolder.ForceUpdateDaggers();
    }

	void ExitDashing(int nextState)
	{
		currentSpeed = SprintTopSpeed;
		lastSprintSoundTick = ScaledTime.TicksMsec;
		lastDashTick = ScaledTime.TicksMsec;
	}
    #endregion

    #region Dead State
    async void EnterDead(int previousState)
	{
		if (MeshInstance != null)
			MeshInstance.Visible = false;

		if (!TemporalController.RestoringSnapshots)
		{
			Pocketwatch.StopFastForward();
			await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
			Pocketwatch.StartRewind();
		}
	}

	void ExitDead(int nextState)
	{
		if (MeshInstance != null)
			MeshInstance.Visible = true;
	}
    #endregion
}
