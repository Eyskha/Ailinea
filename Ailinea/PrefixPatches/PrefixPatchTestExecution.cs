using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Ailinea.PrefixPatches
{
    [HarmonyPatch]
    internal class PrefixPatchTestExecution
    {
        private static List<string> NUnitTestAttributes = new List<string>
        {
            "NUnit.Framework.TestAttribute",
            "NUnit.Framework.TestCaseAttribute",
            "NUnit.Framework.TestCaseSourceAttribute",
        };
        private static int NbTestsExecuted = 0;

        /// <summary>
        /// Get the methods to patch:
        /// <list type="bullet">
        ///     <item>The test method in <see cref="AilineaMod.TestTargets"/> if specified</item>
        ///     <item>By default, methods with one of the <see cref="NUnitTestAttributes"/> in the
        ///     test assembly whose path is specified in <see cref="AilineaMod.TestTargets"/></item>
        /// </list>
        /// </summary>
        /// <returns>The list of methods to patch</returns>
        public static IEnumerable<MethodBase> TargetMethods()
        {
            if (!AilineaMod.TestTargets.UseCustomTestingAPI)
            {
                return AccessTools.GetTypesFromAssembly(Assembly.LoadFrom(AilineaMod.TestTargets.PathTestAssembly))
                                                                .SelectMany(type => type.GetMethods())
                                                                .Where(method => method.GetCustomAttributes(true)
                                                                                       .Where(attribute => NUnitTestAttributes.Contains(attribute.ToString()))
                                                                                       .Count() > 0);
            }
            else if (AilineaMod.TestTargets.TestType != null && AilineaMod.TestTargets.TestNameMethod != null)
            {
                return new MethodBase[] { AilineaMod.TestTargets.TestType.GetMethod(AilineaMod.TestTargets.TestNameMethod) };
            }
            return new MethodBase[] { };
        }

        /// <summary>
        /// Prefix patch for the test methods. Add the data about the test in the test log file.
        /// </summary>
        public static void Prefix(MethodBase __originalMethod, object[] __args)
        {
            NbTestsExecuted++;
            if (AilineaMod.ProjectInfo.ExecutionEnvironment.Equals("Test"))
            {
                if (AilineaMod.TestTargets.UseCustomTestingAPI)
                {
                    AddObjectToTestLogFile($"{NbTestsExecuted}-{string.Join("/", __args.Select(o => o?.ToString() ?? "null"))}");
                }
                else
                {
                    AddObjectToTestLogFile($"{NbTestsExecuted}-{__originalMethod.DeclaringType}.{__originalMethod.Name}({string.Join(", ", __args.Select(o => o?.ToString() ?? "null"))})");
                }
            }
        }

        /// <summary>
        /// Save the name of the test and the number of method calls in the test log file.
        /// </summary>
        private static void AddObjectToTestLogFile(string nameTest)
        {
            using (StreamWriter sw = File.AppendText(AilineaMod.PathTestLogFile))
            {
                sw.WriteLine($"\"{nameTest}\": {AilineaMod.NumberMethodCalls},");
            }
        }
    }
}
