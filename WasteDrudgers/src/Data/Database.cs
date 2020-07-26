using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiceNotation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Blaggard.Common;
using Blaggard.Graphics;
using WasteDrudgers.Entities;
using WasteDrudgers.Level;

namespace WasteDrudgers.Data
{
    public class Database
    {
        private string[] tileIndexMapping;
        private Dictionary<string, Color> dbColors;
        private Dictionary<string, DBMaterial> dbMaterials;
        private Dictionary<string, DBMaterialGroup> dbMaterialGroups;
        private Dictionary<string, DBItem> dbItems;
        private Dictionary<string, DBLootList> dbLootLists;
        private Dictionary<string, Tile> dbTiles;
        private Dictionary<string, DBLevel> dbLevels;
        private Dictionary<string, DBFeature> dbFeatures;
        private Dictionary<string, DBLogMessage> dbLogMessages;
        private Dictionary<string, DBSpell> dbSpells;
        private Dictionary<string, DBNaturalAttack> dbNaturalAttacks;
        private Dictionary<string, DBRace> dbRaces;
        private Dictionary<string, DBCreature> dbCreatures;
        private Dictionary<string, DBCreatureList> dbCreatureLists;
        private Dictionary<ItemType, List<string>> dbObfuscatedNames;

        private Func<JToken, string> GetId = (JToken token) => token["id"].ToString();

        public Database()
        {
            var root = GetRootNode(JToken.Parse(File.ReadAllText("./data.cdb")));

            dbColors = GetNodeFromRoot("colors", root).ToDictionary(GetId, c => c["color"].ToObject<Color>());

            dbMaterials = GetNodeFromRoot("materials", root).ToDictionary(GetId, m =>
            {
                var material = m.ToObject<DBMaterial>();
                material.Color = dbColors[m["color"].ToString()];
                return material;
            });

            dbMaterialGroups = GetNodeFromRoot("materialGroups", root).ToDictionary(GetId, m =>
            {
                var materialGroup = new DBMaterialGroup();
                materialGroup.Materials = m["materials"].Select(m => (dbMaterials[m["material"].ToString()], (int)m["probability"])).ToList();
                return materialGroup;
            });

            dbLogMessages = GetNodeFromRoot("messages", root).ToDictionary(GetId, l => l.ToObject<DBLogMessage>());

            dbSpells = GetNodeFromRoot("spells", root).ToDictionary(GetId, s =>
            {
                var spell = s.ToObject<DBSpell>();


                var diceString = s["magnitude"].ToString();
                if (diceString != "")
                {
                    var dice = Dice.Parse(diceString);
                    spell.MinMagnitude = dice.MinRoll().Value;
                    spell.MaxMagnitude = dice.MaxRoll().Value;
                }

                spell.Message = dbLogMessages[s["message"].ToString()];

                return spell;
            });

            dbItems = GetNodeFromRoot("items", root).ToDictionary(GetId, i =>
            {
                var dbItem = i.ToObject<DBItem>();
                dbItem.BaseMaterial = dbMaterials[i["baseMaterial"].ToString()];

                if (i["damage"] != null)
                {
                    var dice = Dice.Parse(i["damage"].ToString());
                    dbItem.MinDamage = dice.MinRoll().Value;
                    dbItem.MaxDamage = dice.MaxRoll().Value;
                }

                if (i["useSpell"] != null)
                {
                    dbItem.UseSpell = dbSpells[i["useSpell"].ToString()];
                }

                if (i["materialGroup"] != null)
                {
                    dbItem.MaterialGroup = dbMaterialGroups[i["materialGroup"].ToString()];
                }

                if (dbItem.Glyph > 0)
                {
                    dbItem.Glyph += (char)256;
                }
                return dbItem;
            });

            dbLootLists = GetNodeFromRoot("lootLists", root).ToDictionary(GetId, l =>
            {
                var dbLootList = l.ToObject<DBLootList>();
                dbLootList.Items = l["items"].Select(li => dbItems[li["item"].ToString()]).ToArray();
                return dbLootList;
            });

            var dbProfessions = GetNodeFromRoot("professions", root).ToDictionary(GetId, p =>
            {
                return p["skills"].Select(s => s["skill"].ToObject<SkillType>()).ToList();
            });

            dbRaces = GetNodeFromRoot("races", root).ToDictionary(GetId, r =>
            {
                var race = r.ToObject<DBRace>();
                race.Color = dbColors[r["color"].ToString()];
                return race;
            });

            dbNaturalAttacks = GetNodeFromRoot("naturalAttacks", root).ToDictionary(GetId, n =>
            {
                var naturalAttack = n.ToObject<DBNaturalAttack>();
                if (n["castOnStrike"] != null)
                {
                    naturalAttack.CastOnStrike = dbSpells[n["castOnStrike"].ToString()];
                }
                return naturalAttack;
            });

            dbCreatures = GetNodeFromRoot("creatures", root).ToDictionary(GetId, cr =>
            {
                var creature = cr.ToObject<DBCreature>();
                creature.Race = dbRaces[cr["race"].ToString()];
                creature.Professions = cr["professions"].Select(p => (dbProfessions[p["profession"].ToString()], (int)p["level"])).ToList();

                if (cr["naturalAttack"] != null)
                {
                    creature.NaturalAttack = dbNaturalAttacks[cr["naturalAttack"].ToString()];
                }

                return creature;
            });

            dbCreatureLists = GetNodeFromRoot("creatureLists", root).ToDictionary(GetId, cl =>
            {
                var creatureList = cl.ToObject<DBCreatureList>();
                creatureList.Creatures = cl["creatures"].Select(li => dbCreatures[li["creature"].ToString()]).ToArray();
                return creatureList;
            });

            tileIndexMapping = new string[128];
            dbTiles = GetNodeFromRoot("tiles", root).ToDictionary(GetId, t =>
            {
                var tile = t.ToObject<Tile>();
                if (tile.glyph > 0)
                {
                    tile.glyph += (char)256;
                }
                tile.foreground = dbColors[t["foreground"].ToString()];
                tile.background = dbColors[t["background"].ToString()];

                tileIndexMapping[t["index"].ToObject<int>()] = t["id"].ToString();
                return tile;
            });

            dbFeatures = GetNodeFromRoot("features", root).ToDictionary(GetId, l =>
            {
                var feature = l.ToObject<DBFeature>();
                feature.Color = dbColors[l["color"].ToString()];
                return feature;
            });

            dbLevels = GetNodeFromRoot("levels", root).ToDictionary(GetId, l =>
            {
                var level = l.ToObject<DBLevel>(new JsonSerializer { TypeNameHandling = TypeNameHandling.All });
                level.Creatures = l["creatures"].Select(c => dbCreatureLists[c["lists"].ToString()]).ToList();
                level.Loot = l["loot"].Select(lt => dbLootLists[lt["lists"].ToString()]).ToList();
                level.Portals = l["portals"].Select(p => new DBPortal
                {
                    TargetLevelId = p["targetLevel"].ToString(),
                    Feature = dbFeatures[p["feature"].ToString()]
                }).ToList();

                return level;
            });

            dbObfuscatedNames = GetNodeFromRoot("obfuscatedNames", root).ToDictionary(
                o => o["type"].ToObject<ItemType>(),
                o => o["names"].Select(n => n["name"].ToString()).ToList());

            CreateTilesetGraphic();
        }

