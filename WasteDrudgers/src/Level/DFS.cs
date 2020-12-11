using System;

namespace WasteDrudgers.Level
{
    static class DFS
    {
        private const int MAX_DISTANCE = 24;

        public static int[] Calculate(Map map, int origin, Func<IMapCell, bool> isBlocking)
        {
            var distanceMap = new int[map.Width * map.Height];
            for (int i = 0; i < distanceMap.Length; i++)
            {
                distanceMap[i] = MAX_DISTANCE;
            }

            Calc(map, origin, 0, MAX_DISTANCE, ref distanceMap, isBlocking);
            return distanceMap;
        }

        public static void Print(Map map, int[] distanceMap)
        {
            for (int y = 0; y < map.Height; y++)
            {
                var line = "";
                for (int x = 0; x < map.Width; x++)
                {
                    line += distanceMap[x + y * map.Width].ToString("D2");
                }
                Console.WriteLine(line);
            }
        }

        private static void Calc(Map map, int node, int distance, int maxDistance, ref int[] distanceMap, Func<IMapCell, bool> isBlocking)
        {
            if (node < 0 || node >= distanceMap.Length) return;
            if (isBlocking(map[node])) return;

            var current = distanceMap[node];
            var proposed = distance + 1;

            if (proposed < maxDistance && proposed < current)
            {
                distanceMap[node] = distance;
                var (w, h) = (map.Width, map.Height);
                var (x, y) = (node % w, node / w);
                if (x >= 1 && proposed < distanceMap[node - 1])
                {
                    Calc(map, node - 1, proposed, maxDistance, ref distanceMap, isBlocking);
                }
                if (x < w - 1 && proposed < distanceMap[node + 1])
                {
                    Calc(map, node + 1, proposed, maxDistance, ref distanceMap, isBlocking);
                }
                if (y >= 1 && proposed < distanceMap[node - w])
                {
                    Calc(map, node - w, proposed, maxDistance, ref distanceMap, isBlocking);
                }
                if (y < h - 1 && proposed < distanceMap[node + w])
                {
                    Calc(map, node + w, proposed, maxDistance, ref distanceMap, isBlocking);
                }
            }
        }
    }
}
