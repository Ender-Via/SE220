using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Woodcutter.Demo;

namespace Woodcutter.EditorTools
{
    /// <summary>
    /// Dựng scene demo giới thiệu game: Woodcutter > Demo > Build Demo Scene.
    /// Chạy lại bất cứ lúc nào để dựng lại từ đầu (ghi đè scene).
    /// Đi từ trái sang phải: 1 Đánh thường → 2 Nhặt đá + mẩu giấy → 3 Sói tuyết → 4 Rìu lửa phá băng → 5 Vòng lặp & sắp tới.
    /// </summary>
    public static class DemoSceneBuilder
    {
        const string ScenePath = "Assets/_Project/Demo/Sandbox_DemoGioiThieu.unity";
        const string Enemies = "Assets/_Project/Art/Characters/Enemies/";

        static Sprite square;

        [MenuItem("Woodcutter/Demo/Build Demo Scene")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            square = AssetDatabase.LoadAllAssetsAtPath("Assets/_Project/Art/Characters/Square.png").OfType<Sprite>().FirstOrDefault();
            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer < 0) groundLayer = 0;

            // ---------- Camera, ánh sáng, UI ----------
            var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5.625f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.78f, 0.82f, 0.86f);
            camGo.transform.position = new Vector3(4, 4.5f, -10);
            var follow = camGo.AddComponent<DemoCameraFollow>();
            follow.minX = 4f; follow.maxX = 138f;

            var light = new GameObject("Global Light 2D").AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Global;
            light.intensity = 1f;

            new GameObject("DemoUI").AddComponent<DemoUI>();

            // ---------- Mặt đất ----------
            var level = new GameObject("Level").transform;
            Box("Ground", level, new Vector2(66, -1.5f), new Vector2(150, 3), new Color(0.32f, 0.35f, 0.4f), groundLayer, true);
            Box("Snow", level, new Vector2(66, -0.05f), new Vector2(150, 0.1f), new Color(0.95f, 0.97f, 1f), groundLayer, false);
            Box("WallLeft", level, new Vector2(-6, 5), new Vector2(2, 14), new Color(0.32f, 0.35f, 0.4f), groundLayer, true);
            Box("WallRight", level, new Vector2(142, 5), new Vector2(2, 14), new Color(0.32f, 0.35f, 0.4f), groundLayer, true);

            // ---------- Người chơi ----------
            var pf = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Player/PF_PlayerPrototype.prefab");
            var player = (GameObject)PrefabUtility.InstantiatePrefab(pf);
            player.transform.position = new Vector3(0, 0.1f, 0);
            // PF_PlayerPrototype trên main còn collider 1x1 đặt giữa (thời hình vuông), còn sprite tiều phu có gốc ở chân.
            // Chỉ chỉnh trên bản sao trong scene demo (không sửa prefab): collider ôm người, GroundCheck ở chân.
            var pcol = player.GetComponent<BoxCollider2D>();
            if (pcol != null) { pcol.size = new Vector2(0.8f, 1.7f); pcol.offset = new Vector2(0f, 0.85f); }
            var gc = player.transform.Find("GroundCheck");
            if (gc != null) gc.localPosition = Vector3.zero;
            var combat = player.AddComponent<DemoPlayerCombat>();
            combat.fireSprite = square;
            player.AddComponent<DemoPlayerHealth>();
            var vis = player.AddComponent<DemoPlayerVisuals>();
            vis.controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/_Project/Art/Characters/Woodcutter/Animations/AC_Woodcutter.controller");
            follow.target = player.transform;

            // ---------- 0. Mở đầu ----------
            Sign("Sign_0_Intro", new Vector2(4, 7.6f), 520,
                "TIỀU PHU VÀ NHỮNG VIÊN ĐÁ MÀU – bản demo",
                "Thế giới chỉ còn trắng, đen và xanh băng. Ông tiều phu lần theo dấu nhóm phiêu lưu đã phản bội nhau để thu lại 3 viên đá màu và trả màu cho thế giới.\n\nĐi sang phải (D) để bắt đầu →");

