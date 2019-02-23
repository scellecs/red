#if (CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))) && UNITY_EDITOR
namespace Red.Editor {
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using JetBrains.Annotations;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;

    /// <summary>
    ///     https://gist.github.com/svermeulen/8927b29b2bfab4e84c950b6788b0c677
    /// </summary>
    public class MultiSceneSetup : ScriptableObject {
        public SceneSetup[] Setups;
    }

    public static class MultiSceneSetupMenu {
        [MenuItem("Assets/Multi Scene Setup/Create")]
        public static void CreateNewSceneSetup() {
            var folderPath = TryGetSelectedFolderPathInProjectsTab();

            var assetPath = ConvertFullAbsolutePathToAssetPath(
                Path.Combine(folderPath, "SceneSetup.asset"));

            SaveCurrentSceneSetup(assetPath);
        }

        [MenuItem("Assets/Multi Scene Setup/Create", true)]
        public static bool CreateNewSceneSetupValidate() => TryGetSelectedFolderPathInProjectsTab() != null;

        [MenuItem("Assets/Multi Scene Setup/Overwrite")]
        public static void SaveSceneSetup() {
            var assetPath = ConvertFullAbsolutePathToAssetPath(TryGetSelectedFilePathInProjectsTab());

            SaveCurrentSceneSetup(assetPath);
        }

        private static void SaveCurrentSceneSetup(string assetPath) {
            var loader = ScriptableObject.CreateInstance<MultiSceneSetup>();

            loader.Setups = EditorSceneManager.GetSceneManagerSetup();

            AssetDatabase.CreateAsset(loader, assetPath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Scene setup '{Path.GetFileNameWithoutExtension(assetPath)}' saved");
        }

        [MenuItem("Assets/Multi Scene Setup/Load")]
        public static void RestoreSceneSetup() {
            var assetPath = ConvertFullAbsolutePathToAssetPath(TryGetSelectedFilePathInProjectsTab());

            var loader = AssetDatabase.LoadAssetAtPath<MultiSceneSetup>(assetPath);

            EditorSceneManager.RestoreSceneManagerSetup(loader.Setups);

            Debug.Log($"Scene setup '{Path.GetFileNameWithoutExtension(assetPath)}' restored");
        }

        [MenuItem("Assets/Multi Scene Setup", true)]
        public static bool SceneSetupRootValidate() => HasSceneSetupFileSelected();

        [MenuItem("Assets/Multi Scene Setup/Overwrite", true)]
        public static bool SaveSceneSetupValidate() => HasSceneSetupFileSelected();

        [MenuItem("Assets/Multi Scene Setup/Load", true)]
        public static bool RestoreSceneSetupValidate() => HasSceneSetupFileSelected();

        private static bool HasSceneSetupFileSelected() => TryGetSelectedFilePathInProjectsTab() != null;

        private static List<string> GetSelectedFilePathsInProjectsTab() =>
            GetSelectedPathsInProjectsTab()
                .Where(File.Exists).ToList();

        [CanBeNull]
        private static string TryGetSelectedFilePathInProjectsTab() {
            var selectedPaths = GetSelectedFilePathsInProjectsTab();

            if (selectedPaths.Count == 1) {
                return selectedPaths[0];
            }

            return null;
        }

        // Returns the best guess directory in projects pane
        // Useful when adding to Assets -> Create context menu
        // Returns null if it can't find one
        // Note that the path is relative to the Assets folder for use in AssetDatabase.GenerateUniqueAssetPath etc.
        [CanBeNull]
        private static string TryGetSelectedFolderPathInProjectsTab() {
            var selectedPaths = GetSelectedFolderPathsInProjectsTab();

            if (selectedPaths.Count == 1) {
                return selectedPaths[0];
            }

            return null;
        }

        // Note that the path is relative to the Assets folder
        private static List<string> GetSelectedFolderPathsInProjectsTab() =>
            GetSelectedPathsInProjectsTab()
                .Where(Directory.Exists).ToList();

        private static List<string> GetSelectedPathsInProjectsTab() {
            var paths = new List<string>();

            var selectedAssets = Selection.GetFiltered(
                typeof(Object), SelectionMode.Assets);

            foreach (var item in selectedAssets) {
                var relativePath = AssetDatabase.GetAssetPath(item);

                if (!string.IsNullOrEmpty(relativePath)) {
                    var fullPath = Path.GetFullPath(Path.Combine(
                        Application.dataPath, Path.Combine("..", relativePath)));

                    paths.Add(fullPath);
                }
            }

            return paths;
        }

        private static string ConvertFullAbsolutePathToAssetPath(string fullPath) =>
            "Assets/" + Path.GetFullPath(fullPath)
                .Remove(0, Path.GetFullPath(Application.dataPath).Length + 1)
                .Replace("\\", "/");
    }
}
#endif