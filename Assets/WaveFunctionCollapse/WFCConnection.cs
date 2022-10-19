using System;

namespace WFCGenerator
{
    [Serializable]
    public class WFCConnection
    {
        public string name;

        public bool ConnectsTo(WFCConnection other)
        {
            return name == other.name;
        }
    }
}