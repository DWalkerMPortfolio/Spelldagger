///scr_loadGrid(grid,storedGrid,loadPlayer,[layerOffset])

//variables
var grid=argument[0]
var storedGrid=argument[1]
var loadPlayer=argument[2]
if argument_count>=4
    var layerOffset=argument[3]
else
    var layerOffset=0

var grid=grid[loadPlayer]

var gridLayer=obj_control.layer[loadPlayer]+layerOffset
//check for layer out of bounds
if gridLayer<0 or gridLayer>obj_control.maxLayer
    {
    ds_grid_destroy(grid)
    grid=ds_grid_create(0,0)
    return false
    }

var gridX=obj_control.sectorX[loadPlayer]
var gridY=obj_control.sectorY[loadPlayer]+gridLayer*obj_control.maxSectorY

var storedGridSector1=storedGrid[gridX,gridY]
var storedGridSector2=storedGrid[gridX+1,gridY]
var storedGridSector3=storedGrid[gridX,gridY+1]
var storedGridSector4=storedGrid[gridX+1,gridY+1]

var totalHeight=ds_grid_height(storedGridSector1)+ds_grid_height(storedGridSector2)+ds_grid_height(storedGridSector3)+ds_grid_height(storedGridSector4)
var maxWidth=max(ds_grid_width(storedGridSector1),ds_grid_width(storedGridSector2),ds_grid_width(storedGridSector3),ds_grid_width(storedGridSector4))

//load grid
if totalHeight=0
    {
    ds_grid_destroy(grid)
    grid=ds_grid_create(0,0)
    }
else
    {
    ds_grid_resize(grid,maxWidth,max(1,totalHeight))
    ds_grid_clear(grid,0)
    ds_grid_add_grid_region(grid,storedGridSector1,0,0,ds_grid_width(storedGridSector1)-1,ds_grid_height(storedGridSector1)-1,0,0)
    ds_grid_add_grid_region(grid,storedGridSector2,0,0,ds_grid_width(storedGridSector2)-1,ds_grid_height(storedGridSector2)-1,0,ds_grid_height(storedGridSector1))
    ds_grid_add_grid_region(grid,storedGridSector3,0,0,ds_grid_width(storedGridSector3)-1,ds_grid_height(storedGridSector3)-1,0,ds_grid_height(storedGridSector1)+ds_grid_height(storedGridSector2))
    ds_grid_add_grid_region(grid,storedGridSector4,0,0,ds_grid_width(storedGridSector4)-1,ds_grid_height(storedGridSector4)-1,0,ds_grid_height(storedGridSector1)+ds_grid_height(storedGridSector2)+ds_grid_height(storedGridSector3))
    }
