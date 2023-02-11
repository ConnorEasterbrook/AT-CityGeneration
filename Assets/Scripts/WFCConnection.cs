using System.Collections.Generic;
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

        public bool ConnectsTo(int moduleNumber, List<WFCModule> modules)
        {
            return name == modules[moduleNumber].name;
        }
    }
}