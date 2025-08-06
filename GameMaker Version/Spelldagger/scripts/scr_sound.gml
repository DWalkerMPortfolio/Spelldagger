///scr_sound(x,y,range,[ignoreGuard])

sound=instance_create(argument[0],argument[1],class_sound)
sound.range=argument[2]
sound.creator=self
if argument_count=4
    sound.ignore=argument[3]
