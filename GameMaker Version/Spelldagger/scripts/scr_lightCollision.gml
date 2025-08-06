///scr_lightCollision(x,y)

with(class_light)
    {
    if point_distance(x,y,argument0,argument1)<=range
        {
        if collision_line(x,y,argument0,argument1,class_wallCollider,true,true)=noone
            return true
        }
    }
return false
