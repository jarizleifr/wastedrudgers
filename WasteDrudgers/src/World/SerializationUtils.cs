using System;
using System.IO;
using Blaggard.FileIO;

namespace WasteDrudgers
{
    public static class SerializationUtils
    {
        public static void Save(World world)
        {
            var playerData = world.PlayerData;
            var global = world.ecs.Serialize("global");
            var level = world.ecs.Serialize();

            ZipUtils.SaveJsonToZip(GetSavePath(playerData.name),
                new JsonSave("global.json", global),
                new JsonSave(playerData.currentLevel + ".json", level)
            );
        }

        public static string GetSaveFolderPath()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return Path.Combine(path, "My Games", "WasteDrudgers", "Save");
        }

        public static string GetSavePath(string playerName)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return Path.Combine(path, "My Games", "WasteDrudgers", "Save", playerName + ".sav");
        }

        public static void DeleteSave(string playerName)
        {
            File.Delete(GetSavePath(playerName));
        }
    }
}