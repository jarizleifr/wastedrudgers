using WasteDrudgers.Entities;
using WasteDrudgers.Render;

namespace WasteDrudgers.UI
{
    public enum CharacterMode
    {
        Stats,
        Skills,
        Talents,
        Spells,
    };

    public class CharacterScreen : Menu
    {
        private Menu statMenu;
        private ScrollMenu skillMenu;

        public Menu Current => Mode switch
        {
            CharacterMode.Stats => statMenu,
            CharacterMode.Skills => skillMenu,
        };

        public CharacterMode Mode => (CharacterMode)Selected;

        public int CurrentCost => Mode switch
        {
            CharacterMode.Stats => Formulae.GetStatCost((StatType)statMenu.Selected),
            CharacterMode.Skills => 2,
        };

        public int CharacterPoints { get; set; }

        public CharacterScreen() : base(2)
        {
            statMenu = new Menu(9);
            skillMenu = new ScrollMenu(CharacterUI.STAT_SELECTION_LENGTH, 10);
        }

        public void Buy(World world, PlayerData playerData)
        {
            ref var points = ref world.ecs.GetRef<Experience>(playerData.entity);
            if (points.characterPoints < CurrentCost) return;

            points.characterPoints -= CurrentCost;
            CharacterPoints = points.characterPoints;

            switch (Mode)
            {
                case CharacterMode.Stats:
                    ref var stats = ref world.ecs.GetRef<Stats>(playerData.entity);
                    ref var health = ref world.ecs.GetRef<Health>(playerData.entity);

                    if (Current.Selected < 6)
                    {
                        stats[(StatType)Current.Selected]++;
                    }
                    else
                    {
                        switch (Current.Selected)
                        {
                            case 6: health.health.Mod++; break;
                            case 7: health.vigor.Mod++; break;
                            case 8: break;
                        }
                    }
                    break;

                case CharacterMode.Skills:
                    ref var skills = ref world.ecs.GetRef<Skills>(playerData.entity);
                    skills.Increment((SkillType)Current.Index);
                    break;
            }
            Creatures.UpdateCreature(world, playerData.entity);
        }
    }
}