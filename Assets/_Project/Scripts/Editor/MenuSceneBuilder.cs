using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Woodcutter.Data;
using Woodcutter.UI;

namespace Woodcutter.EditorTools
{
    /// <summary>
    /// Dựng giao diện gray-box cho T021 (FR-SYS-01, FR-SYS-02):
    /// - Scene Scenes/MainMenu.unity (index 0 trong Build Profiles).
    /// - Prefab Prefabs/UI/PF_PauseMenu.prefab, đặt vào scene nền Sandbox_RoomTransition.
    /// Chữ dùng UI Text + font LegacyRuntime (font động, có dấu tiếng Việt qua font hệ thống);
    /// ARTE thay font/chất liệu giấy-gỗ sau.
    /// </summary>
    public static class MenuSceneBuilder
    {
        private const string MainMenuScenePath = "Assets/_Project/Scenes/MainMenu.unity";
        private const string PauseMenuPrefabPath = "Assets/_Project/Prefabs/UI/PF_PauseMenu.prefab";
        private const string PersistentScenePath = "Assets/_Project/Scenes/Sandbox/Sandbox_RoomTransition.unity";
        private const string LanguageFilePath = "Assets/_Project/Localization/vi.json";
        private const string SaveSettingsPath = "Assets/_Project/Data/SaveRespawnSettings.asset";

        private static readonly Color BackgroundColor = new Color(0.08f, 0.09f, 0.1f);
        private static readonly Color PanelColor = new Color(0.12f, 0.13f, 0.15f, 0.95f);
        private static readonly Color ButtonColor = new Color(0.22f, 0.24f, 0.27f);
        private static readonly Color TextColor = new Color(0.95f, 0.95f, 0.95f);

        private static Font font;
        private static TextAsset languageFile;

        [MenuItem("Woodcutter/UI/Build Main Menu & Pause Menu")]
        public static void BuildAll()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            BuildAllBatch();
            EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
            Debug.Log("[MenuSceneBuilder] Xong. Mở MainMenu rồi bấm Play.");
        }

        /// <summary>Dòng lệnh: -executeMethod Woodcutter.EditorTools.MenuSceneBuilder.BuildAllBatch</summary>
        public static void BuildAllBatch()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            languageFile = AssetDatabase.LoadAssetAtPath<TextAsset>(LanguageFilePath);
            if (languageFile == null)
            {
                Debug.LogError($"[MenuSceneBuilder] Không tìm thấy {LanguageFilePath}.");
                return;
            }

