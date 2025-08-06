using Godot;
using System;

public partial class ClockOverlay : MeshInstance3D
{
    [Export] MeshInstance3D HourHand;
    [Export] MeshInstance3D MinuteHand;
    [Export] float FadeInDuration;
    [Export] float FlashDuration;
    [Export] float RotationSpeedFactor;

    Tween tween;
    Color startingColor;

    public override void _Ready()
    {
        base._Ready();

        startingColor = ((StandardMaterial3D)MaterialOverride).Emission;
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        StandardMaterial3D material = (StandardMaterial3D)MaterialOverride;
        material.Emission = startingColor;
        material.AlbedoColor = new Color(0, 0, 0, 0);
    }

    public void FadeInOut(bool visible)
    {
        if (visible)
        {
            MinuteHand.Rotation = Vector3.Zero;
            HourHand.Rotation = Vector3.Zero;
        }
        
        float targetTransparency = visible ? 1.0f : 0.0f;

        tween?.Kill();
        tween = CreateTween();

        if (!visible)
        {
            tween.TweenProperty(MaterialOverride, "emission", Colors.White, FlashDuration);
            tween.TweenProperty(MaterialOverride, "emission", startingColor, FlashDuration);
        }

        tween.TweenProperty(MaterialOverride, "albedo_color:a", targetTransparency, FadeInDuration);
    }

    public void Rotate(float amount)
    {
        MinuteHand.RotateY(amount * RotationSpeedFactor);
        HourHand.RotateY(amount * RotationSpeedFactor / (Mathf.Pi * 2));
    }

    public void RotateTo(float degreeAngle, bool flash = true)
    {
        MinuteHand.RotationDegrees = MinuteHand.RotationDegrees with { Y = degreeAngle % 360 };
        HourHand.RotationDegrees = HourHand.RotationDegrees with { Y = Mathf.Floor((degreeAngle) / 360) * 30 };

        if (flash)
        {
            tween?.Kill();
            tween = CreateTween();
            tween.TweenProperty(MaterialOverride, "emission", Colors.White, FlashDuration);
            tween.TweenProperty(MaterialOverride, "emission", startingColor, FlashDuration);
        }
    }
}