            // ---------- 1. Đánh thường ----------
            Sign("Sign_1_Combat", new Vector2(15, 7.6f), 470,
                "1. ĐÁNH THƯỜNG",
                "• <b>J</b>: chém. Bấm J lần nữa ngay sau đó = <b>combo 2 nhịp</b> (1 + 1 sát thương).\n• <b>Giữ J</b> ~0,6 s rồi thả: <b>đòn nặng</b> 3 sát thương, có hit-stop + rung màn hình.\n• <b>K</b>: lăn né (có khoảnh khắc bất tử).\nThử chém các khúc gỗ – chúng tự mọc lại.");
            for (int i = 0; i < 3; i++)
            {
                var log = Box("TrainingLog_" + (i + 1), null, new Vector2(11 + i * 4, 0.9f), new Vector2(0.8f, 1.8f), new Color(0.45f, 0.36f, 0.27f), 0, false);
                log.GetComponent<SpriteRenderer>().sortingOrder = 1;
                var bc = log.AddComponent<BoxCollider2D>(); bc.isTrigger = true;
                var d = log.AddComponent<DemoDamageable>(); d.maxHp = 3; d.respawnDelay = 1.5f;
            }

            // ---------- 2. Nhặt đá + mẩu giấy ----------
            Sign("Sign_2_Stone", new Vector2(33, 7.6f), 470,
                "2. MẨU GIẤY VÀ VIÊN ĐÁ ĐỎ",
                "Đầu game, ông tiều phu tìm thấy một <b>mẩu giấy cháy dở</b> và <b>Đá Đỏ</b> trong một ngách nhỏ.\n• Đứng gần, bấm <b>E</b> để đọc mẩu giấy – manh mối dẫn đường.\n• Bấm <b>E</b> nhặt đá: nhận năng lực <b>Rìu lửa</b>, +1 vạch máu, <b>màu đỏ loang ra</b> (táo, hoa) và <b>lửa trại bùng cháy</b>.", true);
            var note = Box("Pickup_Note", null, new Vector2(30, 1.1f), new Vector2(0.7f, 0.5f), new Color(0.9f, 0.84f, 0.7f), 0, false);
            note.transform.rotation = Quaternion.Euler(0, 0, -8);
            var np = note.AddComponent<DemoPickup>();
            np.kind = DemoPickup.Kind.Note;
            np.noteText = "<b>Mẩu giấy cháy dở</b>\n\n\"…chúng tôi đã phá cây để lấy đá. Đêm đó, cả nhóm phản bội nhau. Tôi ôm viên đá đỏ chạy về phía đông, qua bãi săn, tới rừng chết… Nếu ai đọc được, hãy mang đá trả về gốc cây. Đừng đi qua lối băng một mình…\"\n\n<i>Phần còn lại đã cháy mất.</i>";
            // ngách nhỏ chứa đá
            Box("Niche_Back", level, new Vector2(38, 1.2f), new Vector2(2.6f, 2.4f), new Color(0.22f, 0.24f, 0.28f), 0, false).GetComponent<SpriteRenderer>().sortingOrder = -2;
            var stone = Diamond("Pickup_RedStone", new Vector2(38, 1.3f), 0.55f, new Color(0.95f, 0.18f, 0.15f));
            var sp = stone.AddComponent<DemoPickup>();
            sp.kind = DemoPickup.Kind.RedStone;

            // Lửa trại (tắt, củi xám) – bùng cháy khi có Đá Đỏ hoặc khi chém Rìu lửa vào
            Campfire(new Vector2(34.5f, 0f));

            // Cây cối: thân và tán luôn trắng-đen (chỉ đá ĐỎ đã về, chưa có màu khác).
            // Vùng hồi màu chỉ chứa những thứ màu ĐỎ: táo trên cây, hoa đỏ dưới gốc – xám lúc đầu, đỏ lên khi nhặt đá.
            var trees = new GameObject("Trees").transform;
            var zoneGo = new GameObject("ColorZone_Red");
            var zone = zoneGo.AddComponent<DemoColorZone>();
            sp.colorZone = zone;
            var red = new Color(0.9f, 0.15f, 0.12f);
            for (int i = 0; i < 12; i++)
            {
                float x = 24 + i * 5.5f + (i % 2) * 1.3f;
                float h = 2.5f + (i % 3) * 0.8f;
                var trunk = Box("Tree_" + i + "_Trunk", trees, new Vector2(x, h / 2f), new Vector2(0.5f, h), new Color(0.3f, 0.3f, 0.32f), 0, false);
                trunk.GetComponent<SpriteRenderer>().sortingOrder = -5;
                var top = Diamond("Tree_" + i + "_Leaves", new Vector2(x, h + 0.6f), 1.6f, new Color(0.62f, 0.66f, 0.7f));
                top.transform.SetParent(trees, true);
                top.GetComponent<SpriteRenderer>().sortingOrder = -4;
                // 3–4 quả táo rải trong tán
                Vector2[] apples = { new Vector2(-0.45f, 0.15f), new Vector2(0.35f, 0.45f), new Vector2(0.15f, -0.4f), new Vector2(-0.1f, 0.75f) };
                for (int a = 0; a < (i % 2 == 0 ? 4 : 3); a++)
                {
                    var apple = Box("Tree_" + i + "_Apple_" + a, zoneGo.transform, new Vector2(x, h + 0.6f) + apples[a], new Vector2(0.22f, 0.22f), red, 0, false);
                    apple.GetComponent<SpriteRenderer>().sortingOrder = -3;
                }
                var flower = Box("Flower_" + i, zoneGo.transform, new Vector2(x + 1.6f, 0.15f), new Vector2(0.25f, 0.25f), new Color(0.95f, 0.3f, 0.3f), 0, false);
                flower.GetComponent<SpriteRenderer>().sortingOrder = -3;
            }

