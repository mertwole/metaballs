#version 440 core

layout(location = 0) in float none;

out VS_OUT {
    int id;
} vs_out;

void main()
{
	gl_Position = vec4(0);

	vs_out.id = gl_InstanceID;
}