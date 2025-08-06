shader_type spatial;

group_uniforms Fragment;
uniform vec3 color : source_color;
uniform vec3 fresnel_color : source_color;
uniform float fresnel_amount;
uniform float fresnel_brightness;

group_uniforms Vertex;
uniform float distortion_strength_1;
uniform int distortion_waves_1;
uniform float distortion_speed_1;
uniform float distortion_strength_2;
uniform int distortion_waves_2;
uniform float distortion_speed_2;
uniform float trail_offset_strength;
uniform float trail_offset_distance;

instance uniform vec3 trail_offset;

varying float distortion_wave;

// From: https://godotshaders.com/snippet/fresnel/
float fresnel(float amount, vec3 normal, vec3 view)
{
	return pow((1.0 - clamp(dot(normalize(normal), normalize(view)), 0.0, 1.0 )), amount);
}

void vertex() {
	// Wave distortion
	float distortion_wave_1 = sin(UV.x * 2.0 * PI * float(distortion_waves_1) + TIME * distortion_speed_1) * distortion_strength_1;
	float distortion_wave_2 = sin(UV.x * 2.0 * PI * float(distortion_waves_2) + TIME * distortion_speed_2) * distortion_strength_2;
	distortion_wave = distortion_wave_1 + distortion_wave_2;
	VERTEX += NORMAL * UV.y * distortion_wave;
	
	// Trail
	float trail_vertex = clamp(1.0 - (distance(VERTEX, trail_offset) / trail_offset_distance), 0.0, 1.0);
	VERTEX += trail_offset * UV.y * trail_vertex * trail_offset_strength;
}

void fragment() {
	ALBEDO = color;
	EMISSION = fresnel(fresnel_amount, NORMAL, VIEW) * fresnel_color * fresnel_brightness;
	EMISSION += pow(clamp(distortion_wave, 0, 1), 2);
}
