#version 440 core

out vec4 color;

struct metaball
{
	vec3 pos_charge;
	vec3 color;
};

layout(binding = 0, std430) buffer METABALLS
{
	metaball[] metaballs;
};

in vec2 Pos;

uniform float threshold;

float ChargeEvalFunc(vec3 ball_pos_charge)
{
	vec2 dist_vec = ball_pos_charge.xy - Pos;
	return ball_pos_charge.z / dot(dist_vec, dist_vec);
}

void main()
{
	float charge = 0;
	vec3 blended_color;

	for(int i = 0; i < metaballs.length(); i++)
	{
		float this_charge = ChargeEvalFunc(metaballs[i].pos_charge);
		charge += this_charge;
		blended_color += this_charge * metaballs[i].color;
	}

	if(charge > threshold)
		color = vec4(blended_color / charge, 1);
	else
		discard;
}