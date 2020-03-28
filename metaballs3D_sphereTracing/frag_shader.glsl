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
#define GRADIENT_SAMPLING_STEP 0.0001

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

float GetDistance(vec3 point)
{
	float charge = 0.0;
	for(int i = 0; i < metaballs.length(); i++)
		charge += metaballs[i].color_charge.w / distance(metaballs[i].pos.xyz, point);
	return THRESHOLD - charge;
}

vec3 GetNormal(vec3 point) 
{
	float point_distance = GetDistance(point);
	vec3 normal = vec3( 
	GetDistance(point + vec3(GRADIENT_SAMPLING_STEP, 0.0, 0.0)) - point_distance,
	GetDistance(point + vec3(0.0, GRADIENT_SAMPLING_STEP, 0.0)) - point_distance,
	GetDistance(point + vec3(0.0, 0.0, GRADIENT_SAMPLING_STEP)) - point_distance);
	return normalize(normal);
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
			hit.normal = GetNormal(check_point);
			hit.intersection = true;
			hit.albedo = dist_col.yzw;
			return hit;
		}
	}
	return hit;
}

struct point_light
{
	vec3 pos;
	vec3 color;
};

#define LIGHT_COUNT 3
const point_light[LIGHT_COUNT] lights = 
{
	{ vec3(5.0, 5.0, 1.0), vec3(0.3) },
	{ vec3(-5.0, -5.0, -1.0), vec3(0.3) },
	{ vec3(0.0, 5.0, 5.0), vec3(0.3) }
};

const vec3 ambient_light = vec3(0.1);
const float shininess = 8;
const float specular_k = 0.1;
const float diffuse_k = 1.0;

void main(){
	vec4 look_at = transform_mat * vec4(pos, -1.0, 1.0);
	vec3 ray_dir = normalize(look_at.xyz / look_at.w - cam_pos);
	vec3 ray_source = cam_pos;

	HitParams hit = Trace(ray_dir, ray_source);

	// Phong
	vec3 out_color = ambient_light;
	for(int i = 0; i < LIGHT_COUNT; i++)
	{
		vec3 light_dir = normalize(lights[i].pos - hit.point);
		// Diffuse
		float diffuse = max(dot(hit.normal, light_dir), 0.0);
		out_color += diffuse * lights[i].color * diffuse_k;
		// Specular
		vec3 reflected = -reflect(light_dir, hit.normal);
		float specular = pow(max(dot(reflected, normalize(cam_pos - hit.point)), 0.0), shininess);
		out_color += specular * lights[i].color * specular_k;
	}
	out_color *= hit.albedo;

	color = hit.intersection ? vec4(out_color, 1.0) : vec4(0.0);
}