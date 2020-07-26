using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using ManulECS;

namespace WasteDrudgers.Entities
{
    [SerializationProfile("global")]
    public class ObfuscatedNames
    {
        [JsonProperty]
        private Dictionary<ItemType, Dictionary<string, string>> obfuscated = new Dictionary<ItemType, Dictionary<string, string>>();

        [JsonProperty]
        private HashSet<string> known = new HashSet<string>();

        public void SetKnown(string itemName) => known.Add(itemName);

        public bool IsKnown(string itemName) => known.Contains(itemName);

        public string GetObfuscatedName(World world, ItemType type, string itemName)
        {
            if (obfuscated.TryGetValue(type, out var names))
            {
                if (names.TryGetValue(itemName, out var name))
                {
                    return GetName(type, name);
                }
                else
                {
                    var obfuscatedName = Add(world, type, itemName, names);
                    return GetName(type, obfuscatedName);
                }
            }
            else
            {
                var dict = new Dictionary<string, string>();
                var obfuscatedName = Add(world, type, itemName, dict);
                obfuscated.Add(type, dict);
                return GetName(type, obfuscatedName);
            }
        }

        private static string Add(World world, ItemType type, string itemName, Dictionary<string, string> dict)
        {
            var available = world.database.GetObfuscatedNames(type).Except(dict.Values).ToList();
            var i = RNG.Int(0, available.Count);

            var obfuscated = available[i];
            dict.Add(itemName, obfuscated);
            return obfuscated;
        }

        private string GetName(ItemType type, string obfuscatedName) => type switch
        {
            ItemType.Potion => obfuscatedName + " potion",
            ItemType.Scroll => $"scroll labeled '{obfuscatedName}'",
            _ => obfuscatedName
        };
    }
}