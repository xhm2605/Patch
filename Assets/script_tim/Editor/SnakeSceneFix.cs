using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class SnakeSceneFix
{
    private const string Source = "Assets/Snake.unity";
    private const string Target = "Assets/Scenes/Snake.unity";

    // Au-dela de cette taille, le fichier dans Scenes/ contient du vrai travail
    private const long EmptySceneLimit = 20000;

    static SnakeSceneFix()
    {
        EditorApplication.delayCall += () => Run(false);
    }

    [MenuItem("Patch/Ranger la scene Snake")]
    public static void RunFromMenu()
    {
        Run(true);
    }

    static void Run(bool verbose)
    {
        if (!File.Exists(Source))
        {
            if (verbose) Debug.Log("SnakeSceneFix : rien a ranger, " + Source + " n'existe pas.");
            return;
        }

        string active = EditorSceneManager.GetActiveScene().path;
        if (active == Source || active == Target)
        {
            Debug.LogWarning("SnakeSceneFix : la scene Snake est ouverte. Ouvre une autre scene " +
                             "puis relance par le menu Patch > Ranger la scene Snake.");
            return;
        }

        if (File.Exists(Target))
        {
            long size = new FileInfo(Target).Length;

            if (size > EmptySceneLimit)
            {
                Debug.LogWarning("SnakeSceneFix : " + Target + " fait " + size +
                                 " octets, ce n'est pas la scene vide. Rien n'a ete supprime, " +
                                 "verifie toi-meme laquelle garder.");
                return;
            }

            if (!AssetDatabase.DeleteAsset(Target))
            {
                Debug.LogError("SnakeSceneFix : impossible de supprimer " + Target);
                return;
            }

            Debug.Log("SnakeSceneFix : scene vide supprimee (" + size + " octets).");
        }

        string error = AssetDatabase.MoveAsset(Source, Target);

        if (!string.IsNullOrEmpty(error))
        {
            Debug.LogError("SnakeSceneFix : deplacement refuse : " + error);
            return;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        PutInBuildSettings(Target);

        Debug.Log("SnakeSceneFix : la scene Snake est maintenant dans " + Target +
                  " et inscrite dans les Build Settings.");
    }

    static void PutInBuildSettings(string path)
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();

        foreach (EditorBuildSettingsScene s in EditorBuildSettings.scenes)
        {
            if (Path.GetFileName(s.path) == "Snake.unity") continue;
            scenes.Add(s);
        }

        scenes.Add(new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
