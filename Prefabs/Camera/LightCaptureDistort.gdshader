shader_type spatial;
render_mode unshaded, depth_draw_never;

uniform sampler2D distortion_noise : source_color;
uniform float distortion_scale;
uniform float distortion_strength;
uniform float distortion_speed;
uniform sampler2D flicker_noise : source_color;
uniform float flicker_strength;
uniform float flicker_speed;

uniform sampler2D screen_texture : hint_screen_texture;
uniform sampler2D depth_texture : hint_depth_texture;

// From: https://godotshaders.com/shader/depth-to-worldspaceposition4-3/
vec3 get_world_position(vec2 screen_uv, mat4 inv_projection_matrix, mat4 inv_view_matrix)
{
	vec4 clipSpacePosition = vec4(screen_uv * 2.0 - 1.0, texture(depth_texture, screen_uv).r, 1.0);
	vec4 viewSpacePosition = inv_projection_matrix * clipSpacePosition;
	viewSpacePosition /= viewSpacePosition.w;
	vec4 worldSpacePosition = inv_view_matrix * viewSpacePosition;
	return worldSpacePosition.rgb;
}

void vertex() {
	// Force positioning on top of screen
	POSITION = vec4(VERTEX.xy, 1.0, 1.0);
}

void fragment() {
	// Distort red values
	ALBEDO = texture(screen_texture, SCREEN_UV).rgb;

	vec3 world_position = get_world_position(SCREEN_UV, INV_PROJECTION_MATRIX, INV_VIEW_MATRIX);
	ALBEDO.r += texture(distortion_noise, world_position.xz * distortion_scale + TIME * distortion_speed).r * distortion_strength;
	ALBEDO.r += texture(flicker_noise, vec2(TIME) * flicker_speed).r * flicker_strength;
}
