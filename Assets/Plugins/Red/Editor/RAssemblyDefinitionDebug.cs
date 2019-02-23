#if UNITY_EDITOR
namespace Red.Editor {
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using UnityEditor;
    using UnityEditor.Compilation;
    using UnityEngine;

    /// <summary>
    ///     https://gist.github.com/karljj1/9c6cce803096b5cd4511cf0819ff517b
    /// </summary>
    [InitializeOnLoad]
    public class RAssemblyDefinitionDebug {
        private const string ASSEMBLY_RELOAD_EVENTS_EDITOR_PREF          = "AssemblyReloadEventsTime";
        private const string ASSEMBLY_COMPILATION_EVENTS_EDITOR_PREF     = "AssemblyCompilationEvents";
        private const string ASSEMBLY_TOTAL_COMPILATION_TIME_EDITOR_PREF = "AssemblyTotalCompilationTime";

        private static readonly int ScriptAssembliesPathLen = "Library/ScriptAssemblies/".Length;

        private static readonly Dictionary<string, DateTime> StartTimes = new Dictionary<string, DateTime>();

        private static readonly StringBuilder BuildEvents = new StringBuilder();

        private static double compilationTotalTime;

        static RAssemblyDefinitionDebug() {
            CompilationPipeline.assemblyCompilationStarted  += CompilationPipelineOnAssemblyCompilationStarted;
            CompilationPipeline.assemblyCompilationFinished += CompilationPipelineOnAssemblyCompilationFinished;
            AssemblyReloadEvents.beforeAssemblyReload       += AssemblyReloadEventsOnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload        += AssemblyReloadEventsOnAfterAssemblyReload;
        }

        private static void CompilationPipelineOnAssemblyCompilationStarted(string assembly) =>
            StartTimes[assembly] = DateTime.UtcNow;

        private static void CompilationPipelineOnAssemblyCompilationFinished(string assembly, CompilerMessage[] arg2) {
            var timeSpan = DateTime.UtcNow - StartTimes[assembly];
            compilationTotalTime += timeSpan.TotalMilliseconds;
            BuildEvents.AppendFormat("{0} {1:0.00}s\n", assembly.Substring(ScriptAssembliesPathLen, assembly.Length - ScriptAssembliesPathLen),
                timeSpan.TotalMilliseconds / 1000f);
        }

        private static void AssemblyReloadEventsOnBeforeAssemblyReload() {
            var totalCompilationTimeSeconds = compilationTotalTime / 1000f;
            BuildEvents.AppendFormat("Compilation total: {0:0.00}s\n", totalCompilationTimeSeconds);
            EditorPrefs.SetString(ASSEMBLY_RELOAD_EVENTS_EDITOR_PREF, DateTime.UtcNow.ToBinary().ToString());
            EditorPrefs.SetString(ASSEMBLY_COMPILATION_EVENTS_EDITOR_PREF, BuildEvents.ToString());
            EditorPrefs.SetString(ASSEMBLY_TOTAL_COMPILATION_TIME_EDITOR_PREF, totalCompilationTimeSeconds.ToString(CultureInfo.InvariantCulture));
        }

        private static void AssemblyReloadEventsOnAfterAssemblyReload() {
            var binString                   = EditorPrefs.GetString(ASSEMBLY_RELOAD_EVENTS_EDITOR_PREF);
            var totalCompilationTimeString  = EditorPrefs.GetString(ASSEMBLY_TOTAL_COMPILATION_TIME_EDITOR_PREF);
            var totalCompilationTimeSeconds = float.Parse(totalCompilationTimeString, NumberStyles.Any, CultureInfo.InvariantCulture);


            long bin;
            if (long.TryParse(binString, out bin)) {
                var date             = DateTime.FromBinary(bin);
                var time             = DateTime.UtcNow - date;
                var compilationTimes = EditorPrefs.GetString(ASSEMBLY_COMPILATION_EVENTS_EDITOR_PREF);
                var totalTimeSeconds = totalCompilationTimeSeconds + time.TotalSeconds;
                if (!string.IsNullOrEmpty(compilationTimes)) {
                    Debug.Log($"Compilation Report: {totalTimeSeconds:F2} seconds\n" + compilationTimes + "Assembly Reload Time: " + time.TotalSeconds +
                              "s\n");
                }
            }
        }
    }
}
#endif