using Godot;
using System;

[Tool]
public partial class GuardVisionCone : GuardPerception
{
    enum SenseTargets { Player, Daggers};

    [Export] float Range
    {
        get { return _range; }
        set { _range = value; UpdateLights(); }
    }
    float _range = 12;
    [Export] bool IsSpot;
    [Export]
    float SpotAngle
    {
        get { return _spotAngle; }
        set { _spotAngle = value; UpdateLights(); }
    }
    float _spotAngle = 26.5f;
    [Export] float AwarenessIncreaseSpeed = 5;
    [Export] float DarknessAwarenessIncreaseSpeed = 1;

    [ExportGroup("Internal")]
    [Export] Light3D Light;
    [Export] Gradient AwarenessGradient;
    [Export] SenseTargets SenseTarget = SenseTargets.Player;
    [Export(PropertyHint.Layers3DPhysics)] uint VisionObstructionLayers = 16;

    SpotLight3D spotLight;
    OmniLight3D omniLight;
    Tween visibleTween;

    public override void _Ready()
    {
        base._Ready();

        UpdateLights();
    }

    void UpdateLights()
    {
        if (Light == null)
            return;

        if (IsSpot)
        {
            spotLight = (SpotLight3D)Light;
            spotLight.SpotRange = Range;
            spotLight.SpotAngle = SpotAngle;
        }
        else
        {
            omniLight = (OmniLight3D)Light;
            omniLight.OmniRange = Range;
        }
    }

    public override void SetVisibility(bool value)
    {
        if (!TemporalController.RestoringSnapshots)
        {
            // Tween
            visibleTween?.Kill();
            visibleTween = CreateTween();

            if (value)
            {
                if (IsSpot)
                    visibleTween.TweenProperty(spotLight, "spot_range", Range, 0.2f).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
                else
                    visibleTween.TweenProperty(omniLight, "omni_range", Range, 0.2f).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
            }
            else
            {
                if (IsSpot)
                    visibleTween.TweenProperty(spotLight, "spot_range", 0, 0.2f).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
                else
                    visibleTween.TweenProperty(omniLight, "omni_range", 0, 0.2f).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
            }
        }
        else
        {
            if (IsSpot)
                spotLight.SpotRange = value ? Range : 0;
            else
                omniLight.OmniRange = value ? Range : 0;
        }
    }

    public override float UpdateAwareness()
    {
        if (SenseTarget == SenseTargets.Player)
        {
            if (PlayerController.Instance.IsDead())
                return 0;

            if (TargetVisible(PlayerController.Instance))
            {
                if (PlayerController.Instance.LightDetector.Illuminated)
                    return AwarenessIncreaseSpeed;
                else
                    return DarknessAwarenessIncreaseSpeed;
            }
            return 0;
        }
        else if (SenseTarget == SenseTargets.Daggers)
        {
            if (TargetVisible(PlayerController.Instance.DaggerHolder.DaggerL))
                return AwarenessIncreaseSpeed;
            if (TargetVisible(PlayerController.Instance.DaggerHolder.DaggerR))
                return AwarenessIncreaseSpeed;
            return 0;
        }

        return 0;
    }

    bool TargetVisible(Node3D target)
    {
        // Range check
        float playerDistanceSquared = GlobalPosition.DistanceSquaredTo(target.GlobalPosition);
        if (playerDistanceSquared < Range * Range)
        {
            //GD.Print("Player in vision range");

            // Angle check
            if (!IsSpot || Mathf.Abs(Mathf.RadToDeg((target.GlobalPosition - GlobalPosition).AngleTo(-GlobalBasis.Z))) < SpotAngle)
            {
                if (IsTargetInLineOfSight(target))
                {
                    //GD.Print("Player in line of sight");
                    return true;
                }
            }
        }
        return false;
    }

    public override bool IsPlayerInLineOfSight()
    {
        return IsTargetInLineOfSight(PlayerController.Instance);
    }

    bool IsTargetInLineOfSight(Node3D target)
    {
        PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
        PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(GlobalPosition, target.GlobalPosition, VisionObstructionLayers);
        Godot.Collections.Dictionary result = spaceState.IntersectRay(query);
        return result.Count == 0;
    }

    public override void AwarenessUpdated()
    {
        Light.LightColor = AwarenessGradient.Sample(owner.awareness);
    }
}
