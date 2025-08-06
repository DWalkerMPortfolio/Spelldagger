///scr_selectRows(grid,minSelectLayer,maxSelectLayer,selectPoints,columnX,columnY)

var grid=argument[0];
var MinSelectLayer=argument[1];
var MaxSelectLayer=argument[2];
var selectPoints=argument[3];
var columnX=argument[4];
var columnY=argument[5];
var selectRows=array_create(0);

for (var i=minSelectLayer; i<=maxSelectLayer; i+=1)
    {
    for (var j=0; j<ds_grid_height(grid[i]); j+=1)
        {
        if ds_grid_get(grid[i],columnX,j)>=selectPoints[0,0] and ds_grid_get(grid[i],columnY,j)>=selectPoints[1,0] and ds_grid_get(grid[i],columnX,j)<=selectPoints[0,1] and ds_grid_get(grid[i],columnY,j)<=selectPoints[1,1]
            selectRows[i,array_length_2d(selectRows,i)]=j
        }
    }
    
return selectRows
