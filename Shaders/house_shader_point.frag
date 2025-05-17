#version 330 core

in vec3 FragPos;
in vec3 Normal;
in vec4 vColor;

out vec4 resColor;

uniform vec3 lightPos;
uniform vec3 lightColor;
uniform vec3 viewPos;

void main() {
	// AMBIENT
	float ambientStrength = 0.6;
	vec3 ambient = ambientStrength * lightColor;
	// DIFFUSE
	vec3 norm = normalize(Normal);
	vec3 lightDir = normalize(lightPos - FragPos);
	float diff = max(dot(norm, lightDir), 0.0);
	vec3 diffuse = diff * lightColor;
	float specularStrength = 1.0;
	// SPECULAR
	vec3 viewDir = normalize(viewPos - FragPos);
	vec3 reflectDir = reflect(-lightDir, norm);
	float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32); //The 32 is the shininess of the material.
	vec3 specular = specularStrength * spec * lightColor;
	// ATTENUATION
	float distance = length(lightPos - FragPos);
	float attenuation = 1.0 / (1.0f + 0.09f * distance + 0.032f * (distance * distance));
	vec3 result = (ambient + diffuse + specular) * attenuation * vec3(vColor);
	resColor = vec4(result, 1.0);
}
