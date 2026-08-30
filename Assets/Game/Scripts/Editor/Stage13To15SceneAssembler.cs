using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class Stage13To15SceneAssembler
{
    [MenuItem("Game/Setup/Assemble Stages 13-15")]
    public static void Assemble()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (scene.path != "Assets/Game/Scenes/VerticalSlice.unity")
        {
            Debug.LogError("Откройте сцену VerticalSlice перед сборкой этапов 13-15.");
            return;
        }

        SceneReferences references = Object.FindAnyObjectByType<SceneReferences>();
        WorldCompositionRoot composition = Object.FindAnyObjectByType<WorldCompositionRoot>();
        RaidRuntimeController raidRuntime = Object.FindAnyObjectByType<RaidRuntimeController>();
        SunExposureCoordinator sun = Object.FindAnyObjectByType<SunExposureCoordinator>();
        Transform world = GameObject.Find("World").transform;

        RaidTargetPoint[] raidTargets =
        {
            EnsureTarget("RaidTarget_GreedWall", references.wallSocket01.position, RaidTargetKind.GreedWall, 0, 300, new Color(.35f,.12f,.42f)),
            EnsureTarget("RaidTarget_EyeTower", references.towerSocket01.position, RaidTargetKind.EyeTower, 1, 160, new Color(.5f,.15f,.6f)),
            EnsureTarget("RaidTarget_Heart", references.heartPoint.position, RaidTargetKind.Heart, 2, 500, new Color(.65f,.08f,.3f)),
            EnsureMainGreedTarget()
        };

        SetReference(raidRuntime, "humanSpawnPoint", references.humanWaitingPoint);
        SetReference(raidRuntime, "humanReturnPoint", references.humanWaitingPoint);
        SetArray(raidRuntime, "targets", raidTargets);
        RebuildGreedAgentPrefab();
        RebuildHumanRaidPrefab();
        RaidUnitPoolBehaviour pool = Object.FindAnyObjectByType<RaidUnitPoolBehaviour>();
        GameObject humanRaidPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Prefabs/Human/HumanRaidAgent.prefab");
        SetReference(pool, "prefab", humanRaidPrefab.GetComponent<RaidUnitView>());

        GameObject assaultRoot = EnsureObject("AssaultRuntime", GameObject.Find("Systems").transform);
        AssaultRuntimeController assault = GetOrAdd<AssaultRuntimeController>(assaultRoot);
        GameObject greedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Prefabs/Characters/GreedAgent.prefab");
        SetReference(assault, "fighterPrefab", greedPrefab);
        SetReference(assault, "commanderPrefab", greedPrefab);
        SetReference(assault, "formationPoint", references.greedFormationPoint);
        SetReference(assault, "sunExposureCoordinator", sun);

        GameObject returnPoint = EnsureObject("AssaultReturnPoint", world);
        returnPoint.transform.position = new Vector3(12.5f, references.heartPoint.position.y, 0f);
        SetReference(assault, "infectionReturnPoint", returnPoint.transform);

        Transform[] assaultTargets =
        {
            EnsureTarget("AssaultTarget_OuterWall", references.humanOuterWallPoint.position, RaidTargetKind.OtherBuilding, 10, 350, new Color(.45f,.45f,.5f)).transform,
            EnsureTarget("AssaultTarget_InnerWall", references.humanInnerWallPoint.position, RaidTargetKind.OtherBuilding, 11, 500, new Color(.4f,.4f,.45f)).transform,
            EnsureTarget("AssaultTarget_Monarch", references.humanMonarchPoint.position, RaidTargetKind.OtherBuilding, 12, 60, new Color(.8f,.7f,.25f)).transform
        };
        SetArray(assault, "orderedTargets", assaultTargets);
        SetReference(composition, "assaultRuntimeController", assault);

        GameObject command = GameObject.Find("CommandNode_Runtime");
        CommandNodePresenter presenter = command.GetComponent<CommandNodePresenter>();
        CommandNodeController controller = GetOrAdd<CommandNodeController>(command);
        SetReference(controller, "presenter", presenter);
        AssembleCommandNodeLabels(command, presenter);

        EditorUtility.SetDirty(composition);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("Этапы 13-15: runtime-объекты и ссылки собраны.");
    }

    private static RaidTargetPoint EnsureTarget(string name, Vector3 position, RaidTargetKind kind, int priority, int health, Color color)
    {
        GameObject target = EnsureObject(name, GameObject.Find("World").transform);
        target.transform.position = position;
        target.transform.localScale = new Vector3(1.2f, 3f, 1f);
        SpriteRenderer renderer = GetOrAdd<SpriteRenderer>(target);
        renderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        renderer.color = color;
        PlayerHealth targetHealth = GetOrAdd<PlayerHealth>(target);
        targetHealth.Configure(2000 + priority, health);
        GetOrAdd<DamageReceiver>(target);
        RaidTargetPoint point = GetOrAdd<RaidTargetPoint>(target);
        SetEnum(point, "kind", (int)kind);
        SetInteger(point, "priority", priority);
        return point;
    }

    private static RaidTargetPoint EnsureMainGreedTarget()
    {
        RaidTargetPoint point = GetOrAdd<RaidTargetPoint>(GameObject.Find("MainGreed"));
        SetEnum(point, "kind", (int)RaidTargetKind.MainGreed);
        SetInteger(point, "priority", 3);
        return point;
    }

    private static void RebuildHumanRaidPrefab()
    {
        const string path = "Assets/Game/Prefabs/Human/HumanRaidAgent.prefab";
        GameObject root = new("HumanRaidAgent");
        Rigidbody2D body = root.AddComponent<Rigidbody2D>();
        body.freezeRotation = true;
        root.AddComponent<CapsuleCollider2D>();
        RaidUnitView view = root.AddComponent<RaidUnitView>();
        root.AddComponent<PlayerHealth>();
        root.AddComponent<DamageReceiver>();
        FactionMember faction = root.AddComponent<FactionMember>();
        SetEnum(faction, "faction", (int)Faction.Human);
        GameObject visualRoot = CreateSpriteChild(root.transform, "VisualRoot", new Color(.2f,.55f,.85f), Vector3.zero, new Vector3(1f,2f,1f));
        GameObject weapon = CreateSpriteChild(visualRoot.transform, "Weapon", new Color(.75f,.75f,.8f), new Vector3(.7f,0f,0f), new Vector3(.8f,.15f,1f));
        visualRoot.AddComponent<Animator>();
        SetReference(view, "visual", visualRoot.GetComponent<SpriteRenderer>());
        SetReference(view, "weapon", weapon.transform);
        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
    }

    private static void RebuildGreedAgentPrefab()
    {
        const string path = "Assets/Game/Prefabs/Characters/GreedAgent.prefab";
        GameObject root = new("GreedAgent");
        Rigidbody2D body = root.AddComponent<Rigidbody2D>();
        body.freezeRotation = true;
        root.AddComponent<CapsuleCollider2D>();
        root.AddComponent<AgentMotor>();
        root.AddComponent<AgentBrain>();
        root.AddComponent<PlayerHealth>();
        root.AddComponent<DamageReceiver>();
        root.AddComponent<FactionMember>();
        GameObject carry = new("CarrySocket");
        carry.transform.SetParent(root.transform, false);
        carry.transform.localPosition = new Vector3(0f,.7f,0f);
        CreateSpriteChild(root.transform, "VisualRoot", new Color(.55f,.15f,.75f), Vector3.zero, new Vector3(.8f,1.4f,1f));
        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
    }

    private static GameObject CreateSpriteChild(Transform parent, string name, Color color, Vector3 position, Vector3 scale)
    {
        GameObject child = new(name);
        child.transform.SetParent(parent, false);
        child.transform.localPosition = position;
        child.transform.localScale = scale;
        SpriteRenderer renderer = child.AddComponent<SpriteRenderer>();
        renderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        renderer.color = color;
        return child;
    }

    private static void AssembleCommandNodeLabels(GameObject command, CommandNodePresenter presenter)
    {
        Transform oldCanvas = command.transform.Find("WorldCanvas");
        if (oldCanvas != null) Object.DestroyImmediate(oldCanvas.gameObject);
        GameObject canvasObject = new("WorldCanvas", typeof(RectTransform), typeof(Canvas));
        canvasObject.transform.SetParent(command.transform, false);
        canvasObject.transform.localPosition = new Vector3(0f, 2f, 0f);
        canvasObject.transform.localScale = Vector3.one * .02f;
        canvasObject.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
        TMP_Text price = CreateLabel(canvasObject.transform, "Price", "Цена: 6", 60f);
        TMP_Text composition = CreateLabel(canvasObject.transform, "Composition", "Командир + 4 Бойца · минимум 75 с", 0f);
        TMP_Text status = CreateLabel(canvasObject.transform, "Status", string.Empty, -60f);
        SetReference(presenter, "priceLabel", price);
        SetReference(presenter, "compositionLabel", composition);
        SetReference(presenter, "statusLabel", status);
    }

    private static TMP_Text CreateLabel(Transform parent, string name, string text, float y)
    {
        GameObject labelObject = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(parent, false);
        RectTransform rect = labelObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(500f, 60f);
        rect.anchoredPosition = new Vector2(0f, y);
        TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = 28f;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.raycastTarget = false;
        return label;
    }

    private static GameObject EnsureObject(string name, Transform parent)
    {
        GameObject result = GameObject.Find(name) ?? new GameObject(name);
        result.transform.SetParent(parent, true);
        return result;
    }

    private static T GetOrAdd<T>(GameObject target) where T : Component
    {
        T existing = target.GetComponent<T>();
        return existing != null ? existing : target.AddComponent<T>();
    }

    private static void SetReference(Object target, string name, Object value)
    {
        SerializedObject serialized = new(target);
        serialized.FindProperty(name).objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetInteger(Object target, string name, int value)
    {
        SerializedObject serialized = new(target);
        serialized.FindProperty(name).intValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetEnum(Object target, string name, int value)
    {
        SerializedObject serialized = new(target);
        serialized.FindProperty(name).enumValueIndex = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetArray<T>(Object target, string name, T[] values) where T : Object
    {
        SerializedObject serialized = new(target);
        SerializedProperty array = serialized.FindProperty(name);
        array.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }
}
