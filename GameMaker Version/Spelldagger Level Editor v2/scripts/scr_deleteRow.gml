///scr_deleteRow(grid,rowIndex)

//variables
var deleteGrid=argument[0];
var deleteGridWidth=ds_grid_width(deleteGrid);
var deleteGridHeight=ds_grid_height(deleteGrid);
var deleteY=argument[1];

//move lower rows up
for (var deleteY; deleteY<deleteGridHeight-1; deleteY+=1)
    {
    for (var deleteX=0; deleteX<deleteGridWidth; deleteX+=1)
        ds_grid_set(deleteGrid,deleteX,deleteY,ds_grid_get(deleteGrid,deleteX,deleteY+1))
    }

//resize grid    
if ds_grid_height(deleteGrid)>1
    ds_grid_resize(deleteGrid,deleteGridWidth,deleteGridHeight-1)
else
    {
    ds_grid_destroy(deleteGrid)
    deleteGrid=ds_grid_create(deleteGridWidth,0)
    }