        public DBMapData GetMapData(string mapId)
        {
            var root = GetRootNode(JToken.Parse(File.ReadAllText("./data.cdb")));
            var mapToken = GetNodeFromRoot("maps", root).Where(t => t["id"].ToString() == mapId).Single();

            var data = mapToken.ToObject<DBMapData>();
            var bytes = mapToken["layers"][0]["data"]["data"].ToObject<byte[]>();

            var size = bytes.Count() / sizeof(short);
            var shorts = new short[size];
            for (var index = 0; index < size; index++)
            {
                shorts[index] = BitConverter.ToInt16(bytes, index * sizeof(short));
            }

            data.Tiles = shorts.Select(i => (byte)(i - 1)).ToArray();

            return data;
        }

        public void CreateTilesetGraphic()
        {
            var tempDisplay = new Display(16, 16, 1, false);
            IBlittable layer = new Canvas(tempDisplay, 16, 16);

            int count = dbTiles.Values.Count;

            tempDisplay.SetRenderTarget(layer.Texture);

            int i = 0;
            foreach (var tile in dbTiles.Values)
            {
                layer.PutChar(i % 16, i / 16, tile.characters[0], tile.foreground, tile.background);
                i++;
            }

            layer.Render();

            tempDisplay.SaveTextureToFile(layer.Texture);

            layer.Dispose();
            tempDisplay.Dispose();
        }

        public List<string> GetObfuscatedNames(ItemType type) => dbObfuscatedNames[type];

        public Color GetColor(string id) => dbColors[id];

        public DBSpell GetSpell(string id) => dbSpells[id];

        public DBLevel GetLevel(string id) => dbLevels[id];

        public DBFeature GetFeature(string id) => dbFeatures[id];

        public Tile GetTile(string id) => dbTiles[id];
        public Tile GetTileByIndex(int index) => dbTiles[tileIndexMapping[index]];

        public DBMaterial GetMaterial(string id) => dbMaterials[id];

        public DBItem GetItem(string id) => dbItems[id];

        public List<DBItem> GetItemsFromLootLists(string[] ids) => ids
            .SelectMany(id => dbLootLists[id].Items)
            .Distinct()
            .ToList();

        public DBLogMessage GetLogMessage(string id) => dbLogMessages[id];

        public DBCreature GetCreature(string id) => dbCreatures[id];

        public DBCreatureList GetCreatureList(string id) => dbCreatureLists[id];

        private static JToken GetRootNode(JToken token) => token["sheets"];

        private static JToken GetNodeFromRoot(string nodeId, JToken root) => root.Where(t => t["name"].ToString() == nodeId).Single()["lines"];
    }
}