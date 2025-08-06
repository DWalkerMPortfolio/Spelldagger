#if TOOLS
using Godot;
using System;

[Tool]
public partial class BoxSelectButton : Button
{
    public override void _Toggled(bool toggledOn)
    {
        base._Toggled(toggledOn);

        if (!toggledOn)
            return;

        Button editPointsButton = (Button)GetParent().GetChild(1);
        editPointsButton.EmitSignal(SignalName.Pressed); // Switch to edit points before switching to custom mode. Otherwise editor will still try to create points when clicking
        editPointsButton.SetPressedNoSignal(false);
        SetPressedNoSignal(true);
    }
}
#endif