shader_type spatial;
render_mode unshaded, depth_draw_never;

global uniform sampler2D light_capture : source_color, repeat_disable;

// Exposed properties
uniform vec4 outline_color : source_color;
uniform float depth_threshold = 1.0;
uniform float depth_normal_threshold = 0.9;
uniform vec2 normal_threshold;
uniform float normal_strength;
uniform float light_threshold;
uniform vec4 light_outline_color : source_color;

// Hinted properties
uniform sampler2D depth_texture : hint_depth_texture, source_color, repeat_disable;
uniform sampler2D normal_texture : hint_normal_roughness_texture, source_color, repeat_disable;

// Constants
const float sample_offset = 1.0;
const int sample_count = 8;
const vec2 sample_offsets[sample_count] = {
	// Plus shape
	vec2(0.0, -sample_offset),
	vec2(0.0, sample_offset),
	vec2(-sample_offset, 0.0),
	vec2(sample_offset, 0.0),

	// Corners
	vec2(-sample_offset, -sample_offset),
	vec2(-sample_offset, sample_offset),
	vec2(sample_offset, -sample_offset),
	vec2(sample_offset, sample_offset)
};

// Utility functions
// From: https://docs.godotengine.org/en/stable/tutorials/shaders/advanced_postprocessing.html
float get_linear_depth(vec2 coord, mat4 inv_projection_matrix)
{
	float depth = texture(depth_texture, coord).x;
	vec3 normalized_depth_coordinates = vec3(vec2(coord) * 2.0 - 1.0, depth);
	vec4 view = inv_projection_matrix * vec4(normalized_depth_coordinates, 1.0);
	view.xyz /= view.w;
	return -view.z;
}

vec3 get_world_normal(vec2 coord, mat4 inv_view_matrix)
{
	vec3 normal = texture(normal_texture, coord).rgb;
	normal = normal * 2.0 - 1.0;
	normal = (inv_view_matrix * vec4(normal, 0.0)).rgb;
	return normalize(normal);
}

// Effect functions
float edge_detection(vec2 viewport_size, vec2 screen_uv, mat4 inv_projection_matrix, mat4 inv_view_matrix, vec3 view)
{
	// Get samples at the current pixel
	vec2 sample_pos = screen_uv.xy;

	vec3 normal_sample = get_world_normal(sample_pos, inv_view_matrix);
	float depth_sample = get_linear_depth(sample_pos, inv_projection_matrix);

	// Compare with samples from offset pixels
	float sum_normal_difference = 0.0;
	float sum_depth = 0.0;
	float depth_normal_check = step(depth_normal_threshold, abs(dot(normal_sample, vec3(0,1,0))));
	for (int i=0; i<sample_count; i++)
	{
		vec2 offset_sample_pos = sample_pos + sample_offsets[i] / viewport_size;

		// Normal
		vec3 offset_normal_sample = get_world_normal(offset_sample_pos, inv_view_matrix);
		sum_normal_difference += 1.0 - abs(dot(offset_normal_sample, normal_sample));

		// Depth
		float offset_depth_sample = get_linear_depth(offset_sample_pos, inv_projection_matrix);
		sum_depth += offset_depth_sample;

		depth_normal_check = min(depth_normal_check, step(depth_normal_threshold, abs(dot(offset_normal_sample, vec3(0,1,0)))));
	}

	// Combine
	float avg_normal_difference = sum_normal_difference / float(sample_count);
	float normal_result = smoothstep(normal_threshold.x, normal_threshold.y, avg_normal_difference) * normal_strength;

	float depth_difference = abs(depth_sample * float(sample_count) - sum_depth);
	float depth_result = step(depth_threshold, depth_difference) * depth_normal_check;

	return max(depth_result, normal_result);
}

// R: Light capture value
// A: Edge
vec4 light_capture_sample(vec2 screen_uv, vec2 viewport_size, float time)
{
	float light_capture_value = texture(light_capture, screen_uv).r;
	float max_value = light_capture_value;
	float edge = 0.0;
	for (int i=0; i<sample_count; i++)
	{
		vec2 offset_sample_pos = screen_uv + sample_offsets[i] / viewport_size;
		float sample_value = texture(light_capture, offset_sample_pos).r;
		max_value = max(max_value, sample_value);
		edge = max(edge, abs(step(light_threshold, light_capture_value) - step(light_threshold, sample_value)));
	}
	return vec4(max_value, 0, 0, edge);
}

// Main functions
void vertex() {
	// Force positioning on top of screen
	POSITION = vec4(VERTEX.xy, 1.0, 1.0);
}

void fragment() {
	vec4 light_capture_sample = light_capture_sample(SCREEN_UV, VIEWPORT_SIZE, TIME);

	// Edge outlines
	float edge = edge_detection(VIEWPORT_SIZE, SCREEN_UV, INV_PROJECTION_MATRIX, INV_VIEW_MATRIX, VIEW);
	float light_capture_step = step(light_threshold, light_capture_sample.r);
	ALPHA = edge * (1.0 - light_capture_step) * outline_color.a;

	// Light outlines
	ALPHA += light_capture_sample.a * light_outline_color.a;
	ALBEDO = mix(light_outline_color.rgb, outline_color.rgb, edge);
}
