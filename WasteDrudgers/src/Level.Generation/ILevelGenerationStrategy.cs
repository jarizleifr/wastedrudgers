namespace WasteDrudgers.Level.Generation
{
    public interface ILevelGenerationStrategy
    {
        void Generate(World world, string levelName, ref Map map);
    }
}