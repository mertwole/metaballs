#version 440 core

layout (points) in;
layout (triangle_strip, max_vertices = 3) out;

uniform mat4 transform_mat;

out vec3 normal;
out vec3 pos;
out vec3 col;

layout(binding = 2, std430) buffer MESH
{
	vec4[] vertices;
};

in VS_OUT {
    int id;
} gs_in[]; 

void main() 
{    
	vec4 pos_0 = transform_mat * vec4(vertices[gs_in[0].id * 4].xyz, 1);
	vec4 pos_1 = transform_mat * vec4(vertices[gs_in[0].id * 4 + 1].xyz, 1);
	vec4 pos_2 = transform_mat * vec4(vertices[gs_in[0].id * 4 + 2].xyz, 1);

	normal = cross(pos_0.xyz - pos_1.xyz, pos_1.xyz - pos_2.xyz);
	col = vec3(1);

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