using ManulECS;
using WasteDrudgers.Level;

namespace WasteDrudgers.Entities
{
    public static partial class Systems
    {
        public static void EffectSystem(IContext ctx, World world)
        {
            var map = world.Map;
            world.ecs.Loop<Effect, Position>((Entity entity, ref Effect eff, ref Position pos) =>
            {
                switch (map[pos.coords].Visibility)
                {
                    case Visibility.Visible:
                        world.ShouldRedraw = true;
                        eff.delta += ctx.DeltaTime * 20;
                        if ((int)eff.delta == eff.characters.Length)
                        {
                            world.ecs.Remove(entity);
                        }
                        break;
                    default:
                        world.ecs.Remove(entity);
                        break;
                }
            });
        }
    }
}