using System;
using Blaggard.Common;
using WasteDrudgers.Level;

namespace WasteDrudgers.Entities
{
    public static partial class Systems
    {
        // TODO: Not sure if this should be in Systems since this isn't really ECS specific
        public static void RepeatActionSystem(IContext ctx, World world)
        {
            if (world.repeatActionQueue.Count > 0)
            {
                var command = world.repeatActionQueue.Peek();
                var playerData = world.PlayerData;

                if (command >= Command.MoveSouthWestRepeat && command <= Command.MoveNorthEastRepeat)
                {
                    DoRepeatMove(world, command, playerData.coords);
                }

                if (world.fov.CreaturesInSight(world))
                {
                    world.repeatActionQueue.Clear();
                }
            }
        }

        private static void DoRepeatMove(World world, Command command, Vec2 pos)
        {
            var position = command switch
            {
                Command.MoveSouthWestRepeat => new Vec2(pos.x - 1, pos.y + 1),
                Command.MoveSouthRepeat => new Vec2(pos.x, pos.y + 1),
                Command.MoveSouthEastRepeat => new Vec2(pos.x + 1, pos.y + 1),
                Command.MoveWestRepeat => new Vec2(pos.x - 1, pos.y),
                Command.MoveEastRepeat => new Vec2(pos.x + 1, pos.y),
                Command.MoveNorthWestRepeat => new Vec2(pos.x - 1, pos.y - 1),
                Command.MoveNorthRepeat => new Vec2(pos.x, pos.y - 1),
                Command.MoveNorthEastRepeat => new Vec2(pos.x + 1, pos.y - 1),
                _ => throw new Exception("Inapplicable command")
            };
            if (LevelUtils.IsPassable(world, position))
            {
                world.repeatActionQueue.Enqueue(command);
            }
            else
            {
                world.repeatActionQueue.Clear();
            }
        }
    }
}