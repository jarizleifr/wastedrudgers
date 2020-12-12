using System;
using System.Collections.Generic;
using Blaggard.Common;

namespace WasteDrudgers.Level
{
    public static class LightCaster
    {
        private readonly struct Octant
        {
            public readonly int xx, xy, yx, yy;
            public Octant(int xx, int xy, int yx, int yy)
            {
                this.xx = xx; this.xy = xy;
                this.yx = yx; this.yy = yy;
            }
        }

        private static Octant[] octants = new Octant[8]
        {
            new Octant( 1, 0, 0, 1), new Octant( 0, 1, 1, 0),
            new Octant( 0,-1, 1, 0), new Octant(-1, 0, 0, 1),
            new Octant(-1, 0, 0,-1), new Octant( 0,-1,-1, 0),
            new Octant( 0, 1,-1, 0), new Octant( 1, 0, 0,-1),
        };

        public static void CalculateFOV(Map map, List<Vec2> cells, Vec2 origin, int radius, Func<IMapCell, bool> isBlocking)
        {
            cells.Clear();
            cells.Add(origin);
            foreach (var octant in octants)
            {
                CastLight(map, cells, origin, radius, 1, 1.0f, 0.0f, octant, isBlocking);
            }
        }

        private static void CastLight(Map map, List<Vec2> cells, Vec2 origin, int radius, int startRow, float startSlope, float endSlope, Octant octant, Func<IMapCell, bool> isBlocking)
        {
            if (startSlope < endSlope) return;

            float newStartSlope = 0.0f;
            bool previousBlocked = false;
            for (int row = startRow; row < radius && !previousBlocked; row++)
            {
                int deltaY = -row;
                for (int deltaX = -row; deltaX <= 0; deltaX++)
                {
                    float leftSlope = (deltaX - 0.5f) / (deltaY + 0.5f),
                          rightSlope = (deltaX + 0.5f) / (deltaY - 0.5f);

                    if (startSlope < rightSlope)
                    {
                        continue;
                    }
                    else if (endSlope > leftSlope)
                    {
                        break;
                    }

                    int x = origin.x + deltaX * octant.xx + deltaY * octant.xy;
                    int y = origin.y + deltaX * octant.yx + deltaY * octant.yy;
                    var pos = new Vec2(x, y);
                    if (deltaX * deltaX + deltaY * deltaY <= radius * radius)
                    {
                        cells.Add(pos);
                    }

                    bool currentBlocked = isBlocking(map[pos]);
                    if (previousBlocked)
                    {
                        if (currentBlocked)
                        {
                            newStartSlope = rightSlope;
                            continue;
                        }
                        previousBlocked = false;
                        startSlope = newStartSlope;
                    }
                    else
                    {
                        if (currentBlocked && row < radius)
                        {
                            previousBlocked = true;
                            CastLight(map, cells, origin, radius, row + 1, startSlope, leftSlope, octant, isBlocking);
                            newStartSlope = rightSlope;
                        }
                    }
                }
            }
        }
    }
}