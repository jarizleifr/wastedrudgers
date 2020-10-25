namespace WasteDrudgers.State
{
    internal class SaveGameState : IRunState
    {
        public string saveName;
        public IRunState nextState;

        public void Run(IContext ctx, World world)
        {
            SerializationUtils.Save(world);
            world.Clear();
            world.SetState(ctx, nextState);
        }
    }
}