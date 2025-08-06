using Godot;
using System;

public partial class TrainCar : Node2D
{
    const float ROTATION_OFFSET = 90;

    [Export] LevelEditor CarSubLevel;
    [Export] Node2D FrontWheel;
    [Export] Node2D BackWheel;

    public float distanceToFrontWheel { get; private set; }
    public float distanceToBackWheel { get; private set; }
    float pivotDistanceProportion;

    public override void _EnterTree()
    {
        base._EnterTree();

        GlobalPosition = CarSubLevel.GlobalPosition;
        distanceToFrontWheel = CarSubLevel.GlobalPosition.DistanceTo(FrontWheel.GlobalPosition);
        distanceToBackWheel = CarSubLevel.GlobalPosition.DistanceTo(BackWheel.GlobalPosition);
    }

    public void SetDistance(Curve2D curve, float distance)
    {
        Vector2 frontWheelPosition = curve.SampleBaked(distance + distanceToFrontWheel);
        Vector2 backWheelPosition = curve.SampleBaked(distance - distanceToBackWheel);

        Vector2 pivotPosition = backWheelPosition + backWheelPosition.DirectionTo(frontWheelPosition) * distanceToBackWheel;

        GlobalPosition = pivotPosition;
        GlobalRotation = backWheelPosition.AngleToPoint(frontWheelPosition) + Mathf.DegToRad(ROTATION_OFFSET);
        CarSubLevel.UpdateTransform(GlobalTransform);
    }
}
