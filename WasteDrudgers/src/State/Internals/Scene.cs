using System;
namespace WasteDrudgers.State
{
    public abstract class Scene : Parent
    {
        private string[] inputDomains;
        public string[] InputDomains
        {
            get
            {
                if (inputDomains != null)
                {
                    return inputDomains;
                }
                var attribute = (InputDomainsAttribute)Attribute.GetCustomAttribute(GetType(), typeof(InputDomainsAttribute));
                return inputDomains = attribute != null
                    ? attribute.GetDomains()
                    : Array.Empty<string>();
            }
        }

        public virtual void Run(IContext ctx, World world)
        {
            Update(ctx, world);
            if (children != null)
            {
                foreach (var child in children)
                {
                    child.Render();
                }
            }
        }

        public virtual void Initialize(IContext ctx, World world) { }

        public abstract void Update(IContext ctx, World world);
    }
}
