///scr_createTextbox(defaultText,textboxID)

//variables
var defaultText=argument[0]
var textboxID=argument[1]

//create textbox
var created=instance_create(0,0,obj_textbox);
created.defaultText=defaultText
created.text=defaultText
keyboard_string=defaultText
created.textboxID=textboxID
