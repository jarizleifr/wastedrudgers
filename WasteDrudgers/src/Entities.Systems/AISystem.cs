using System;
using Blaggard.Common;
using WasteDrudgers.Level;

namespace WasteDrudgers.Entities
{
    public static partial class Systems
    {
        public static void AISystem(IContext ctx, World world)
        {
            var playerData = world.PlayerData;
            foreach (var e in world.ecs.View<Position, Turn, AI>())
            {
                ref var pos = ref world.ecs.GetRef<Position>(e);

                var move = Vec2.Zero;
                if (LevelUtils.HasLineOfSight(world, pos.coords, playerData.coords))
                {
                    move = GetMoveTowards(pos.coords, playerData.coords);
                }
                else
                {
                    move = Vec2.FromDirection(RNG.RandomDirection);
                }
                world.ecs.Assign(e, new IntentionMove { transform = move });
            }
        }

        private static Vec2 GetMoveTowards(Vec2 origin, Vec2 target)
        {
            int mx = Math.Sign(target.x - origin.x);
            int my = Math.Sign(target.y - origin.y);
            return new Vec2(mx, my);
        }
    }
}