            // ---------- 3. Sói tuyết ----------
            Sign("Sign_3_Wolf", new Vector2(52, 7.6f), 480,
                "3. QUÁI VẬT: SÓI TUYẾT",
                "Mọi đòn của quái đều có <b>báo hiệu ≥ 0,4 s</b>.\n• Sói <b>gầm (rung + dấu !)</b> rồi <b>vồ thẳng</b>.\n• Đúng lúc nó vồ: <b>lăn (K) xuyên qua</b>, rồi <b>chém sau lưng</b> khi nó đang nghỉ.\n• Sói có 3 máu. Bị cắn mất 1 vạch; hết máu thì về điểm lưu.", true);
            Wolf("Enemy_SnowWolf", new Vector2(60, 0f));

            // ---------- 4. Rìu lửa phá băng ----------
            Sign("Sign_4_FireAxe", new Vector2(72, 7.6f), 480,
                "4. NĂNG LỰC ĐÁ ĐỎ: RÌU LỬA",
                "Một <b>tường băng</b> chặn đường. Rìu thường không phá được.\n• Bấm <b>L</b>: Rìu lửa – 2 sát thương, <b>x2 với băng</b>, hồi 1,5 s.\n• Mỗi viên đá vừa là <b>năng lực chiến đấu</b>, vừa <b>mở lối mới</b> trên bản đồ.", true);
            var ice = new GameObject("IceWall");
            ice.layer = groundLayer;
            ice.transform.position = new Vector3(80, 3.5f, 0);
            var iceCol = ice.AddComponent<BoxCollider2D>();
            iceCol.size = new Vector2(1.4f, 7f);
            Box("Block", ice.transform, new Vector2(80, 3.5f), new Vector2(1.4f, 7f), new Color(0.72f, 0.88f, 1f, 0.95f), groundLayer, false)
                .GetComponent<SpriteRenderer>().sortingOrder = 2;
            for (int i = 0; i < 4; i++)
                Box("IceShard_" + i, ice.transform, new Vector2(80 + (i % 2 == 0 ? -0.5f : 0.45f), 1 + i * 1.6f), new Vector2(0.35f, 0.9f), new Color(0.88f, 0.96f, 1f), 0, false)
                    .GetComponent<SpriteRenderer>().sortingOrder = 3;
            var iceD = ice.AddComponent<DemoDamageable>();
            iceD.maxHp = 4; iceD.requiresFire = true; iceD.isIce = true;

            // ---------- 5. Vòng lặp & sắp tới ----------
            Sign("Sign_5_Loop", new Vector2(91, 7.8f), 520,
                "5. VÒNG LẶP CỦA GAME",
                "Bạn vừa chơi trọn một vòng nhỏ:\n<b>Chặt chém → Khám phá (theo mẩu giấy) → Nhận đá (năng lực + hồi màu) → Quay lại mở lối cũ</b>\n\nVòng này lặp lại với 2 viên đá nữa, quái mạnh hơn và 2 trận boss.\nNguyên tắc: <b>màu sắc = tiến trình = năng lực</b>.", true);

