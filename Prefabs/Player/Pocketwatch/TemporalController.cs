using Godot;
using System;
using System.Collections.Generic;

public partial class TemporalController : Node
{
	class TemporalData
	{
		public Dictionary<string, Variant> PropertyValues = new Dictionary<string, Variant>();
		public Dictionary<string, Variant> CustomValues = new Dictionary<string, Variant>();
	}

	static SortedDictionary<int, List<TemporalController>> instances = new SortedDictionary<int, List<TemporalController>>(); // All registered temporal controller instances, sorted by priority

	public static bool RestoringSnapshots = false; // Whether snapshots are currently being restored
	public static int MaxSnapshots { get; private set; } = 300; // The number of snapshots to save at a time
	public static int SnapshotCount { get; private set; } = 0; // The number of snapshots out of the maximum count that have been created so far
	public static double SnapshotDelta { get; private set; } = 1.0 / 20.0; // The time in seconds between taking each regular snapshot

	[Export] Node ControlledNode;
	[Export] int Priority; // Lower priorities go first

	bool initialized = false;
	ITemporalControl controlledInterface;
	string[] temporalProperties; // The properties that are automatically saved and reloaded as temporal data
	List<TemporalData> savedData = new List<TemporalData>(); // All the saved temporal data for this controller
	ulong previousSnapshotNumber = 0; // The number of the snapshot that would have been recorded in the past process

	#region Godot Functions
	public override void _Ready()
	{
		if (ControlledNode != null)
		{
			ITemporalControl controlledNodeInterface = ControlledNode as ITemporalControl;
			if (controlledNodeInterface != null)
				Initialize(controlledNodeInterface);
			else
				GD.PushError("Node assigned to " + Name + " TemporalController doesn't implement ITemporalControl");
		}
    }

	public override void _Process(double delta)
	{
		if (!initialized)
			return;

		ulong snapshotDeltaMsec = (ulong)Mathf.FloorToInt(SnapshotDelta * 1000);
		ulong snapshotNumber = ScaledTime.TicksMsec / snapshotDeltaMsec;

		if (snapshotNumber - previousSnapshotNumber >= 1)
		{
			SaveData();
		}

		previousSnapshotNumber = snapshotNumber;
	}

	public override void _ExitTree()
	{
		// Unregister self
		instances[Priority].Remove(this);
		if (instances[Priority].Count == 0)
			instances.Remove(Priority);
	}
	#endregion

	#region Static Functions
	public static void RestoreSnapshot(int index)
	{
		foreach (int priority in instances.Keys)
		{
			foreach (TemporalController controller in instances[priority])
			{
				controller.RestoreData(index);
			}
		}
	}

	public static void ClearSnapshotsFromIndex(int index)
	{
        foreach (int priority in instances.Keys)
        {
            foreach (TemporalController controller in instances[priority])
            {
                controller.ClearDataFromIndex(index);
            }
        }

		SnapshotCount -= index;
    }

	public static void ClearSnapshots()
	{
		foreach (int priority in instances.Keys)
		{
			foreach (TemporalController controller in instances[priority])
			{
				controller.ClearData();
			}
		}

		SnapshotCount = 0;
	}
    #endregion

    #region Public Functions
    public void Initialize(ITemporalControl controlledInterface)
	{
        this.controlledInterface = controlledInterface;

        // Register self
        if (!instances.ContainsKey(Priority))
            instances.Add(Priority, new List<TemporalController>());
        instances[Priority].Add(this);

        // Get temporal properties
        temporalProperties = controlledInterface.GetTemporalProperties();

		initialized = true;
    }
    #endregion

    #region Private Functions
	void SaveData()
	{
		// Save the new data
		TemporalData data = new TemporalData();
		data.CustomValues = controlledInterface.SaveCustomTemporalState();
		data.PropertyValues = controlledInterface.SavePropertiesTemporalState(temporalProperties);
		savedData.Insert(0, data);

		// Eject old data
		if (savedData.Count > MaxSnapshots)
			savedData.RemoveAt(savedData.Count - 1);

		// Update created snapshot count
		if (savedData.Count > SnapshotCount)
			SnapshotCount = savedData.Count;
	}

	void RestoreData(int index)
	{
		if (index < savedData.Count)
		{
			TemporalData data = savedData[index];
			controlledInterface.RestorePropertiesTemporalState(data.PropertyValues);
			controlledInterface.RestoreCustomTemporalState(data.CustomValues);
		}
		else
		{
			// No data saved for this index
			controlledInterface.RestoreCustomTemporalState(null);
		}
	}

	void ClearData()
	{
		savedData.Clear();
		previousSnapshotNumber = 0;
	}

	void ClearDataFromIndex(int index)
	{
		savedData.RemoveRange(0, index);
		previousSnapshotNumber = 0;
	}
    #endregion
}
