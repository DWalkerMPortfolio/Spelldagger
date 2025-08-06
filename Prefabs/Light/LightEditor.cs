using Godot;
using System;

[Tool]
public partial class LightEditor : Node3D
{
    [Export] public float Range
    {
        get { return _range; }
        set { _range = value; UpdateRange(); }
    }
    float _range = 10f;
    [Export] Color Color
    {
        get { return _color; }
        set { _color = value; UpdateColor(); }
    }
    Color _color = Colors.White;
    [Export] float Energy
    {
        get { return _energy; }
        set { _energy = value; UpdateEnergy(); }
    }
    float _energy = 1.5f;
    [Export] float SpotAngle
    {
        get { return _spotAngle; }
        set { _spotAngle = value; UpdateSpotAngle(); }
    }
    float _spotAngle = 30;

    [ExportGroup("Internal")]
    [Export] bool IsSpotLight;
    [Export] OmniLight3D OmniLight;
    [Export] OmniLight3D LightCaptureOmniLight;
    [Export] SpotLight3D SpotLight;
    [Export] SpotLight3D LightCaptureSpotLight;
    [Export] CollisionShape3D CollisionShape;
    [Export] LightArea Area;
    [Export] float AreaRangeMultiplier = 0.825f;

    void UpdateRange()
    {
        if (IsSpotLight)
        {
            if (SpotLight != null)
                SpotLight.SpotRange = Range;
            if (LightCaptureSpotLight != null)
                LightCaptureSpotLight.SpotRange = Range;
        }
        else
        {
            if (OmniLight != null)
                OmniLight.OmniRange = Range;
            if (LightCaptureOmniLight != null)
                LightCaptureOmniLight.OmniRange = Range;
        }
        if (CollisionShape != null)
            CollisionShape.Scale = Vector3.One * (Range * AreaRangeMultiplier);
    }

    void UpdateColor()
    {
        if (IsSpotLight)
        {
            if (SpotLight != null)
                SpotLight.LightColor = Color;
        }
        else
        {
            if (OmniLight != null)
                OmniLight.LightColor = Color;
        }
    }

    void UpdateEnergy()
    {
        if (IsSpotLight)
        {
            if (SpotLight != null)
                SpotLight.LightEnergy = Energy;
        }
        else
        {
            if (OmniLight != null)
                OmniLight.LightEnergy = Energy;
        }
    }

    void UpdateSpotAngle()
    {
        if (IsSpotLight)
        {
            if (SpotLight != null)
                SpotLight.SpotAngle = SpotAngle;
            if (LightCaptureSpotLight != null)
                LightCaptureSpotLight.SpotAngle = SpotAngle;
            if (Area != null)
                Area.Angle = SpotAngle;
        }
    }
}