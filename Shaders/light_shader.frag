#version 330 core

out vec4 pixelColor;

uniform vec4 lightColor;

void main()
{
	pixelColor = lightColor;
}