using System.Collections.Generic;
using System.Linq;
using Blaggard.Common;
using ManulECS;

namespace WasteDrudgers.Level
{
    public class FOV
    {
        private IEnumerable<IMapCell> cells = Enumerable.Empty<IMapCell>();

        public void Clear() => cells = Enumerable.Empty<IMapCell>();

        public void Recalculate(Map map, Vec2 origin, int fovRange)
        {
            var newCells = LightCaster.CalculateFOV(map, origin, fovRange, (cell) => cell.Flags(TileFlags.BlocksVision));

            foreach (var cell in cells)
            {
                if (newCells.Contains(cell)) continue;
                cell.Visibility = Visibility.Explored;
            }
            foreach (var cell in newCells)
            {
                if (cells.Contains(cell)) continue;
                cell.Visibility = Visibility.Visible;
            }
            cells = newCells;
        }
    }
}