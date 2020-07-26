using System;
using Blaggard.Common;
using ManulECS;
using WasteDrudgers.Level;

namespace WasteDrudgers.Entities
{
    public static partial class Systems
    {
        public static void AISystem(IContext ctx, World world)
        {
            var playerData = world.ecs.FetchResource<PlayerData>();
            world.ecs.Loop((Entity entity, ref Turn turn, ref Position position, ref AI ai) =>
            {
                var move = Vec2.Zero;
                if (LevelUtils.HasLineOfSight(world, position.coords, playerData.coords))
                {
                    move = GetMoveTowards(position.coords, playerData.coords);
                }
                else
                {
                    move = Vec2.FromDirection(RNG.RandomDirection);
                }
                world.ecs.Assign(entity, new IntentionMove { transform = move });
            });
        }

        private static Vec2 GetMoveTowards(Vec2 origin, Vec2 target)
        {
            int mx = Math.Sign(target.x - origin.x);
            int my = Math.Sign(target.y - origin.y);
            return new Vec2(mx, my);
        }
    }
}