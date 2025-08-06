///scr_message(message, player)

//variables
var message=argument[0]
var player=argument[1]

//message
obj_control.message[player]=message
obj_control.displayMessage[player]=true
