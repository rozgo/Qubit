precision highp float;

uniform vec2 resolution;
varying vec2 vUV;

float sdSphere( vec3 p, float s )
{
  return length(p)-s;
}

void main() {

    vec3 eye = vec3(0, 0, -1);
    vec3 up = vec3(0, 1, 0);
    vec3 right = vec3(1, 0, 0);

    vec2 res = vec2(320,180);

    //float u = vUV.x * 2.0 / res.x - 1.0;
    //float v = vUV.y * 2.0 / res.y - 1.0;
    vec3 ro = eye;// + right * u + up * v;
    vec3 forward = normalize(cross(right, up));
    vec3 rd = normalize(forward + right * vUV.x * (res.x/res.y) + up * vUV.y);
    //vec3 rd = normalize(cross(right, up));

    vec4 color = vec4(0.0); // Sky color
    float g_rmEpsilon = 0.001;

    float t = 0.0;
    const int maxSteps = 32;
    for(int i = 0; i < maxSteps; ++i)
    {
        vec3 p = ro + rd * t;
        float d = sdSphere(p, 0.5);
        if(d < g_rmEpsilon)
        {
            color = vec4(1.0-t); // Sphere color
            break;
        }

        t += d;
    }

    if (vUV.x < -0.9 || vUV.x > 0.9) {
    	color.x = 1.0;
    }
	//
    //if (vUV.y > -0.1 && vUV.y < 0.1) {
    //	color.y = 1.0;
    //}

    // vUV is equal to gl_FragCoord/uScreenResolution
    // do some pixel shader related work
    //gl_FragColor = vec4(vUV.x,vUV.y,1,1);
    //gl_FragColor = vec4(1,0,0,1);
    gl_FragColor = color;
}
