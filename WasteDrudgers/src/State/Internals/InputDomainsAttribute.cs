using System;

namespace WasteDrudgers.State
{
    public class InputDomainsAttribute : Attribute
    {
        private string[] domains;

        public InputDomainsAttribute(params string[] domains) =>
            this.domains = domains;

        public string[] GetDomains() => domains;
    }
}