using Godot;
using System;
using System.Collections.Generic;

public partial class GuardLaser : GuardWeapon, ITemporalControl
{
    readonly string[] TEMPORAL_PROPERTIES =
    {
        PropertyName.charge,
    };

    [Export] float ChargeDuration;

    [ExportGroup("Internal")]
    [Export] MeshInstance3D MeshInstance;

    float charge;

    public string[] GetTemporalProperties()
    {
        return TEMPORAL_PROPERTIES;
    }

    public void RestoreCustomTemporalState(Dictionary<string, Variant> customData)
    {
        UpdateVisuals();
    }

    public override void EnteredAlert(int previousState)
    {
        base.EnteredAlert(previousState);

        MeshInstance.Visible = true;
    }

    public override void ExitedAlert(int nextState)
    {
        base.ExitedAlert(nextState);

        charge = 0;
        MeshInstance.Visible = false;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (!active)
            return;

        // Update charge
        if (owner.IsPlayerInLineOfSight())
        {
            charge += (float)delta / ChargeDuration;
            if (charge >= 1)
            {
                PlayerController.Instance.TakeDamage(IDamageable.Teams.Guards, this);
                charge = 0;
            }
        }
        else
            charge = 0;

        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        MeshInstance.LookAt(PlayerController.Instance.GlobalPosition);
        MeshInstance.Scale = new Vector3(charge, 1, GlobalPosition.DistanceTo(PlayerController.Instance.GlobalPosition));
    }
}
