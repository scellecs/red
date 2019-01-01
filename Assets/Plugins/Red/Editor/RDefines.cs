#if (CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))) && UNITY_EDITOR
namespace Red.Editor {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    [InitializeOnLoad]
    public class RDefines {
        private static readonly string[] Defines = {"UNIRX", "RED"};

        private static readonly char[] DefineSeperators = new char[] {
            ';',
            ',',
            ' '
        };

        static RDefines() {
            UpdateDefineSymbols();
        }

        private static void UpdateDefineSymbols() {
            var target = EditorUserBuildSettings.selectedBuildTargetGroup;

            var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(target)
                .Split(DefineSeperators, StringSplitOptions.RemoveEmptyEntries);
            var newSymbols = new List<string>(symbols);

            Defines
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