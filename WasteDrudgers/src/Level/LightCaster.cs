using System;
using System.Collections.Generic;
using Blaggard;
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

        public static IEnumerable<IMapCell> CalculateFOV(Map map, Vec2 origin, int radius, Func<IMapCell, bool> isBlocking)
        {
            HashSet<IMapCell> positions = new HashSet<IMapCell>() { map[origin] };
            foreach (var octant in octants)
            {
                CastLight(map, origin, radius, positions, 1, 1.0f, 0.0f, octant, isBlocking);
            }
            return positions;
        }

        private static void CastLight(Map map, Vec2 origin, int radius, HashSet<IMapCell> positions, int startRow, float startSlope, float endSlope, Octant octant, Func<IMapCell, bool> isBlocking)
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
                    var position = map[new Vec2(x, y)];

                    if (deltaX * deltaX + deltaY * deltaY <= radius * radius)
                    {
                        positions.Add(position);
                    }

                    bool currentBlocked = isBlocking(position);
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
                            CastLight(map, origin, radius, positions, row + 1, startSlope, leftSlope, octant, isBlocking);
                            newStartSlope = rightSlope;
                        }
                    }
                }
            }
        }
    }
}