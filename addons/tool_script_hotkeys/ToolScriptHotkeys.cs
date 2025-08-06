#if TOOLS
using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class ToolScriptHotkeys : EditorPlugin, ISerializationListener
{
	const string DISPLAY_HOTKEYS_ACTION = "Display Tool Script Hotkeys";

	public delegate void HotkeyPressed();

	public static ToolScriptHotkeys Instance;

	[Export] public string CurrentMainScreen { get; private set; } // Export ensures value is saved through rebuilds

	Dictionary<string, List<HotkeyPressed>> hotkeys = new Dictionary<string, List<HotkeyPressed>>();

    public override void _EnterTree()
    {
        base._EnterTree();

		Instance = this;

		// Register main screen callback
		MainScreenChanged += OnMainScreenChanged;
		EditorInterface.Singleton.SetMainScreenEditor("2D");

		// Register display hotkeys action
		if (!InputMap.HasAction(DISPLAY_HOTKEYS_ACTION))
		{
			InputMap.AddAction(DISPLAY_HOTKEYS_ACTION);

			InputEventKey displayHotkeysEvent = new InputEventKey();
			displayHotkeysEvent.Keycode = Key.F1;
			displayHotkeysEvent.ShiftPressed = true;

			InputMap.ActionAddEvent(DISPLAY_HOTKEYS_ACTION, displayHotkeysEvent);
		}
    }

    public override void _ExitTree()
	{
		base._ExitTree();

		// Remove actions
		foreach (string action in hotkeys.Keys)
		{
			if (InputMap.HasAction(action))
			{
				InputMap.EraseAction(action);
			}
		}

		Instance = null;
	}

    public void OnBeforeSerialize()
    {
        Instance = null;
    }

    public void OnAfterDeserialize()
    {
        Instance = this;
    }

    private void OnMainScreenChanged(string screenName)
    {
        CurrentMainScreen = screenName;
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

		// Don't execute any editor hotkeys while the game is running
		if (!Engine.IsEditorHint())
			return;

		// Catch the display hotkeys hotkey
		if (@event.IsActionPressed(DISPLAY_HOTKEYS_ACTION, exactMatch: true))
		{
			DisplayHotkeys();
			return;
		}

		// If this input event is a registered hotkey, invoke the relevant functions
		foreach (string action in hotkeys.Keys)
		{
			if (@event.IsActionPressed(action, exactMatch: true))
			{
				foreach (HotkeyPressed pressed in hotkeys[action])
				{
					pressed?.Invoke();
				}
			}
		}
    }

	/// <summary>
	/// Register a new hotkey. Must be re-registered after every build. Automatically prevents duplicates
	/// </summary>
	/// <param name="actionName">The name of the hotkey's input action</param>
	/// <param name="event">The input event that triggers the hotkey</param>
	/// <param name="delegate">The delegate to invoke when the hotkey is pressed</param>
	public void RegisterHotkey(string actionName, InputEvent @event, HotkeyPressed @delegate)
	{
		// Add hotkey to input map
		if (!InputMap.HasAction(actionName))
			InputMap.AddAction(actionName);
		InputMap.ActionAddEvent(actionName, @event);
		
		// Add hotkey to dictionary
		if (!hotkeys.ContainsKey(actionName))
			hotkeys[actionName] = new List<HotkeyPressed>();
		if (!hotkeys[actionName].Contains(@delegate))
			hotkeys[actionName].Add(@delegate);
	}

	/// <summary>
	/// Remove a previously registered hotkey, if it exists
	/// </summary>
	/// <param name="actionName">The name of the hotkey's input action</param>
	/// <param name="delegate">The delegate that was invoked when the hotkey was pressed</param>
	public void UnregisterHotkey(string actionName, HotkeyPressed @delegate)
	{
		if (hotkeys.ContainsKey(actionName))
		{
			hotkeys[actionName].Remove(@delegate);
			if (hotkeys[actionName].Count == 0)
			{
				hotkeys.Remove(actionName);
				InputMap.EraseAction(actionName);
			}
		}
	}

	void DisplayHotkeys()
	{
		GD.Print("Currently registered hotkeys: ");
		foreach (string action in hotkeys.Keys)
		{
			GD.Print(action + ": " + InputMap.ActionGetEvents(action)[0].AsText());
		}
	}
}
#endif
