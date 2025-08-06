shader_type spatial;
render_mode ambient_light_disabled;

uniform bool is_wall = false;
uniform float wall_check_threshold = 0.9;

instance uniform float transparency;

varying vec3 node_position;

void fragment()
{
	ALBEDO = vec3(1);
	node_position = NODE_POSITION_WORLD;

	// Transparency fade
	float opaque = step(0.999, ALPHA);
	ALPHA = step(1.0 - distance(FRAGCOORD.xy, VIEWPORT_SIZE * 0.5) / (sqrt(VIEWPORT_SIZE.x * VIEWPORT_SIZE.x + VIEWPORT_SIZE.y * VIEWPORT_SIZE.y) * 0.5), 1.0 - transparency) * opaque;
	ALPHA_SCISSOR_THRESHOLD = 0.5;
}

void light()
{	
	// If this is a wall, don't light capture on upwards-facing faces
	vec3 world_normal = (INV_VIEW_MATRIX * vec4(NORMAL, 0.0)).rgb;
	float wall_check = 1.0 - step(wall_check_threshold, dot(world_normal, vec3(0, 1, 0)));
	wall_check = max(1.0 - float(is_wall), wall_check);
	
	// Write light to light channel (R)
	DIFFUSE_LIGHT.r += ATTENUATION * LIGHT_COLOR.r / PI * wall_check;
}