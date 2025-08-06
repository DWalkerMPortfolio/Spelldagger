using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Defines a node that can be controlled by a temporal controller
/// </summary>
public interface ITemporalControl
{
    /// <summary>
    /// Return the properties that's values should be saved as temporal data
    /// </summary>
    /// <returns>The properties that's values should be saved as temporal data</returns>
    public string[] GetTemporalProperties();

    /// <summary>
    /// Gets the value of each temporal property
    /// </summary>
    /// <param name="temporalProperties">The properties to save the value of</param>
    /// <returns>A dictionary of property names to property values</returns>
    public Dictionary<string, Variant> SavePropertiesTemporalState(string[] temporalProperties)
    {
        if (temporalProperties == null)
            return null;

        Dictionary<string, Variant> data = new Dictionary<string, Variant>();
        GodotObject gdObject = (GodotObject)this;
        foreach (string property in temporalProperties)
        {
            data.Add(property, gdObject.Get(property));
        }
        return data;
    }

    /// <summary>
    /// Sets the value of each temporal property
    /// </summary>
    /// <param name="propertyData">The names and saved values of each temporal property</param>
    public void RestorePropertiesTemporalState(Dictionary<string, Variant> propertyData)
    {
        if (propertyData == null)
            return;

        GodotObject gdObject = (GodotObject)this;
        foreach (string property in propertyData.Keys)
        {
            gdObject.Set(property, propertyData[property]);
        }
    }

    /// <summary>
    /// Use to perform pre-processing before saving temporal state
    /// </summary>
    /// <returns>Any additional custom data to save</returns>
    public Dictionary<string, Variant> SaveCustomTemporalState()
    {
        return null;
    }

    /// <summary>
    /// Use to perform post-processing after loading temporal state
    /// </summary>
    /// <param name="customData">Any custom data returned from OnSaveTemporalState</param>
    public void RestoreCustomTemporalState(Dictionary<string, Variant> customData)
    {

    }
}
