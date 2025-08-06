using Godot;
using System;
using System.Collections.Generic;

public partial class TrainController : Path2D, ITemporalControl
{
    enum EndBehaviors { Loop, Stop, Reverse}

    readonly string[] TEMPORAL_PROPERTIES =
    {
        PropertyName.lastCarDistance,
    };

    [Export] EndBehaviors EndBehavior;
    [Export] float Speed;
    [Export] float Direction = 1;
    [Export] NodePath[] TrainCarPaths;
    
    TrainCar[] trainCars;
    float[] trainCarLengths;
    float lastCarDistance;
    float trackStartDistance;
    float trackEndDistance;

    public override void _Ready()
    {
        base._Ready();

        trainCars = Globals.ConvertNodePathArray<TrainCar>(this, TrainCarPaths);
        CalculateTrainCarLengths();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        lastCarDistance += Speed * Direction * Globals.PixelsPerUnit * (float)delta;

        if (lastCarDistance > trackEndDistance || lastCarDistance < trackStartDistance)
        {
            switch (EndBehavior)
            {
                case EndBehaviors.Stop:
                    lastCarDistance = trackEndDistance;
                    Direction = 0;
                    break;
                case EndBehaviors.Loop:
                    lastCarDistance %= Curve.GetBakedLength();
                    break;
                case EndBehaviors.Reverse:
                    Direction *= -1;
                    break;
            }
        }
        
        UpdateCarTransforms();
    }

    public string[] GetTemporalProperties()
    {
        return TEMPORAL_PROPERTIES;
    }

    public void RestoreCustomTemporalState(Dictionary<string, Variant> customData)
    {
        UpdateCarTransforms();
    }
    
    private void CalculateTrainCarLengths()
    {
        float totalLength = 0;
        trainCarLengths = new float[trainCars.Length];
        for (int i=0; i<trainCars.Length - 1; i++)
        {
            float carLength = trainCars[i].GlobalPosition.Y - trainCars[i + 1].GlobalPosition.Y;
            trainCarLengths[i] = carLength;
            totalLength+=carLength;
        }

        trackStartDistance = trainCars[0].distanceToBackWheel;
        trackEndDistance = Curve.GetBakedLength() - (totalLength + trainCars[trainCars.Length - 1].distanceToFrontWheel);

        lastCarDistance = trackStartDistance;
    }

    private void UpdateCarTransforms()
    {
        float carDistance = lastCarDistance;
        for (int i=0; i<trainCars.Length; i++)
        {
            trainCars[i].SetDistance(Curve, carDistance);

            carDistance += trainCarLengths[i];
            carDistance %= Curve.GetBakedLength();
        }
    }
}
