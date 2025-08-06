///scr_getReturnedText(textboxID)

//variables
var textboxID=argument[0]

//get returned text
if obj_control.textReceived=false and obj_control.textReturned=textboxID
    {
    obj_control.textReceived=true
    return obj_control.textReturn
    }
else
    return -1
