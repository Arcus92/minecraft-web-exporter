using System.Collections.Generic;
using MinecraftWebExporter.Structs;

namespace MinecraftWebExporter.Minecraft.World
{
    /// <summary>
    /// The biome data
    /// </summary>
    public readonly struct Biome
    {
        /// <summary>
        /// Gets the name of the biom
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Gets the default temperature
        /// </summary>
        public float Temperature { get; init; }
        
        /// <summary>
        /// Gets the default rainfall value
        /// </summary>
        public float Rainfall { get; init; }

        /// <summary>
        /// Gets the water surface color
        /// </summary>
        public Vector3 WaterSurfaceColor { get; init; }

        #region Static

        /// <summary>
        /// The default water surface color
        /// </summary>
        public static readonly Vector3 DefaultWaterSurfaceColor = new() {X = 37f / 255f, Y = 150f / 255f, Z = 190f / 255f};
        
        /// <summary>
        /// The internal biome list
        /// </summary>
        private static readonly Dictionary<string, Biome> _biomes = new();

        static Biome()
        {
            var color44AFF5 = new Vector3() {X = 37f / 255f, Y = 150f / 255f, Z = 190f / 255f};
            var color32A598 = new Vector3() {X = 50f / 255f, Y = 165f / 255f, Z = 152f / 255f};
            var color007BF7 = new Vector3() {X = 0f / 255f, Y = 123f / 255f, Z = 247f / 255f};
            var color1E97F2 = new Vector3() {X = 30f / 255f, Y = 151f / 255f, Z = 242f / 255f};
            var color20A3CC = new Vector3() {X = 32f / 255f, Y = 163f / 255f, Z = 204f / 255f};
            var color287082 = new Vector3() {X = 40f / 255f, Y = 112f / 255f, Z = 230f / 255f};
            var color1E6B82 = new Vector3() {X = 30f / 255f, Y = 107f / 255f, Z = 130f / 255f};
            var color4C6559 = new Vector3() {X = 76f / 255f, Y = 101f / 255f, Z = 89f / 255f};
            var color0084FF = new Vector3() {X = 0f / 255f, Y = 132f / 255f, Z = 255f / 255f};
            var color905957 = new Vector3() {X = 144f / 255f, Y = 89f / 255f, Z = 87f / 255f};
            var color62529E = new Vector3() {X = 98f / 255f, Y = 82f / 255f, Z = 158f / 255f};
            var color185390 = new Vector3() {X = 24f / 255f, Y = 83f / 255f, Z = 144f / 255f};
            var color14559B = new Vector3() {X = 20f / 255f, Y = 85f / 255f, Z = 155f / 255f};
            var color1156A7 = new Vector3() {X = 17f / 255f, Y = 86f / 255f, Z = 167f / 255f};
            var color8A8997 = new Vector3() {X = 138f / 255f, Y = 137f / 255f, Z = 151f / 255f};
            var color818193 = new Vector3() {X = 129f / 255f, Y = 129f / 255f, Z = 147f / 255f};
            var color157CAB = new Vector3() {X = 21f / 255f, Y = 124f / 255f, Z = 171f / 255f};
            var color1A7AA1 = new Vector3() {X = 26f / 255f, Y = 122f / 255f, Z = 161f / 255f};
            var color056BD1 = new Vector3() {X = 5f / 255f, Y = 107f / 255f, Z = 209f / 255f};
            var color236583 = new Vector3() {X = 35f / 255f, Y = 101f / 255f, Z = 131f / 255f};
            var color045CD5 = new Vector3() {X = 4f / 255f, Y = 92f / 255f, Z = 213f / 255f};
            var color14A2C5 = new Vector3() {X = 20f / 255f, Y = 162f / 255f, Z = 197f / 255f};
            var color1B9ED8 = new Vector3() {X = 27f / 255f, Y = 158f / 255f, Z = 216f / 255f};
            var color0D8AE3 = new Vector3() {X = 13f / 255f, Y = 138f / 255f, Z = 227f / 255f};
            var color0D67BB = new Vector3() {X = 13f / 255f, Y = 103f / 255f, Z = 187f / 255f};
            
            var color2C8B9C = new Vector3() {X = 44f / 255f, Y = 139f / 255f, Z = 156f / 255f};
            var color2590A8 = new Vector3() {X = 37f / 255f, Y = 144f / 255f, Z = 168f / 255f};
            var color4E7F81 = new Vector3() {X = 78f / 255f, Y = 127f / 255f, Z = 129f / 255f};
            var color497F99 = new Vector3() {X = 73f / 255f, Y = 127f / 255f, Z = 153f / 255f};
            var color55809E = new Vector3() {X = 85f / 255f, Y = 128f / 255f, Z = 158f / 255f};
            
            var color1787D4 = new Vector3() {X = 23f / 255f, Y = 135f / 255f, Z = 212f / 255f};
            var color02B0E5 = new Vector3() {X = 2f / 255f, Y = 179f / 255f, Z = 229f / 255f};
            var color0D96DB = new Vector3() {X = 13f / 255f, Y = 150f / 255f, Z = 219f / 255f};
            var color2080C9 = new Vector3() {X = 32f / 255f, Y = 128f / 255f, Z = 201f / 255f};
            var color2570B5 = new Vector3() {X = 37f / 255f, Y = 112f / 255f, Z = 181f / 255f};
            
            Add("minecraft:the_void", 0.5f, 0.5f);
            Add("minecraft:plains", 0.8f, 0.4f, color44AFF5);
            Add("minecraft:sunflower_plains", 0.8f, 0.4f, color44AFF5);
            Add("minecraft:snowy_plains", 0f, 0.5f, color14559B);
            Add("minecraft:ice_spikes", 0f, 0.5f, color14559B);
            Add("minecraft:desert", 2f, 0f, color32A598);
            Add("minecraft:swamp", 0.8f, 0.4f, color4C6559);
            Add("minecraft:forest", 0.7f, 0.8f, color1E97F2);
            Add("minecraft:flower_forest", 0.7f, 0.8f, color20A3CC);
            Add("minecraft:birch_forest", 0.6f, 0.6f);
            Add("minecraft:dark_forest", 0.7f, 0.8f);
            Add("minecraft:old_growth_birch_forest", 0.6f, 0.6f);
            Add("minecraft:old_growth_pine_taiga", 0.3f, 0.8f);
            Add("minecraft:old_growth_spruce_taiga", 0.25f, 0.8f);
            Add("minecraft:taiga", 0.25f, 0.8f, color287082);
            Add("minecraft:snowy_taiga", -0.5f, 0.4f);
            Add("minecraft:savanna", 1.2f, 0f, color2C8B9C);
            Add("minecraft:savanna_plateau", 1.0f, 0f, color2590A8);
            Add("minecraft:windswept_hills", 0.2f, 0.3f);
            Add("minecraft:windswept_gravelly_hills", 0.2f, 0.3f);
            Add("minecraft:windswept_forest", 0.2f, 0.3f);
            Add("minecraft:windswept_savanna", 1.1f, 0f, color2590A8);
            Add("minecraft:jungle", 0.95f, 0.9f, color14A2C5);
            Add("minecraft:sparse_jungle", 0.95f, 0.8f, color1B9ED8);
            Add("minecraft:bamboo_jungle", 0.95f, 0.9f, color14A2C5);
            Add("minecraft:badlands", 2f, 0f, color4E7F81);
            Add("minecraft:eroded_badlands", 2f, 0f, color497F99);
            Add("minecraft:wooded_badlands", 2f, 0f, color497F99);
            Add("minecraft:meadow", 0.5f, 0.8f);
            Add("minecraft:grove", -0.2f, 0.8f);
            Add("minecraft:snowy_slopes", -0.3f, 0.9f, color1156A7);
            Add("minecraft:frozen_peaks", -0.7f, 0.9f);
            Add("minecraft:jagged_peaks", -0.7f, 0.9f);
            Add("minecraft:stony_peaks", 1.0f, 0.3f);
            Add("minecraft:river", 0.5f, 0.5f, color0084FF);
            Add("minecraft:frozen_river", 0f, 0.5f, color185390);
            Add("minecraft:beach", 0.8f, 0.4f, color157CAB);
            Add("minecraft:snowy_beach", 0.05f, 0.3f);
            Add("minecraft:stony_shore", 0.2f, 0.3f, color0D67BB);
            Add("minecraft:warm_ocean", 0.5f, 0.5f, color02B0E5);
            Add("minecraft:lukewarm_ocean", 0.5f, 0.5f, color0D96DB);
            Add("minecraft:deep_lukewarm_ocean", 0.5f, 0.5f, color0D96DB);
            Add("minecraft:ocean", 0.5f, 0.5f, color1787D4);
            Add("minecraft:deep_ocean", 0.5f, 0.5f, color1787D4);
            Add("minecraft:cold_ocean", 0.5f, 0.5f, color2080C9);
            Add("minecraft:deep_cold_ocean", 0.5f, 0.5f, color2080C9);
            Add("minecraft:frozen_ocean", 0f, 0.5f, color2570B5);
            Add("minecraft:deep_frozen_ocean", 0.5f, 0.5f, color2570B5);
            Add("minecraft:mushroom_fields", 0.9f, 1.0f, color8A8997);
            Add("minecraft:dripstone_caves", 0.8f, 0.4f);
            Add("minecraft:lush_caves", 0.5f, 0.5f);
            Add("minecraft:nether_wastes", 0.5f, 0.5f, color905957);
            Add("minecraft:warped_forest", 0.5f, 0.5f);
            Add("minecraft:crimson_forest", 0.5f, 0.5f);
            Add("minecraft:soul_sand_valley", 0.5f, 0.5f);
            Add("minecraft:basalt_deltas", 0.5f, 0.5f);
            Add("minecraft:the_end", 0.5f, 0.5f, color62529E);
            Add("minecraft:end_highlands", 0.5f, 0.5f);
            Add("minecraft:end_midlands", 0.5f, 0.5f);
            Add("minecraft:small_end_islands", 0.5f, 0.5f);
            Add("minecraft:end_barrens", 0.5f, 0.5f);
        }
        
        /// <summary>
        /// Adds a biome
        /// </summary>
        /// <param name="name"></param>
        /// <param name="temperature"></param>
        /// <param name="rainfall"></param>
        /// <param name="waterSurfaceColor"></param>
        public static void Add(string name, float temperature, float rainfall, Vector3 waterSurfaceColor)
        {
            _biomes.Add(name, new Biome()
            {
                Name = name,
                Temperature = temperature,
                Rainfall = rainfall,
                WaterSurfaceColor = waterSurfaceColor,
            });
        }
        
        /// <summary>
        /// Adds a biome
        /// </summary>
        /// <param name="name"></param>
        /// <param name="temperature"></param>
        /// <param name="rainfall"></param>
        public static void Add(string name, float temperature, float rainfall)
        {
            _biomes.Add(name, new Biome()
            {
                Name = name,
                Temperature = temperature,
                Rainfall = rainfall,
                WaterSurfaceColor = DefaultWaterSurfaceColor,
            });
        }

        /// <summary>
        /// Gets the biome by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Biome Get(string? name)
        {
            if (name is not null && _biomes.TryGetValue(name, out var biome))
            {
                return biome;
            }
            return new Biome()
            {
                Name = "minecraft:the_void",
                Temperature = 0.5f,
                Rainfall = 0.5f,
                WaterSurfaceColor = DefaultWaterSurfaceColor,
            };
        }

        #endregion Static
    }
}