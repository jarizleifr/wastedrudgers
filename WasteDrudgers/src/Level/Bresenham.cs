using System;
using System.Collections.Generic;
using Blaggard.Common;

namespace WasteDrudgers.Level
{
    public static class Bresenham
    {
        public static IEnumerable<IMapCell> GetPath(Map map, Vec2 origin, Vec2 target)
        {
            int x1, y1, x2, y2;
            bool steep = Math.Abs(target.y - origin.y) > Math.Abs(target.x - origin.x);

            if (!steep)
            {
                x1 = origin.x; y1 = origin.y;
                x2 = target.x; y2 = target.y;
            }
            else
            {
                x1 = origin.y; y1 = origin.x;
                x2 = target.y; y2 = target.x;
            }

            int sign = 1;
            if (x1 > x2)
            {
                sign = -1;
                x1 *= sign; x2 *= sign;
            }

            int dx = x2 - x1;
            int dy = Math.Abs(y2 - y1);
            int err = dx / 2;

            int yStep = y1 < y2 ? 1 : -1;
            int y = y1;

            var nodes = new List<IMapCell>();
            for (int x = x1; x <= x2; x++)
            {
                var pos = (steep) ? new Vec2(y, x * sign) : new Vec2(x * sign, y);
                nodes.Add(map[pos]);
                err -= Math.Abs(dy);
                if (err < 0)
                {
                    y += yStep;
                    err += dx;
                }
            }
            return nodes;
        }
    }
}