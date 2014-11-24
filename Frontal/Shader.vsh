attribute vec4 position;
attribute vec4 color;

varying vec4 colorVarying;

uniform mat4 model;
uniform mat4 view;
uniform mat4 proj;

void main()
{
    gl_Position = proj * view * model * position;

    colorVarying = color;
}
