///scr_floor(x,y,textureGridIndex)

if obj_control.sectorChanged[argument[2]]=true or obj_control.initializedLevelData[argument[2]]=false
    return true
else
    {
    for (var floorValue=0; floorValue<ds_grid_height(obj_control.textureGrid[argument[2]]); floorValue+=1)
        {
        var originX=ds_grid_get(obj_control.textureGrid[argument[2]],1,floorValue)
        var originY=ds_grid_get(obj_control.textureGrid[argument[2]],2,floorValue)
        for (var floorPointValue=3; floorPointValue<ds_grid_width(obj_control.textureGrid[argument[2]])-3; floorPointValue+=2)
            {
            var X1=ds_grid_get(obj_control.textureGrid[argument[2]],floorPointValue,floorValue)
            var Y1=ds_grid_get(obj_control.textureGrid[argument[2]],floorPointValue+1,floorValue)
            var X2=ds_grid_get(obj_control.textureGrid[argument[2]],floorPointValue+2,floorValue)
            var Y2=ds_grid_get(obj_control.textureGrid[argument[2]],floorPointValue+3,floorValue)
            if X1>0 and Y1>0 and X2>0 and Y2>0 and point_in_triangle(argument[0],argument[1],originX,originY,X1,Y1,X2,Y2)
                return true
            }
        }
    }

return false
