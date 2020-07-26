using Blaggard;

namespace WasteDrudgers.Level
{
    // TODO: Combine with GenUtils and make into a pure function static class
    public class MapBuilder
    {
        private Map map;

        public MapBuilder(Map map)
        {
            this.map = map;
        }

        public MapBuilder Fill(Tile tile)
        {
            Rect(0, 0, map.width, map.height, tile);
            return this;
        }

        public MapBuilder Rect(int x1, int y1, int x2, int y2, Tile tile)
        {
            for (int x = x1; x < x2; x++)
            {
                for (int y = y1; y < y2; y++)
                {
                    SetCell(x, y, tile);
                }
            }
            return this;
        }

        public MapBuilder AddNoise(int x1, int y1, int x2, int y2, int noise, Tile tile)
        {
            for (int x = x1; x < x2; x++)
            {
                for (int y = y1; y < y2; y++)
                {
                    if (RNG.Int(0, 100) < noise)
                        SetCell(x, y, tile);
                }
            }
            return this;
        }

        public MapBuilder SetBorders(Tile tile)
        {
            for (int x = 0; x < map.width; x++)
            {
                SetCell(x, 0, tile);
                SetCell(x, map.height - 1, tile);
            }
            for (int y = 0; y < map.height; y++)
            {
                SetCell(0, y, tile);
                SetCell(map.width - 1, y, tile);
            }
            return this;
        }

        public MapBuilder CellularAutomata(int iterations, Tile alive, Tile dead, TileFlags flags)
        {
            int iter = iterations;
            while (iter > 0)
            {
                // Wall is alive/true, floor is dead/false
                bool[,] temp = new bool[map.width, map.height];

                for (int x = 1; x < map.width - 1; x++)
                {
                    for (int y = 1; y < map.height - 1; y++)
                    {
                        int aliveNeighbors = 0;
                        var cell = map[Util.IndexFromXY(x, y, map.width)];

                        foreach (var v in cell.Neighbors)
                        {
                            var neighbor = map[v];
                            if (neighbor.Flags(flags))
                                aliveNeighbors++;
                        }

                        // If 6 or more alive neighbors, born
                        // If 3 or more alive neighbors, survive
                        // If less than 3, die
                        if (aliveNeighbors >= 6)
                            temp[x, y] = true;
                        else if (aliveNeighbors >= 3)
                            temp[x, y] = cell.Flags(flags);
                        else
                            temp[x, y] = false;
                    }
                }

                for (int x = 1; x < map.width - 1; x++)
                {
                    for (int y = 1; y < map.height - 1; y++)
                    {
                        if (temp[x, y])
                            SetCell(x, y, alive);
                        else
                            SetCell(x, y, dead);
                    }
                }
                iter--;
            }
            return this;
        }

        public Map Build() => map;

        private void SetCell(int x, int y, Tile tile) => map[Util.IndexFromXY(x, y, map.width)].Tile = tile;
    }
}