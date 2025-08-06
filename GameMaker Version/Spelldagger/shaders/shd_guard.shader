//
// Simple passthrough vertex shader
//
attribute vec3 in_Position;                  // (x,y,z)
//attribute vec3 in_Normal;                  // (x,y,z)     unused in this shader.	
attribute vec4 in_Colour;                    // (r,g,b,a)
attribute vec2 in_TextureCoord;              // (u,v)

varying vec2 v_vTexcoord;
varying vec4 v_vColour;

void main()
{
    vec4 object_space_pos = vec4( in_Position.x, in_Position.y, in_Position.z, 1.0);
    gl_Position = gm_Matrices[MATRIX_WORLD_VIEW_PROJECTION] * object_space_pos;
    
    v_vColour = in_Colour;
    v_vTexcoord = in_TextureCoord;
}

//######################_==_YOYO_SHADER_MARKER_==_######################@~//
// Color replacement shader
//
varying vec2 v_vTexcoord;
varying vec4 v_vColour;

vec3 colorMatch1 = vec3(1, 0, 1);
vec3 colorMatch2 = vec3(1, 1, 0);

vec3 guardColor00 = vec3(0.552, 0.552, 0.552);
vec3 guardColor01 = vec3(0.4, 0.4, 0.4);

vec3 guardColor10 = vec3(0.4, 0, 0.533);
vec3 guardColor11 = vec3(0.271, 0, 0.357);

vec3 guardColor20 = vec3(0.522, 0.012, 0.086);
vec3 guardColor21 = vec3(0.376, 0.007, 0.063);

uniform int guardColorIndex;

void main()
{
    vec4 pixelColor = v_vColour * texture2D( gm_BaseTexture, v_vTexcoord );
    if (pixelColor.rgb == colorMatch1)
        {
        if (guardColorIndex == 0)
            pixelColor.rgb = guardColor00;
        if (guardColorIndex == 1)
            pixelColor.rgb = guardColor10;
        if (guardColorIndex == 2)
            pixelColor.rgb = guardColor20;
        }
    if (pixelColor.rgb == colorMatch2)
        {
        if (guardColorIndex == 0)
            pixelColor.rgb = guardColor01;
        if (guardColorIndex == 1)
            pixelColor.rgb = guardColor11;
        if (guardColorIndex == 2)
            pixelColor.rgb = guardColor21;
        }
    gl_FragColor = pixelColor;
}

