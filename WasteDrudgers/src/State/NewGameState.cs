using System.IO;
using System.Linq;
using Blaggard.Common;
using WasteDrudgers.Entities;

namespace WasteDrudgers.State
{
    internal class NewGameState : IRunState
    {
        public Skills skills;

        public void Run(IContext ctx, World world)
        {
            world.ecs.Clear();

            var name = GenerateName();

            var player = world.ecs.Create();
            world.ecs.Assign(player, new Position { coords = new Vec2(0, 0) });
            world.ecs.Assign(player, new Renderable { character = '@', glyph = (char)256, color = world.database.GetColor("c_white") });
            world.ecs.Assign(player, new Actor { energy = 0, speed = 100 });
            world.ecs.Assign(player, new Player { });
            world.ecs.Assign(player, new Identity { name = name, rawName = "player" });
            world.ecs.Assign(player, skills);

            var stats = new Stats { strength = 10, endurance = 10, finesse = 10, intellect = 10, resolve = 10, awareness = 10 };
            world.ecs.Assign(player, stats);
            world.ecs.Assign(player, new Health
            {
                vigor = Formulae.BaseVigor(stats),
                health = Formulae.BaseHealth(stats)
            });

            var startingLevel = "lvl_starting_cave_01";
            world.ecs.SetResource(new PlayerData { entity = player, name = name, coords = new Vec2(0, 0), currentLevel = startingLevel, lastTarget = null });
            world.ecs.SetResource(new ObfuscatedNames());

            Creatures.UpdateCreature(world, player);

            world.SetState(ctx, RunState.LevelGeneration(startingLevel, null, true));
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