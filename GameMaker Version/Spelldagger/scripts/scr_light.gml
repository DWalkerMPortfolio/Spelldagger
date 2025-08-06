///scr_light(x,y,range)

var nearestLight=instance_nearest(argument[0],argument[1],class_light)
if nearestLight=noone or point_distance(argument[0],argument[1],nearestLight.x,nearestLight.y)>0
    {
    light=instance_create(argument[0],argument[1],class_light)
    light.creator=self
    light.range=argument[2]
    return light
    }

return noone
