using System.Collections.Generic;
using System.IO;
using System.Linq;
using Blaggard.Common;
using WasteDrudgers.Entities;

namespace WasteDrudgers.State
{
    internal class NewGameState : Scene
    {
        public override void Update(IContext ctx, World world)
        {
            world.ecs.Clear();

            var name = GenerateName();

            var player = world.ecs.Create();
            world.ecs.Assign(player, new Position { coords = new Vec2(0, 0) });
            world.ecs.Assign(player, new Renderable { character = '@', color = Data.Colors.white });
            world.ecs.Assign(player, new Actor { energy = 0, speed = 100 });
            world.ecs.Assign<Player>(player);
            world.ecs.Assign(player, new Identity { name = name, rawName = "player" });
            world.ecs.Assign(player, new Experience { level = 1, experience = 0, characterPoints = 100 });
            world.ecs.Assign(player, new Skills { set = new List<Skill>() });

            world.ecs.Assign(player, new Stats
            {
                strength = 10,
                endurance = 10,
                finesse = 10,
                intellect = 10,
                resolve = 10,
                awareness = 10
            });

            world.ecs.Assign(player, new Actor { });
            world.ecs.Assign(player, new Pools { });
            world.ecs.Assign(player, new Attack { });
            world.ecs.Assign(player, new Defense { });

            var startingLevel = "dungeon_01";
            world.PlayerData = new PlayerData { entity = player, name = name, coords = Vec2.Zero, currentLevel = startingLevel, lastTarget = null };
            world.ObfuscatedNames = new ObfuscatedNames();
            world.Calendar = new Calendar(135, 1, 1, 12, 0, 0);

#if DEBUG
            // TODO: Once we implement cheat console, and scripts for it, 
            // we can get rid of this

            // testing code, doesn't run on Release,
            var startingItems = new List<string>
            {
                "food_insect",
                "food_insect",
                "food_insect",
                "food_insect",
                "scroll_shield",
                "potion_poison",
            };
            foreach (var i in startingItems)
            {
                Items.PickUpItem(world, player, Items.Create(world, i, Vec2.Zero));
            }
            Effects.AddTalent(world, "tough_skin", player);
#endif

            world.ecs.Assign(player, new HungerClock { nutrition = 1000 });

            Creatures.UpdateCreature(world, player);
            world.SetState(ctx, RunState.Chargen);
        }

        public static string GenerateName()
        {
            var path = SerializationUtils.GetSaveFolderPath();
            var dir = new System.IO.DirectoryInfo(path);

            var existingNames = dir.GetFiles().Select(f => Path.GetFileNameWithoutExtension(f.Name)).ToHashSet();

            var start = new string[]
            {
                "Ar", "Baga", "Bal", "Bat", "Bog", "Dar", "Gan", "Ger", "Gur", "Er", "Il", "Mong", "Nar", "Ong", "Ot", "Sar", "Tom", "Qar", "Zin"
            };
            var mid = new string[]
            {
                "an", "baj", "gon", "jag", "ke", "lu", "nag", "or", "ral", "ran", "ri", "teu", "ug", "zor"
            };
            var end = new string[]
            {
                "ban", "bek", "bhatar", "dur", "gai", "gis", "ghu", "gui", "hul", "ig", "jar", "khul", "lik", "shik", "taar", "tur", "za", "zi"
            };

            string name = "";
            while (name == "" || existingNames.Contains(name))
            {
                name = RNG.Int(2) switch
                {
                    0 => start[RNG.Int(start.Length)] + mid[RNG.Int(mid.Length)] + end[RNG.Int(end.Length)],
                    1 => start[RNG.Int(start.Length)] + mid[RNG.Int(mid.Length)],
                    2 => start[RNG.Int(start.Length)] + end[RNG.Int(end.Length)]
                };
            }

            return name;
        }
    }
}