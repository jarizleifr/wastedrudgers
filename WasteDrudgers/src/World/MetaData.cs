using System;
using Blaggard.Common;
using ManulECS;

using Newtonsoft.Json;

using WasteDrudgers.Entities;

namespace WasteDrudgers
{
    public class TargetData
    {
        public readonly Renderable renderable;
        public readonly Health health;
        public readonly string name;
        public readonly int hitChance;

        public static TargetData Create(World world, PlayerData data)
        {
            var target = data.lastTarget;
            if (!target.HasValue || !world.ecs.IsAlive(target.Value)) return null;

            return new TargetData(world, data.entity, target.Value);
        }

        private TargetData(World world, Entity player, Entity target)
        {
            renderable = world.ecs.GetRef<Renderable>(target);
            health = world.ecs.GetRef<Health>(target);
            name = world.ecs.GetRef<Identity>(target).rawName;

            var attacker = world.ecs.GetRef<Combat>(player);
            var defender = world.ecs.GetRef<Combat>(target);

            hitChance = Math.Max(1, (attacker.hitChance * (100 - defender.dodge)) / 100);
        }
    }

    [SerializationProfile("global")]
    public class PlayerData
    {
        public Entity entity;
        public string name;
        public string currentLevel;
        public int turns;

        [JsonIgnore]
        public Vec2 coords;

        [JsonIgnore]
        public Entity? lastTarget;
    }
}