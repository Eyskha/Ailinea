using System;

namespace Ailinea.Inputs
{
    public class TestTargets
    {
        public string PathTestAssembly;
        public Type? TestType;
        public string? TestNameMethod;

        public bool UseCustomTestingAPI => TestType != null && TestNameMethod != null;
    }
}
