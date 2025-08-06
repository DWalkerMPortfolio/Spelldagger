///FINISH scr_selectRows(grid,selectRows,layer)

//variables
var grid=argument[0];
var selectRows=argument[1];
var layer=argument[2];

//select rows
for (var selectValue=0; selectValue<ds_grid_height(wallGrid[selectLayer]); selectValue+=1)
    {
    if ds_grid_get(wallGrid[selectLayer],0,selectValue)>=X1 and ds_grid_get(wallGrid[selectLayer],1,selectValue)>=Y1 and ds_grid_get(wallGrid[selectLayer],0,selectValue)<=X2 and ds_grid_get(wallGrid[selectLayer],1,selectValue)<=Y2
        selectRows[selectLayer,array_length_2d(selectRowsWall,selectLayer)]=selectValue
    }
    
return selectArray
