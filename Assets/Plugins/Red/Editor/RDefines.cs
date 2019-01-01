#if (CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))) && UNITY_EDITOR
namespace Red.Editor {
    using UnityEditor;
    using UnityEngine;

    [InitializeOnLoad]
    public class RDefines {
        private static readonly string[] defs = {"UNIRX", "RED"};
        
        static RDefines() {
            var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
 
            defs.ForEach(define => {
                if (defines.Contains(define)) {            
                    return;
                }
                
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, (defines + ";" + define));
                Debug.LogWarning("<b>"+define+"</b> added to <i>Scripting Define Symbols</i> for selected build target ("+EditorUserBuildSettings.activeBuildTarget.ToString()+").");
            });
            
        }
    }

}
#endif