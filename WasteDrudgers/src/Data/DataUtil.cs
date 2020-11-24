using System;
using System.IO;
using System.Linq;
using Blaggard.Graphics;
using Newtonsoft.Json.Linq;

namespace WasteDrudgers
{
    public static class DataUtil
    {
        public static void CreateTilesetGraphic()
        {
            var tempDisplay = new Display(16, 16, 1, false);
            IBlittable layer = new Canvas(tempDisplay, 16, 16);

            tempDisplay.SetRenderTarget(layer.Texture);

            var tiles = Data.IndexedTiles;
            for (int i = 0; i < tiles.Length; i++)
            {
                var tile = tiles[i];
                layer.SetCell(i % 16, i / 16, tile.characters[0], tile.foreground, tile.background);
            }

            layer.Render();

            tempDisplay.SaveTextureToFile(layer.Texture);

            layer.Dispose();
            tempDisplay.Dispose();
        }
    }
}