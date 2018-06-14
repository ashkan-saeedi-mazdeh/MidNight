using System;
using System.IO;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Allows us to right click on a scene and build it.
/// </summary>
class SceneBuilder
{
    /// <summary>
    /// Creates the menu and builds the scene. checks for being a scene by extension.
    /// Builds using the active platform and the scene doesn't require to be in build settings list.
    /// </summary>
    [MenuItem("Assets/Build Scene %u")]
    static void BuildSceneMenu()
    {
        var selectedObjects = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets);
        if (selectedObjects != null && selectedObjects.Length == 1)
        {
            string[] scenes = new string[1];
            scenes[0] = AssetDatabase.GetAssetOrScenePath(selectedObjects[0]);
            if (!scenes[0].Contains(".unity"))
            {
                Debug.LogWarning("You did not choose a scene file to buid");
                return;
            }

            string path;
            string ext = "";
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.Android:
                    ext = "apk";
                    break;
                case BuildTarget.StandaloneWindows:
                    ext = "exe";
                    break;
                case BuildTarget.StandaloneOSX:
                    ext = "app";
                    break;
                default:
                    break;
            }
            if (EditorPrefs.HasKey(scenes[0]))
            {
                path = EditorPrefs.GetString(scenes[0]);
            }
            else
            {
                path = Application.dataPath;
            }
            path = EditorUtility.SaveFilePanel("Build file", Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + "." + ext, ext);
            if (!string.IsNullOrEmpty(path))
            {
                EditorPrefs.SetString(scenes[0], Path.GetFullPath(path));
                if (EditorBuildSettings.scenes.Length > 0)
                {
                    var first = EditorBuildSettings.scenes[0];
                    var newone = new EditorBuildSettingsScene(path, true);
                    EditorBuildSettings.scenes[0] = newone;
                    BuildPipeline.BuildPlayer(scenes, path, EditorUserBuildSettings.activeBuildTarget, BuildOptions.ShowBuiltPlayer);
                    EditorBuildSettings.scenes[0] = first;
                }
                else
                {
                    var tempScenes = new EditorBuildSettingsScene[1];
                    var newone = new EditorBuildSettingsScene(path, true);
                    tempScenes[0] = newone;
                    EditorBuildSettings.scenes = tempScenes;
                    BuildPipeline.BuildPlayer(scenes, path, EditorUserBuildSettings.activeBuildTarget, BuildOptions.ShowBuiltPlayer);
                    EditorBuildSettings.scenes = null;
                }
            }

        }
    }

}