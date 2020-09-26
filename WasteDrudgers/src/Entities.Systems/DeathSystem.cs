using ManulECS;

namespace WasteDrudgers.Entities
{
    public static partial class Systems
    {
        public static void DeathSystem(IContext ctx, World world)
        {
            world.ecs.Loop<Death>((Entity entity, ref Death death) =>
            {
                if (!world.ecs.Has<Player>(entity))
                {
                    world.ecs.Remove(entity);
                }
            });
        }
    }
}