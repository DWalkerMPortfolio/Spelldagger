///scr_findRow(grid,column,value,[column2],[value2],...)

//variables
var grid=argument[0]
var column=array_create(0)
var value=array_create(0)
for (i=1; i<argument_count; i+=2)
    {
    column[array_length_1d(column)]=argument[i]
    value[array_length_1d(value)]=argument[i+1]
    }

//find row
for (i=0; i<ds_grid_height(grid); i+=1)
    {
    for (j=0; j<array_length_1d(column); j+=1)
        {
        if ds_grid_get(grid,column[j],i)!=value[j]
            break
        if j=array_length_1d(column)-1
            return i
        }
    }
return -1
