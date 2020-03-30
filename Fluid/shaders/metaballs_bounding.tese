#version 440 core
layout(quads, equal_spacing, ccw) in;

uniform mat4 transform_mat;
// defined as max_influence_radius / charge ( particularly, max_influence_radius when charge = 1 )
uniform float cutoff_radius_k;

in float[] charge;

#define PI 3.14

void main()
{
	vec3 center = gl_in[0].gl_Position.xyz;
	float radius = cutoff_radius_k * charge[0];

	float a = gl_TessCoord.x * 2 * PI;
	float b = gl_TessCoord.y * PI;

	float sinb = sin(b);
	gl_Position = vec4(vec3(sinb * cos(a),sinb * sin(a), cos(b)) * radius + center, 1);

	gl_Position = transform_mat * gl_Position;
}