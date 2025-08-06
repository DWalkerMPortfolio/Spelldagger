shader_type spatial;
render_mode ambient_light_disabled;

global uniform sampler2D light_capture;

uniform float alpha;
uniform float darkness_alpha;

instance uniform float transparency;

varying float fade_alpha;

void fragment()
{
	ALPHA = 0.0;

	// Transparency fade
	float transparency_distance = 1.0 - distance(FRAGCOORD.xy, VIEWPORT_SIZE * 0.5) / (sqrt(VIEWPORT_SIZE.x * VIEWPORT_SIZE.x + VIEWPORT_SIZE.y * VIEWPORT_SIZE.y) * 0.5);
	fade_alpha = step(transparency_distance, 1.0 - transparency);
}

void light()
{
	float front_light = step(0, dot(LIGHT, NORMAL)); // Checks whether the light is shining on the front of the face

	float light_capture_value = clamp(texture(light_capture, SCREEN_UV).r, 0.0, 1.0);
	float light_capture_alpha = mix(alpha, darkness_alpha, 1.0 - light_capture_value);

	float light_alpha = step(0.01, ATTENUATION) * front_light;

	vec3 initial_diffuse_light = mix(DIFFUSE_LIGHT, LIGHT_COLOR, light_alpha); // The diffuse light value for the first light
	vec3 additive_diffuse_light = mix(DIFFUSE_LIGHT, mix(DIFFUSE_LIGHT, LIGHT_COLOR, 0.5), light_alpha); // The diffuse light value for subsequent lights
	DIFFUSE_LIGHT = mix(initial_diffuse_light, additive_diffuse_light, step(0.01, DIFFUSE_LIGHT.x));

	ALPHA += light_alpha;
	ALPHA = clamp(ALPHA, 0.0, light_capture_alpha) * fade_alpha;
}
