#version 440 core

layout(location = 0) in float none;

uniform vec3 MarchingCubesMin;
uniform ivec3 MarchingCubesCount;
uniform float MarchingCubesStep;

void main()
{
	ivec3 ipos = ivec3(mod(gl_InstanceID, MarchingCubesCount.x),
	floor(gl_InstanceID / float(MarchingCubesCount.x * MarchingCubesCount.z)),
	mod(floor(gl_InstanceID / MarchingCubesCount.x), MarchingCubesCount.y));// 3d index of cude

	vec3 pos = MarchingCubesMin + ipos * MarchingCubesStep;

	gl_Position = vec4(pos, 1);
}