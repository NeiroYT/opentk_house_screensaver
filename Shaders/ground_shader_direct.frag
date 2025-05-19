#version 330 core

struct Light {
	vec3 direction;
	vec3 color;
	float ambient;
	float diffuse;
	float specular;
};

in vec3 FragPos;
in vec3 Normal;
in vec2 TexCoords;
// shadow
in vec4 FragPosLightSpace;
//

out vec4 resColor;

uniform Light light;
uniform vec3 viewPos;
uniform sampler2D texture0;
// shadow
uniform sampler2D shadowMap;
//

float ShadowCalculation(vec4 fragPosLightSpace)
{
    // perform perspective divide
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    // transform to [0,1] range
    projCoords = projCoords * 0.5 + 0.5;
    // get closest depth value from light's perspective (using [0,1] range fragPosLight as coords)
    float closestDepth = texture(shadowMap, projCoords.xy).r; 
    // get depth of current fragment from light's perspective
    float currentDepth = projCoords.z;
    // calculate bias (based on depth map resolution and slope)
    vec3 normal = normalize(Normal);
    vec3 lightDir = normalize(-light.direction);
    float bias = max(0.0005 * (1.0 - dot(normal, lightDir)), 0.00005);
	//float bias = 0.0;
    // check whether current frag pos is in shadow
    // float shadow = currentDepth - bias > closestDepth  ? 1.0 : 0.0;
    // PCF
    float shadow = 0.0;
    vec2 texelSize = 1.0 / textureSize(shadowMap, 0);
    for(int x = -1; x <= 1; ++x)
    {
        for(int y = -1; y <= 1; ++y)
        {
            float pcfDepth = texture(shadowMap, projCoords.xy + vec2(x, y) * texelSize).r; 
            shadow += currentDepth - bias > pcfDepth  ? 1.0 : 0.0;        
        }    
    }
    shadow /= 9.0;
    
    // keep the shadow at 0.0 when outside the far_plane region of the light's frustum.
    if(projCoords.z > 1.0)
        shadow = 0.0;
        
    return shadow;
}

vec3 CalcDirLight(Light light, vec3 normal)
{
	vec3 lightDir = normalize(-light.direction);
	float diff = max(dot(normal, lightDir), 0.0);
	vec3 viewDir = normalize(viewPos - FragPos);
	vec3 reflectDir = reflect(-lightDir, normal);
	float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32);
	vec3 ambient  = light.ambient  * light.color;
	vec3 diffuse  = light.diffuse  * diff * light.color;
	vec3 specular = light.specular * spec * light.color;
	float shadow = ShadowCalculation(FragPosLightSpace);
	return (ambient + (1.0 - shadow) * (diffuse + specular));
}

void main() {
	vec3 result = CalcDirLight(light, normalize(Normal)) * vec3(texture(texture0, TexCoords));
	resColor = vec4(result, 1.0);
}
