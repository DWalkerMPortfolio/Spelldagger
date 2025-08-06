shader_type fog;

uniform sampler2D noise : repeat_enable;
uniform float noise_scale;
uniform vec4 color : source_color;

void fog() {
	DENSITY = texture(noise, WORLD_POSITION.rb * noise_scale).r * color.a;
	EMISSION = color.rgb;
}
