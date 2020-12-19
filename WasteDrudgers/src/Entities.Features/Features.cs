using Blaggard.Common;
using ManulECS;

namespace WasteDrudgers.Entities
{
    public static class Features
    {
        public static Entity CreatePortal(World world, Vec2 pos, DBPortal portal)
        {
            var entity = CreateFeature(world, pos, portal.Feature);
            world.ecs.Assign(entity, new Portal { targetLevel = portal.Target });
            return entity;
        }

        public static Entity CreateFeature(World world, Vec2 pos, string id) => CreateFeature(world, pos, Data.GetFeature(id));

        public static Entity CreateFeature(World world, Vec2 pos, DBFeature feature)
        {
            var entity = world.ecs.Create();

            world.ecs.Assign(entity, new Position { coords = pos });
            world.ecs.Assign(entity, new Feature { });
            world.ecs.Assign(entity, new Identity
            {
                name = feature.Name,
                rawName = feature.Id,
                description = feature.Description,
            });
            world.ecs.Assign(entity, new Renderable
            {
                character = feature.Char,
                color = feature.Color,
            });

            if (feature.Trigger != EntryTriggerType.None)
            {
                world.ecs.Assign<EntryTrigger>(entity, new EntryTrigger { type = feature.Trigger });
            }

            world.spatial.SetFeature(pos, entity);

            return entity;
        }

        public static void Trigger(World world, Entity entity, Entity feature, Vec2 position, EntryTrigger trigger)
        {
            switch (trigger.type)
            {
                case EntryTriggerType.Thorns:
                    var effect = VisualEffects.Create(world, position);
                    var damage = RNG.IntInclusive(1, 6);
                    world.WriteToLog("stepped_on_thorns", position, LogArgs.Actor(entity), LogArgs.Num(damage));
                    var damageEntity = world.ecs.Create();
                    world.ecs.Assign(damageEntity, new Damage { target = entity, damage = damage });
                    break;
            }
        }
    }
}