using System;
using Blaggard;
using Blaggard.Common;
using Blaggard.Graphics;
using ManulECS;
using WasteDrudgers.Level;

namespace WasteDrudgers.Render
{
    public static class Viewport
    {
        public static void Draw(IContext ctx, IBlittable layer, Theme theme, World world, Vec2 origin)
        {
            switch (ctx.Config.Style)
            {
                case GraphicsStyle.Tiles:
                    DrawSpriteMode(ctx, layer, world, origin);
                    break;

                default:
                    DrawASCIIMode(ctx, layer, world, origin);
                    break;
            }
        }

        // TODO: There are bound errors when screen is larger than map
        public static void DrawASCIIMode(IContext ctx, IBlittable layer, World world, Vec2 origin)
        {
            var rect = new Rect(0, 0, layer.Width, layer.Height);
            layer.ResetColors();
            layer.Clear();

            var map = world.ecs.FetchResource<Map>();
            var tiles = map.GetTiles();
            var vis = map.GetVisibility();

            CameraOffset(origin, map.width, map.height, rect.width, rect.height, out int xOffset, out int yOffset);

            for (int y = 0; y < rect.height; y++)
            {
                for (int x = 0; x < rect.width; x++)
                {
                    var i = Util.IndexFromXY(x + xOffset, y + yOffset, map.width);
                    if (ctx.Config.Style == GraphicsStyle.Glyphs)
                    {
                        bool wallBelow = i + map.width > map.width * map.height || (tiles[i + map.width].flags & TileFlags.BlocksMovement) > 0;
                        DrawTile(ctx, layer, tiles[i], vis[i], x, y, i, wallBelow);
                    }
                    else
                    {
                        DrawTile(ctx, layer, tiles[i], vis[i], x, y, i);
                    }
                }
            }

            //DrawEntities<Portal>(ctx, layer, world, rect, xOffset, yOffset, map.width, vis, false);
            DrawEntities<Feature>(ctx, layer, world, rect, xOffset, yOffset, map.width, vis, false);
            DrawItems(ctx, layer, world, rect, xOffset, yOffset, map.width, vis);
            DrawEntities<Actor>(ctx, layer, world, rect, xOffset, yOffset, map.width, vis);

            world.ecs.ThreadedLoop<Position, Effect>((Entity Entity, ref Position pos, ref Effect eff) =>
            {
                int x = pos.coords.x - xOffset;
                int y = pos.coords.y - yOffset;
                if (!rect.IsWithinBounds(x, y)) return;

                var i = pos.coords.ToIndex(map.width);
                if (vis[i] != Visibility.Visible) return;

                layer.PutChar(x, y, eff.characters[(int)eff.delta], eff.color);
            });
        }

        public static void DrawSpriteMode(IContext ctx, IBlittable layer, World world, Vec2 origin)
        {
            var rect = new Rect(0, 0, layer.Width / 2, layer.Height);
            layer.Clear();

            var map = world.ecs.FetchResource<Map>();
            var tiles = map.GetTiles();
            var vis = map.GetVisibility();

            CameraOffset(origin, map.width, map.height, rect.width, rect.height, out int xOffset, out int yOffset);

            for (int y = 0; y < rect.height; y++)
            {
                for (int x = 0; x < rect.width; x++)
                {
                    var i = Util.IndexFromXY(x + xOffset, y + yOffset, map.width);
                    var tile = tiles[i];

                    if ((tile.flags & TileFlags.BlocksMovement) == 0)
                    {
                        switch (vis[i])
                        {
                            case Visibility.Visible:
                                layer.DrawSprite(new Sprite(4 + x * 24, y * 16, ctx.TextureData.Get(tile.sprite), 0));
                                break;
                            case Visibility.Explored:
                                layer.DrawSprite(new Sprite(4 + x * 24, y * 16, ctx.TextureData.Get("floor_dark"), 0));
                                break;
                        }
                    }
                    else
                    {
                        switch (vis[i])
                        {
                            case Visibility.Visible:
                                layer.DrawSprite(new Sprite(4 + x * 24, y * 16, ctx.TextureData.Get(tile.sprite), 10 + y));
                                break;
                            case Visibility.Explored:
                                layer.DrawSprite(new Sprite(4 + x * 24, y * 16, ctx.TextureData.Get("wall_dark"), 10 + y));
                                break;
                        }
                    }
                }
            }

            DrawEntities<Actor>(ctx, layer, world, rect, xOffset, yOffset, map.width, vis);
        }

