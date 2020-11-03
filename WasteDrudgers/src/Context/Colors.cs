using Blaggard.Common;

namespace WasteDrudgers
{
    public class Colors
    {
        public readonly Color black;

        public readonly Color shadow;
        public readonly Color shadowLight;

        public readonly Color violetDark;
        public readonly Color violet;
        public readonly Color violetLight;

        public readonly Color brownDark;
        public readonly Color brown;
        public readonly Color brownLight;

        public readonly Color skinDark;
        public readonly Color skin;
        public readonly Color skinLight;

        public readonly Color beigeDark;
        public readonly Color beige;
        public readonly Color beigeLight;

        public readonly Color greenDark;
        public readonly Color green;
        public readonly Color greenLight;

        public readonly Color tealDark;
        public readonly Color teal;
        public readonly Color tealLight;

        public readonly Color blueDark;
        public readonly Color blue;
        public readonly Color blueLight;

        public readonly Color fuchsiaDark;
        public readonly Color fuchsia;
        public readonly Color fuchsiaLight;

        public readonly Color redDark;
        public readonly Color red;
        public readonly Color redLight;

        public readonly Color bronzeDark;
        public readonly Color bronze;
        public readonly Color bronzeLight;

        public readonly Color greyDark;
        public readonly Color grey;
        public readonly Color greyLight;

        public readonly Color white;

        public Colors(World world)
        {
            var db = world.database;

            black = db.GetColor("c_black");

            shadow = db.GetColor("c_shadow");
            shadowLight = db.GetColor("c_shadow_light");

            violetDark = db.GetColor("c_violet_dark");
            violet = db.GetColor("c_violet");
            violetLight = db.GetColor("c_violet_light");

            brownDark = db.GetColor("c_brown_dark");
            brown = db.GetColor("c_brown");
            brownLight = db.GetColor("c_brown_light");

            skinDark = db.GetColor("c_skin_dark");
            skin = db.GetColor("c_skin");
            skinLight = db.GetColor("c_skin_light");

            beigeDark = db.GetColor("c_beige_dark");
            beige = db.GetColor("c_beige");
            beigeLight = db.GetColor("c_beige_light");

            greenDark = db.GetColor("c_green_dark");
            green = db.GetColor("c_green");
            greenLight = db.GetColor("c_green_light");

            tealDark = db.GetColor("c_teal_dark");
            teal = db.GetColor("c_teal");
            tealLight = db.GetColor("c_teal_light");

            blueDark = db.GetColor("c_blue_dark");
            blue = db.GetColor("c_blue");
            blueLight = db.GetColor("c_blue_light");

            fuchsiaDark = db.GetColor("c_fuchsia_dark");
            fuchsia = db.GetColor("c_fuchsia");
            fuchsiaLight = db.GetColor("c_fuchsia_light");

            redDark = db.GetColor("c_red_dark");
            red = db.GetColor("c_red");
            redLight = db.GetColor("c_red_light");

            bronzeDark = db.GetColor("c_bronze_dark");
            bronze = db.GetColor("c_bronze");
            bronzeLight = db.GetColor("c_bronze_light");

            greyDark = db.GetColor("c_grey_dark");
            grey = db.GetColor("c_grey");
            greyLight = db.GetColor("c_grey_light");

            white = db.GetColor("c_white");
        }
    }
}