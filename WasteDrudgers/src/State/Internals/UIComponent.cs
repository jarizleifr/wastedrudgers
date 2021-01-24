using System;

namespace WasteDrudgers.State
{
    public abstract class UIComponentBase : Parent
    {
        public void Render()
        {
            Draw();
            if (children != null)
            {
                foreach (var child in children)
                {
                    child.Render();
                }
            }
        }

        public abstract void Draw();
    }

    public abstract class UIComponent : UIComponentBase
    {
        public virtual void Refresh()
        {
            throw new Exception("Trying to Refresh component without a Refresh() override!");
        }
    }

    public abstract class UIComponent<T> : UIComponentBase
    {
        public abstract void Refresh(T props);
    }
}