# Ailinea

Ailinea is a technique to generate tailored mods that instrument targets within the game, monitor their execution at runtime, and collect corresponding data.

The Ailinea mod is generic and inputs have to be passed to it to use it with a C# program. An example of how to load Ailinea is presented below. Make sure to have this piece of code loaded at the start-up of the program. 

```c#
AilineaMod.CreateMod(
    new ProjectInformation()
    {
        ProjectName = "Osu",
        ExecutionEnvironment = "Field"
    },
    new Targets()
    {
        PathAssemblyToInspect = Path.Combine(path, "osu.Game.dll"),
        NamespacesToInspect = new string[] {
            "osu.Game.Rulesets.Mods",
        };,
    },
    new TestTargets()
    {
        PathTestAssembly = Path.Combine(pathTest, "osu.Game.Tests.dll")
    }
);
```