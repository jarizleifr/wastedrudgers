using System;
using System.Collections.Generic;
using System.Linq;
using Blaggard.Graphics;
using WasteDrudgers.Entities;
using WasteDrudgers.Render;

namespace WasteDrudgers.State
{
    public class LevelUpState : IRunState
    {
        private enum LevelUpMode
        {
            Finesse,
            Knowledge,
        }

        public int selection;
        public string[] InputDomains { get; set; } = { "menu" };

        private LevelUpMode mode;
        private int finessePoints;
        private int knowledgePoints;
        private (string, string)[] items;
        private Stats stats;
        private Skills skills;
        private Skills added;
        private PlayerData playerData;

        public void Initialize(IContext ctx, World world)
        {
            playerData = world.ecs.FetchResource<PlayerData>();
            stats = world.ecs.GetRef<Stats>(playerData.entity);
            skills = world.ecs.GetRef<Skills>(playerData.entity);

            finessePoints = Formulae.FinessePointsPerLevel(stats.finesse.Current);
            knowledgePoints = Formulae.KnowledgePointsPerLevel(stats.intellect.Current);

            added = new Skills { set = new List<Skill>() };
        }

        public void Run(IContext ctx, World world)
        {
            switch (ctx.Command)
            {
                case Command.MenuAccept:
                    if (mode == LevelUpMode.Finesse)
                    {
                        mode = LevelUpMode.Knowledge;
                        selection = 0;
                        break;
                    }

                    ref var exp = ref world.ecs.GetRef<Experience>(playerData.entity);
                    exp.level++;

                    foreach (var s in added.set)
                    {
                        skills.Add(s.type, s.value);
                    }
                    world.ecs.AssignOrReplace(playerData.entity, skills);
                    Creatures.UpdateCreature(world, playerData.entity);

                    world.SetState(ctx, RunState.AwaitingInput);
                    return;

                case Command.MenuLeft:
                    if (added.GetRank(Selected) != 0)
                    {
                        Points++;
                        added.Decrement(Selected);
                    }
                    break;

                case Command.MenuRight:
                    if (Points > 0)
                    {
                        Points--;
                        added.Increment(Selected);
                    }
                    break;

                case Command.MenuUp:
                    selection = Menu.Prev(selection, items.Length);
                    break;
                case Command.MenuDown:
                    selection = Menu.Next(selection, items.Length);
                    break;

                case Command.Exit:
                    world.SetState(ctx, RunState.MainMenu(0));
                    break;
            }

            var root = ctx.QueueCanvas(RenderLayer.Root);
            root.SetRenderPosition(0, 0);

            root.DefaultFore = ctx.Theme.windowFrame;
            root.DefaultBack = ctx.Theme.windowBackground;
            root.Clear();

            root.Print(root.Width / 2, 3, $"Assign {(mode == LevelUpMode.Finesse ? "Finesse" : "Knowledge")} skills", ctx.Theme.selectedColor, TextAlignment.Center);
            root.Print(root.Width / 2, 5, $"Points remaining {Points}", ctx.Theme.selectedColor, TextAlignment.Center);
            CreateItems(mode);
            Menu.DrawValueMenu(root, ctx.Theme, root.Width / 2, root.Height / 2 - items.Length + 1, selection, items);
        }

        public int Points
        {
            get => (mode == LevelUpMode.Finesse) ? finessePoints : knowledgePoints;
            set
            {
                if (mode == LevelUpMode.Finesse)
                {
                    finessePoints = value;
                }
                else
                {
                    knowledgePoints = value;
                }
            }
        }

        public SkillType Selected => (mode == LevelUpMode.Finesse) ? (SkillType)selection : (SkillType)selection + 9;

        private void CreateItems(LevelUpMode mode)
        {
            IEnumerable<SkillType> skillTypes;
            if (mode == LevelUpMode.Finesse)
            {
                skillTypes = Enum.GetValues(typeof(SkillType)).Cast<SkillType>().Where(s => s.IsFinesseSkill());
            }
            else
            {
                skillTypes = Enum.GetValues(typeof(SkillType)).Cast<SkillType>().Where(s => s.IsKnowledgeSkill());
            }
            items = skillTypes.Select(s => (s.ToString(), $"{Formulae.BaseSkill(s, stats) + skills.GetRank(s) + added.GetRank(s)}%")).ToArray();
        }
    }
}