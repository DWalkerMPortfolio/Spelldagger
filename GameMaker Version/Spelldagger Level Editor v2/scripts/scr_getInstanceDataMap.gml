///scr_getInstanceDataMap(instanceID)

//variables
var instanceID=argument[0]

//get instance data map
if ds_map_exists(dataMap,instanceID)
    return ds_map_find_value(dataMap,instanceID)
else
    {
    var instanceDataMap=ds_map_create()
    ds_map_add(dataMap,instanceID,instanceDataMap)
    return instanceDataMap
    }
