using Godot;
using System;
using System.Collections.Generic;

public partial class Dagger : RigidBody3D, ITemporalControl
{
	const string LEFT_INPUT = "dagger_left";
	const string RIGHT_INPUT = "dagger_right";
	const string RECALL_INPUT = "dagger_recall";
	const string WARP_INPUT = "rune_warp";
	const string DISTRACTION_INPUT = "rune_distraction";

	readonly string[] TEMPORAL_PROPERTIES = { 
		PropertyName.GlobalPosition,
		PropertyName.GlobalRotation,
		PropertyName.LinearVelocity,
		PropertyName.throwStartPositon,
		PropertyName.stuckBody,
		PropertyName.stuckOffset,
		PropertyName.stuckRotationOffset,
	};

    enum Chiralities { Left, Right };
	public enum States { Held, Thrown, Stuck, Fallen}

	static Chiralities chiralityTargetedForRetrieval;

	[Export] Chiralities Chirality;
	[Export] Node3D Slot;
	[Export] PlayerController PlayerBody;
	
	[ExportGroup("Internal")]
	[Export] public StateMachine StateMachine { get; private set; } // The state machine node used to control the dagger's state
	[Export] MeshInstance3D MeshInstance; // The mesh instance node holding the dagger model
	[Export] Node3D MeshRoot; // The root node of the dagger's mesh, used for tweening
	[Export] HazardVolume HazardVolume; // The dagger's hazard volume, activated when throwing
	[Export] GpuParticles3D ThrowParticles; // The particles used for the throw effect
	[Export] GpuParticles3D ReturnParticles; // The particles to play when teleporting the dagger back
	[Export] PackedScene TrailPrefab; // The trail packed scene to instantiate
	[Export] Node3D TrailRoot; // The node to parent the trail to
	[Export] OmniLight3D FarsightVisionLight;
	[Export] CameraTarget CameraTarget;
	[Export] RuneItem FarsightRuneItem;
	[Export] RuneItem WarpRuneItem;
	[Export] RuneItem DistractionRuneItem;
	[Export] float ThrowSpeed; // How fast the dagger travels when thrown
	[Export] float ThrowDistance; // Distance before the dagger drops to the ground
	[Export] float StuckImpaleDistance; // The distance to impale the dagger into the object it is stuck in
	[Export] float FallenLinearDamp; // The linear damping to apply while the dagger is in the fallen state
	[Export] float FallenGravity; // The gravity scale to apply while the dagger is in the fallen state
	[Export] float FallenAngularVelocityStrength; // The maximum magnitude of the random angular velocity applied when entering the fallen state
	[Export] float FarsightVisionRange;
	[Export] float WarpSoundRadius;
	[Export] float DistractionSoundRadius;
	[Export] float DistractionSoundDuration;
	[Export] float DistractionCooldown;

	public string inputAction { get; private set; } // The input action this dagger responds to
	bool startedThrow;
	bool focused = false;
	ulong lastDistractionTick;
	Vector3 throwStartPositon; // The starting position of the last dagger throw
	Node3D stuckBody; // The body the dagger is stuck in
	Vector3 stuckOffset; // Where the dagger is stuck in an object as an offset in the object's local space
	Vector3 stuckRotationOffset; // The rotation of the dagger when stuck in an object as an offset in the object's local space
	Tween currentTween; // The currently playing tween (if any)
	Tween farsightVisionLightTween;
	Node3D trail; // The instantiated trail (if any)
	Vector3 collisionPoint; // The current point of collision (if any) with another object

    #region Godot Functions
    public override void _Ready()
    {
		// Initialize chirality
		if (Chirality == Chiralities.Left)
		{
			inputAction = LEFT_INPUT;
			MeshInstance.Scale *= new Vector3(-1, 1, 1);
		}
		else
			inputAction = RIGHT_INPUT;

		BodyEntered += OnBodyEntered;
		
		StateMachine.RegisterState((int)States.Held, enter: EnterHeld, exit: ExitHeld, physicsProcess: PhysicsProcessHeld);
		StateMachine.RegisterState((int)States.Thrown, enter: EnterThrown, exit: ExitThrown, physicsProcess: PhysicsProcessThrown);
		StateMachine.RegisterState((int)States.Stuck, enter: EnterStuck, exit: ExitStuck, physicsProcess: PhysicsProcessStuck);
		StateMachine.RegisterState((int)States.Fallen, enter: EnterFallen, exit: ExitFallen);

		StateMachine.SwitchState((int)States.Held);
    }

