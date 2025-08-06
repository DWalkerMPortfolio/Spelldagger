///scr_stringPosExt(string,substring,start,backwards)

//variables
var str=argument[0]
var substring=argument[1]
var start=argument[2]
var dir=-((argument[3]*2)-1)

//last line
for (var i=start; i>=1 and i<=string_length(str); i+=1*dir)
    {
    if string_char_at(str,i)=substring
        return i;
    }

return -1;
