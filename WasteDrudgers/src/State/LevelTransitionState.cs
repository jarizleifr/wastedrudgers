using Blaggard.FileIO;

namespace WasteDrudgers.State
{
    internal class LevelTransitionState : Scene
    {
        public string levelName;
        public override void Update(IContext ctx, World world)
        {
            var playerData = world.PlayerData;

            // Autosave before transitions
            SerializationUtils.Save(world);
            world.Clear();

            var savePath = SerializationUtils.GetSavePath(playerData.name);

            var global = ZipUtils.LoadJsonFromZip(savePath, "global.json");
            world.ecs.Deserialize(global);

            Scene newState;
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