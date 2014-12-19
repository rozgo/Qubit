#version 300 es

precision highp float;

uniform sampler2D chan0;

in vec4 colorVarying;
in vec2 uvVarying;

out vec4 finalColor;

void main ()
{
	vec4 sample = texture (chan0, uvVarying);
    finalColor = sample;
}

