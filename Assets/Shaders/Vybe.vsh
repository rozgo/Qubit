#version 300 es

#define PI 3.1415926535897932384626433832795

uniform mat4 model;
uniform mat4 view;
uniform mat4 proj;
uniform sampler2D chan0;

in vec4 position;
in vec4 color;
in vec4 uv;

out vec4 colorVarying;
out vec2 uvVarying;



void main ()
{
	float a = (float (gl_InstanceID) / 10.0) * 2.0 * PI;
	float r = 6.0;
	vec4 i = vec4 (r * cos(a), r * sin(a), 0.0, 0.0);
	gl_Position = proj * view * model * (position + i);
    //gl_Position = proj * view * model * position;
    colorVarying = color;
    uvVarying = uv.xy;
}
