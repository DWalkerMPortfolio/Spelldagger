///scr_moveSelection(grid,selectRows,selectOffsetX,selectOffsetY,firstXColumn,[lastXColumn])

//variables
var grid=argument[0];
var selectRows=argument[1];
var selectOffsetX=argument[2];
var selectOffsetY=argument[3];
var firstXColumn=argument[4];

//set lastXColumn
if argument_count=6
    var lastXColumn=argument[5];
else
    {
    var lastXColumn=0
    for (var i=0; i<array_height_2d(selectRows); i+=1)
       lastXColumn=max(ds_grid_width(grid[i]),lastXColumn) 
    }
    
//move selection    
for (var i=0; i<array_height_2d(selectRows); i+=1)
    {
    for (var j=0; j<array_length_2d(selectRows,i); j+=1)
        {
        for (var k=firstXColumn; k<=lastXColumn; k+=2)
            {
            ds_grid_add(grid[i],k,selectRows[i,j],selectOffsetX)
            ds_grid_add(grid[i],k+1,selectRows[i,j],selectOffsetY)
            }
        }
    }
