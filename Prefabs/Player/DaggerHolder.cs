using Godot;
using System;

public partial class DaggerHolder : Node3D
{
    [Export] public Dagger DaggerL { get; private set; }
    [Export] public Dagger DaggerR { get; private set; }

    public override void _PhysicsProcess(double delta)
    {
        UpdateAiming();
    }

    public void ForceUpdateDaggers()
    {
        UpdateAiming();
        DaggerL.ForceUpdateTransform();
        DaggerR.ForceUpdateTransform();
    }

    void UpdateAiming()
    {
        if (InputManager.Instance.IsInputUnlocked())
        {
            // Aim
            Vector2 mousePosition = GetViewport().GetMousePosition();
            Camera3D camera = GetViewport().GetCamera3D();
            Vector3 projectedMousePosition = camera.ProjectPosition(mousePosition, camera.Position.Y);

            Vector3 aimDirection = projectedMousePosition - GlobalPosition;
            aimDirection.Y = 0;
            aimDirection = aimDirection.Normalized();

            LookAt(GlobalPosition + aimDirection);
        }
    }
}
