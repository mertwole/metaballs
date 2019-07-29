#version 440 core

layout(location = 0) in vec2 pos;

out vec2 Pos;

void main()
{
	gl_Position = vec4(pos, 0, 1);

	Pos = pos;
}