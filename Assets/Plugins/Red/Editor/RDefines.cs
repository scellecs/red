#if (CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))) && UNITY_EDITOR
namespace Red.Editor {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    [InitializeOnLoad]
    public class RDefines {
        private static readonly string[] DefaultDefines = {"UNIRX", "RED"};
        private static readonly Dictionary<string, string> PossibleDefines = new Dictionary<string, string> {
            ["NSubstitute"] = "NSUBSTITUTE", 
            ["zenject"] = "ZENJECT"
        };

        private static readonly char[] DefineSeparators = {
            ';',
            ',',
            ' '
        };

        static RDefines() => RDefines.UpdateDefineSymbols();

        private static void UpdateDefineSymbols() {
            var target = EditorUserBuildSettings.selectedBuildTargetGroup;

            var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(target)
                .Split(RDefines.DefineSeparators, StringSplitOptions.RemoveEmptyEntries);
            var newSymbols = new List<string>(symbols);

            var defines = new List<string>(  RDefines.DefaultDefines);
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            RDefines.PossibleDefines.ForEach(pair => {
                var temp = loadedAssemblies.FirstOrDefault(assembly => assembly.GetName().Name == pair.Key);
                if (temp != null) {
                    defines.Add(pair.Value);
                }
            });
            
            defines
                .Where(def => newSymbols.Contains(def) == false)
                .ForEach(def => {
                    newSymbols.Add(def);
                    Debug.LogWarning($"<b>{def}</b> added to <i>Scripting Define Symbols</i> " +
                                     $"for selected build target ({EditorUserBuildSettings.activeBuildTarget.ToString()}).");
                });

            PlayerSettings.SetScriptingDefineSymbolsForGroup(target,
                string.Join(";", newSymbols.ToArray()));
        }
    }
}
#endif