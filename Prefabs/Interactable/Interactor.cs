using Godot;
using System;
using System.Collections.Generic;

public partial class Interactor : Area3D
{
    List<Interactable> PossibleInteractables = new List<Interactable>();

    public override void _EnterTree()
    {
        base._EnterTree();

        AreaEntered += OnAreaEntered;
        AreaExited += OnAreaExited;
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        AreaEntered -= OnAreaEntered;
        AreaExited -= OnAreaExited;
    }

    public void Interact()
    {
        if (PossibleInteractables.Count == 0)
            return;

        float closestDistanceSquared = Mathf.Inf;
        Interactable closestInteractable = null;
        foreach (Interactable interactable in PossibleInteractables)
        {
            if (!interactable.Active)
                return;

            float distanceSquared = GlobalPosition.DistanceSquaredTo(interactable.GlobalPosition);
            if (distanceSquared < closestDistanceSquared)
            {
                closestInteractable = interactable;
                closestDistanceSquared = distanceSquared;
            }
        }

        closestInteractable?.Interact();
    }

    private void OnAreaEntered(Area3D area)
    {
        Interactable interactable = area as Interactable;
        if (interactable != null)
        {
            PossibleInteractables.Add(interactable);
        }
    }

    private void OnAreaExited(Area3D body)
    {
        Interactable interactable = body as Interactable;
        if (interactable != null)
        {
            PossibleInteractables.Remove(interactable);
        }
    }
}
