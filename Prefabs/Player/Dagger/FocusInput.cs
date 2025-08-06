using Godot;
using System;

public partial class FocusInput : Node
{
    [Export] Dagger Dagger;

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);

        if (Dagger.StateMachine.CurrentState == (int)Dagger.States.Stuck || Dagger.StateMachine.CurrentState == (int)Dagger.States.Fallen)
        {
            if (@event.IsActionPressed(Dagger.inputAction))
                Dagger.SetFocused(true);
            else if (@event.IsActionReleased(Dagger.inputAction))
                Dagger.SetFocused(false);
        }
    }
}
