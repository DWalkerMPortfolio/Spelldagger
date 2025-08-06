shader_type spatial;
render_mode world_vertex_coords, ambient_light_disabled, specular_disabled, depth_test_disabled, cull_disabled;

global uniform vec3 player_vision_position;

uniform float offset_distance;
uniform vec2 light_step;

group_uniforms Noise;
uniform vec3 color1 : source_color;
uniform vec3 color2 : source_color;
uniform sampler2D noise : repeat_enable;
uniform vec2 noise_scale;
uniform float noise_strength;
uniform float noise_contrast;
uniform sampler2D distortion : repeat_enable;
uniform vec2 distortion_scale;
uniform vec2 distortion_flow;
uniform float distortion_strength;

instance uniform bool transparency_fade_backwards;
instance uniform float transparency;

varying vec3 world_pos;

void vertex() {
	vec3 player_vision_direction = normalize(VERTEX - player_vision_position) * vec3(1, 0, 1);
	VERTEX += player_vision_direction * UV.g * offset_distance;

	world_pos = VERTEX;
}

void fragment() {
	ALBEDO = vec3(0.0);

	// Transparency fade
	float transparency_distance = distance(FRAGCOORD.xy, VIEWPORT_SIZE * 0.5) / (sqrt(VIEWPORT_SIZE.x * VIEWPORT_SIZE.x + VIEWPORT_SIZE.y * VIEWPORT_SIZE.y) * 0.5);
	transparency_distance = abs((1.0 - float(transparency_fade_backwards)) - transparency_distance);
	ALPHA = step(transparency_distance, 1.0 - transparency);

	// Noise texture
	vec2 noise_uv = world_pos.xz + texture(distortion, world_pos.xz * distortion_scale + distortion_flow * TIME).rg * distortion_strength;
	float noise_value = pow(texture(noise, noise_uv * noise_scale).r, noise_contrast) * noise_strength;
	EMISSION = mix(color1, color2, noise_value);
}

void light() {
	ALPHA = min(ALPHA, smoothstep(light_step.x, light_step.y, 1.0 - (ATTENUATION * LIGHT_COLOR.r)));
}