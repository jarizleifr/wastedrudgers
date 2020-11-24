#pragma warning disable CS0649

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using WasteDrudgers.Entities;
using WasteDrudgers.Level;

namespace WasteDrudgers
{
    public static class Data
    {
        private class Database
        {
            [JsonProperty("colors")]
            internal readonly Colors colors;
            [JsonProperty("creatures")]
            internal Dictionary<string, DBCreature> creatures;
            [JsonProperty("features")]
            internal Dictionary<string, DBFeature> features;
            [JsonProperty("items")]
            internal Dictionary<string, DBItem> items;
            [JsonProperty("spells")]
            internal Dictionary<string, DBSpell> spells;
            [JsonProperty("naturalAttacks")]
            internal Dictionary<string, DBNaturalAttack> naturalAttacks;
            [JsonProperty("tiles")]
            internal Dictionary<string, Tile> tiles;
            [JsonProperty("levels")]
            internal Dictionary<string, DBLevel> levels;
            [JsonProperty("materials")]
            internal Dictionary<string, DBMaterial> materials;
            [JsonProperty("professions")]
            internal Dictionary<string, List<ProfessionFocus>> professions;
            [JsonProperty("lootLists")]
            internal Dictionary<string, List<string>> lootLists;
            [JsonProperty("spawnLists")]
            internal Dictionary<string, List<string>> spawnLists;
            [JsonProperty("materialLists")]
            internal Dictionary<string, List<string>> materialLists;
            [JsonProperty("messages")]
            internal Dictionary<string, DBLogMessage> messages;
            [JsonProperty("obfuscated")]
            internal Dictionary<ItemType, List<string>> obfuscatedNames;
        }

        private static Database d;
        static Data() => d = JsonConvert.DeserializeObject<Database>(
            File.ReadAllText("./assets/gamedata.json"),
            new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.All
            }
        );

        public static Colors Colors => d.colors;
        public static DBCreature GetCreature(string id) => d.creatures[id];
        public static DBFeature GetFeature(string id) => d.features[id];
        public static DBItem GetItem(string id) => d.items[id];
        public static DBSpell GetSpell(string id) => d.spells[id];
        public static DBNaturalAttack GetNaturalAttack(string id) =>
            d.naturalAttacks[id];
        public static Tile GetTile(string id) => d.tiles[id];
        public static DBLevel GetLevel(string id) => d.levels[id];
        public static DBMaterial GetMaterial(string id) => d.materials[id];
        public static DBLogMessage GetMessage(string id) => d.messages[id];

        public static List<ProfessionFocus> GetProfession(string id)
            => d.professions[id];

        public static Tile[] IndexedTiles => d.tiles.Values.OrderBy(t => t.index).ToArray();

        public static List<string> GetObfuscatedNames(ItemType type) =>
            d.obfuscatedNames[type];

        public static List<DBMaterial> GetItemMaterials(DBItem item) =>
            item.MaterialTags
                .Aggregate(new List<string>() { item.Material }, (acc, cur) => acc
                    .Concat(d.materialLists[cur])
                    .ToList())
                .Distinct()
                .Select(id => d.materials[id])
                .ToList();

        public static List<DBCreature> GetLevelSpawns(DBLevel level) =>
            level.LevelTags
                .Aggregate(new List<string>(), (acc, cur) => acc
                    .Concat(d.spawnLists[cur])
                    .ToList())
                .Distinct()
                .Select(id => d.creatures[id])
                .ToList();

        public static List<DBItem> GetLevelLoot(DBLevel level) =>
            level.LevelTags
                .Aggregate(new List<string>(), (acc, cur) =>
                {
                    if (d.lootLists.TryGetValue(cur, out var list))
                    {
                        return acc.Concat(list).ToList();
                    }
                    return acc;
                })
                .Distinct()
                .Select(id => d.items[id])
                .ToList();

        public static DBMapData LoadMap(string id) =>
            JsonConvert.DeserializeObject<DBMapData>(File.ReadAllText($"./assets/maps/{id}.map"));
    }
}