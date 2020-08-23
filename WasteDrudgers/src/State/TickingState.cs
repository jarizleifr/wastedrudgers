using System;
using WasteDrudgers.Entities;

namespace WasteDrudgers.State
{
    internal class TickingState : IRunState
    {
        private PlayerData playerData;

        public void Run(IContext ctx, World world)
        {
            playerData ??= world.ecs.FetchResource<PlayerData>();
            playerData.lastTarget = null;
            world.log.Description = null;

            while (true)
            {
                // Denote that 100 energy worth of time has gone by
                world.IncrementGameTicks();

                Systems.ItemSystem(ctx, world);

                Systems.RegenSystem(ctx, world);

                Systems.BumpSystem(ctx, world);
                Systems.AttackSystem(ctx, world);
                Systems.StatusSystem(ctx, world);

                Systems.DamageSystem(ctx, world);

                // FIXME: Game locks down sometimes on death? Couldn't reproduce anymore?

                // If player has died, return early
                if (world.ecs.Has<Death>(playerData.entity))
                {
                    world.SetState(ctx, RunState.GameOver);
                    return;
                }

                Systems.MovementSystem(ctx, world);
                Systems.TriggerSystem(ctx, world);

                Systems.DeathSystem(ctx, world);
                Systems.InitiativeSystem(ctx, world);
                Systems.AISystem(ctx, world);

                if (world.ecs.Has<Turn>(playerData.entity))
                {
                    Systems.FOVSystem(ctx, world);
                    break;
                }
            }

            ref var exp = ref world.ecs.GetRef<Experience>(playerData.entity);
            if (Formulae.ExperienceNeededForLevel(exp.level + 1) - exp.experience <= 0)
            {
                exp.level++;
                exp.characterPoints += 15;
                world.WriteToLog("character_points_earned", playerData.coords, LogItem.Num(15));
            }

            world.log.UpdateMessageBuffer();
            world.SetState(ctx, world.log.HasMessages() ? RunState.MoreMessages : RunState.AwaitingInput);
        }
    }
}