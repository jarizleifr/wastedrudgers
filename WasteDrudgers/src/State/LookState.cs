using Blaggard;
using Blaggard.Common;

using WasteDrudgers.Render;

namespace WasteDrudgers.State
{
    [InputDomains("game")]
    public class LookState : Scene
    {
        public Vec2 coords;

        public override void Update(IContext ctx, World world)
        {
            var cmd = ctx.Command;
            switch (cmd)
            {
                case Command.Exit:
                case Command.Look:
                    world.SetState(ctx, RunState.AwaitingInput);
                    return;
            }

            var move = FromCommand(world, cmd);

            var map = world.Map;
            if (move != Vec2.Zero && Util.IsWithinBounds(coords.x + move.x, coords.y + move.y, map.width, map.height))
            {
                coords += move;
                world.ShouldRedraw = true;
            }

            Views.DrawLookView(ctx, world, coords);
        }

        public Vec2 FromCommand(World world, Command cmd)
        {
            if (cmd >= Command.MoveSouthWest && cmd <= Command.MoveNorthEast)
            {
                world.ShouldRedraw = true;
                return Vec2.FromDirection(cmd switch
                {
                    Command.MoveSouthWest => Direction.SouthWest,
                    Command.MoveSouth => Direction.South,
                    Command.MoveSouthEast => Direction.SouthEast,
                    Command.MoveWest => Direction.West,
                    Command.MoveEast => Direction.East,
                    Command.MoveNorthWest => Direction.NorthWest,
                    Command.MoveNorth => Direction.North,
                    Command.MoveNorthEast => Direction.NorthEast,
                });
            }
            return Vec2.Zero;
        }
    }
}