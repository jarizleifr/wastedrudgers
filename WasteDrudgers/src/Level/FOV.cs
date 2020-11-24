using System;
using System.Collections.Generic;
using Blaggard.Common;

namespace WasteDrudgers.Level
{
    public class FOV
    {
        private List<IMapCell> cells = new List<IMapCell>(128);
        private Func<IMapCell, bool> callback = (cell) =>
            cell.Flags(TileFlags.BlocksVision);

        public void Recalculate(Map map, Vec2 origin, int fovRange)
        {
            foreach (var pos in cells)
            {
                pos.Visibility = Visibility.Explored;
            }

            LightCaster.CalculateFOV(map, cells, origin, fovRange, callback);

            foreach (var pos in cells)
            {
                pos.Visibility = Visibility.Visible;
            }
        }
    }
}