shader_type canvas_item;
render_mode blend_mix, world_vertex_coords;

// credit: afk on godotshaders.com
// https://godotshaders.com/shader/tilemap-cell-uv/
const float tile_size = 1.0 / 24.0;
vec2 get_cell_uv(vec2 uv, vec2 pixel_size) {
	vec2 ut = vec2(0, 0);
	// No idea why I need these weird offsets; they weren't in the original code
	ut.x = mod(1.0 - uv.x + 0.20193, pixel_size.x / tile_size) / pixel_size.x * tile_size;
	ut.y = mod(1.0 - uv.y + 0.20962, pixel_size.y / tile_size) / pixel_size.y * tile_size;
	
	return ut;
}

// credit: blaze_the_star on godot forums
// https://forum.godotengine.org/t/in-a-shader-how-to-map-uv-coordinates-of-texture-to-the-uv-coordinates-of-a-tile-in-a-tileset/13502
uniform vec2 node_size = vec2(400.0, 400.0);
varying vec2 vert;
void vertex(){
	vert = VERTEX;
}


void fragment() {
	// credit: blaze_the_star on godot forums
	vec2 full_uv = vert/node_size;
	
	// credit: afk on godotshaders.com
	vec2 cell_uv = get_cell_uv(UV, TEXTURE_PIXEL_SIZE);
	COLOR = vec4(full_uv.x, cell_uv.y, 0.0, 1.0);
	
	// rest is mine
	float xFactor = 2.0 * PI;
	float yOffset = -0.7;
	float waveHeightFactor = 0.05;
	float surfaceThickness = 1.0;
	
	// Colors
	vec4 foamColor  = vec4(0.984314, 1.000000, 0.525490, 1.000000);
	vec4 fluidColor = vec4(0.840693, 0.880000, 0.290400, 0.750000);
	
	// ~waves~
	float height = cell_uv.y + yOffset;
	height += sin( TIME + full_uv.x * xFactor) * waveHeightFactor;
	height += sin( TIME + full_uv.x * xFactor * 2.0 + 1.25) * waveHeightFactor * 0.5;
	height += sin(-TIME + full_uv.x * xFactor * 3.0 + 2.12) * waveHeightFactor * 0.25;
	height += sin(-TIME + full_uv.x * xFactor * 5.0 + 0.73) * waveHeightFactor * 0.125;
	
	// the 0.02 is just a magic number; no idea what it depends on
	if(abs(height) < 0.02 * surfaceThickness)
	{
		COLOR.rgb = foamColor.xyz;
		COLOR.a = foamColor.w;
	}
	else if(height < 0.0)
	{
		COLOR.rgb = fluidColor.xyz;
		COLOR.a = fluidColor.w;
	}
	else
	{
		COLOR.rgb = vec3(0, 0, 0);
		COLOR.a = 0.0;
	}
}