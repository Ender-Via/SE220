using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Woodcutter.Editor
{
    /// <summary>
    /// Menu Tools/Woodcutter/Setup Sprites & Animations:
    /// chỉnh import các sheet pixel của tiều phu, cắt ô (64×64, đòn đánh 96×64) và dựng lại Animation Clip + state.
    /// Menu Tools/Woodcutter/Build Animation Gallery: scene xem thử mọi animation cạnh nhau.
    /// Menu Tools/Woodcutter/Add Pixel Perfect Camera: gắn Pixel Perfect Camera cho Main Camera của scene đang mở.
    /// Chạy lại mỗi khi họa sĩ cập nhật sheet.
    /// </summary>
    public static class WoodcutterSpriteSetup
    {
        const string ArtDir = "Assets/_Project/Art/Characters/Woodcutter/";
        const string AnimDir = ArtDir + "Animations/";
        const int CellHeight = 64;
        // 1 ô = 32 px cho mọi hình (nhân vật, tileset, quái). Nhân vật ~57 px → cao ~1,8 ô.
        public const float PixelsPerUnit = 32f;
        // Pivot tính bằng pixel từ góc trái-dưới của ô: giữa chân, đáy bàn chân.
        // Giữ nguyên số pixel cho mọi sheet (kể cả ô rộng 96) để thân người không bị lệch khi đổi state.
        const float PivotPxX = 36f, PivotPxY = 2f;

        const string ControllerPath = AnimDir + "AC_Woodcutter.controller";

        // sheet, state trong Animator (trùng tên code gọi SetVisualState), độ rộng ô (px), fps, lặp,
        // thời lượng từng frame (giây; null = đều theo fps). Clip không lặp giữ frame cuối.
        // Đòn đánh dùng ô 96 px để rìu vươn ra mà thân người vẫn ở cùng chỗ với Idle.
        static readonly (string sheet, string state, int cellWidth, float fps, bool loop, float[] durations)[] Clips =
        {
            ("Idle", "Idle", 64, 5f, true, null),
            ("Run", "Move", 64, 10f, true, null),
            ("Jump", "Jump", 64, 10f, false, new[] { 0.20f, 0.13f, 0.55f }),
            ("Fall", "Fall", 64, 5f, true, null),
            ("Hit", "Hit", 64, 10f, false, new[] { 0.30f }),
            ("Death", "Death", 64, 10f, false, new[] { 0.25f, 0.80f }),
            ("Roll", "Roll", 64, 12.5f, true, null),
            ("Attack", "Attack", 96, 10f, false, new[] { 0.38f, 0.30f }),
            ("Attack2", "Attack2", 96, 10f, false, new[] { 0.38f, 0.30f }),
            ("HeavyAttack", "HeavyAttack", 96, 10f, false, new[] { 0.50f, 0.45f }),
        };

        /// <summary>Tên các state có clip, theo thứ tự trên (dùng cho scene xem thử animation).</summary>
        public static IEnumerable<(string state, bool loop)> States()
        {
            foreach (var c in Clips) yield return (c.state, c.loop);
        }

        [MenuItem("Tools/Woodcutter/Setup Sprites & Animations")]
        public static void Run()
        {
            var log = new System.Text.StringBuilder();
            var controller = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(ControllerPath);
            foreach (var c in Clips)
            {
                var sprites = SliceSheet(ArtDir + "SPR_Woodcutter_" + c.sheet + ".png", c.cellWidth);
                var clip = RebuildClip(AnimDir + "AN_Woodcutter_" + c.sheet + ".anim", sprites, c.fps, c.loop, c.durations);
                if (controller != null) EnsureState(controller, c.state, clip);
                log.AppendLine($"{c.sheet} → state {c.state}: {sprites.Count} frame");
            }
            SetPrefabIdleSprite();
            AssetDatabase.SaveAssets();
            Debug.Log("[WoodcutterSpriteSetup]\n" + log);
        }

        [MenuItem("Tools/Woodcutter/Add Pixel Perfect Camera")]
        public static void AddPixelPerfectCamera()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                Debug.LogWarning("[WoodcutterSpriteSetup] Scene đang mở không có Main Camera.");
                return;
            }
            var ppc = cam.GetComponent<UnityEngine.Rendering.Universal.PixelPerfectCamera>();
            if (ppc == null) ppc = Undo.AddComponent<UnityEngine.Rendering.Universal.PixelPerfectCamera>(cam.gameObject);
            Undo.RecordObject(ppc, "Setup Pixel Perfect Camera");
            ppc.assetsPPU = (int)PixelsPerUnit;
            ppc.refResolutionX = 640;   // 1280×720 phóng ×2 → thấy 20×11,25 ô
            ppc.refResolutionY = 360;
            ppc.gridSnapping = UnityEngine.Rendering.Universal.PixelPerfectCamera.GridSnapping.UpscaleRenderTexture;
            ppc.cropFrame = UnityEngine.Rendering.Universal.PixelPerfectCamera.CropFrame.None;
            EditorUtility.SetDirty(ppc);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(cam.gameObject.scene);
            Debug.Log("[WoodcutterSpriteSetup] Đã gắn Pixel Perfect Camera (640×360, PPU 32). Nhớ Ctrl+S để lưu scene.");
        }

        // Sprite tĩnh của prefab (hiện trong Scene khi chưa Play) = frame Idle đầu tiên.
        // Phần nhìn của prefab nhân vật: sprite tĩnh, collider, GroundCheck, Animator.
        static void SetPrefabIdleSprite()
        {
            const string prefabPath = "Assets/_Project/Prefabs/Player/PF_PlayerPrototype.prefab";
            Sprite idle = null;
            foreach (var o in AssetDatabase.LoadAllAssetRepresentationsAtPath(ArtDir + "SPR_Woodcutter_Idle.png"))
                if (o is Sprite s && s.name.EndsWith("_00")) idle = s;
            var root = PrefabUtility.LoadPrefabContents(prefabPath);
            var sr = root.GetComponent<SpriteRenderer>();
            if (sr != null && idle != null) sr.sprite = idle;
            // Collider khớp hình vẽ: rộng ~0,75 ô, cao ~1,75 ô, đáy ở chân.
            var col = root.GetComponent<BoxCollider2D>();
            if (col != null)
            {
                col.size = new Vector2(0.75f, 1.75f);
                col.offset = new Vector2(0f, 0.875f);
            }
            // Pivot ở chân nên điểm kiểm tra chạm đất cũng đặt ở chân (gốc của nhân vật).
            var groundCheck = root.transform.Find("GroundCheck");
            if (groundCheck != null) groundCheck.localPosition = Vector3.zero;

            // Animator + controller, và nối vào CharacterContext.Animator.
            // Đặt lại mỗi lần chạy vì "Woodcutter/Build Sandbox Scene" dựng lại prefab từ đầu.
            var animator = root.GetComponent<Animator>();
            if (animator == null) animator = root.AddComponent<Animator>();
            animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath);
            var context = root.GetComponent("CharacterContext");
            if (context != null)
            {
                var so = new SerializedObject(context);
                var prop = so.FindProperty("Animator");
                if (prop != null)
                {
                    prop.objectReferenceValue = animator;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            PrefabUtility.UnloadPrefabContents(root);
        }

        static List<Sprite> SliceSheet(string path, int cellWidth)
        {
            var pivot = new Vector2(PivotPxX / cellWidth, PivotPxY / CellHeight);
            var ti = (TextureImporter)AssetImporter.GetAtPath(path);
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Multiple;
            ti.spritePixelsPerUnit = PixelsPerUnit;
            ti.filterMode = FilterMode.Point;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.mipmapEnabled = false;
            ti.alphaIsTransparency = true;
            ti.maxTextureSize = 2048;

            ti.GetSourceTextureWidthAndHeight(out int w, out int h);
            string baseName = System.IO.Path.GetFileNameWithoutExtension(path);
            var metas = new List<SpriteMetaData>();
            int i = 0;
            for (int y = h - CellHeight; y >= 0; y -= CellHeight)
                for (int x = 0; x + cellWidth <= w; x += cellWidth)
                    metas.Add(new SpriteMetaData
                    {
                        name = $"{baseName}_{i++:00}",
                        rect = new Rect(x, y, cellWidth, CellHeight),
                        alignment = (int)SpriteAlignment.Custom,
                        pivot = pivot,
                    });
#pragma warning disable 618
            ti.spritesheet = metas.ToArray();
#pragma warning restore 618
            ti.SaveAndReimport();

            var sprites = new List<Sprite>();
            foreach (var o in AssetDatabase.LoadAllAssetRepresentationsAtPath(path))
                if (o is Sprite s) sprites.Add(s);
            sprites.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
            return sprites;
        }

        const string GalleryScenePath = "Assets/_Project/Scenes/Sandbox/Sandbox_AnimationGallery.unity";
        const string SquareSpritePath = "Assets/_Project/Art/Characters/Square.png";

        /// <summary>
        /// Dựng lại scene xem thử: mỗi state một nhân vật, xếp lưới 5 cột, có vạch mặt đất để soi chân.
        /// Chạy lại sau khi thêm state mới.
        /// </summary>
        [MenuItem("Tools/Woodcutter/Build Animation Gallery")]
        public static void BuildGallery()
        {
            if (!UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath);
            if (controller == null)
            {
                Debug.LogError("[WoodcutterSpriteSetup] Chưa có AC_Woodcutter. Chạy 'Setup Sprites & Animations' trước.");
                return;
            }

            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);

            // Camera + Pixel Perfect (640×360, PPU 32 → thấy 20×11,25 ô)
            var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5.625f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.13f, 0.14f, 0.18f);
            camGo.transform.position = new Vector3(0f, 0f, -10f);
            var ppc = camGo.AddComponent<UnityEngine.Rendering.Universal.PixelPerfectCamera>();
            ppc.assetsPPU = (int)PixelsPerUnit;
            ppc.refResolutionX = 640;
            ppc.refResolutionY = 360;
            ppc.gridSnapping = UnityEngine.Rendering.Universal.PixelPerfectCamera.GridSnapping.UpscaleRenderTexture;

            // Sprite mặc định của URP 2D là Lit, cần đèn global để không bị đen
            var light = new GameObject("Global Light 2D").AddComponent<UnityEngine.Rendering.Universal.Light2D>();
            light.lightType = UnityEngine.Rendering.Universal.Light2D.LightType.Global;
            light.intensity = 1f;

            var gallery = new GameObject("AnimationGallery").AddComponent<Woodcutter.Player.AnimationGallery>();
            Sprite square = AssetDatabase.LoadAssetAtPath<Sprite>(SquareSpritePath);
            Sprite idle = null;
            foreach (var o in AssetDatabase.LoadAllAssetRepresentationsAtPath(ArtDir + "SPR_Woodcutter_Idle.png"))
                if (o is Sprite s && s.name.EndsWith("_00")) idle = s;

            const int columns = 5;
            const float spacingX = 3.5f, spacingY = 4.5f, left = -7f, top = 1f;
            int index = 0;
            foreach (var (state, loop) in States())
            {
                int col = index % columns, row = index / columns;
                var go = new GameObject(state);
                go.transform.position = new Vector3(left + col * spacingX, top - row * spacingY, 0f);
                go.AddComponent<SpriteRenderer>().sprite = idle;
                var animator = go.AddComponent<Animator>();
                animator.runtimeAnimatorController = controller;
                gallery.items.Add(new Woodcutter.Player.AnimationGallery.Item { stateName = state, animator = animator, loop = loop });

                // Vạch mặt đất 1 px ngay dưới chân để soi nhân vật có lún/lơ lửng không
                if (square != null && col == 0)
                {
                    var ground = new GameObject($"Ground_Row{row}").AddComponent<SpriteRenderer>();
                    ground.sprite = square;
                    ground.color = new Color(0.45f, 0.5f, 0.55f);
                    Vector2 size = square.bounds.size;
                    float width = columns * spacingX;
                    ground.transform.position = new Vector3(left - spacingX / 2f + width / 2f, go.transform.position.y - 0.5f / PixelsPerUnit, 0f);
                    ground.transform.localScale = new Vector3(width / size.x, (1f / PixelsPerUnit) / size.y, 1f);
                    ground.sortingOrder = -1;
                }
                index++;
            }

            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, GalleryScenePath);
            Debug.Log($"[WoodcutterSpriteSetup] Đã dựng {GalleryScenePath} với {index} state. Bấm Play để xem.");
        }

        // Thêm state vào layer đầu nếu chưa có, và gán clip.
        static void EnsureState(UnityEditor.Animations.AnimatorController controller, string stateName, AnimationClip clip)
        {
            var sm = controller.layers[0].stateMachine;
            foreach (var cs in sm.states)
            {
                if (cs.state.name == stateName)
                {
                    cs.state.motion = clip;
                    return;
                }
            }
            sm.AddState(stateName).motion = clip;
            EditorUtility.SetDirty(controller);
        }

        static AnimationClip RebuildClip(string path, List<Sprite> sprites, float fps, bool loop, float[] durations)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null)
            {
                clip = new AnimationClip();
                AssetDatabase.CreateAsset(clip, path);
            }
            clip.ClearCurves();
            clip.frameRate = 60f;

            // Thêm 1 key cuối lặp lại frame cuối, để frame cuối hiện đủ thời lượng trước khi clip lặp.
            var keys = new ObjectReferenceKeyframe[sprites.Count + 1];
            float t = 0f;
            for (int i = 0; i < sprites.Count; i++)
            {
                keys[i] = new ObjectReferenceKeyframe { time = t, value = sprites[i] };
                t += durations != null && i < durations.Length ? durations[i] : 1f / fps;
            }
            keys[sprites.Count] = new ObjectReferenceKeyframe { time = t, value = sprites[sprites.Count - 1] };
            var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            settings.stopTime = t;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip);
            return clip;
        }
    }
}
