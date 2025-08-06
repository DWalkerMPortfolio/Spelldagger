///scr_exportGrid(grid,gridName,instance,addID,firstXColumn,[lastXColumn]) |must have .ini open|

//variables
var grid=argument[0]
var gridName=argument[1]
var instance=argument[2]
var addID=argument[3]
var firstXColumn=argument[4]

//editor values
var sectorWidth=obj_control.sectorWidth
var sectorHeight=obj_control.sectorHeight
var maxSectorX=obj_control.maxSectorX
var maxSectorY=obj_control.maxSectorY
var maxLayer=obj_control.maxLayer
var objectGrid=obj_control.objectGrid

//find max height
var maxHeight=0
var maxWidth=0
for (var i=0; i<=maxLayer; i+=1)
    {
    maxHeight=max(maxHeight,ds_grid_height(grid[i]))
    maxWidth=max(maxWidth,ds_grid_width(grid[i]))
    }

//find lastXColumn
if argument_count>=6
    var lastXColumn=argument[5]
else
    var lastXColumn=maxWidth-1
    
//find ID column
if addID=true
    IDColumn=-1
else if instance=true
    IDColumn=-2

//export grid (i=layer, j=sectorX, k=sectorY, l=gridY, m=gridX, n=add Y-offset)
for (var i=0; i<=maxLayer; i+=1)
    {
    for (var j=0; j<maxSectorX; j+=1)
        {
        for (var k=0; k<maxSectorY; k+=1)
            {
            //initialize save grid
            var width=ds_grid_width(grid[i])+addID+instance
            var saveGrid=ds_grid_create(width,0)
            
            for (var l=0; l<ds_grid_height(grid[i]); l+=1)
                {
                //get influence
                if instance=true
                    var influence=ds_grid_get(objectGrid,4,ds_grid_value_y(objectGrid,1,0,1,ds_grid_height(objectGrid)-1,ds_grid_get(grid[i],0,l)))
                else
                    var influence=1
                
                //get ID
                if addID=true
                    var ID=l+i*maxHeight
                else if instance=true
                    var ID=ds_grid_get(grid[i],4,l)
                
                for (var m=firstXColumn; m<=lastXColumn and m<ds_grid_width(grid[i]); m+=2)
                    {
                    //check if in sector
                    var X=ds_grid_get(grid[i],m,l)
                    var Y=ds_grid_get(grid[i],m+1,l)
                    var sectorX=j*sectorWidth
                    var sectorY=k*sectorHeight
                    
                    if X="empty"
                        break;                    
                    else if rectangle_in_rectangle(X-influence,Y-influence,X+influence,Y+influence,sectorX,sectorY,sectorX+sectorWidth,sectorY+sectorHeight)
                        {
                        //check if already saved
                        if (instance=false and addID=false) or !ds_grid_value_exists(saveGrid,ds_grid_width(saveGrid)+IDColumn,0,ds_grid_width(saveGrid)+IDColumn,ds_grid_height(saveGrid)-1,ID)
                            {
                            var saveRow=ds_grid_height(saveGrid)
                            ds_grid_resize(saveGrid,ds_grid_width(saveGrid),ds_grid_height(saveGrid)+1)
                            ds_grid_set_grid_region(saveGrid,grid[i],0,l,ds_grid_width(grid[i])-1,l,0,saveRow)
                            
                            //add Y-offset
                            for (var n=firstXColumn+1; n<=lastXColumn+1 and n<ds_grid_width(grid[i]); n+=2)
                                ds_grid_add(saveGrid,n,saveRow,i*maxSectorY*sectorHeight)
                            
                            //append ID
                            if addID=true
                                ds_grid_set(saveGrid,ds_grid_width(saveGrid)-1,saveRow,ID)
                            
                            //append influence
                            if instance=true
                                ds_grid_set(saveGrid,ds_grid_width(saveGrid)-1,saveRow,influence)
                            
                            break;
                            }
                        }
                    }
                }
            //condense grid
            scr_condenseGrid(saveGrid,addID)
            
            //write to .ini
            ini_write_string(string(j)+","+string(k)+","+string(i),gridName,ds_grid_write(saveGrid))
            
            //destroy grid
            ds_grid_destroy(saveGrid)
            }
        }
    }
    
