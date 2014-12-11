precision highp float;

uniform sampler2D chan0;

varying vec4 colorVarying;
varying vec2 uvVarying;

void main()
{
	vec4 sample = texture2D(chan0, uvVarying);
    gl_FragColor = sample;
}
