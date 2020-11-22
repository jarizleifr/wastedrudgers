using Blaggard.Common;
using ManulECS;

namespace WasteDrudgers.Entities
{
    public static class Effects
    {
        public static Entity Create(World world, Vec2 pos)
        {
            var entity = world.ecs.Create();

            world.ecs.Assign(entity, new Position { coords = pos });
            world.ecs.Assign(entity, new Effect
            {
                characters = "*X+",
                color = Data.Colors.blueLight,
                delta = 0
            });

            return entity;
        }
    }
}