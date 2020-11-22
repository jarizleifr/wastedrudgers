using Blaggard.Common;
using WasteDrudgers.Entities;
using WasteDrudgers.State;
using Xunit;

namespace WasteDrudgers.Tests
{
    public class HungerClockSystemTests
    {
        private World world;
        private IEngineContext ctx;

        public HungerClockSystemTests() => (world, ctx) = TestUtils.CreateGame();

        [Fact]
        public void AutoConsumesFood_WhenZeroNutrition()
        {
            RNG.Seed(0);
            var player = world.PlayerData.entity;

            var foodItem = Items.Create(world, "food_insect", new Vec2(0, 2));
            Items.PickUpItem(world, player, foodItem);
            world.ecs.Clear<EventInventoryUpdated>();

            world.ecs.AssignOrReplace(player, new HungerClock { food = 800 });

            world.SetState(ctx, RunState.Ticking);
            world.Tick(ctx);

            var clock = world.ecs.GetRef<HungerClock>(player);
            Assert.Equal(HungerState.Sated, clock.State);
            Assert.Equal(800, clock.nutrition);

            var stats = world.ecs.GetRef<Stats>(player);
            Assert.Equal(0, stats.strength.Mod);
        }

        [Fact]
        public void AutoConsumesNewFood_WhenHungry()
        {
            RNG.Seed(0);
            var playerData = world.PlayerData;
            var player = playerData.entity;
            world.ecs.AssignOrReplace(player, new HungerClock { });

            world.SetState(ctx, RunState.Ticking);
            world.Tick(ctx);

            var foodItem = Items.Create(world, "food_insect", playerData.coords);
            world.ecs.Assign(player, new IntentionGetItem { });

            world.SetState(ctx, RunState.Ticking);
            world.Tick(ctx);

            var clock = world.ecs.GetRef<HungerClock>(player);
            Assert.Equal(HungerState.Sated, clock.State);
            Assert.Equal(800, clock.nutrition);

            var stats = world.ecs.GetRef<Stats>(player);
            Assert.Equal(0, stats.strength.Mod);
        }

        [Fact]
        public void BecomesHungryAndFatigued_WhenReachesZeroNutritionWithoutFood()
        {
            RNG.Seed(0);
            var player = world.PlayerData.entity;
            world.ecs.AssignOrReplace(player, new HungerClock { nutrition = 1 });
            world.ecs.Assign(player, new EventActed { nutritionLoss = 1 });

            world.SetState(ctx, RunState.Ticking);
            world.Tick(ctx);

            var clock = world.ecs.GetRef<HungerClock>(player);
            Assert.Equal(HungerState.Hungry, clock.State);

            var health = world.ecs.GetRef<Health>(player);
            Assert.True(health.fatigued);

            var stats = world.ecs.GetRef<Stats>(player);
            Assert.Equal(-2, stats.strength.Mod);
        }
    }
}