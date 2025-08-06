using Godot;
using System;

[Tool]
public partial class GuardBehaviorStunned : GuardStateBehavior
{
    [Export] float StunnedSoundRadius;
    [Export] bool Shout = true;

    public override void Initialize(GuardController controller)
    {
        base.Initialize(controller);

        // Exit stunned when damage sources are removed
        owner.WeakpointDamageSourcesRemoved += () => { owner.StateMachine.SwitchState((int)GuardController.States.Idle); };
    }

    public override void EnterState(int previousState)
    {
        owner.OverrideImmovable(true);
        owner.updateAwareness = false;
        owner.SetPerceptionVisibility(false);
        owner.ClearNavigationPath();

        if (!TemporalController.RestoringSnapshots)
        {
            owner.SetHighAlert(true);

            if (Shout)
                SoundManager.Instance.CreateSound(owner, owner.Foot.GlobalPosition, StunnedSoundRadius, Sound.Messages.Alert);
        }
    }

    public override void ExitState(int nextState)
    {
        owner.ResetImmovable();
        owner.updateAwareness = true;
        owner.SetPerceptionVisibility(true);
    }
}