            Diamond("Show_Stone_Red", new Vector2(99, 2.2f), 0.7f, new Color(0.95f, 0.18f, 0.15f));
            Diamond("Show_Stone_Yellow", new Vector2(105, 2.2f), 0.7f, new Color(1f, 0.85f, 0.2f));
            Diamond("Show_Stone_Green", new Vector2(111, 2.2f), 0.7f, new Color(0.3f, 0.8f, 0.35f));
            Sign("Label_Red", new Vector2(99, 5.4f), 230, "ĐÁ ĐỎ – Lửa", "Rìu lửa (L)\nTan băng, đốt mạng nhện, soi hang tối.\n<i>Khu 1 – đã chơi</i>");
            Sign("Label_Yellow", new Vector2(105, 5.4f), 230, "ĐÁ VÀNG – Ánh sáng", "Dash trên không (Shift)\nVượt khe xa 7–9 ô.\n<i>Sau Boss 1</i>");
            Sign("Label_Green", new Vector2(111, 5.4f), 230, "ĐÁ XANH – Sự sống", "Gieo hạt (I)\nMọc bệ để leo vách cao.\n<i>Khu 3</i>");

            Sign("Sign_6_Enemies", new Vector2(122, 7.8f), 470, "QUÁI SẮP GẶP",
                "Mỗi khu có quái riêng, mỗi con dạy người chơi một kỹ năng:\nSói tuyết (lao tới) · Nhện băng (rơi từ trần) · Quạ xám (ném đá băng)\nGiáp băng · Dơi hang · Tinh linh sương");
            Showcase("Show_Wolf", Enemies + "SkullWolf/SkullWolf_Animator.controller", new Vector2(117, 0), 1f);
            Showcase("Show_Spider", Enemies + "PixelSpider/PixelSpider_Animator.controller", new Vector2(122, 0), 2f);
            Showcase("Show_Crow", Enemies + "GreyCrow/GreyCrow_Animator.controller", new Vector2(127, 0), 1f);
            Sign("Label_Wolf", new Vector2(117, 3.6f), 170, "Sói tuyết", "Gầm rồi vồ");
            Sign("Label_Spider", new Vector2(122, 3.6f), 170, "Nhện băng", "Rơi từ trần");
            Sign("Label_Crow", new Vector2(127, 3.6f), 170, "Quạ xám", "Ném đá băng");

