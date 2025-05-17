#version 330 core

struct DirLight {
	vec3 direction;

	float ambient;
	float diffuse;
	float specular;
};

in vec3 FragPos;
in vec3 Normal;
in vec2 TexCoords;

out vec4 resColor;

uniform DirLight dirLight;
uniform vec3 lightColor;
uniform vec3 viewPos;
uniform sampler2D texture0;

vec3 CalcDirLight(DirLight light, vec3 normal)
{
	vec3 lightDir = normalize(-light.direction);
	float diff = max(dot(normal, lightDir), 0.0);
	vec3 viewDir = normalize(viewPos - FragPos);
	vec3 reflectDir = reflect(-lightDir, normal);
	float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32);
	vec3 ambient  = light.ambient  * lightColor;
	vec3 diffuse  = light.diffuse  * diff * lightColor;
	vec3 specular = light.specular * spec * lightColor;
	return (ambient + diffuse + specular);
}

void main() {
	vec3 result = CalcDirLight(dirLight, normalize(Normal)) * vec3(texture(texture0, TexCoords));
	resColor = vec4(result, 1.0);
}
