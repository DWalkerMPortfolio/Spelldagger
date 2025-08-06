///scr_drawArrow(X1,Y1,X2,Y2,width)

//variables
var X1=argument[0]
var Y1=argument[1]
var X2=argument[2]
var Y2=argument[3]
var width=argument[4]
var reverseArrowDirection=point_direction(X2,Y2,X1,Y1)
var headSize=width*4

//draw arrow
draw_line_width(X1,Y1,X2,Y2,width)
draw_triangle(X2,Y2,X2+lengthdir_x(headSize,reverseArrowDirection+45),Y2+lengthdir_y(headSize,reverseArrowDirection+45),X2+lengthdir_x(headSize,reverseArrowDirection-45),Y2+lengthdir_y(headSize,reverseArrowDirection-45),false)
