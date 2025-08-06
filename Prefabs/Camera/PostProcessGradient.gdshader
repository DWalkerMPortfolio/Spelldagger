shader_type spatial;
render_mode unshaded, depth_draw_never;

uniform sampler2D vignette_texture : repeat_disable;
uniform float vignette_strength;
uniform sampler2D gradient_texture : repeat_disable;
uniform float gradient_strength;

void vertex() {
	// Force positioning on top of screen
	POSITION = vec4(VERTEX.xy, 1.0, 1.0);
}

void fragment() {
	ALPHA = texture(vignette_texture, SCREEN_UV).a * vignette_strength;
	ALBEDO = texture(gradient_texture, SCREEN_UV).rgb * gradient_strength;
}
