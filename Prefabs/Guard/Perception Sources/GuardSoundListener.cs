using Godot;
using System;

public partial class GuardSoundListener : GuardPerception
{
    [ExportGroup("Internal")]
    [Export] SoundListener SoundListener;

    public override void _Ready()
    {
        base._Ready();

        SoundListener.OnHeardSound += OnHeardSound;
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        SoundListener.OnHeardSound -= OnHeardSound;
    }

    void OnHeardSound(Node source, Vector3 position, Sound.Messages message)
    {
        if (source == owner)
            return;

        switch (message)
        {
            case Sound.Messages.Alert:
                owner.InvestigatePosition(position, true);
                break;
            case Sound.Messages.Investigate:
                owner.InvestigatePosition(position, false);
                break;
            case Sound.Messages.AllClear:
                owner.AllClear(); 
                break;
        }
    }
}
