#if TOOLS
using Godot;
using System;

[Tool]
public partial class GenerateMeshPropsPopup : Window
{
    [Export] CheckBox SnapToFloor;
    [Export] CheckBox SnapToWall;
    [Export] CheckBox AddCollision;
    [Export] CheckBox LightCapture;
    [Export] Button Confirm;
    [Export] Button Cancel;

    GenerateMeshProps owner;

    public override void _Ready()
    {
        base._Ready();

        CloseRequested += OnCloseRequested;
        Cancel.ButtonDown += OnCloseRequested;
        Confirm.ButtonDown += OnConfirm;
    }

    public void Initialize(GenerateMeshProps owner)
    {
        this.owner = owner;
    }

    private void OnCloseRequested()
    {
        QueueFree();
    }

    private void OnConfirm()
    {
        owner.Generate(SnapToFloor.ButtonPressed, SnapToWall.ButtonPressed, AddCollision.ButtonPressed, LightCapture.ButtonPressed);

        QueueFree();
    }
}
#endif