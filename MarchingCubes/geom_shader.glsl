#version 440 core

layout (points) in;
layout (triangle_strip, max_vertices = 36) out;

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

uniform mat4 transform_mat;

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

out vec3 normal;
out vec3 pos;
out vec3 col;

uniform int show_mesh;
uniform int show_debug;

void main() 
{    
	vec3 center_position = gl_in[0].gl_Position.xyz;
	float mcs = MarchingCubesStep * 0.5;

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

	if(cube_id != 0 && show_mesh == 1)
	{
		int i = 0;

		while(pointIndicesByCubeID[cube_start + i] != -1)
		{
			vec4 pos_0 = transform_mat * (gl_in[0].gl_Position + vec4(MidEdgePositions[pointIndicesByCubeID[cube_start + i]] * mcs, 0));
			vec4 pos_1 = transform_mat * (gl_in[0].gl_Position + vec4(MidEdgePositions[pointIndicesByCubeID[cube_start + i + 1]] * mcs, 0));
			vec4 pos_2 = transform_mat * (gl_in[0].gl_Position + vec4(MidEdgePositions[pointIndicesByCubeID[cube_start + i + 2]] * mcs, 0));

			normal = cross(pos_0.xyz - pos_1.xyz, pos_1.xyz - pos_2.xyz);
			col = vec3(1);

			gl_Position = pos_0;
			pos = pos_0.xyz;
		    EmitVertex();

			gl_Position = pos_1;
			pos = pos_1.xyz;
			EmitVertex();

			gl_Position = pos_2;
			pos = pos_2.xyz;
			EmitVertex();				

			EndPrimitive();

			i += 3;
			if(i == 12)
				break;
		}
	}

	//--------------DEBUG----------------------------
		
	if(show_debug == 1 && cube_id != 0)
	{
		normal = vec3(0);
		pos = gl_in[0].gl_Position.xyz;

		for(int i = 0; i < 8; i++)
		{
			vec3 pos_;

			switch(i)
			{
				case 0 : { pos_ = vec3(-mcs, -mcs, -mcs); break; }
				case 1 : { pos_ = vec3(-mcs, mcs, -mcs); break; }
				case 2 : { pos_ = vec3(mcs, -mcs, -mcs); break; }
				case 3 : { pos_ = vec3(mcs, mcs, -mcs); break; }
				case 4 : { pos_ = vec3(-mcs, -mcs, mcs); break; }
				case 5 : { pos_ = vec3(mcs, -mcs, mcs); break; }
				case 6 : { pos_ = vec3(-mcs, mcs, mcs); break; }
				case 7 : { pos_ = vec3(mcs, mcs, mcs); break; }
			};

			col = GetCharge(center_position + pos_) > threshold ? vec3(100, 0, 0) : vec3(0, 100, 0);

			gl_Position = transform_mat * (gl_in[0].gl_Position + vec4(pos_ + vec3(0.01, 0, 0), 0));			
			EmitVertex();

			gl_Position = transform_mat * (gl_in[0].gl_Position + vec4(pos_ + vec3(0, 0.01, 0), 0));
			EmitVertex();

			gl_Position = transform_mat * (gl_in[0].gl_Position + vec4(pos_, 0));
			EmitVertex();

			EndPrimitive();
		}	

		col = vec3(0, 0, 100);

		gl_Position = transform_mat * (gl_in[0].gl_Position + vec4(mcs, mcs, mcs, 0));
		EmitVertex();
		gl_Position = transform_mat * (gl_in[0].gl_Position + vec4(-mcs, mcs, mcs, 0));
		EmitVertex();
		gl_Position = transform_mat * (gl_in[0].gl_Position + vec4(-mcs, mcs, mcs + 0.01, 0));
		EmitVertex();
		EndPrimitive();

		gl_Position = transform_mat * (gl_in[0].gl_Position + vec4(mcs, mcs, mcs, 0));
		EmitVertex();
		gl_Position = transform_mat * (gl_in[0].gl_Position + vec4(mcs, -mcs, mcs, 0));
		EmitVertex();
		gl_Position = transform_mat * (gl_in[0].gl_Position + vec4(mcs, -mcs, mcs + 0.01, 0));
		EmitVertex();
		EndPrimitive();

		gl_Position = transform_mat * (gl_in[0].gl_Position + vec4(mcs, mcs, mcs, 0));
		EmitVertex();
		gl_Position = transform_mat * (gl_in[0].gl_Position + vec4(mcs, mcs, -mcs, 0));
		EmitVertex();
		gl_Position = transform_mat * (gl_in[0].gl_Position + vec4(mcs, mcs, -mcs + 0.01, 0));
		EmitVertex();
		EndPrimitive();
	}
	//-----------------------------------------------
}  