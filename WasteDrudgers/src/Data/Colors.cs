using System;
using Blaggard.Common;
using Newtonsoft.Json;

namespace WasteDrudgers
{
    [Serializable]
    public class Colors
    {
        [JsonProperty("black")]
        public readonly Color black;
        [JsonProperty("shadow")]
        public readonly Color shadow;
        [JsonProperty("shadow_light")]
        public readonly Color shadowLight;
        [JsonProperty("violet_dark")]
        public readonly Color violetDark;
        [JsonProperty("violet")]
        public readonly Color violet;
        [JsonProperty("violet_light")]
        public readonly Color violetLight;
        [JsonProperty("brown_dark")]
        public readonly Color brownDark;
        [JsonProperty("brown")]
        public readonly Color brown;
        [JsonProperty("brown_light")]
        public readonly Color brownLight;
        [JsonProperty("skin_dark")]
        public readonly Color skinDark;
        [JsonProperty("skin")]
        public readonly Color skin;
        [JsonProperty("skin_light")]
        public readonly Color skinLight;
        [JsonProperty("beige_dark")]
        public readonly Color beigeDark;
        [JsonProperty("beige")]
        public readonly Color beige;
        [JsonProperty("beige_light")]
        public readonly Color beigeLight;
        [JsonProperty("green_dark")]
        public readonly Color greenDark;
        [JsonProperty("green")]
        public readonly Color green;
        [JsonProperty("green_light")]
        public readonly Color greenLight;
        [JsonProperty("teal_dark")]
        public readonly Color tealDark;
        [JsonProperty("teal")]
        public readonly Color teal;
        [JsonProperty("teal_light")]
        public readonly Color tealLight;
        [JsonProperty("blue_dark")]
        public readonly Color blueDark;
        [JsonProperty("blue")]
        public readonly Color blue;
        [JsonProperty("blue_light")]
        public readonly Color blueLight;
        [JsonProperty("fuchsia_dark")]
        public readonly Color fuchsiaDark;
        [JsonProperty("fuchsia")]
        public readonly Color fuchsia;
        [JsonProperty("fuchsia_light")]
        public readonly Color fuchsiaLight;
        [JsonProperty("red_dark")]
        public readonly Color redDark;
        [JsonProperty("red")]
        public readonly Color red;
        [JsonProperty("red_light")]
        public readonly Color redLight;
        [JsonProperty("bronze_dark")]
        public readonly Color bronzeDark;
        [JsonProperty("bronze")]
        public readonly Color bronze;
        [JsonProperty("bronze_light")]
        public readonly Color bronzeLight;
        [JsonProperty("grey_dark")]
        public readonly Color greyDark;
        [JsonProperty("grey")]
        public readonly Color grey;
        [JsonProperty("grey_light")]
        public readonly Color greyLight;
        [JsonProperty("white")]
        public readonly Color white;
    }
}