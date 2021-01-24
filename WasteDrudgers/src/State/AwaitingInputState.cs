using System;
using Blaggard.Common;
using WasteDrudgers.Entities;

namespace WasteDrudgers.State
{
    [InputDomains("game")]
    internal class AwaitingInputState : GameScene
    {
        private PlayerData playerData;

        public override void Initialize(IContext ctx, World world) => playerData = world.PlayerData;

        public override void Update(IContext ctx, World world)
        {
            Systems.VisualEffectSystem(ctx, world);

            Command command = Command.None;
            if (world.repeatActionQueue.Count > 0)
            {
                command = world.repeatActionQueue.Dequeue();
            }
            else
            {
                command = ctx.Command;
            }

            var nextState = command switch
            {
                Command.MoveSouthWest => Move(world, Direction.SouthWest),
                Command.MoveSouth => Move(world, Direction.South),
                Command.MoveSouthEast => Move(world, Direction.SouthEast),
                Command.MoveWest => Move(world, Direction.West),
                Command.MoveEast => Move(world, Direction.East),
                Command.MoveNorthWest => Move(world, Direction.NorthWest),
                Command.MoveNorth => Move(world, Direction.North),
                Command.MoveNorthEast => Move(world, Direction.NorthEast),

                Command.MoveSouthWestRepeat => MoveRepeat(world, Direction.SouthWest),
                Command.MoveSouthRepeat => MoveRepeat(world, Direction.South),
                Command.MoveSouthEastRepeat => MoveRepeat(world, Direction.SouthEast),
                Command.MoveWestRepeat => MoveRepeat(world, Direction.West),
                Command.MoveEastRepeat => MoveRepeat(world, Direction.East),
                Command.MoveNorthWestRepeat => MoveRepeat(world, Direction.NorthWest),
                Command.MoveNorthRepeat => MoveRepeat(world, Direction.North),
                Command.MoveNorthEastRepeat => MoveRepeat(world, Direction.NorthEast),

                Command.Wait => Wait(world),
                Command.WaitExtended => WaitExtended(world),

                Command.Operate => Operate(world),
                Command.Inventory => RunState.Inventory(0, 0),
                Command.CharacterSheet => RunState.CharacterSheet(),
                Command.Manual => RunState.Manual,
                Command.GetItem => GetItem(world),
                Command.Look => RunState.Look(playerData.coords),
                Command.GameMenu => RunState.RestMenu,
                Command.Exit => RunState.EscapeMenu(0),
                _ => this
            };

            world.SetState(ctx, nextState);
        }

        private Scene WaitExtended(World world)
        {
            for (int i = 0; i < 20; i++)
            {
                world.repeatActionQueue.Enqueue(Command.Wait);
            }
            return Wait(world);
        }

        private Scene Operate(World world)
        {
            if (world.spatial.TryGetFeature(playerData.coords, out var feature))
            {
                if (world.ecs.TryGet<Portal>(feature, out var portal))
                {
                    playerData.turns++;
                    return RunState.LevelTransition(portal.targetLevel);
                }
            }
            return this;
        }

        private Scene MoveRepeat(World world, Direction direction)
        {
            world.repeatActionQueue.Enqueue(direction switch
            {
                Direction.SouthWest => Command.MoveSouthWestRepeat,
                Direction.South => Command.MoveSouthRepeat,
                Direction.SouthEast => Command.MoveSouthEastRepeat,
                Direction.West => Command.MoveWestRepeat,
                Direction.East => Command.MoveEastRepeat,
                Direction.NorthWest => Command.MoveNorthWestRepeat,
                Direction.North => Command.MoveNorthRepeat,
                Direction.NorthEast => Command.MoveNorthEastRepeat,
                _ => throw new Exception("Inapplicable direction")
            });
            return Move(world, direction);
        }

        private Scene Move(World world, Direction direction)
        {
            world.ecs.Assign(playerData.entity, new IntentionMove { transform = Vec2.FromDirection(direction) });
            playerData.turns++;
            return RunState.Ticking;
        }

        private Scene Wait(World world)
        {
            ref var actor = ref world.ecs.GetRef<Actor>(playerData.entity);
            world.ecs.Assign<EventActed>(playerData.entity, new EventActed { energyLoss = 1000, nutritionLoss = 1 });
            return RunState.Ticking;
        }

        private Scene GetItem(World world)
        {
            if (world.spatial.ItemsCountAt(playerData.coords) == 1)
            {
                world.ecs.Assign<IntentionGetItem>(playerData.entity);
                return RunState.Ticking;
            }
            else if (world.spatial.ItemsCountAt(playerData.coords) > 1)
            {
                return RunState.PickUp(0, 0);
            }
            return this;
        }
    }
}