///scr_condenseGrid(grid,[ignoreLastColumnNumber])

//variables
var grid=argument[0]
if argument_count>=2
    var ignoreLastColumnNumber=argument[1]
else
    var ignoreLastColumnNumber=0

//check and resize grid
if ds_grid_height(grid)>0
    {
    var breaked=false
    while breaked=false
        {
        for (var i=0; i<ds_grid_height(grid); i+=1)
            {
            if ds_grid_get(grid,ds_grid_width(grid)-1-ignoreLastColumnNumber,i)!="empty"
                {
                breaked=true
                break;
                }
            if i=ds_grid_height(grid)-1
                {
                if ignoreLastColumnNumber>0
                    ds_grid_set_grid_region(grid,grid,ds_grid_width(grid)-ignoreLastColumnNumber,0,ds_grid_width(grid)-ignoreLastColumnNumber,ds_grid_height(grid),ds_grid_width(grid)-1-ignoreLastColumnNumber,0)
                ds_grid_resize(grid,ds_grid_width(grid)-1,ds_grid_height(grid))
                }
            }
        }
    }
