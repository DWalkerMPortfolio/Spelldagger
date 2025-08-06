using Godot;
using System;
using System.Collections.Generic;

public partial class CameraTarget : Node3D
{
    public static List<CameraTarget> Instances = new List<CameraTarget>();

    [Export] public bool Active { get; private set; }
    [Export] public int Priority { get; private set; }
    [Export] public bool EaseTransition { get; private set; } = true;
    [Export] Node3D Origin;
    [Export] float MaxDistance;
    [Export] float Easing;
    [Export] Vector3 Offset = Vector3.Up;

    Vector3 previousOffset;

    public override void _EnterTree()
    {
        base._EnterTree();

        Instances.Add(this);
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        Instances.Remove(this);
    }

    public override void _Ready()
    {
        base._Ready();

        UpdatePosition();
    }

    public override void _Process(double delta)
    {
        UpdatePosition();
    }

    void UpdatePosition()
    {
        // Aim targeting
        Vector2 mousePosition = GetViewport().GetMousePosition();
        Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
        mousePosition /= viewportSize;
        mousePosition = mousePosition.Clamp(0, 1);
        mousePosition -= Vector2.One * 0.5f;
        mousePosition *= 2;

        // Apply
        Vector3 targetOffset = Easing * new Vector3(mousePosition.X, 0, mousePosition.Y) * MaxDistance + (1 - Easing) * previousOffset;
        GlobalPosition = Origin.GlobalPosition + targetOffset + Offset;
        previousOffset = targetOffset;
    }

    public void SetActive(bool value)
    {
        Active = value;
        MainCamera.Instance?.TargetActivationChanged(this);
    }
}
