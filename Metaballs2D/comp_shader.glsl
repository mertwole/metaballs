#version 440 core
layout(local_size_x = 32) in;

struct metaball
{
	vec4 pos_velocity;
	vec4 color_charge;
};

layout(binding = 0, std430) buffer METABALLS
{
	metaball[] metaballs;
};

void main()
{
	uint ball_id = gl_GlobalInvocationID.x;

	metaballs[ball_id].pos_velocity.xy += metaballs[ball_id].pos_velocity.zw;

	if(metaballs[ball_id].pos_velocity.x < -1 || metaballs[ball_id].pos_velocity.x > 1)
		metaballs[ball_id].pos_velocity.z *= -1;

	if(metaballs[ball_id].pos_velocity.y < -1 || metaballs[ball_id].pos_velocity.y > 1)
		metaballs[ball_id].pos_velocity.w *= -1;
}