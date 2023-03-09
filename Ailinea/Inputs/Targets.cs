using System;
using System.Collections.Generic;

namespace Ailinea.Inputs
{
    /// <summary>
    /// Namespace, types, and/or methods to target.
    /// </summary>
    public class Targets
    {
        public string PathAssemblyToInspect;
        public string[] NamespacesToInspect;

        public Type[] TypesToExclude = new Type[] { };
        public Type[] TypesToInclude = new Type[] { };
        /// <summary>
        /// Format: (type, name of the method)
        /// </summary>
        public List<(Type, string)> MethodsToExclude = new List<(Type, string)>();
        /// <summary>
        /// Format: (type, name of the method)
        /// </summary>
        public List<(Type, string)> MethodsToInclude = new List<(Type, string)>();
    }
}