            BuildMainMenuScene();
            GameObject pausePrefab = BuildPauseMenuPrefab();
            AddPauseMenuToPersistentScene(pausePrefab);
            UpdateBuildScenes();
            AssetDatabase.SaveAssets();
        }

        // ================================================================
        // MENU CHÍNH
        // ================================================================

        private static void BuildMainMenuScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = BackgroundColor;
            camGo.transform.position = new Vector3(0f, 0f, -10f);

            CreateEventSystem();

            Canvas canvas = CreateCanvas("MainMenuCanvas", 0);
            RectTransform canvasRect = (RectTransform)canvas.transform;

            // Nền tuyết rơi
            RectTransform snow = CreateRect("Snowfall", canvasRect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            snow.gameObject.AddComponent<SnowfallUI>();

            // Tên game (logo tạm)
            RectTransform title = CreateText("Title", canvasRect, "game.title", 44, FontStyle.Bold);
            SetAnchored(title, new Vector2(0.5f, 1f), new Vector2(0f, -150f), new Vector2(1000f, 80f));

            RectTransform column = CreateVerticalColumn("Buttons", canvasRect, new Vector2(0f, -60f), new Vector2(320f, 280f));
            Button newGame = CreateButton("Btn_NewGame", column, "menu.new_game");
            Button cont = CreateButton("Btn_Continue", column, "menu.continue");
            Button options = CreateButton("Btn_Options", column, "menu.options");
            Button quit = CreateButton("Btn_Quit", column, "menu.quit");

            ConfirmDialog dialog = CreateDialog(canvasRect);

            var controllerGo = new GameObject("MainMenu");
            var controller = controllerGo.AddComponent<MainMenuController>();
            var saveSettings = AssetDatabase.LoadAssetAtPath<SaveRespawnSettingsSO>(SaveSettingsPath);
            SetRefs(controller, ("saveSettings", saveSettings), ("newGameButton", newGame), ("continueButton", cont),
                ("optionsButton", options), ("quitButton", quit), ("dialog", dialog));

            EditorSceneManager.SaveScene(scene, MainMenuScenePath);
        }

        // ================================================================
        // MENU TẠM DỪNG
        // ================================================================

        private static GameObject BuildPauseMenuPrefab()
        {
            Canvas canvas = CreateCanvas("PF_PauseMenu", 500); // dưới ScreenFader (1000)
            RectTransform canvasRect = (RectTransform)canvas.transform;

            RectTransform root = CreateRect("Root", canvasRect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var dim = root.gameObject.AddComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.6f);

            RectTransform panel = CreateRect("Panel", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(420f, 400f));
            panel.gameObject.AddComponent<Image>().color = PanelColor;

            RectTransform title = CreateText("Title", panel, "pause.title", 34, FontStyle.Bold);
            SetAnchored(title, new Vector2(0.5f, 1f), new Vector2(0f, -45f), new Vector2(380f, 60f));

            RectTransform column = CreateVerticalColumn("Buttons", panel, new Vector2(0f, -35f), new Vector2(320f, 260f));
            Button resume = CreateButton("Btn_Resume", column, "pause.resume");
            Button notes = CreateButton("Btn_Notes", column, "pause.notes");
            Button options = CreateButton("Btn_Options", column, "pause.options");
            Button mainMenu = CreateButton("Btn_MainMenu", column, "pause.main_menu");

            ConfirmDialog dialog = CreateDialog(canvasRect);

            var controller = canvas.gameObject.AddComponent<PauseMenuController>();
            SetRefs(controller, ("root", root.gameObject), ("resumeButton", resume), ("notesButton", notes),
                ("optionsButton", options), ("mainMenuButton", mainMenu), ("dialog", dialog));
            root.gameObject.SetActive(false);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(canvas.gameObject, PauseMenuPrefabPath);
            Object.DestroyImmediate(canvas.gameObject);
            return prefab;
        }

        private static void AddPauseMenuToPersistentScene(GameObject prefab)
        {
            Scene scene = EditorSceneManager.OpenScene(PersistentScenePath, OpenSceneMode.Single);

            if (Object.FindAnyObjectByType<PauseMenuController>() == null)
                PrefabUtility.InstantiatePrefab(prefab, scene);

            if (Object.FindAnyObjectByType<EventSystem>() == null)
                CreateEventSystem();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void UpdateBuildScenes()
        {
            var scenes = EditorBuildSettings.scenes.Where(s => s.path != MainMenuScenePath).ToList();
            scenes.Insert(0, new EditorBuildSettingsScene(MainMenuScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        // ================================================================
        // KHỐI DỰNG UI
        // ================================================================

        private static void CreateEventSystem()
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>(); // action UI mặc định: phím mũi tên/WASD, Enter, tay cầm
        }

        private static Canvas CreateCanvas(string name, int sortingOrder)
        {
            var go = new GameObject(name);
            go.layer = LayerMask.NameToLayer("UI");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 anchoredPos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = LayerMask.NameToLayer("UI");
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;
            return rect;
        }

        private static void SetAnchored(RectTransform rect, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
        }

        private static RectTransform CreateText(string name, Transform parent, string key, int fontSize, FontStyle style)
        {
            RectTransform rect = CreateRect(name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var text = rect.gameObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = TextColor;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            text.text = key;

            var localized = rect.gameObject.AddComponent<LocalizedText>();
            var so = new SerializedObject(localized);
            so.FindProperty("key").stringValue = key;
            so.FindProperty("languageFile").objectReferenceValue = languageFile;
            so.ApplyModifiedPropertiesWithoutUndo();
            return rect;
        }

        private static RectTransform CreateVerticalColumn(string name, Transform parent, Vector2 pos, Vector2 size)
        {
            RectTransform rect = CreateRect(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size);
            var layout = rect.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 14f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            return rect;
        }

        private static Button CreateButton(string name, Transform parent, string key)
        {
            RectTransform rect = CreateRect(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(320f, 54f));
            var image = rect.gameObject.AddComponent<Image>();
            image.color = Color.white;

            var button = rect.gameObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = ButtonColor;
            colors.highlightedColor = new Color(0.4f, 0.43f, 0.47f);
            colors.selectedColor = new Color(0.4f, 0.43f, 0.47f);
            colors.pressedColor = new Color(0.55f, 0.58f, 0.62f);
            colors.disabledColor = new Color(0.16f, 0.17f, 0.19f, 0.6f); // mờ khi khóa (FR-SYS-01)
            button.colors = colors;

            // Viền khi được chọn bằng tay cầm/bàn phím, để không chỉ dựa vào màu (NFR-12)
            var outline = rect.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 1f, 1f, 0.35f);
            outline.effectDistance = new Vector2(2f, -2f);

            RectTransform label = CreateText("Label", rect, key, 24, FontStyle.Normal);
            label.GetComponent<Text>().color = TextColor;
            return button;
        }

        private static ConfirmDialog CreateDialog(RectTransform canvasRect)
        {
            RectTransform root = CreateRect("ConfirmDialog", canvasRect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            root.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.7f); // chặn bấm phía sau

            RectTransform panel = CreateRect("Panel", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(620f, 260f));
            panel.gameObject.AddComponent<Image>().color = PanelColor;

            RectTransform message = CreateText("Message", panel, "menu.confirm_overwrite", 24, FontStyle.Normal);
            SetAnchored(message, new Vector2(0.5f, 1f), new Vector2(0f, -80f), new Vector2(560f, 120f));

            RectTransform row = CreateRect("Buttons", panel, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 55f), new Vector2(460f, 54f));
            var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 30f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            Button yes = CreateButton("Btn_Yes", row, "common.yes");
            Button no = CreateButton("Btn_No", row, "common.no");

            var dialog = root.gameObject.AddComponent<ConfirmDialog>();
            SetRefs(dialog, ("root", root.gameObject), ("message", message.GetComponent<LocalizedText>()),
                ("yesButton", yes), ("noButton", no));
            return dialog;
        }

        private static void SetRefs(Object target, params (string field, Object value)[] refs)
        {
            var so = new SerializedObject(target);
            foreach (var (field, value) in refs)
            {
                SerializedProperty prop = so.FindProperty(field);
                if (prop == null)
                {
                    Debug.LogError($"[MenuSceneBuilder] {target.GetType().Name} không có trường '{field}'.");
                    continue;
                }
                prop.objectReferenceValue = value;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
