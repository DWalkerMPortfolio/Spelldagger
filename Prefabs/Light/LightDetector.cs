using Godot;
using System;
using System.Collections.Generic;

public partial class LightDetector : Area3D
{
    [Export(PropertyHint.Layers3DPhysics)] uint LightOccluderLayers;
    [Export] bool Debug;

    public bool Illuminated { get; private set; }

    List<LightArea> Lights = new List<LightArea>();

    public override void _Ready()
    {
        base._Ready();

        AreaEntered += OnAreaEntered;
        AreaExited += OnAreaExited;
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        AreaEntered -= OnAreaEntered;
        AreaExited -= OnAreaExited;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        bool inLight = false;
        if (Lights.Count > 0)
        {
            PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
            PhysicsRayQueryParameters3D query = new PhysicsRayQueryParameters3D();
            query.To = GlobalPosition;
            query.CollisionMask = LightOccluderLayers;
            foreach (LightArea light in Lights)
            {
                // Check angle (for spot lights, omni lights have an angle of -1)
                if (light.Angle > 0)
                {
                    //GD.Print(Mathf.RadToDeg((-light.GlobalBasis.Z).AngleTo(GlobalPosition - light.GlobalPosition)));
                    if ((-light.GlobalBasis.Z).AngleTo(GlobalPosition - light.GlobalPosition) > Mathf.DegToRad(light.Angle))
                        continue; // Out of angle, skip
                }

                // Check occlusion
                query.From = light.GlobalPosition;
                if (spaceState.IntersectRay(query).Count == 0)
                {
                    inLight = true;
                    break;
                }
            }
        }

        if (Debug && Illuminated != inLight)
        {
            if (inLight)
                GD.Print("Light detector " + Name + " entered light");
            else
                GD.Print("Light detector " + Name + " left light");
        }

        Illuminated = inLight;
    }

    private void OnAreaEntered(Area3D area)
    {
        LightArea lightArea = area as LightArea;
        if (lightArea != null)
            Lights.Add(lightArea);
    }
    private void OnAreaExited(Area3D area)
    {
        LightArea lightArea = area as LightArea;
        if (lightArea != null)
            Lights.Remove(lightArea);
    }
}
