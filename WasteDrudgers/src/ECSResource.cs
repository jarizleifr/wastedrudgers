namespace WasteDrudgers
{
    /// <summary>
    /// An utility wrapper for serializable resources that should live 
    /// in the ECS, but be cached for ease-of-access and performance
    /// </summary>
    public struct ECSResource<T>
    {
        private T cachedField;

        public T Get(ManulECS.World ecs) => cachedField == null
            ? cachedField = ecs.GetResource<T>()
            : cachedField;

        public void Set(ManulECS.World ecs, T value) =>
            ecs.SetResource<T>(cachedField = value);

        public void Clear(ManulECS.World ecs)
        {
            cachedField = default(T);
            ecs.ClearResource<T>();
        }
    }
}