using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class RoguelikeController : Node
{
    [Export] Node TeleportDestinationsParent;
    [Export] Node2D FinalRoom;
    [Export] Node2D WinRoom;
    [Export] float TeleportHeight;
    [Export] int RoomsPerLoop;
    [Export] int MaxLoops;

    Godot.Collections.Array<Node> teleportDestinations;
    Godot.Collections.Array<Node> remainingTeleportDestinations;
    int roomsThisLoop;
    int loopCount;

    public override void _Ready()
    {
        base._Ready();

        teleportDestinations = TeleportDestinationsParent.GetChildren();
        remainingTeleportDestinations = teleportDestinations.Duplicate();
    }

    public void Loop()
    {
        remainingTeleportDestinations = teleportDestinations.Duplicate();
        roomsThisLoop = 0;
        loopCount++;
        Teleport();
    }

    public void Teleport()
    {
        Node2D destination = FinalRoom;
        if (roomsThisLoop < RoomsPerLoop)
        {
            int destinationIndex = GD.RandRange(0, remainingTeleportDestinations.Count - 1);
            destination = (Node2D)remainingTeleportDestinations[destinationIndex];
            remainingTeleportDestinations.RemoveAt(destinationIndex);
        }
        else if (loopCount >= MaxLoops)
            destination = WinRoom;
        roomsThisLoop++;

        TeleportToNode2D(destination);
        TemporalController.ClearSnapshots();
    }

    void TeleportToNode2D(Node2D destination)
    {
        PlayerController.Instance.GlobalPosition = new Vector3(destination.GlobalPosition.X / Globals.PixelsPerUnit, TeleportHeight, destination.GlobalPosition.Y / Globals.PixelsPerUnit);
    }
}
