// NOTE: Shader automatically converted from Godot Engine 4.4.stable.mono's StandardMaterial3D.

shader_type spatial;
render_mode blend_mix, depth_draw_opaque, cull_back, diffuse_burley, specular_schlick_ggx;

global uniform sampler2D light_capture;

group_uniforms Albedo;
uniform vec4 albedo : source_color = vec4(1.0);
uniform sampler2D texture_albedo : source_color, filter_linear_mipmap_anisotropic, repeat_enable;

group_uniforms RoughnessMetallic;
uniform float roughness : hint_range(0.0, 1.0) = 1.0;
uniform sampler2D texture_metallic : hint_default_white, filter_linear_mipmap_anisotropic, repeat_enable;
uniform vec4 metallic_texture_channel = vec4(1.0, 0.0, 0.0, 0.0);
uniform sampler2D texture_roughness : hint_roughness_r, filter_linear_mipmap_anisotropic, repeat_enable;
uniform float specular : hint_range(0.0, 1.0, 0.01) = 0.5;
uniform float metallic : hint_range(0.0, 1.0, 0.01) = 0.0;

group_uniforms Emission;
uniform sampler2D texture_emission : source_color, hint_default_black, filter_linear_mipmap_anisotropic, repeat_enable;
uniform vec4 emission : source_color = vec4(0.0, 0.0, 0.0, 1.0);
uniform float emission_energy : hint_range(0.0, 100.0, 0.01) = 0.0;

group_uniforms Normals;
uniform sampler2D texture_normal : hint_roughness_normal, filter_linear_mipmap_anisotropic, repeat_enable;
uniform float normal_strength : hint_range(-16.0, 16.0) = 1.0;

group_uniforms AmbientOcclusion;
uniform sampler2D texture_ambient_occlusion : hint_default_white, filter_linear_mipmap_anisotropic, repeat_enable;
uniform vec4 ao_texture_channel = vec4(1.0, 0.0, 0.0, 0.0);
uniform float ao_light_affect : hint_range(0.0, 1.0, 0.01) = 0.0;

group_uniforms BackLight;
uniform vec4 backlight : source_color;
uniform sampler2D texture_backlight : hint_default_black, filter_linear_mipmap_anisotropic, repeat_enable;

group_uniforms Uv;
uniform bool floor_uvs;
uniform vec2 uv_scale = vec2(1.0);
uniform vec2 uv_offset = vec2(0.0);

group_uniforms Darkness;
uniform vec3 dark_albedo : source_color = vec3(0.0, 0.0, 0.0);
uniform sampler2D dark_detail_texture : source_color, filter_linear_mipmap_anisotropic;
uniform float dark_detail_texture_lod = 3.0;
uniform vec2 dark_detail_texture_step = vec2(0.15, 0.3);
uniform vec4 dark_detail_color : source_color = vec4(1.0, 1.0, 1.0, 0.035);
uniform sampler2D dark_detail_noise_texture : source_color;
uniform vec2 dark_detail_noise_scale = vec2(0.35);
uniform vec2 dark_detail_noise_step_range = vec2(0.25, 1.0);
uniform float dark_detail_noise_min_value = 0.6;
uniform vec2 dark_normal_step = vec2(0.8, 1.0);

instance uniform float transparency;

float dither(float alpha, vec2 uv)
{
	// From StandardMaterial3D's distance fade pixel dither
	const vec3 magic = vec3(0.06711056, 0.00583715, 52.9829189);
	return step(fract(magic.z * fract(dot(uv, magic.xy))), alpha);
}

void vertex() {
	// UVs
	vec2 base_uv = UV;
	vec2 floor_uv = VERTEX.rg;
	
	UV = mix(base_uv, floor_uv, float(floor_uvs)) * uv_scale + uv_offset;
}

void fragment() {
	// Transparency fade
	ALPHA = step(1.0 - distance(FRAGCOORD.xy, VIEWPORT_SIZE * 0.5) / (sqrt(VIEWPORT_SIZE.x * VIEWPORT_SIZE.x + VIEWPORT_SIZE.y * VIEWPORT_SIZE.y) * 0.5), 1.0 - transparency);
	ALPHA_SCISSOR_THRESHOLD = 0.5;
	
	// Light capture
	vec4 light_capture_sample = texture(light_capture, SCREEN_UV);
	float light_capture_dither = dither(light_capture_sample.r, FRAGCOORD.rg);
	
	// UVs
	vec2 base_uv = UV;
	
	// Albedo
	vec4 albedo_tex = texture(texture_albedo, base_uv);
	ALBEDO = albedo.rgb * albedo_tex.rgb;
	ALBEDO = mix(ALBEDO * dark_albedo, ALBEDO, light_capture_dither);
	
	// Metallic
	float metallic_tex = dot(texture(texture_metallic, base_uv), metallic_texture_channel);
	METALLIC = metallic_tex * metallic;
	SPECULAR = specular;
	
	// Roughness
	vec4 roughness_texture_channel = vec4(1.0, 0.0, 0.0, 0.0);
	float roughness_tex = dot(texture(texture_roughness, base_uv), roughness_texture_channel);
	ROUGHNESS = roughness_tex * roughness;

	// Normal map
	NORMAL_MAP = texture(texture_normal, base_uv).rgb;
	NORMAL_MAP_DEPTH = smoothstep(dark_normal_step.x, dark_normal_step.y, light_capture_sample.r) * normal_strength;

	// Emission
	vec3 emission_tex = texture(texture_emission, base_uv).rgb;
	EMISSION = (emission.rgb + emission_tex) * emission_energy;
	
	vec3 dark_emission = dark_detail_color.rgb * smoothstep(dark_detail_texture_step.x, dark_detail_texture_step.y, 1.0 - textureLod(dark_detail_texture, base_uv, dark_detail_texture_lod).r) * dark_detail_color.a;
	dark_emission *= clamp(smoothstep(dark_detail_noise_step_range.x, dark_detail_noise_step_range.y, texture(dark_detail_noise_texture, base_uv * dark_detail_noise_scale).r) + dark_detail_noise_min_value, 0, 1);
	EMISSION += dark_emission * (1.0 - light_capture_dither);

	// Ambient Occlusion
	AO = dot(texture(texture_ambient_occlusion, base_uv), ao_texture_channel);
	AO_LIGHT_AFFECT = ao_light_affect;
	
	// Backlight
	vec3 backlight_tex = texture(texture_backlight, base_uv).rgb;
	BACKLIGHT = (backlight.rgb + backlight_tex);
}
