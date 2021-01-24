using System.Collections.Generic;

namespace WasteDrudgers.State
{
    public abstract class Parent
    {
        protected List<UIComponent> children;

        public void AddChild(UIComponent child)
        {
            if (children == null)
            {
                children = new List<UIComponent>();
            }
            children.Add(child);
        }

        public void RemoveChild(UIComponent child)
        {
            if (children != null)
            {
                children.Remove(child);
            }
        }
    }
}