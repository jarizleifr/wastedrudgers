using System;
using System.Collections.Generic;
using Blaggard.Common;

namespace WasteDrudgers.Level
{
    public class FOV
    {
        private List<Vec2> cells = new List<Vec2>(128);
        private Func<IMapCell, bool> callback = (cell) =>
            cell.Flags(TileFlags.BlocksVision);

        public void Recalculate(Map map, Vec2 origin, int fovRange)
        {
            foreach (var pos in cells)
            {
                var cell = map[pos];
                cell.Visibility = Visibility.Explored;
            }

            LightCaster.CalculateFOV(map, cells, origin, fovRange, callback);

            foreach (var pos in cells)
            {
                var cell = map[pos];
                cell.Visibility = Visibility.Visible;
            }
        }

        public bool CreaturesInSight(World world) =>
            world.spatial.HasCreatures(cells, world.PlayerData.coords);

        public void Clear() => cells.Clear();
    }
}