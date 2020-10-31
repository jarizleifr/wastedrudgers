using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Blaggard.Common;
using Blaggard.Graphics;
using DiceNotation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        private Dictionary<string, DBCreature> dbCreatures;
        private Dictionary<string, DBCreatureList> dbCreatureLists;
        private Dictionary<ItemType, List<string>> dbObfuscatedNames;

        private Func<JToken, string> GetId = (JToken token) => token["id"].ToString();

        public Database()
        {
            var root = GetRootNode(JToken.Parse(File.ReadAllText("./data.cdb")));

            dbColors = GetNodeFromRoot("COL", root).ToDictionary(GetId, c => c["color"].ToObject<Color>());

            dbMaterials = GetNodeFromRoot("MAT", root).ToDictionary(GetId, m =>
            {
                var material = m.ToObject<DBMaterial>();
                material.Color = dbColors[m["color"].ToString()];
                return material;
            });

            dbMaterialGroups = GetNodeFromRoot("MGR", root).ToDictionary(GetId, m =>
            {
                var materialGroup = new DBMaterialGroup();
                materialGroup.Materials = m["materials"].Select(m => (dbMaterials[m["material"].ToString()], (int)m["probability"])).ToList();
                return materialGroup;
            });

            dbLogMessages = GetNodeFromRoot("MSG", root).ToDictionary(GetId, l => l.ToObject<DBLogMessage>());

            dbSpells = GetNodeFromRoot("SPL", root).ToDictionary(GetId, s =>
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

            dbLootLists = new Dictionary<string, DBLootList>();
            var allItems = GetNodeFromRoot("ITM", root).ToDictionary(GetId, i => i)
                .Concat(GetNodeFromRoot("WPN", root).ToDictionary(GetId, i => i))
                .Concat(GetNodeFromRoot("APR", root).ToDictionary(GetId, i => i));

            dbItems = allItems.ToDictionary(kv => kv.Key, kv =>
            {
                var i = kv.Value;
                var dbItem = i.ToObject<DBItem>();

                if (dbItem.Type.IsApparel())
                {
                    dbItem = i.ToObject<DBApparel>();
                }
                else if (dbItem.Type.IsWeapon())
                {
                    dbItem = i.ToObject<DBWeapon>();
                    if (i["damage"] != null)
                    {
                        var wpn = (DBWeapon)dbItem;
                        var dice = Dice.Parse(i["damage"].ToString());
                        wpn.MinDamage = dice.MinRoll().Value;
                        wpn.MaxDamage = dice.MaxRoll().Value;
                    }
                }

                dbItem.BaseMaterial = dbMaterials[i["baseMaterial"].ToString()];

                if (i["useSpell"] != null)
                {
                    dbItem.UseSpell = dbSpells[i["useSpell"].ToString()];
                }

                if (i["materialGroup"] != null)
                {
                    dbItem.MaterialGroup = dbMaterialGroups[i["materialGroup"].ToString()];
                }

                if (i["tags"] != null)
                {
                    foreach (var tag in i["tags"].ToString().Split(','))
                    {
                        if (dbLootLists.TryGetValue(tag, out var lootList))
                        {
                            lootList.Items.Add(dbItem);
                        }
                        else
                        {
                            dbLootLists.Add(tag, new DBLootList { Items = new List<DBItem> { dbItem } });
                        }
                    }
                }
                return dbItem;
            });

            dbNaturalAttacks = GetNodeFromRoot("NAT", root).ToDictionary(GetId, n =>
            {
                var naturalAttack = n.ToObject<DBNaturalAttack>();
                if (n["castOnStrike"] != null)
                {
                    naturalAttack.CastOnStrike = dbSpells[n["castOnStrike"].ToString()];
                }
                return naturalAttack;
            });

            var dbProfessions = GetNodeFromRoot("PRF", root).ToDictionary(GetId, p =>
            {
                return p["focus"].Select(s => s["type"].ToObject<ProfessionFocus>()).ToList();
            });

            dbCreatureLists = new Dictionary<string, DBCreatureList>();
            var creatureRoot = GetNodeFromRoot("CRE", root);
            dbCreatures = creatureRoot.ToDictionary(GetId, cr =>
            {
                var creature = MakeCreature(cr, dbProfessions);
                if (cr["base"] != null)
                {
                    var baseCr = creatureRoot
                        .Where(b => b["id"].ToString() == cr["base"].ToString())
                        .Single();
                    var baseCreature = MakeCreature(baseCr, dbProfessions);

                    if (creature.Character == default(char)) creature.Character = baseCreature.Character;
                    if (creature.Color == default(Color)) creature.Color = baseCreature.Color;
                    if (creature.Strength == 0) creature.Strength = baseCreature.Strength;
                    if (creature.Endurance == 0) creature.Endurance = baseCreature.Endurance;
                    if (creature.Finesse == 0) creature.Finesse = baseCreature.Finesse;
                    if (creature.Intellect == 0) creature.Intellect = baseCreature.Intellect;
                    if (creature.Resolve == 0) creature.Resolve = baseCreature.Resolve;
                    if (creature.Awareness == 0) creature.Awareness = baseCreature.Awareness;

                    if (creature.Professions.Count == 0) creature.Professions = baseCreature.Professions;

                    creature.NaturalAttack ??= baseCreature.NaturalAttack;

                    if (cr["tags"] == null)
                    {
                        foreach (var tag in baseCr["tags"].ToString().Split(','))
                        {
                            if (dbCreatureLists.TryGetValue(tag, out var creatureList))
                            {
                                creatureList.Creatures.Add(creature);
                            }
                            else
                            {
                                dbCreatureLists.Add(tag, new DBCreatureList { Id = tag, Creatures = new List<DBCreature> { creature } });
                            }
                        }
                    }
                }

                if (cr["tags"] != null)
                {
                    foreach (var tag in cr["tags"].ToString().Split(','))
                    {
                        if (dbCreatureLists.TryGetValue(tag, out var creatureList))
                        {
                            creatureList.Creatures.Add(creature);
                        }
                        else
                        {
                            dbCreatureLists.Add(tag, new DBCreatureList { Id = tag, Creatures = new List<DBCreature> { creature } });
                        }
                    }
                }

                return creature;
            });

            tileIndexMapping = new string[128];
            dbTiles = GetNodeFromRoot("TIL", root).ToDictionary(GetId, t =>
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

            dbFeatures = GetNodeFromRoot("FEA", root).ToDictionary(GetId, l =>
            {
                var feature = l.ToObject<DBFeature>();
                feature.Color = dbColors[l["color"].ToString()];
                return feature;
            });

            dbLevels = GetNodeFromRoot("LVL", root).ToDictionary(GetId, l =>
            {
                var level = l.ToObject<DBLevel>(new JsonSerializer { TypeNameHandling = TypeNameHandling.All });

                if (l["tags"] != null)
                {
                    level.Creatures = new List<DBCreatureList>();
                    level.Loot = new List<DBLootList>();
                    foreach (var tag in l["tags"].ToString().Split(','))
                    {
                        if (dbCreatureLists.TryGetValue(tag, out var creatureList))
                        {
                            level.Creatures.Add(creatureList);
                        }
                        if (dbLootLists.TryGetValue(tag, out var lootList))
                        {
                            level.Loot.Add(lootList);
                        }
                    }
                }

                level.Portals = new List<DBPortal>();
                if (l["portals"] != null)
                {
                    level.Portals = l["portals"].Select(p => new DBPortal
                    {
                        TargetLevelId = p["targetLevel"].ToString(),
                        Feature = dbFeatures[p["feature"].ToString()]
                    }).ToList();
                }
                return level;
            });

            dbObfuscatedNames = GetNodeFromRoot("OBF", root).ToDictionary(
                o => o["type"].ToObject<ItemType>(),
                o => o["names"].Select(n => n["name"].ToString()).ToList());

#if DEBUG
            CreateTilesetGraphic();
#endif
        }

        public DBMapData GetMapData(string mapId)
        {
            var root = GetRootNode(JToken.Parse(File.ReadAllText("./data.cdb")));
            var mapToken = GetNodeFromRoot("MAP", root).Where(t => t["id"].ToString() == mapId).Single();

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

        private DBCreature MakeCreature(JToken cr, Dictionary<string, List<ProfessionFocus>> dbProfessions)
        {
            var creature = cr.ToObject<DBCreature>();

            if (cr["professions"] != null)
            {
                creature.Professions = cr["professions"]
                    .Select(p => (dbProfessions[p["profession"].ToString()], (int)p["points"]))
                    .ToList();
            }
            else
            {
                creature.Professions = new List<(List<ProfessionFocus>, int)>();
            }

            if (cr["naturalAttack"] != null)
            {
                creature.NaturalAttack = dbNaturalAttacks[cr["naturalAttack"].ToString()];
            }

            if (cr["color"] != null)
            {
                creature.Color = dbColors[cr["color"].ToString()];
            }

            return creature;
        }
    }
}