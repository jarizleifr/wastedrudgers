using WasteDrudgers.Level;

namespace WasteDrudgers.Entities
{
    public static partial class Systems
    {
        public static void EffectSystem(IContext ctx, World world)
        {
            var map = world.Map;
            foreach (var e in world.ecs.View<Effect, Position>())
            {
                ref var eff = ref world.ecs.GetRef<Effect>(e);
                ref var pos = ref world.ecs.GetRef<Position>(e);

                switch (map[pos.coords].Visibility)
                {
                    case Visibility.Visible:
                        world.ShouldRedraw = true;
                        eff.delta += ctx.DeltaTime * 20;
                        if ((int)eff.delta == eff.characters.Length)
                        {
                            world.ecs.Remove(e);
                        }
                        break;
                    default:
                        world.ecs.Remove(e);
                        break;
                }
            }
        }
    }
}