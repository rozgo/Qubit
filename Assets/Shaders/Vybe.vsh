uniform mat4 model;
uniform mat4 view;
uniform mat4 proj;
uniform sampler2D chan0;

attribute vec4 position;
attribute vec4 color;
attribute vec4 uv;

varying vec4 colorVarying;
varying vec2 uvVarying;



void main()
{
    gl_Position = proj * view * model * position;
    colorVarying = color;
    uvVarying = uv.xy;
}
