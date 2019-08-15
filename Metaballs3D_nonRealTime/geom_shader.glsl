#version 440 core

layout (triangles) in;
layout (triangle_strip, max_vertices = 3) out;

out vec3 normal;
out vec3 pos;

void main() 
{    
	vec4 pos_0 = gl_in[0].gl_Position;
	vec4 pos_1 = gl_in[1].gl_Position;
	vec4 pos_2 = gl_in[2].gl_Position;

	normal = cross(pos_0.xyz - pos_1.xyz, pos_1.xyz - pos_2.xyz);

	gl_Position = pos_0;
	pos = pos_0.xyz;
	EmitVertex();

	gl_Position = pos_1;
	pos = pos_1.xyz;
	EmitVertex();

	gl_Position = pos_2;
	pos = pos_2.xyz;
	EmitVertex();				

	EndPrimitive();
}  