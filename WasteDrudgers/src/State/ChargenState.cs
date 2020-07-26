using System;
using System.Collections.Generic;
using Blaggard.Graphics;
using WasteDrudgers.Entities;
using WasteDrudgers.Render;

namespace WasteDrudgers.State
{
    public class ChargenState : IRunState
    {
        public int selection;
        public string[] InputDomains { get; set; } = { "menu" };

        private int points;
        private (string, string)[] items;
        private Skills data;
        private Stats defaultStats;

        public void Initialize(IContext ctx, World world)
        {
            points = 100;
            data = new Skills { set = new List<Skill>() };
            defaultStats = new Stats
            {
                strength = 10,
                endurance = 10,
                finesse = 10,
                intellect = 10,
                resolve = 10,
                awareness = 10,
            };
        }

        public void Run(IContext ctx, World world)
        {
            switch (ctx.Command)
            {
                case Command.MenuAccept:
                    world.SetState(ctx, RunState.NewGame(data));
                    break;

                case Command.MenuLeft:
                    if (data.GetRank((SkillType)selection) != 0)
                    {
                        points++;
                        data.Decrement((SkillType)selection);
                    }
                    break;

                case Command.MenuRight:
                    if (points > 0 && data.GetRank((SkillType)selection) < 60)
                    {
                        points--;
                        data.Increment((SkillType)selection);
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

            root.Print(root.Width / 2, 3, "Assign Skills", ctx.Theme.selectedColor, TextAlignment.Center);
            root.Print(root.Width / 2, 5, $"Points remaining {points}", ctx.Theme.selectedColor, TextAlignment.Center);
            CreateItems();
            Menu.DrawValueMenu(root, ctx.Theme, root.Width / 2, root.Height / 2 - items.Length + 1, selection, items);
        }

        private void CreateItems()
        {
            items = new (string, string)[Enum.GetValues(typeof(SkillType)).Length];
            int i = 0;
            foreach (SkillType s in Enum.GetValues(typeof(SkillType)))
            {
                items[i] = (s.ToString(), $"{Formulae.BaseSkill(s, defaultStats) + data.GetRank(s)}%");
                i++;
            }
        }
    }
}