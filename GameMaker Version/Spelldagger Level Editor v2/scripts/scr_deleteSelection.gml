///scr_deleteSelection(grid,selectRows)

//variables
var grid=argument[0];
var selectRows=argument[1];

//delete selection
for (var i=0; i<array_height_2d(selectRows); i+=1)
    {
    for (var j=0; j<array_length_2d(selectRows,i); j+=1)
        scr_deleteRow(grid[i],selectRows[i,j])
    }
