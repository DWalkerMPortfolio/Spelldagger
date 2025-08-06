using Godot;
using System;

public partial class RoguelikeFinalRoomTeleporter : Area3D
{
    enum AbilitiesToRemove { Warp, Distraction, Farsight, Dagger };

    [Export] NodePath Root;
    [Export] RoguelikeController Controller;
    [Export] AbilitiesToRemove AbilityToRemove;
    [Export] InventoryItem WarpItem;
    [Export] InventoryItem DistractionItem;
    [Export] InventoryItem FarsightItem;

    public override void _Ready()
    {
        base._Ready();

        BodyEntered += OnBodyEntered;
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        BodyEntered -= OnBodyEntered;
    }

    private void OnBodyEntered(Node3D body)
    {
        if (body == PlayerController.Instance)
        {
            RemoveAbility();
            Controller.Loop();
            GetNode(Root).QueueFree();
        }
    }

    void RemoveAbility()
    {
        switch (AbilityToRemove)
        {
            case AbilitiesToRemove.Warp:
                PlayerController.Instance.Inventory.RemoveItem(WarpItem); 
                break;
            case AbilitiesToRemove.Distraction:
                PlayerController.Instance.Inventory.RemoveItem(DistractionItem);
                break;
            case AbilitiesToRemove.Farsight:
                PlayerController.Instance.Inventory.RemoveItem(FarsightItem);
                break;
            case AbilitiesToRemove.Dagger:
                PlayerController.Instance.DaggerHolder.DaggerR.QueueFree();
                break;
        }
    }
}
