using System.Collections.Generic;
using System;

namespace Ailinea
{
    /// <summary>
    /// Data collected for each execution of a target method
    /// </summary>
    internal class MethodPatch
    {
        public string Fqn;
        public DateTime Timestamp;
        public List<string> StackTrace;
        public int DepthStackTrace;
    }
}
