///scr_shadow(x,y,xSurfaceOrigin,ySurfaceOrigin,xMin,yMin,areaWidth,areaHeight,useDoors,whichGrid)

if argument9!=5
    {
    var gridValue=0
    while gridValue<ds_grid_height(obj_control.wallGrid[argument9])
        {
        var gridValue0=ds_grid_get(obj_control.wallGrid[argument9],0,gridValue)
        var gridValue1=ds_grid_get(obj_control.wallGrid[argument9],1,gridValue)
        var gridValue2=ds_grid_get(obj_control.wallGrid[argument9],2,gridValue)
        var gridValue3=ds_grid_get(obj_control.wallGrid[argument9],3,gridValue)
        if rectangle_in_rectangle(gridValue0,gridValue1,gridValue2+1,gridValue3+1,argument4,argument5,argument4+argument6,argument5+argument7)!=0
            {
            draw_primitive_begin(pr_trianglefan)
            draw_vertex(gridValue0-argument2,gridValue1-argument3)
            draw_vertex(argument0+lengthdir_x(10000,point_direction(argument0,argument1,gridValue0,gridValue1))-argument2,argument1+lengthdir_y(10000,point_direction(argument0,argument1,gridValue0,gridValue1))-argument3)
            draw_vertex(argument0+lengthdir_x(10000,point_direction(argument0,argument1,(gridValue0+gridValue2)/2,(gridValue1+gridValue3)/2))-argument2,argument1+lengthdir_y(10000,point_direction(argument0,argument1,(gridValue0+gridValue2)/2,(gridValue1+gridValue3)/2))-argument3)
            draw_vertex(argument0+lengthdir_x(10000,point_direction(argument0,argument1,gridValue2,gridValue3))-argument2,argument1+lengthdir_y(10000,point_direction(argument0,argument1,gridValue2,gridValue3))-argument3)
            draw_vertex(gridValue2-argument2,gridValue3-argument3)
            draw_primitive_end()
            }
        gridValue+=1
        }
    }
else
    {
    var useGrid=0
    for (useGrid=0; useGrid<obj_control.players; useGrid+=1)
        {
        var gridValue=0
        while gridValue<ds_grid_height(obj_control.wallGrid[useGrid])
            {
            var gridValue0=ds_grid_get(obj_control.wallGrid[useGrid],0,gridValue)
            var gridValue1=ds_grid_get(obj_control.wallGrid[useGrid],1,gridValue)
            var gridValue2=ds_grid_get(obj_control.wallGrid[useGrid],2,gridValue)
            var gridValue3=ds_grid_get(obj_control.wallGrid[useGrid],3,gridValue)
            if rectangle_in_rectangle(gridValue0,gridValue1,gridValue2+1,gridValue3+1,argument4,argument5,argument4+argument6,argument5+argument7)!=0
                {
                draw_primitive_begin(pr_trianglefan)
                draw_vertex(gridValue0-argument2,gridValue1-argument3)
                draw_vertex(argument0+lengthdir_x(10000,point_direction(argument0,argument1,gridValue0,gridValue1))-argument2,argument1+lengthdir_y(10000,point_direction(argument0,argument1,gridValue0,gridValue1))-argument3)
                draw_vertex(argument0+lengthdir_x(10000,point_direction(argument0,argument1,(gridValue0+gridValue2)/2,(gridValue1+gridValue3)/2))-argument2,argument1+lengthdir_y(10000,point_direction(argument0,argument1,(gridValue0+gridValue2)/2,(gridValue1+gridValue3)/2))-argument3)
                draw_vertex(argument0+lengthdir_x(10000,point_direction(argument0,argument1,gridValue2,gridValue3))-argument2,argument1+lengthdir_y(10000,point_direction(argument0,argument1,gridValue2,gridValue3))-argument3)
                draw_vertex(gridValue2-argument2,gridValue3-argument3)
                draw_primitive_end()
                }
            gridValue+=1
            }
        }
    }

if argument8=true
    {   
    var doorGridValue=0
    while doorGridValue<ds_grid_height(obj_control.doorGrid)
        {
        var doorGridValue0=ds_grid_get(obj_control.doorGrid,0,doorGridValue)
        var doorGridValue1=ds_grid_get(obj_control.doorGrid,1,doorGridValue)
        var doorGridValue2=ds_grid_get(obj_control.doorGrid,2,doorGridValue)
        var doorGridValue3=ds_grid_get(obj_control.doorGrid,3,doorGridValue)
        if rectangle_in_rectangle(doorGridValue0,doorGridValue1,doorGridValue2+1,doorGridValue3+1,argument4,argument5,argument4+argument6,argument5+argument7)!=0
            {
            draw_primitive_begin(pr_trianglefan)
            draw_vertex(doorGridValue0-argument2,doorGridValue1-argument3)
            draw_vertex(argument0+lengthdir_x(10000,point_direction(argument0,argument1,doorGridValue0,doorGridValue1))-argument2,argument1+lengthdir_y(10000,point_direction(argument0,argument1,doorGridValue0,doorGridValue1))-argument3)
            draw_vertex(argument0+lengthdir_x(10000,point_direction(argument0,argument1,(doorGridValue0+doorGridValue2)/2,(doorGridValue1+doorGridValue3)/2))-argument2,argument1+lengthdir_y(10000,point_direction(argument0,argument1,(doorGridValue0+doorGridValue2)/2,(doorGridValue1+doorGridValue3)/2))-argument3)
            draw_vertex(argument0+lengthdir_x(10000,point_direction(argument0,argument1,doorGridValue2,doorGridValue3))-argument2,argument1+lengthdir_y(10000,point_direction(argument0,argument1,doorGridValue2,doorGridValue3))-argument3)
            draw_vertex(doorGridValue2-argument2,doorGridValue3-argument3)
            draw_primitive_end()
            }
        doorGridValue+=1
        }
    }
