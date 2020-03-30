#version 440 core
struct Metaball
{
	vec4 pos;
	vec4 velocity;
	vec4 color_charge;
};

layout(std430, binding = 0) buffer METABALLS
{
	Metaball metaballs[];

};

out float charge;

void main()
{
	gl_Position = vec4(metaballs[gl_InstanceID].pos.xyz, 1.0);
	charge = metaballs[gl_InstanceID].color_charge.w;
}