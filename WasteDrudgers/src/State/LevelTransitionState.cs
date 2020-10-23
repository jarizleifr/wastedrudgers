using Blaggard.FileIO;
using WasteDrudgers.Level;

namespace WasteDrudgers.State
{
    internal class LevelTransitionState : IRunState
    {
        public string levelName;
        public void Run(IContext ctx, World world)
        {
            var playerData = world.PlayerData;

            // Autosave before transitions
            SerializationUtils.Save(world);
            world.ecs.Clear();

            var savePath = SerializationUtils.GetSavePath(playerData.name);

            var global = ZipUtils.LoadJsonFromZip(savePath, "global.json");
            world.ecs.Deserialize(global);

            IRunState newState;
            if (ZipUtils.TryLoadJsonFromZip(savePath, levelName + ".json", out var level))
            {
                world.ecs.Deserialize(level);
                newState = RunState.LevelGeneration(levelName, world.Map);
            }
            else
            {
                newState = RunState.LevelGeneration(levelName, null);
            }
            world.SetState(ctx, newState);
        }
    }
}