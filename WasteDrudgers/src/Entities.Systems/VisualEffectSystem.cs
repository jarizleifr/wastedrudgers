using WasteDrudgers.Level;

namespace WasteDrudgers.Entities
{
    public static partial class Systems
    {
        public static void VisualEffectSystem(IContext ctx, World world)
        {
            var map = world.Map;
            foreach (var e in world.ecs.View<VisualEffect, Position>())
            {
                ref var eff = ref world.ecs.GetRef<VisualEffect>(e);
                ref var pos = ref world.ecs.GetRef<Position>(e);

                switch (map[pos.coords].Visibility)
                {
                    case Visibility.Visible:
                        var oldIndex = (int)eff.delta;
                        eff.delta += ctx.DeltaTime * 20;
                        var newIndex = (int)eff.delta;

                        if (oldIndex != newIndex)
                        {
                            world.ShouldRedraw = true;
                        }
                        if (newIndex >= eff.characters.Length)
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