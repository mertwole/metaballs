#version 440 core

out vec4 color;

struct metaball
{
	vec4 pos_velocity;
	vec4 color_charge;
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
		float this_charge = ChargeEvalFunc(vec3(metaballs[i].pos_velocity.xy, metaballs[i].color_charge.w));
		charge += this_charge;
		blended_color += this_charge * metaballs[i].color_charge.xyz;
	}

	if(charge > threshold)
		color = vec4(blended_color / charge, 1);
	else
		color = vec4(0);
}