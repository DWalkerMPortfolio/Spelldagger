///scr_targetInstance(X,Y,layer)

//variables
var X=argument[0]
var Y=argument[1]
var layer=argument[2]
var targetInstanceRow=-1

//target instance
for (var i=0; i<ds_grid_height(instanceGrid[layer]); i+=1)
    {
    if ds_grid_get(instanceGrid[layer],1,i)=X and ds_grid_get(instanceGrid[layer],2,i)=Y
        {
        var targetInstanceRow=i
        break;
        }
    }
    
return targetInstanceRow;
