using Godot;
using System;

public partial class ProximityFader : Node3D
{
    // TODO: Fix lights bleeding through floor

    const string TRANSPARENCY_SHADER_PARAMETER = "transparency";

    [Export] bool Static = true;
    [Export] NodePath[] ShaderGeometryInstancePaths;
    GeometryInstance3D[] shaderGeometryInstances;
    [Export] NodePath[] GeometryInstancePaths;
    GeometryInstance3D[] geometryInstances;
    [Export] NodePath[] Light3dPaths;
    Light3D[] light3ds;
    [Export] NodePath[] Node3dPaths;
    Node3D[] node3ds;

    int floor;
    bool visible;
    Tween fadeTween;
    float[] light3DInitialEnergy;

    public override void _Ready()
    {
        base._Ready();

        // Initialize arrays
        shaderGeometryInstances = Globals.ConvertNodePathArray<GeometryInstance3D>(this, ShaderGeometryInstancePaths);
        geometryInstances = Globals.ConvertNodePathArray<GeometryInstance3D>(this, GeometryInstancePaths);
        light3ds = Globals.ConvertNodePathArray<Light3D>(this, Light3dPaths);
        node3ds = Globals.ConvertNodePathArray<Node3D>(this, Node3dPaths);

        MainCamera.Instance.FloorChanged += OnCameraFloorChanged;
        CallDeferred(MethodName.InitialTransparency);

        // Save the initial energy for light3Ds
        light3DInitialEnergy = new float[light3ds.Length];
        for (int i = 0; i < light3ds.Length; i++)
            light3DInitialEnergy[i] = light3ds[i].LightEnergy;

        // Don't process if static
        if (Static)
            SetProcess(false);
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        MainCamera.Instance.FloorChanged -= OnCameraFloorChanged;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        int newFloor = Mathf.FloorToInt(GlobalPosition.Y / Globals.FloorHeight);
        if (newFloor != floor)
        {
            floor = newFloor;
            OnCameraFloorChanged(MainCamera.Instance.CurrentFloor);
        }
    }

    void OnCameraFloorChanged(int newFloor)
    {
        if (visible && floor > newFloor)
        {
            if (!TemporalController.RestoringSnapshots)
            {
                fadeTween?.Kill();
                fadeTween = CreateTween();
                fadeTween.TweenMethod(Callable.From((float t) => { SetTransparency(t); }), 0.0f, 1.0f, Globals.FloorFadeDuration).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
                fadeTween.TweenCallback(Callable.From(() => { SetVisible(false); }));
            }
            else
            {
                SetVisible(false);
                SetTransparency(1);
            }
        }
        else if (!visible && floor <= newFloor)
        {
            SetVisible(true);

            if (!TemporalController.RestoringSnapshots)
            {
                fadeTween?.Kill();
                fadeTween = CreateTween();
                fadeTween.TweenMethod(Callable.From((float t) => { SetTransparency(t); }), 1.0f, 0.0f, Globals.FloorFadeDuration).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
            }
            else
            {
                SetVisible(true);
                SetTransparency(0);
            }
        }
    }

    void SetTransparency(float value)
    {
        float earlyFadeValue = (float)Mathf.Clamp(Mathf.InverseLerp(0, 0.5, value), 0, 1);

        if (shaderGeometryInstances != null)
        {
            foreach (GeometryInstance3D shaderGeometryInstance in shaderGeometryInstances)
                shaderGeometryInstance.SetInstanceShaderParameter(TRANSPARENCY_SHADER_PARAMETER, value);
        }

        if (geometryInstances != null)
        {
            foreach (GeometryInstance3D geometryInstance in geometryInstances)
            {
                geometryInstance.Transparency = earlyFadeValue;
            }
        }

        if (light3ds != null)
        {
            for (int i = 0; i < light3ds.Length; i++)
                light3ds[i].LightEnergy = Mathf.Lerp(light3DInitialEnergy[i], 0, earlyFadeValue);
        }
    }

    new void SetVisible(bool value)
    {
        visible = value;

        if (shaderGeometryInstances != null)
        {
            foreach (GeometryInstance3D shaderGeometryInstance in shaderGeometryInstances)
                shaderGeometryInstance.Visible = value;
        }

        if (geometryInstances != null)
        {
            foreach (GeometryInstance3D geometryInstance in geometryInstances)
                geometryInstance.Visible = value;
        }

        if (light3ds != null)
        {
            foreach (Light3D light3D in light3ds)
                light3D.Visible = value;
        }

        if (node3ds != null)
        {
            foreach (Node3D node3D in node3ds)
                node3D.Visible = value;
        }
    }

    void InitialTransparency()
    {
        floor = Mathf.FloorToInt((GlobalPosition.Y) / Globals.FloorHeight);
        visible = floor <= MainCamera.Instance.CurrentFloor;
        SetTransparency(visible ? 0 : 1);
        Visible = visible;
    }
}