//slightly less old export system (for reference if something breaks)
/*         
    //initialize save grid (j=sectorX, k=sectorY) +
    var width=ds_grid_width(grid[i])
    var saveGrid=array_create(0)
    for (var j=maxSectorX; j>=0; j-=1)
        {
        for (var k=maxSectorY; k>=0; k-=1)
            saveGrid[j,k]=ds_grid_create(width+1,0)
        }
    
    //populate save grid (j=gridY, k=gridX, l=influence corner)
    for(var j=0; j<ds_grid_height(grid[i]); j+=1)
        {
        //get infuence
        if instance=true
            var influence=ds_grid_get(obj_control.objectGrid,4,ds_grid_get(grid,0,j))
        for (var k=firstXColumn; k<=min(ds_grid_width(grid[i]),lastXColumn); k+=2)
            {
            if  ds_grid_get(grid[i],k,j)!="empty"
                {
                var currentSaveSectorX=floor(ds_grid_get(grid[i],k,j)/sectorWidth)
                var currentSaveSectorY=floor(ds_grid_get(grid[i],k+1,j)/sectorHeight)
                var currentSaveGrid=saveGrid[currentSaveSectorX,currentSaveSectorY]
                if IDColumn="none" or !ds_grid_value_exists(currentSaveGrid,ds_grid_width(currentSaveGrid)+IDColumn,0,ds_grid_width(currentSaveGrid)+IDColumn,ds_grid_height(currentSaveGrid)-1,i*maxHeight+j)
                    {
                    ds_grid_resize(currentSaveGrid,ds_grid_width(currentSaveGrid),ds_grid_height(currentSaveGrid)+1)
                    ds_grid_set_grid_region(currentSaveGrid,grid[i],0,j,ds_grid_width(grid[i])-1,j,0,ds_grid_height(currentSaveGrid)-1)
                    if addID=true   //Set unique ID (if add ID)
                        ds_grid_set(currentSaveGrid,ds_grid_width(currentSaveGrid)-1,ds_grid_height(currentSaveGrid)-1,i*maxHeight+j)
                    if instance=true    //Append influence (if instance)
                        {
                        var object=ds_grid_get(currentSaveGrid,0,ds_grid_height(currentSaveGrid)-1)
                        var influence=ds_grid_get(objectGrid,4,ds_grid_value_y(objectGrid,1,0,1,ds_grid_height(objectGrid)-1,object))
                        ds_grid_set(currentSaveGrid,ds_grid_width(currentSaveGrid)-1,ds_grid_height(currentSaveGrid)-1,influence)
                        }
                    }
                }
            else
                break;
            }
        }
          
    //final processing (j=sectorX, k=sectorY, l=gridY, m=gridX)
    for (var j=0; j<=maxSectorX; j+=1)
        {
        for (var k=0; k<maxSectorY; k+=1)
            {
            //add y-offset
            for (var l=0; l<ds_grid_height(saveGrid[j,k]); l+=1)
                {
                for (var m=firstXColumn+1; m<=lastXColumn+1; m+=2)
                    {
                    if ds_grid_get(saveGrid[j,k],m,l)!="empty"
                        ds_grid_add(saveGrid[j,k],m,l,i*maxSectorY*sectorHeight)
                    else
                        break;
                    }
                }
                
            //condense grid
            scr_condenseGrid(saveGrid[j,k],1)
            
            //write to ini
            ini_write_string(string(j)+","+string(k)+","+string(i),gridName,ds_grid_write(saveGrid[j,k]))
            
            //destroy grid
            ds_grid_destroy(saveGrid[j,k])
            }
        }
    }
