using HarmonyLib;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace Ailinea.PrefixPatches
{
    [HarmonyPatch]
    internal class PrefixPatch
    {
        private static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
        };

        /// <summary>
        /// <br>Use the <see cref="AilineaMod.Targets"/>.</br>
        /// <br>For all types in the <see cref="Inputs.Targets.TypesToInclude"/> and in the
        ///     <see cref="Inputs.Targets.NamespacesToInspect"/> except <see cref="Inputs.Targets.TypesToExclude"/> get all methods to patch.</br>
        /// <para>
        ///     Methods filtered: <see cref="MethodBase.IsAbstract"/>, <see cref="MethodBase.IsSpecialName"/>, <see cref="MethodBase.IsGenericMethod"/>
        ///     <br>Types filtered: <see cref="Type.IsValueType"/>, <see cref="Type.IsAbstract"/>, <see cref="Type.IsGenericType"/> (see <see cref="FilterTypesToInspect"/>)</br>
        /// </para>
        /// <br>Add the methods in <see cref="Inputs.Targets.MethodsToInclude"/></br>
        /// <br>Remove the methods in <see cref="Inputs.Targets.MethodsToExclude"/></br>
        /// </summary>
        /// <returns>The target methods</returns>
        public static IEnumerable<MethodBase> TargetMethods()
        {
            List<Type> typesToInspect = GetTypesToInspect().ToList();
            var targetMethodsFromTypesToInspect = typesToInspect.SelectMany(type => type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                                                                                        .Where(method => !method.IsAbstract
                                                                                                      && !method.IsSpecialName
                                                                                                      && !method.IsGenericMethod
                                                                                                      && method.HasMethodBody()
                                                                                                      && !AilineaMod.Targets.MethodsToExclude.Contains((type, method.Name)))
                                                                                        .Cast<MethodBase>());
            var targetConstructors = typesToInspect.SelectMany(type => type.GetConstructors());
            var methodsToInclude = GetMethodsToInclude();
            return targetMethodsFromTypesToInspect.Concat(targetConstructors).Concat(methodsToInclude);
        }

        private static IEnumerable<Type> GetTypesToInspect()
        {
            IEnumerable<Type> nonFilteredTypes = AccessTools.GetTypesFromAssembly(Assembly.LoadFrom(AilineaMod.Targets.PathAssemblyToInspect))
                                                            .Where(type => AilineaMod.Targets.NamespacesToInspect.Contains(type.Namespace) && !AilineaMod.Targets.TypesToExclude.Contains(type)
                                                                        || AilineaMod.Targets.TypesToInclude.Contains(type));
            return FilterTypesToInspect(nonFilteredTypes);
        }

        private static IEnumerable<Type> FilterTypesToInspect(IEnumerable<Type> types)
        {
            return types.Where(type => !type.IsValueType
                                    && !type.IsInterface
                                    && !type.IsGenericType
                                    && !type.GetTypeInfo().IsDefined(typeof(CompilerGeneratedAttribute), true));
        }

        private static IEnumerable<MethodBase> GetMethodsToInclude()
        {
            List<MethodBase> methodsToInclude = new List<MethodBase>();
            foreach (var tuple in AilineaMod.Targets.MethodsToInclude)
            {
                methodsToInclude.Add(AccessTools.Method(tuple.Item1, tuple.Item2));
            }
            return methodsToInclude;
        }


        /// <summary>
        /// Prefix patch for the targets methods. Save a <see cref="MethodPatch"/> in the log file on
        /// each invocation.
        /// </summary>
        /// <param name="__originalMethod"></param>
        public static void Prefix(MethodBase __originalMethod)
        {
            AilineaMod.NumberMethodCalls++;

            var trace = new StackTrace();
            AddObjectToLogFile(new MethodPatch
            {
                Fqn = __originalMethod.FullDescription(),
                Timestamp = DateTime.Now,
                StackTrace = trace.GetFrames().Select(frame => frame.GetMethod().FullDescription()).ToList(),
                DepthStackTrace = trace.FrameCount
            });
        }

        private static void AddObjectToLogFile(object objectToSerialize)
        {
            FileLog.Log(JsonConvert.SerializeObject(objectToSerialize, jsonSettings) + ",");
        }
    }
}
