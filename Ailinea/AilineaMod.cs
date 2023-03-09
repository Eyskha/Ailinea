using HarmonyLib;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;
using System;
using Ailinea.Inputs;

namespace Ailinea
{
    public static class AilineaMod
    {
        internal static int NumberMethodCalls;
        internal static ProjectInformation ProjectInfo;
        internal static Targets Targets;
        internal static TestTargets TestTargets;
        internal static string PathTestLogFile;

        /// <summary>
        /// Entry point; used to pass inputs to the generic mod and patch the target methods
        /// </summary>
        /// <param name="targets">Namespaces, tests, or methods to target</param>
        public static void CreateMod(ProjectInformation projectInfo,
                                     Targets targets,
                                     TestTargets testTargets)
        {
            NumberMethodCalls = 0;
            ProjectInfo = projectInfo;
            Targets = targets;
            TestTargets = testTargets;
            Patch();
        }

        /// <summary>
        /// Patch the target methods using Harmony to create detours.
        /// Initialise the log files.
        /// </summary>
        private static void Patch()
        {
            string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            string time = DateTime.Now.ToString("yyyy-MM-dd_HH-mm");

            InitialiseLogFile(folderPath, time);
            InitialiseTestLogFile(folderPath, time);

            // Patch
            var harmony = new Harmony("com.test.harmony");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        /// <summary>
        /// Initialise the log file that will contain the execution trace (list of <see cref="MethodPatch"/>).
        /// </summary>
        private static void InitialiseLogFile(string folder, string time)
        {
            FileLog.Reset();

            // Log file execution data
            string fileName = $"{ProjectInfo.ProjectName}_{ProjectInfo.ExecutionEnvironment}_{time}.json";
            Environment.SetEnvironmentVariable("HARMONY_LOG_FILE", Path.Combine(folder, fileName));

            // Execution info
            FileLog.Log($"{{\n" +
                            $"\"Assembly\": \"{Targets.PathAssemblyToInspect.Replace("\\", "/")}\",\n" +
                            $"\"Namespaces\": {JsonConvert.SerializeObject(Targets.NamespacesToInspect)},\n" +
                            $"\"ExecutionEnvironment\": \"{ProjectInfo.ExecutionEnvironment}\",\n" +
                            $"\"MethodsPatches\": [");
        }

        /// <summary>
        /// Initialise the test log file that will contain the names of the tests and for each, the number of 
        /// methods called before the tests is executed.
        /// </summary>
        private static void InitialiseTestLogFile(string folder, string time)
        {
            if (ProjectInfo.ExecutionEnvironment.Equals("Test"))
            {
                string fileNameTestLog = $"{ProjectInfo.ProjectName}_TestsCorrespondence_{time}.json";
                PathTestLogFile = Path.Combine(folder, fileNameTestLog);
                using (StreamWriter sw = File.CreateText(PathTestLogFile))
                {
                    sw.WriteLine("{");
                }
            }
        }
    }

}
