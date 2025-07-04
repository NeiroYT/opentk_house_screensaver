#version 330 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoords;
layout (location = 3) in vec3 aColor;

out vec3 FragPos;
out vec3 Normal;
out vec2 TexCoords;
out vec3 vColor;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main() {
	FragPos = vec3(model * vec4(aPosition, 1.0f));
	gl_Position = projection * view * model * vec4(aPosition, 1.0f);
	Normal = aNormal;
	TexCoords = aTexCoords;
	vColor = aColor;
}
