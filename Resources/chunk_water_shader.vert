﻿#version 460 core
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in float aLayer;

out vec2 texCoord;
flat out int layer;
out float visibility;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

uniform float globalTime;

const float density = 0.015;
const float gradient  = 4.0;

vec4 getWorldPos()
{
    vec3 inVert = (vec4(aPosition, 1) * model).xyz;
    inVert.y += sin((globalTime + inVert.x) * 1.5) / 10.8f;
    inVert.y += cos((globalTime + inVert.z) * 1.5) / 9.1f;
    inVert.y -= 0.2;
    return vec4(inVert, 1);
}


void main() 
{
	layer = int(aLayer);
	texCoord = aTexCoord;

	vec4 positionRelativeToCam =  getWorldPos() * view;
	float distance = length(positionRelativeToCam.xyz);
	visibility = exp(-pow((distance*density),gradient));
	visibility = clamp(visibility, 0.0, 1.0);

	gl_Position = positionRelativeToCam * projection;
}