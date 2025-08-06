using Godot;
using System;

public partial class Interactable : Area3D
{
    public delegate void OnInteracted();
    public event OnInteracted Interacted;                      
    
    [Export] public bool Active = true;
    [Export] public string InteractionName;

    public void Interact()
    {
        Interacted?.Invoke();
    }
}
