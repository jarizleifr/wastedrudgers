using System.IO;
using Newtonsoft.Json;

namespace WasteDrudgers.Level.Generation
{
    public class LoadStrategy : ILevelGenerationStrategy
    {
        public string Filename { get; set; }

        public void Generate(World world, string levelName, ref Map map)
        {
            var data = world.database.GetMapData(Filename);
            if (map == null)
            {
                map = new Map(levelName, data.Width, data.Height);
            }
            RNG.Seed(map.seed);

            for (int i = 0; i < data.Width * data.Height; i++)
            {
                map[i].Tile = world.database.GetTileByIndex(data.Tiles[i]);
            }
        }
    }
}