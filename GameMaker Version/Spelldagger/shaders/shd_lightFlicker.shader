attribute vec4 in_Position;
attribute vec2 in_TextureCoord;
attribute vec4 in_Colour;
varying vec2 vTc;

void main() 
    {
    gl_Position = gm_Matrices[MATRIX_WORLD_VIEW_PROJECTION] * in_Position;
    vTc = in_TextureCoord;
    }
//######################_==_YOYO_SHADER_MARKER_==_######################@~
varying vec2 vTc;
varying float alpha;
uniform float flickerAlpha;

void main()
    {
    vec4 irgba=texture2D(gm_BaseTexture,vTc);
    gl_FragColor=irgba;
    if (irgba.a>0.1)
        {
        if (irgba.a<1.0)
            {
            gl_FragColor=vec4(irgba.r,irgba.g,irgba.b,flickerAlpha);
            }
        }
    }

