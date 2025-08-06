using Godot;
using System;
using System.Collections.Generic;

public partial class GuardDamageRadius : GuardWeapon, ITemporalControl
{
    readonly string[] TEMPORAL_PROPERTIES =
    {
        PropertyName.charge
    };

    [Export] float ChargeDuration;
    [Export] float Radius;

    [ExportGroup("Internal")]
    [Export] CollisionShape3D CollisionShape;
    [Export] MeshInstance3D MeshInstance;

    float charge;
    bool charged;
    Tween scaleTween;

    public string[] GetTemporalProperties()
    {
        return TEMPORAL_PROPERTIES;
    }

    public Dictionary<string, Variant> SaveCustomTemporalState()
    {
        Dictionary<string, Variant> data = new Dictionary<string, Variant>();
        data.Add(PropertyName.charged, charged);
        return data;
    }

    public void RestoreCustomTemporalState(Dictionary<string, Variant> customData)
    {
        if (customData == null)
            return;

        SetCharged((bool)customData[PropertyName.charged]);
    }

    public override void ExitedAlert(int nextState)
    {
        base.ExitedAlert(nextState);

        SetCharged(false);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (!active)
            return;

        if (!charged)
        {
            charge += (float)delta / ChargeDuration;
            if (charge >= 1)
                SetCharged(true);
        }
    }

    void SetCharged(bool value)
    {
        if (charged == value)
            return;
        charged = value;
        charge = charged ? 1 : 0;

        Vector3 targetScale = new Vector3(0, 1, 0);
        if (charged)
            targetScale = new Vector3(Radius, 1, Radius);

        if (charged)
            MeshInstance.Visible = true;

        if (!TemporalController.RestoringSnapshots)
        {
            scaleTween?.Kill();
            scaleTween = CreateTween();
            scaleTween.TweenProperty(CollisionShape.Shape, (string)CylinderShape3D.PropertyName.Radius, targetScale.X, 0.5f);
            scaleTween.Parallel().TweenProperty(MeshInstance, (string)Node3D.PropertyName.Scale, targetScale, 0.5f);
            if (!charged)
                scaleTween.TweenCallback(Callable.From(() => { MeshInstance.Visible = false; }));
        }
        else
        {
            ((CylinderShape3D)CollisionShape.Shape).Radius = targetScale.X;
            MeshInstance.Scale = targetScale;

            if (!charged)
                MeshInstance.Visible = false;
        }
    }
}