            Sign("Sign_7_Bosses", new Vector2(136, 7.8f), 420, "BOSS & KẾT THÚC",
                "• <b>Người gác vách băng</b>: giai đoạn 2 khoác giáp, chỉ rìu lửa phá được.\n• <b>Bóng của nhóm phiêu lưu</b>: dùng lại cả 3 năng lực của người chơi.\n• Kết: đặt 3 viên đá, tháo cán rìu trả về gốc cây → thế giới hồi màu.\n\n<b>— Hết demo —</b>");

            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log("[DemoSceneBuilder] Đã dựng " + ScenePath);
        }

        // ================= helpers =================
        static GameObject Box(string name, Transform parent, Vector2 pos, Vector2 size, Color color, int layer, bool solid)
        {
            var go = new GameObject(name);
            if (parent) go.transform.SetParent(parent);
            go.layer = layer;
            go.transform.position = pos;
            go.transform.localScale = new Vector3(size.x, size.y, 1);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = square;
            sr.color = color;
            if (solid) go.AddComponent<BoxCollider2D>();
            return go;
        }

        static GameObject Diamond(string name, Vector2 pos, float size, Color color)
        {
            var go = Box(name, null, pos, new Vector2(size, size), color, 0, false);
            go.transform.rotation = Quaternion.Euler(0, 0, 45);
            go.GetComponent<SpriteRenderer>().sortingOrder = 4;
            return go;
        }

        static void Sign(string name, Vector2 pos, float width, string title, string body, bool checkpoint = false)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            var s = go.AddComponent<DemoSign>();
            s.title = title; s.body = body; s.width = width; s.isCheckpoint = checkpoint;
        }

        static GameObject AnimatedSprite(string name, string controllerPath, Vector2 pos, float scale, Transform parent)
        {
            var go = new GameObject(name);
            if (parent) go.transform.SetParent(parent);
            go.transform.localPosition = parent ? Vector3.zero : (Vector3)pos;
            go.transform.localScale = new Vector3(scale, scale, 1);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 5;
            var ctrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
            var anim = go.AddComponent<Animator>();
            anim.runtimeAnimatorController = ctrl;
            // Frame đầu của state mặc định để thấy được trong Edit mode
            if (ctrl != null && ctrl.animationClips.Length > 0)
            {
                var ac = ctrl as UnityEditor.Animations.AnimatorController;
                var clip = ac != null && ac.layers[0].stateMachine.defaultState != null
                    ? ac.layers[0].stateMachine.defaultState.motion as AnimationClip : ctrl.animationClips[0];
                var keys = AnimationUtility.GetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"));
                if (keys != null && keys.Length > 0) sr.sprite = keys[0].value as UnityEngine.Sprite;
            }
            return go;
        }

        static void Campfire(Vector2 pos)
        {
            var root = new GameObject("Campfire");
            root.transform.position = pos;
            var col = root.AddComponent<BoxCollider2D>();
            col.isTrigger = true; col.size = new Vector2(1.4f, 1.2f); col.offset = new Vector2(0, 0.6f);

            // đá kê + củi chéo (xám)
            for (int i = 0; i < 5; i++)
                Box("Stone_" + i, root.transform, pos + new Vector2(-0.7f + i * 0.35f, 0.08f), new Vector2(0.28f, 0.18f), new Color(0.4f, 0.42f, 0.45f), 0, false)
                    .GetComponent<SpriteRenderer>().sortingOrder = 3;
            var log1 = Box("Log_1", root.transform, pos + new Vector2(0, 0.22f), new Vector2(1.2f, 0.16f), new Color(0.28f, 0.28f, 0.3f), 0, false);
            log1.transform.rotation = Quaternion.Euler(0, 0, 18); log1.GetComponent<SpriteRenderer>().sortingOrder = 4;
            var log2 = Box("Log_2", root.transform, pos + new Vector2(0, 0.22f), new Vector2(1.2f, 0.16f), new Color(0.24f, 0.24f, 0.26f), 0, false);
            log2.transform.rotation = Quaternion.Euler(0, 0, -18); log2.GetComponent<SpriteRenderer>().sortingOrder = 4;

            // 3 lớp lửa: đỏ ngoài, cam giữa, vàng trong (gốc ở đáy để lập lòe hướng lên)
            Color[] cs = { new Color(0.9f, 0.2f, 0.08f), new Color(1f, 0.5f, 0.1f), new Color(1f, 0.85f, 0.3f) };
            Vector2[] sz = { new Vector2(0.7f, 0.95f), new Vector2(0.5f, 0.7f), new Vector2(0.28f, 0.45f) };
            var flames = new Transform[3];
            for (int i = 0; i < 3; i++)
            {
                var pivot = new GameObject("Flame_" + i).transform;
                pivot.SetParent(root.transform); pivot.position = pos + new Vector2(0, 0.25f); pivot.localScale = new Vector3(sz[i].x, sz[i].y, 1);
                var d = new GameObject("Shape");
                d.transform.SetParent(pivot, false);
                d.transform.localPosition = new Vector3(0, 0.5f, 0);
                d.transform.localRotation = Quaternion.Euler(0, 0, 45);
                d.transform.localScale = new Vector3(0.71f, 0.71f, 1);
                var sr = d.AddComponent<SpriteRenderer>(); sr.sprite = square; sr.color = cs[i]; sr.sortingOrder = 5 + i;
                flames[i] = pivot;
            }

            var lightGo = new GameObject("Glow");
            lightGo.transform.SetParent(root.transform); lightGo.transform.position = pos + new Vector2(0, 0.6f);
            var l = lightGo.AddComponent<Light2D>();
            l.lightType = Light2D.LightType.Point;
            l.color = new Color(1f, 0.6f, 0.25f);
            l.pointLightOuterRadius = 4.5f; l.pointLightInnerRadius = 0.5f;
            l.intensity = 1f;

            var cf = root.AddComponent<DemoCampfire>();
            cf.flames = flames; cf.glow = l; cf.emberSprite = square;
        }

        static void Showcase(string name, string controllerPath, Vector2 pos, float scale) => AnimatedSprite(name, controllerPath, pos, scale, null);

        static void Wolf(string name, Vector2 pos)
        {
            var root = new GameObject(name);
            root.transform.position = pos;
            var col = root.AddComponent<BoxCollider2D>();
            col.isTrigger = true; col.size = new Vector2(2.2f, 1.1f); col.offset = new Vector2(0, 0.55f);
            var dmg = root.AddComponent<DemoDamageable>();
            dmg.maxHp = 3;
            var spriteGo = AnimatedSprite("Sprite", Enemies + "SkullWolf/SkullWolf_Animator.controller", Vector2.zero, 1f, root.transform);
            var wolf = root.AddComponent<DemoWolf>();
            wolf.sprite = spriteGo.transform;
        }
    }
}
