///scr_loadedPlayer(x,y,influence)

//variables
var X=argument[0]
var Y=argument[1]
var influence=argument[2]

var intersecting=-1

//loaded player
for (var i=0; i<=obj_control.players-1; i+=1)
    { 
    var intersection=rectangle_in_rectangle(X-influence,Y-influence,X+influence,Y+influence,obj_control.loadX[0],obj_control.loadY[0],obj_control.loadX[0]+obj_control.sectorWidth*2,obj_control.loadY[0]+obj_control.sectorHeight*2)
    if intersection=1
        return i
    else if intersection=2
        intersecting=i
    }
    
return intersecting
