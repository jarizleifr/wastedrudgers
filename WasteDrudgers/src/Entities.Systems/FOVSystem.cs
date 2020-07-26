using WasteDrudgers.Level;

namespace WasteDrudgers.Entities
{
    public static partial class Systems
    {
        public static void FOVSystem(IContext ctx, World world)
        {
            (var map, var playerData) = world.ecs.FetchResource<Map, PlayerData>();
            var stats = world.ecs.GetRef<Stats>(playerData.entity);
            world.fov.Recalculate(map, playerData.coords, stats.awareness);
        }
    }
}