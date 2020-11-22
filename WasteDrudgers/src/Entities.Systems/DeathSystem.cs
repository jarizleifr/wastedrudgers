namespace WasteDrudgers.Entities
{
    public static partial class Systems
    {
        public static void DeathSystem(IContext ctx, World world)
        {
            foreach (var e in world.ecs.View<Death>())
            {
                if (!world.ecs.Has<Player>(e))
                {
                    world.ecs.Remove(e);
                }
            }
        }
    }
}