    public override void _IntegrateForces(PhysicsDirectBodyState3D state)
    {
        base._IntegrateForces(state);

		if (state.GetContactCount() > 0)
			collisionPoint = state.GetContactLocalPosition(0);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
		if (InputManager.Instance.IsInputUnlocked())
		{
			if (StateMachine.CurrentState == (int)States.Held)
			{
				if (@event.IsActionPressed(inputAction))
                    StateMachine.SwitchState((int)States.Thrown);
            }
			else
			{
				if (@event.IsActionPressed(RECALL_INPUT))
				{
					if (chiralityTargetedForRetrieval == Chirality && StateMachine.CurrentState != (int)States.Held)
					{
						StateMachine.SwitchState((int)States.Held);
						GetViewport().SetInputAsHandled();
					}
				}
				else if (StateMachine.CurrentState == (int)States.Stuck || StateMachine.CurrentState == (int)States.Fallen)
				{
					if (Chirality == chiralityTargetedForRetrieval)
					{
						if (@event.IsActionPressed(WARP_INPUT))
						{
							WarpRune();
							GetViewport().SetInputAsHandled();
						}
						else if (@event.IsActionPressed(DISTRACTION_INPUT))
						{
							SoundRune();
							GetViewport().SetInputAsHandled();
						}
					}
				}
			}
		}
    }

    private void OnBodyEntered(Node body)
    {
		// Get the SDPhysicsMaterial of the collided object
		PhysicsMaterial physicsMaterial = null;
		if (body is StaticBody3D)
			physicsMaterial = ((StaticBody3D)body).PhysicsMaterialOverride;
		SDPhysicsMaterial collidedMaterial = null;
		if (physicsMaterial is SDPhysicsMaterial)
			collidedMaterial = (SDPhysicsMaterial)physicsMaterial;

        if (StateMachine.CurrentState == (int)States.Thrown)
		{
			if (collidedMaterial != null)
			{
				if (collidedMaterial.DaggerHitSoundRadius > 0)
					SoundManager.Instance.CreateSound(this, collisionPoint, collidedMaterial.DaggerHitSoundRadius, Sound.Messages.Investigate, duration: 1, screenShakeAmplitude: 1.25f, screenShakeDuration: 0.2f);

				if (collidedMaterial.DaggersStick)
				{
					stuckBody = (Node3D)body;
					StateMachine.SwitchState((int)States.Stuck);
				}
				else
					StateMachine.SwitchState((int)States.Fallen);
			}
			else
			{
                stuckBody = (Node3D)body;
                StateMachine.SwitchState((int)States.Stuck);
			}
		}
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
		data.Add(nameof(chiralityTargetedForRetrieval), (int)chiralityTargetedForRetrieval);
		return data;
    }

	public void RestoreCustomTemporalState(Dictionary<string, Variant> data)
	{
        if (data == null)
            return;

		int dataState = (int)data[StateMachine.PropertyName.CurrentState];
		if (dataState == (int)States.Held)
		{
			GlobalPosition = Slot.GlobalPosition;
			GlobalRotation = Slot.GlobalRotation;
		}

		chiralityTargetedForRetrieval = (Chiralities)(int)data[nameof(chiralityTargetedForRetrieval)];

		StateMachine.SwitchState(dataState);
    }
    #endregion

    #region Public Functions
    public void SetFocused(bool value)
    {
        if (focused == value)
            return;

        focused = value;

        if (!value || PlayerController.Instance.Inventory.HasItem(FarsightRuneItem))
            CameraTarget.SetActive(focused);

        if (focused)
            chiralityTargetedForRetrieval = Chirality;
    }
    #endregion

    #region Held State
    void EnterHeld(int previousState)
	{
        Freeze = true;
		if (Chirality == Chiralities.Left)
			chiralityTargetedForRetrieval = Chiralities.Right;
		else
			chiralityTargetedForRetrieval = Chiralities.Left;

		if (!TemporalController.RestoringSnapshots)
		{
			// Tween
			NewTween();
			currentTween.TweenProperty(MeshRoot, "scale", Vector3.One * 1.5f, 0.1f);
			currentTween.TweenProperty(MeshRoot, "scale", Vector3.One, 0.1f);

			// VFX
			ReturnParticles.Restart();
		}
	}

	void ExitHeld(int nextState)
	{
		// Reset rotation
		MeshRoot.Rotation = Vector3.Zero;
	}

	void PhysicsProcessHeld(double delta)
	{
		if (!PlayerBody.IsDead())
		{
			// Stay in slot
			GlobalPosition = Slot.GlobalPosition;
			GlobalRotation = Slot.GlobalRotation;
		}
		else
			StateMachine.SwitchState((int)States.Fallen);
	}
    #endregion

    #region Thrown State
    void EnterThrown(int previousState)
	{
		startedThrow = false;
		HazardVolume.Active = true;
		chiralityTargetedForRetrieval = Chirality;
    }

