#version 330 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec4 aColor;
layout (location = 2) in vec3 aNormal;

out vec3 Normal;
out vec4 vColor;
out vec3 FragPos;
// shadow
out vec4 FragPosLightSpace;
//

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
// shadow
uniform mat4 lightSpaceMatrix;
//

void main() {
	FragPos = vec3(model * vec4(aPosition, 1.0f));
	gl_Position = projection * view * model * vec4(aPosition, 1.0f);
	vColor = aColor;
	Normal = aNormal;
	FragPosLightSpace = lightSpaceMatrix * vec4(FragPos, 1.0);
}
