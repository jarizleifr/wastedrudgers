namespace WasteDrudgers.Level.Generation
{
    public class WorldStrategy : ILevelGenerationStrategy
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public string Floor { get; set; }
        public string Wall { get; set; }

        public void Generate(World world, string levelName, ref Map map)
        {
            if (map == null)
            {
                map = new Map(levelName, Width, Height);
            }
            RNG.Seed(map.seed);

            var floor = world.database.GetTile(Floor);
            var wall = world.database.GetTile(Wall);

            map = new MapBuilder(map)
                .Fill(floor)
                .AddNoise(0, 0, Width, Height, 40, wall)
                .SetBorders(wall)
                .CellularAutomata(5, wall, floor, TileFlags.BlocksMovement)
                .Build();
        }
    }
}