	async void ExitThrown(int nextState)
	{
		if (!TemporalController.RestoringSnapshots)
		{
			// Tween
			NewTween();
			currentTween.TweenProperty(MeshRoot, "scale", Vector3.One, 0.1f);
		}

        // Clear trail
        if (startedThrow && trail != null && trail.IsInsideTree())
        {
            Vector3 trailPosition = trail.GlobalPosition;
            trail.TopLevel = true; // Makes the trail stop following the dagger
        }

        ThrowParticles.Emitting = false;

		await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame); // Make sure one physics step has passed to handle collisions before deactivating the hazard volume
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
        HazardVolume.Active = false;
    }

	void PhysicsProcessThrown(double delta)
	{
        // Need to do this during physics step
        if (!startedThrow)
        {
            // Throw
            Freeze = false;
            LinearDamp = 0;
            AngularVelocity = Vector3.Zero;
            LinearVelocity = -GlobalBasis.Z * ThrowSpeed;
			
			if (!TemporalController.RestoringSnapshots)
			{
				throwStartPositon = PlayerBody.GlobalPosition with { Y = GlobalPosition.Y };
				GlobalPosition = throwStartPositon;
				
				// Tween
				NewTween();
				currentTween.TweenProperty(MeshRoot, "scale", new Vector3(0.7f, 1.5f, 2), 0.1f);

				// VFX
				ThrowParticles.Emitting = true;
				trail = (Node3D)TrailPrefab.Instantiate();
				TrailRoot.AddChild(trail);
			}

			startedThrow = true;
        }

        // Fall to the ground after traveling a set distance
        if (GlobalPosition.DistanceTo(throwStartPositon) > ThrowDistance)
		{
			StateMachine.SwitchState((int)States.Fallen);
		}
	}
	#endregion

	#region Stuck State
	void EnterStuck(int previousState)
	{
		Freeze = true;
		LinearVelocity = Vector3.Zero;
		AngularVelocity = Vector3.Zero;
		
		if (!TemporalController.RestoringSnapshots)
		{
			GlobalPosition += -Basis.Z * StuckImpaleDistance;
			stuckOffset = stuckBody.GlobalBasis.Inverse() * (GlobalPosition - stuckBody.GlobalPosition);
			stuckRotationOffset = stuckBody.GlobalBasis.Inverse() * (GlobalRotation - stuckBody.GlobalRotation);
		}

		SetFarsightRuneVision(true);
    }

	void ExitStuck(int nextState)
	{
		SetFarsightRuneVision(false);
		SetFocused(false);
	}

	void PhysicsProcessStuck(double delta)
	{
        // Stay stuck in the same place even if the object moves or rotates
        GlobalPosition = stuckBody.GlobalPosition + stuckBody.GlobalBasis * stuckOffset;
        GlobalRotation = stuckBody.GlobalRotation + stuckBody.GlobalBasis * stuckRotationOffset;
    }
    #endregion

    #region Fallen State
    void EnterFallen(int previousState)
	{
		Freeze = false;
		LinearVelocity = Vector3.Zero;
		AngularVelocity = new Vector3(0, (float)GD.RandRange(-1f, 1f), 0) * FallenAngularVelocityStrength;
		GravityScale = FallenGravity;
		LinearDamp = FallenLinearDamp;

		SetFarsightRuneVision(true);
	}

	void ExitFallen(int nextState)
	{
		SetFarsightRuneVision(false);
		SetFocused(false);
		GravityScale = 0;
	}
    #endregion

    #region Runes
	void SetFarsightRuneVision(bool enabled)
	{
		if (!PlayerController.Instance.Inventory.HasItem(FarsightRuneItem))
			return;

		FarsightVisionLight.Visible = enabled;

		if (enabled)
		{
			if (!TemporalController.RestoringSnapshots)
			{
				FarsightVisionLight.OmniRange = 0;
				farsightVisionLightTween?.Kill();
				farsightVisionLightTween = CreateTween();
				farsightVisionLightTween.TweenProperty(FarsightVisionLight, (string)OmniLight3D.PropertyName.OmniRange, FarsightVisionRange, 0.5f).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
			}
			else
				FarsightVisionLight.OmniRange = FarsightVisionRange;
		}
	}

	void WarpRune()
	{
		if (!PlayerController.Instance.Inventory.HasItem(WarpRuneItem))
			return;

		PlayerController.Instance.GlobalPosition = GlobalPosition;
		StateMachine.SwitchState((int)States.Held);
		SoundManager.Instance.CreateSound(this, GlobalPosition, WarpSoundRadius, Sound.Messages.Investigate);
	}

	void SoundRune()
	{
		if (!PlayerController.Instance.Inventory.HasItem(DistractionRuneItem))
			return;

		if (lastDistractionTick + DistractionCooldown * 1000 >= ScaledTime.TicksMsec && lastDistractionTick != 0)
			return;

		SoundManager.Instance.CreateSound(this, GlobalPosition, DistractionSoundRadius, Sound.Messages.Investigate, duration: DistractionSoundDuration);
		lastDistractionTick = ScaledTime.TicksMsec;
	}
    #endregion

    #region Private Functions
    void NewTween()
	{
		currentTween?.Kill();
		currentTween = CreateTween();
	}
	#endregion
}
