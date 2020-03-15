#version 430 core
out vec4 color;

in vec2 pos;

uniform mat4 transform_mat;
uniform vec3 cam_pos;

struct Metaball
{
	vec4 pos;
	vec4 color_charge;
};

layout(std430, binding = 0) buffer METABALLS
{
	Metaball metaballs[];
};

#define EPSILON 0.000001
#define STEPS 1024
#define MAX_DISTANCE 1000
#define THRESHOLD 1.7

struct HitParams
{
	bool intersection;

	vec3 normal;
	vec3 point;
	vec3 albedo;
};

vec4 GetDistance_Color(vec3 point)
{
	float charge = 0.0;
	vec3 col = vec3(0.0);

	for(int i = 0; i < metaballs.length(); i++)
	{
		float curr_charge = metaballs[i].color_charge.w / distance(metaballs[i].pos.xyz, point);
		charge += curr_charge;
		col += metaballs[i].color_charge.xyz * curr_charge;
	}

	return vec4(THRESHOLD - charge, col / charge);
}

HitParams Trace(vec3 ray_dir, vec3 ray_source)
{
	HitParams hit;
	hit.intersection = false;

	float dist_sum = 0.0;

	for(int i = 0; i < STEPS; i++)
	{
		vec3 check_point = ray_source + ray_dir * dist_sum;
		vec4 dist_col = GetDistance_Color(check_point);
		dist_sum += dist_col.x;
		if(dist_sum > MAX_DISTANCE)
			return hit;
		if(dist_col.x < EPSILON)
		{
			hit.point = check_point;
			hit.intersection = true;
			hit.albedo = dist_col.yzw;
			return hit;
		}
	}

	return hit;
}

void main(){
	vec4 look_at = transform_mat * vec4(pos, -1.0, 1.0);
	vec3 ray_dir = normalize(look_at.xyz / look_at.w - cam_pos);
	vec3 ray_source = cam_pos;

	HitParams hit = Trace(ray_dir, ray_source);

	color = hit.intersection ? vec4(hit.albedo, 1.0) : vec4(0.0);
}