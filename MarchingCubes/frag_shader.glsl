#version 440 core

out vec4 color;

vec3[] lights = vec3[]
(
	vec3(2, 1, 0)
);

vec3 ambient = vec3(0.1);

in vec3 normal;
in vec3 pos;
in vec3 col;

void main()
{
	vec3 diffuse;

	for(int i = 0; i < lights.length(); i++)
		diffuse += vec3(max(dot(normalize(lights[i] - pos), normal), 0));

	color = vec4(col * (ambient + diffuse * 10), 1);
}