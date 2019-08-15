#version 440 core

layout(location = 0) in vec3 position;

uniform mat4 transform_mat;

void main()
{
	gl_Position = transform_mat * vec4(position, 1);
}