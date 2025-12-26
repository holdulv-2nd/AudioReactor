using Flax.Build;
using Flax.Build.NativeCpp;
using System.IO;

public class AudioReactor : GameModule
{
    public override void Setup(BuildOptions options)
    {
        base.Setup(options);

        // This allows the code to run in both Game and Editor
        BuildNativeCode = false;

        // 🔗 NAudio Reference
        // This assumes NAudio.dll is sitting right next to this file!
        string naudioPath = Path.Combine(FolderPath, "NAudio.dll");

        // 1. Compile Time: Let the code "see" NAudio
        options.ScriptingAPI.FileReferences.Add(naudioPath);

        // 2. Runtime: FORCE copy the DLL to the bin folder so the game can run
        options.DependencyFiles.Add(naudioPath);

        // 🔗 IMPORTANT: Allow access to Editor features (for the plugin logic)
        if (options.Target.IsEditor)
        {
            options.PublicDependencies.Add("FlaxEditor");
        }
    }
}