namespace WasteDrudgers.State
{
    internal class SaveGameState : Scene
    {
        public string saveName;
        public Scene nextState;

        public override void Update(IContext ctx, World world)
        {
            SerializationUtils.Save(world);
            world.Clear();
            world.SetState(ctx, nextState);
        }
    }
}