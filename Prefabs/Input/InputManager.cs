using Godot;
using System;
using System.Collections.Generic;

public partial class InputManager : Node
{
    public static InputManager Instance;

    HashSet<string> inputLocks = new HashSet<string>(); // Stores all input locks currently applied. Input is only unlocked when this is empty

    public override void _Ready()
    {
        base._Ready();

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            GD.Print("Duplicate input manager: " + Name);
            QueueFree();
        }
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        if (Instance == this)
            Instance = null;
    }

    public void AddInputLock(string lockId)
    {
        inputLocks.Add(lockId);
    }

    public void RemoveInputLock(string lockId)
    {
        inputLocks.Remove(lockId);
    }

    public bool IsInputUnlocked()
    {
        return inputLocks.Count == 0;
    }
}
