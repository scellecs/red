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
        private static readonly string[] Defines = {"UNIRX", "RED"};

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

            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            var nsub = loadedAssemblies.FirstOrDefault(assembly => assembly.GetName().Name == "NSubstitute");
            var zenject = loadedAssemblies.FirstOrDefault(assembly => assembly.GetName().Name == "zenject");

            var defines = new List<string>(  RDefines.Defines);
            if (nsub != null) {
                defines.Add("NSUBSTITUTE");
            }
            if (zenject != null) {
                
                defines.Add("ZENJECT");
            }
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