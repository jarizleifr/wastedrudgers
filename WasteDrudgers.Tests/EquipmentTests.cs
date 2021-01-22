using System.Collections.Generic;
using Blaggard.Common;
using ManulECS;
using WasteDrudgers.Entities;
using Xunit;

namespace WasteDrudgers.Tests
{
    public class EquipmentTests
    {
        private World world;
        private IEngineContext ctx;

        public EquipmentTests() => (world, ctx) = TestUtils.CreateGame();

        [Fact]
        public void ParryUsesShieldRank_WhenShieldEquipped()
        {
            var player = world.PlayerData.entity;
            var item1 = Items.CreateItem(world, "round_shield", "iron", new Vec2(0, 0));

            var testSkills = new Skills { set = new List<Skill>() };
            testSkills.Add(SkillType.Shield, 70);
            world.ecs.AssignOrReplace(player, testSkills);

            Items.EquipItem(world, player, item1);
            Creatures.UpdateCreature(world, player);

            var attack = world.ecs.GetRef<Attack>(player);

            Assert.Equal(100, attack.parry);
        }
    }
}
