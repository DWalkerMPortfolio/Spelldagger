///scr_moveSelectionLayer(grid,selectRows,layer,layerPrevious)

//variables
var grid=argument[0];
var selectRows=argument[1];
var layer=argument[2];
var layerPrevious=argument[3];

//move selection layer
for (var i=0; i<array_length_2d(selectRows,layerPrevious); i+=1)
    {
    copyValue=ds_grid_height(grid[layer])
    ds_grid_resize(grid[layer],ds_grid_width(grid[layer]),ds_grid_height(grid[layer])+1)
    ds_grid_add_grid_region(grid[layer],grid[layerPrevious],0,selectRowsWall[layerPrevious,i],ds_grid_width(grid[layerPrevious])-1,selectRowsWall[layerPrevious,i],0,copyValue)
    scr_deleteRow(grid[layerPrevious],selectRowsWall[layerPrevious,i])
    selectRowsWall[layerPrevious,i]=copyValue
    }
