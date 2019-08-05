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

const int[][] pointIndicesByCubeID = int[][]
(
	/*0*/	int[] (-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),
	/*1*/	int[] (7, 8, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1),
	/*2*/	int[] (10, 9, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1),
	/*3*/	int[] (7, 8, 9, 7, 9, 5, -1, -1, -1, -1, -1, -1),
	/*4*/	int[] (6, 11, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1),
	/*5*/	int[] (6, 11, 10, 6, 10, 7, -1, -1, -1, -1, -1, -1),
	/*6*/	int[] (6, 11, 8, 10, 9, 5, -1, -1, -1, -1, -1, -1),
	/*7*/	int[] (5, 7, 6, 5, 6, 9, 9, 6, 11, -1, -1, -1),
	/*8*/	int[] (4, 9, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1),
	/*9*/	int[] (4, 9, 11, 7, 8, 10, -1, -1, -1, -1, -1, -1),
	/*10*/	int[] (4, 5, 10, 4, 10, 11, -1, -1, -1, -1, -1, -1),
	/*11*/	int[] (4, 5, 7, 4, 7, 11, 11, 7, 8, -1, -1, -1),
	/*12*/	int[] (6, 4, 9, 6, 9, 8, -1, -1, -1, -1, -1, -1),
	/*13*/	int[] (4, 7, 6, 7, 4, 10, 10, 4, 9, -1, -1, -1),
	/*14*/	int[] (4, 5, 6, 6, 5, 10, 6, 10, 8, -1, -1, -1),
	/*15*/	int[] (4, 5, 6, 6, 5, 7, -1, -1, -1, -1, -1, -1),
	/*16*/	int[] (0, 7, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1),
	/*17*/	int[] (0, 8, 2, 2, 8, 10, -1, -1, -1, -1, -1, -1),
	/*18*/	int[] (0, 7, 2, 5, 10, 9, -1, -1, -1, -1, -1, -1),
	/*19*/	int[] (0, 8, 9, 0, 9, 2, 2, 9, 5, -1, -1, -1),
	/*20*/	int[] (0, 7, 2, 6, 11, 8, -1, -1, -1, -1, -1, -1),
	/*21*/	int[] (2, 11, 10, 2, 6, 11, 6, 2, 0, -1, -1, -1),
	/*22*/	int[] (0, 7, 2, 6, 11, 8, 5, 10, 9, -1, -1, -1),
	/*23*/	int[] (0, 6, 11, 0, 11, 2, 2, 11, 9, 2, 9, 5),
	/*24*/	int[] (0, 7, 2, 4, 9, 11, -1, -1, -1, -1, -1, -1),
	/*25*/	int[] (0, 8, 2, 2, 8, 10, 4, 9, 11, -1, -1, -1),
	/*26*/	int[] (4, 5, 10, 4, 10, 11, 0, 7, 2, -1, -1, -1),
	/*27*/	int[] (0, 8, 2, 4, 8, 11, 2, 8, 4, 2, 4, 5),
	/*28*/	int[] (4, 8, 6, 4, 9, 8, 0, 7, 2, -1, -1, -1),
	/*29*/	int[] (0, 6, 4, 0, 4, 10, 4, 9, 10, 0, 10, 2),
	/*30*/	int[] (4, 5, 6, 5, 10, 6, 6, 10, 8, 0, 7, 2),
	/*31*/	int[] (4, 5, 6, 2, 6, 5, 0, 6, 2, -1, -1, -1),
	/*32*/	int[] (1, 2, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1),
	/*33*/	int[] (1, 2, 5, 7, 8, 10, -1, -1, -1, -1, -1, -1),
	/*34*/	int[] (1, 2, 9, 2, 10, 9, -1, -1, -1, -1, -1, -1),
	/*35*/	int[] (1, 8, 9, 1, 2, 8, 2, 7, 8, -1, -1, -1),
	/*36*/	int[] (6, 11, 8, 1, 2, 5, -1, -1, -1, -1, -1, -1),
	/*37*/	int[] (6, 10, 7, 6, 11, 10, 1, 2, 5, -1, -1, -1),
	/*38*/	int[] (6, 11, 8, 1, 2, 9, 2, 10, 9, -1, -1, -1),
	/*39*/	int[] (0, 6, 2, 2, 6, 9, 6, 11, 9, 1, 2, 9),
	/*40*/	int[] (4, 9, 11, 1, 2, 5, -1, -1, -1, -1, -1, -1),
	/*41*/	int[] (4, 9, 11, 4, 2, 5, 7, 8, 10, -1, -1, -1),
	/*42*/	int[] (2, 10, 11, 1, 2, 11, 1, 11, 4, -1, -1, -1),
	/*43*/	int[] (4, 8, 11, 4, 7, 8, 4, 1, 7, 2, 7, 1),
	/*44*/	int[] (1, 2, 5, 4, 8, 6, 4, 9, 8, -1, -1, -1),
	/*45*/	int[] (1, 2, 5, 4, 7, 6, 7, 4, 10, 10, 4, 9),
	/*46*/	int[] (-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),//TODO
	/*47*/	int[] (4, 6, 7, 1, 4, 7, 1, 7, 2, -1, -1, -1),
	/*48*/	int[] (0, 7, 1, 1, 7, 5, -1, -1, -1, -1, -1, -1),
	/*49*/	int[] (0, 8, 1, 1, 8, 5, 5, 8, 10, -1, -1, -1),
	/*50*/	int[] (0, 9, 1, 0, 7, 9, 7, 10, 9, -1, -1, -1),
	/*51*/	int[] (0, 8, 1, 1, 8, 9, -1, -1, -1, -1, -1, -1),
	/*52*/	int[] (6, 11, 8, 0, 7, 1, 1, 7, 5, -1, -1, -1),
	/*53*/	int[] (-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),//TODO
	/*54*/	int[] (6, 11, 8, 0, 9, 1, 0, 7, 9, 7, 10, 9),
	/*55*/	int[] (0, 9, 1, 0, 6, 9, 6, 11, 9, -1, -1, -1),
	/*56*/	int[] (4, 9, 11, 0, 7, 1, 1, 7, 5, -1, -1, -1),
	/*57*/	int[] (4, 9, 11, 0, 7, 1, 1, 7, 5, -1, -1, -1),
	/*58*/	int[] (-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1),//TODO
	/*59*/	int[] (0, 1, 8, 1, 4, 8, 4, 11, 8, -1, -1, -1),
	/*60*/	int[] (0, 7, 1, 1, 7, 5, 4, 9, 6, 6, 9, 8),
	/*61*/	int[] (5, 9, 10, 0, 6, 4, 0, 4, 1, -1, -1, -1),
	/*62*/	int[] (1, 4, 6, 0, 1, 6, 7, 10, 8, -1, -1, -1),
	/*63*/	int[] (1, 4, 6, 0, 1, 6, -1, -1, -1, -1, -1, -1),
	/*64*/	int[] (0, 3, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1),
	/*65*/	int[] (0, 3, 6, 7, 8, 10, -1, -1, -1, -1, -1, -1),
	/*66*/	int[] (0, 3, 6, 5, 10, 9, -1, -1, -1, -1, -1, -1),
	/*67*/	int[] (0, 3, 6, 7, 8, 9, 5, 7, 9, -1, -1, -1),
	/*68*/	int[] (0, 3, 11, 0, 11, 8, -1, -1, -1, -1, -1, -1),
	/*69*/	int[] (3, 11, 10, 0, 3, 10, 0, 10, 7, -1, -1, -1),
	/*70*/	int[] (5, 10, 9, 0, 3, 11, 0, 11, 8, -1, -1, -1),
	/*71*/	int[] (0, 3, 7, 3, 9, 7, 3, 11, 9, 5, 7, 9),
	/*72*/	int[] (0, 3, 6, 4, 9, 11, -1, -1, -1, -1, -1, -1),
	/*73*/	int[] (0, 3, 6, 4, 9, 11, 7, 8, 10, -1, -1, -1),
	/*74*/	int[] (0, 3, 6, 4, 5, 10, 4, 10, 11, -1, -1, -1),
	/*75*/	int[] (0, 3, 6, 4, 5, 7, 4, 7, 11, 11, 7, 8),
	/*76*/	int[] (0, 9, 8, 0, 4, 9, 0, 3, 4, -1, -1, -1),
	/*77*/	int[] (0, 10, 7, 0, 9, 10, 0, 3, 9, 3, 4, 9),
	/*78*/	int[] (0, 3, 8, 3, 5, 8, 1, 5, 3, 5, 10, 8),
	/*79*/	int[] (4, 5, 7, 3, 4, 7, 0, 3, 7, -1, -1, -1),
	/*80*/	int[] (2, 3, 6, 2, 6, 7, -1, -1, -1, -1, -1, -1),
	/*81*/	int[] (2, 3, 10, 3, 6, 10, 6, 8, 10, -1, -1, -1),
	/*82*/	int[] (2, 3, 6, 2, 6, 7, 5, 10, 9, -1, -1, -1),
	/*83*/	int[] (3, 6, 8, 3, 8, 5, 1, 3, 5, 5, 8, 9),
	/*84*/	int[] (2, 3, 11, 2, 11, 7, 7, 11, 8, -1, -1, -1),
	/*85*/	int[] (2, 3, 11, 2, 11, 10, -1, -1, -1, -1, -1, -1),
	/*86*/	int[] (2, 3, 11, 2, 11, 7, 7, 11, 8, 5, 10, 9),
	/*87*/	int[] (2, 3, 11, 2, 5, 11, 5, 9, 11, -1, -1, -1),
	/*88*/	int[] (2, 3, 6, 2, 6, 7, 4, 9, 11, -1, -1, -1),
	/*89*/	int[] (2, 3, 10, 3, 6, 10, 6, 8, 10, 4, 9, 11),
	/*90*/	int[] (2, 3, 6, 2, 6, 7, 4, 5, 10, 4, 10, 11),
	/*91*/	int[] (6, 8, 11, 2, 3, 4, 2, 4, 5, -1, -1, -1),
	/*92*/	int[] (7, 8, 9, 0, 3, 2, 3, 7, 9, 3, 9, 4),
	/*93*/	int[] (2, 3, 10, 3, 9, 10, 3, 4, 9, -1, -1, -1),
	/*94*/	int[] (2, 3, 4, 2, 4, 5, 7, 9, 8, -1, -1, -1),
	/*95*/	int[] (2, 3, 4, 2, 4, 5, -1, -1, -1, -1, -1, -1),
	/*96*/	int[] (0, 3, 6, 1, 2, 5, -1, -1, -1, -1, -1, -1),
	/*97*/	int[] (0, 3, 6, 1, 2, 5, 7, 8, 10, -1, -1, -1),
	/*98*/	int[] (0, 3, 6, 1, 2, 9, 2, 10, 9, -1, -1, -1),
	/*99*/	int[] (0, 3, 6, 1, 8, 9, 1, 2, 8, 2, 7, 8),
	/*100*/	int[] (1, 2, 5, 0, 3, 11, 0, 11, 8, -1, -1, -1),
	/*101*/	int[] (1, 2, 5, 3, 11, 10, 0, 3, 10, 0, 10, 7),
	/*102*/	int[] (0, 3, 11, 0, 11, 8, 1, 2, 9, 2, 10, 9),
	/*103*/	int[] (1, 3, 11, 1, 11, 9, 0, 2, 7, -1, -1, -1),
	/*104*/	int[] (0, 3, 6, 1, 2, 5, 4, 9, 11, -1, -1, -1),
	/*105*/	int[] (0, 3, 6, 1, 2, 5, 7, 8, 10, 4, 9, 11),
	/*106*/	int[] (0, 3, 6, 2, 10, 11, 1, 2, 11, 1, 11, 4),
	/*107*/	int[] (1, 3, 4, 8, 11, 6, 0, 2, 7, -1, -1, -1),
	/*108*/	int[] (1, 2, 5, 0, 9, 8, 0, 4, 9, 0, 3, 4),
	/*109*/	int[] (5, 9, 10, 1, 3, 4, 0, 2, 7, -1, -1, -1),
	/*110*/	int[] (1, 3, 4, 0, 2, 8, 2, 10, 8, -1, -1, -1),
	/*111*/	int[] (1, 3, 4, 0, 2, 7, -1, -1, -1, -1, -1, -1),
	/*112*/	int[] (5, 6, 7, 3, 6, 5, 1, 3, 5, -1, -1, -1),
	/*113*/	int[] (3, 6, 8, 3, 8, 10, 1, 3, 10, 1, 10, 5),
	/*114*/	int[] (3, 6, 7, 3, 7, 9, 1, 3, 9, 7, 10, 9),
	/*115*/	int[] (1, 8, 9, 1, 3, 8, 3, 6, 8, -1, -1, -1),
	/*116*/	int[] (1, 7, 5, 1, 11, 7, 1, 4, 11, 7, 11, 8),
	/*117*/	int[] (3, 11, 10, 1, 3, 10, 1, 10, 5, -1, -1, -1),
	/*118*/	int[] (7, 10, 8, 1, 3, 11, 1, 11, 9, -1, -1, -1),
	/*119*/	int[] (1, 3, 11, 1, 11, 9, -1, -1, -1, -1, -1, -1),
	/*120*/	int[] (4, 9, 11, 5, 6, 7, 3, 6, 5, 1, 3, 5),
	/*121*/	int[] (6, 8, 11, 1, 3, 4, 5, 9, 10, -1, -1, -1),
	/*122*/	int[] (1, 3, 4, 6, 7, 11, 7, 10, 11, -1, -1, -1),
	/*123*/	int[] (1, 3, 4, 6, 8, 11, -1, -1, -1, -1, -1, -1),
	/*124*/	int[] (1, 3, 4, 5, 9, 8, 5, 8, 7, -1, -1, -1),
	/*125*/	int[] (1, 3, 4, 5, 9, 10, -1, -1, -1, -1, -1, -1),
	/*126*/	int[] (1, 3, 4, 7, 10, 8, -1, -1, -1, -1, -1, -1),
	/*127*/	int[] (1, 3, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1)
);

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

	if(cube_id >= 128)
		cube_id = 255 - cube_id;

	if(cube_id != 0 && show_mesh == 1)
	{
		int[] curr_cube = pointIndicesByCubeID[cube_id];

		int i = 0;

		while(curr_cube[i] != -1 && i != 12)
		{
			vec4 pos_0 = transform_mat * (gl_in[0].gl_Position + vec4(MidEdgePositions[curr_cube[i]] * mcs, 0));
			vec4 pos_1 = transform_mat * (gl_in[0].gl_Position + vec4(MidEdgePositions[curr_cube[i + 1]] * mcs, 0));
			vec4 pos_2 = transform_mat * (gl_in[0].gl_Position + vec4(MidEdgePositions[curr_cube[i + 2]] * mcs, 0));

			gl_Position = pos_0;
			pos = pos_0.xyz;
			normal = cross(pos_0.xyz - pos_1.xyz, pos_1.xyz - pos_2.xyz);
			col = vec3(1);
			EmitVertex();

			gl_Position = pos_1;
			pos = pos_1.xyz;
			normal = cross(pos_0.xyz - pos_1.xyz, pos_1.xyz - pos_2.xyz);
			col = vec3(1);
			EmitVertex();

			gl_Position = pos_2;
			pos = pos_2.xyz;
			normal = cross(pos_0.xyz - pos_1.xyz, pos_1.xyz - pos_2.xyz);
			col = vec3(1);
			EmitVertex();		
			

			EndPrimitive();

			i += 3;
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