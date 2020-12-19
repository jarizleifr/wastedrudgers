using WasteDrudgers.Entities;

namespace WasteDrudgers.State
{
    internal class TickingState : IRunState
    {
        private PlayerData playerData;

        public void Run(IContext ctx, World world)
        {
            playerData ??= world.PlayerData;
            playerData.lastTarget = null;
            world.log.Description = null;

            while (true)
            {
                // Denote that 100 energy worth of time has gone by
                world.IncrementGameTicks();

                // Tick based systems
                Systems.DurationSystem(ctx, world);
                Systems.SpawnSystem(ctx, world);
                Systems.RegenSystem(ctx, world);

                // Handle actions
                Systems.ItemSystem(ctx, world);
                Systems.MovementSystem(ctx, world);
                Systems.AttackSystem(ctx, world);

                // Clear intentions
                world.ecs.Clear<IntentionGetItem>();
                world.ecs.Clear<IntentionUseItem>();
                world.ecs.Clear<IntentionMove>();
                world.ecs.Clear<IntentionAttack>();

                Systems.PostTurnSystem(ctx, world);

                // Handle results
                Systems.HungerClockSystem(ctx, world);
                Systems.StatusSystem(ctx, world);
                Systems.TriggerSystem(ctx, world);
                Systems.CreatureUpdateSystem(ctx, world);

                // Handle late results
                Systems.DamageSystem(ctx, world);
                world.ecs.Clear<Damage>(); // These shouldn't linger after processing

                // Clear all events
                world.ecs.Clear<EventMoved>();
                world.ecs.Clear<EventActed>();
                world.ecs.Clear<EventStatsUpdated>();
                world.ecs.Clear<EventEffectsUpdated>();
                world.ecs.Clear<EventInventoryUpdated>();
                world.ecs.Clear<Turn>();

                if (world.ecs.Has<Death>(playerData.entity))
                {
                    world.SetState(ctx, RunState.GameOver);
                    return;
                }

                // Clear dead entities
                Systems.DeathSystem(ctx, world);
                world.ecs.Clear<Death>();

                Systems.InitiativeSystem(ctx, world);
                Systems.AISystem(ctx, world);

                // Check for repeat interrupts
                Systems.RepeatActionSystem(ctx, world);

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
                world.WriteToLog("character_points_earned", playerData.coords, LogArgs.Num(15));
            }

            world.log.UpdateMessageBuffer();
            world.SetState(ctx, world.log.HasMessages() ? RunState.MoreMessages : RunState.AwaitingInput);
        }
    }
}