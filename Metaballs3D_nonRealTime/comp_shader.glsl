#version 440 core
layout(local_size_x = 8, local_size_y = 8, local_size_z = 8) in;

//--------------------metaballs-------------------
struct metaball
{
	vec4 position;
	vec4 color_charge;
};

layout(binding = 0, std430) buffer METABALLS
{
	metaball[] metaballs;
};

uniform float MarchingCubesStep;
uniform vec3 MarchingCubesMin;
uniform float threshold;

float GetCharge(vec3 position)
{
	float charge = 0;

	for(int i = 0; i < metaballs.length(); i++)
	{
		vec3 deltapos = metaballs[i].position.xyz - position;
		charge += metaballs[i].color_charge.w / max(dot(deltapos, deltapos), 0.0001);
	}

	return charge;
}
//-------------marching cubes-----------------------
const vec3[] MidEdgePositions = vec3[]
(
	vec3(-1, 0, 1),
	vec3(1, 0, 1),
	vec3(0, -1, 1),
	vec3(0, 1, 1),
	vec3(1, 1, 0),
	vec3(1, -1, 0),
	vec3(-1, 1, 0),
	vec3(-1, -1, 0),
	vec3(-1, 0, -1),
	vec3(1, 0, -1),
	vec3(0, -1, -1),
	vec3(0, 1, -1)
);

layout(std430, binding = 1) buffer Cubes
{
	int[12 * 128] pointIndicesByCubeID;// 12 ints per cube
};
//-------------------------------------------------------
layout(binding = 2, std430) buffer MESH
{
	vec4[] vertices;
};

uniform float default_vert_value;

void main()
{
	uint global_index = gl_GlobalInvocationID.x + gl_GlobalInvocationID.y * gl_NumWorkGroups.x * 8 + gl_GlobalInvocationID.z * gl_NumWorkGroups.x * gl_NumWorkGroups.y * 8 * 8;

	float mcs = MarchingCubesStep * 0.5;
	vec3 center_position = gl_GlobalInvocationID * vec3(MarchingCubesStep) + vec3(mcs);	

	int cube_id = 0;
	cube_id += GetCharge(center_position + vec3(-mcs, -mcs,-mcs))	> threshold ? 1 : 0;
	cube_id += GetCharge(center_position + vec3(mcs, -mcs, -mcs))	> threshold ? 2 : 0;
	cube_id += GetCharge(center_position + vec3(-mcs, mcs, -mcs))	> threshold ? 4 : 0;
	cube_id += GetCharge(center_position + vec3(mcs, mcs, -mcs ))	> threshold ? 8 : 0;
	cube_id += GetCharge(center_position + vec3(-mcs, -mcs,mcs ))	> threshold ? 16 : 0;
	cube_id += GetCharge(center_position + vec3(mcs, -mcs, mcs ))	> threshold ? 32 : 0;
	cube_id += GetCharge(center_position + vec3(-mcs, mcs, mcs ))	> threshold ? 64 : 0;
	cube_id += GetCharge(center_position + vec3(mcs, mcs, mcs  ))	> threshold ? 128 : 0;

	cube_id = min(cube_id, 255 - cube_id);

	int cube_start = cube_id * 12;

	vertices[global_index] = vec4(default_vert_value);

	if(cube_id != 0)
	{
		int i = 0;

		while(pointIndicesByCubeID[cube_start + i] != -1)
		{
		
			vec3 pos_0 = (center_position + vec3(MidEdgePositions[pointIndicesByCubeID[cube_start + i]] * mcs));
			vec3 pos_1 = (center_position + vec3(MidEdgePositions[pointIndicesByCubeID[cube_start + i + 1]] * mcs));
			vec3 pos_2 = (center_position + vec3(MidEdgePositions[pointIndicesByCubeID[cube_start + i + 2]] * mcs));

			vertices[global_index * 12 + i / 3].xyz = pos_0;
			vertices[global_index * 12 + i / 3 + 1].xyz = pos_1;
			vertices[global_index * 12 + i / 3 + 2].xyz = pos_2;

			i += 3;
			if(i == 12)
				break;
				
		}
	}
	
}