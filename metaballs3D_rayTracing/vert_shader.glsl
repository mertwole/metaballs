#version 430 core

layout (location = 0) in vec2 position; 

out vec2 tex_coord;

void main(){
	gl_Position = vec4(position, 0, 1);
	tex_coord = (position + vec2(1)) * 0.5;
}