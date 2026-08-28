using UnityEditor;
using UnityEngine;

public static class BalanceValidator
{
    [MenuItem("Tools/Game/Validate Balance")]
    public static void ValidateBalance()
    {
        string[] guids =
            AssetDatabase.FindAssets("t:GameBalanceConfig");

        if (guids.Length == 0)
        {
            Debug.LogError("GameBalanceConfig не найден.");
            return;
        }

        string path =
            AssetDatabase.GUIDToAssetPath(guids[0]);

        var config =
            AssetDatabase.LoadAssetAtPath<GameBalanceConfig>(path);

        var errors = config.ValidateConfig();

        if (errors.Count == 0)
        {
            Debug.Log("Game Balance: OK");
            return;
        }

        Debug.LogError(
            "Ошибки GameBalance:\n\n" +
            string.Join("\n", errors));
    }
}