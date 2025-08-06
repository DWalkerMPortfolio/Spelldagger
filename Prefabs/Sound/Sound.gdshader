shader_type spatial;
render_mode depth_draw_never, depth_test_disabled, unshaded;

instance uniform float max_distance = 0.0;
instance uniform float alpha = 0.0;
instance uniform vec4 color : source_color;

uniform sampler2D depth_texture : hint_depth_texture;
uniform float wave_frequency;
uniform float wave_thinness;
uniform float wave_smoothness;
uniform float wave_speed;
uniform float wave_fade_power;
uniform float edge_thickness;
uniform float edge_smoothness;
uniform float edge_alpha_multiplier;

varying vec3 node_position;

void vertex()
{
	node_position = NODE_POSITION_WORLD;
}

void fragment()
{
	// Get world position (from https://docs.godotengine.org/en/stable/tutorials/shaders/advanced_postprocessing.html)
	float depth = texture(depth_texture, SCREEN_UV).x;
	vec3 ndc = vec3(SCREEN_UV * 2.0 - 1.0, depth);
	vec4 world = INV_VIEW_MATRIX * INV_PROJECTION_MATRIX * vec4(ndc, 1.0);
	vec3 world_position = world.xyz / world.w;
	// Get distance from the object's center
	float dist = distance(world_position, node_position);

	// Waves
	// Animation
	float animated_distance = dist - TIME * wave_speed;
	// Sine
	float sine_distance = clamp(sin(animated_distance * wave_frequency), 0.0, 1.0);
	float wave_alpha = smoothstep(wave_thinness, wave_thinness + wave_smoothness, sine_distance);
	// Fade out
	wave_alpha *= 1.0 - pow(dist / max_distance, wave_fade_power);

	// Edge
	float edge_distance = max_distance - edge_thickness;
	float edge_alpha = smoothstep(edge_distance, edge_distance + edge_smoothness, dist) * edge_alpha_multiplier;

	// Combine
	ALBEDO = color.rgb;
	ALPHA = clamp(max(wave_alpha, edge_alpha) * color.a, 0.0, 1.0);

	// Clamp max distance
	ALPHA *= 1.0 - step(max_distance, dist);

	// Alpha parameter
	ALPHA *= alpha;
}