shader_type spatial;
render_mode unshaded;

uniform sampler2D noise;
uniform float noise_strength;
uniform sampler2D screen_texture : hint_screen_texture;
uniform vec4 color : source_color;

void fragment() {
	vec2 uv_offset = texture(noise, UV).rg * noise_strength;
	ALBEDO = texture(screen_texture, SCREEN_UV + uv_offset).rgb;
	float color_alpha = 1.0 - abs(UV.y - 0.5) * 2.0;
	ALBEDO += color.rgb * color_alpha * color.a;
}