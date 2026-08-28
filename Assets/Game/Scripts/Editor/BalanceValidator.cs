using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

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

        var report = new List<string>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var config = AssetDatabase.LoadAssetAtPath<GameBalanceConfig>(path);

            if (config == null)
            {
                report.Add($"{path}: asset не удалось загрузить.");
                continue;
            }

            var errors = config.ValidateConfig();

            foreach (string error in errors)
                report.Add($"{path}: {error}");
        }

        if (report.Count == 0)
            Debug.Log($"Game Balance: OK. Проверено assets: {guids.Length}.");
        else
            Debug.LogError("Ошибки GameBalance:\n\n" + string.Join("\n", report));
    }
}
