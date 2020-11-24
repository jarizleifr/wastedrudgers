using System.Collections.Generic;
using ManulECS;

namespace WasteDrudgers
{
    [SerializationProfile("never")]
    public class TurnQueue
    {
        private List<Entity> ready = new List<Entity>();

        public void Add(Entity entity) => ready.Add(entity);
        public bool Empty => ready.Count == 0;
        public List<Entity> Entities => ready;
        public void Clear() => ready.Clear();
    }
}