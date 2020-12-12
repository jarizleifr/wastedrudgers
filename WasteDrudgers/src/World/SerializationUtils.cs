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
            var localAppData = Environment.GetFolderPath
            (
                Environment.SpecialFolder.LocalApplicationData,
                Environment.SpecialFolderOption.Create
            );
            var path = Path.Combine
            (
                localAppData,
                "WasteDrudgers",
                "Save"
            );
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        public static string GetSavePath(string playerName) =>
            Path.Combine(GetSaveFolderPath(), playerName + ".sav");

        public static void DeleteSave(string playerName) =>
            File.Delete(GetSavePath(playerName));
    }
}