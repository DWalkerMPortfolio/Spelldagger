#if TOOLS
using Godot;
using System;

[Tool]
public partial class BottomPanelPin : EditorPlugin, ISerializationListener
{
	Button pinButton;
	Node bottomPanelSelector;

    bool openedManually;

    public override void _EnterTree()
	{
        Control placeholderBottomPanelControl = new Control();
        Button bottomPanelButton = AddControlToBottomPanel(placeholderBottomPanelControl, "Pin Placeholder");
        bottomPanelSelector = bottomPanelButton.GetParent();
        RemoveControlFromBottomPanel(placeholderBottomPanelControl);
        placeholderBottomPanelControl.QueueFree();

        Button tabPinButton = (Button)bottomPanelSelector.GetParent().GetParent().GetChild(6);
        tabPinButton.ButtonPressed = true;

        // Call deferred to ensure the button is the last thing in the list and all other plugin bottom panels have been added
        CallDeferred(MethodName.CreateButton);
	}

    public override void _ExitTree()
	{
		pinButton?.Free();
        UpdateSignals(true);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (pinButton.ButtonPressed && !openedManually)
        {
            foreach (Button bottomPanelButton in bottomPanelSelector.GetChildren())
            {
                if (bottomPanelButton != pinButton && bottomPanelButton.ButtonPressed)
                {
                    bottomPanelButton.ButtonPressed = false;
                }
            }
        }
    }

    public void OnBeforeSerialize()
    {
        UpdateSignals(true);
    }

    public void OnAfterDeserialize()
    {
        UpdateSignals(false);
    }

    void OnBottomPanelButtonToggled(bool toggleValue)
    {
        openedManually = toggleValue;
    }

    void CreateButton()
	{
        pinButton = GD.Load<PackedScene>("addons/bottom_panel_pin/PinButton.tscn").Instantiate<Button>();
        bottomPanelSelector.AddChild(pinButton);

        UpdateSignals(false);
    }

    void UpdateSignals(bool disconnect)
    {
        foreach (Button bottomPanelChild in bottomPanelSelector.GetChildren())
        {
            if (bottomPanelChild != pinButton)
            {
                if (!disconnect)
                    bottomPanelChild.Toggled += OnBottomPanelButtonToggled;
                else
                    bottomPanelChild.Toggled -= OnBottomPanelButtonToggled;
            }
        }
    }
}
#endif