        private static void DrawItems(IContext ctx, IBlittable layer, World world, Rect rect, int xOffset, int yOffset, int mapWidth, Visibility[] vis)
        {
            foreach (var pos in world.spatial.GetPositionsWithItems())
            {
                int x = pos.x - xOffset;
                int y = pos.y - yOffset;
                if (!rect.IsWithinBounds(x, y)) continue;

                var i = pos.ToIndex(mapWidth);
                if (vis[i] == Visibility.Visible)
                {
                    var renderable = world.spatial.GetItemsRenderable(pos, world);

                    // TODO: this should probably be refactored as its own function
                    char ch = renderable.character;
                    if (renderable.glyph != 0 && ctx.Config.Style >= GraphicsStyle.Glyphs)
                    {
                        ch = renderable.glyph;
                    }
                    layer.PutChar(x, y, ch, renderable.color);
                }
            }
        }

        private static void DrawTile(IContext ctx, IBlittable layer, Tile tile, Visibility vis, int x, int y, int i, bool wallBelow = false)
        {
            if (vis == Visibility.Hidden) return;

            switch (ctx.Config.Style)
            {
                default:
                    (var fore, var back) = vis == Visibility.Explored
                        ? (ctx.Theme.shadowLight, ctx.Theme.shadowDark)
                        : (tile.foreground, tile.background);

                    if (ctx.Config.Style == GraphicsStyle.Glyphs && tile.glyph != 0)
                    {
                        if ((tile.flags & TileFlags.BlocksMovement) > 0 && wallBelow)
                        {
                            layer.PutChar(x, y, '▒', fore, back);
                            return;
                        }
                        layer.PutChar(x, y, tile.glyph, fore, back);
                        return;
                    }

                    var ch = (tile.characters.Length > 1 && !ctx.Config.SimpleTiles)
                        ? 1 + HashNoise.GetIndex(i, tile.characters.Length - 1)
                        : 0;

                    layer.PutChar(x, y, tile.characters[ch], fore, back);
                    break;
            }
        }

        private static void DrawEntity(IContext ctx, IBlittable layer, int x, int y, Renderable renderable, Visibility vis, bool onlyVisible)
        {
            switch (ctx.Config.Style)
            {
                case GraphicsStyle.Tiles:
                    layer.DrawSprite(new Sprite(4 + x * 24, y * 16, ctx.TextureData.Get("dude"), 10 + y));
                    break;

                default:
                    switch (vis)
                    {
                        case Visibility.Explored:
                            if (onlyVisible) return;
                            layer.PutChar(x, y, renderable.character, ctx.Theme.shadowLight);

                            break;
                        case Visibility.Visible:
                            layer.PutChar(x, y, renderable.character, renderable.color);
                            break;
                    }
                    break;
            }
        }

        private static void DrawEntities<T>(IContext ctx, IBlittable layer, World world, Rect rect, int xOffset, int yOffset, int mapWidth, Visibility[] vis, bool onlyVisible = true) where T : struct
        {
            world.ecs.Loop<Position, Renderable, T>((Entity entity, ref Position pos, ref Renderable renderable, ref T t) =>
            {
                int x = pos.coords.x - xOffset;
                int y = pos.coords.y - yOffset;
                if (!rect.IsWithinBounds(x, y)) return;

                var i = pos.coords.ToIndex(mapWidth);
                DrawEntity(ctx, layer, x, y, renderable, vis[i], onlyVisible);
            });
        }

        public static void CameraOffset(Vec2 coords, int mapWidth, int mapHeight, int viewportWidth, int viewportHeight, out int xOffset, out int yOffset)
        {
            xOffset = Math.Clamp(coords.x - (viewportWidth / 2), 0, Math.Max(0, mapWidth - viewportWidth));
            yOffset = Math.Clamp(coords.y - (viewportHeight / 2), 0, Math.Max(0, mapHeight - viewportHeight));
        }
    }
}