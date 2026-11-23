using System.Collections.Generic;
using UnityEngine;

namespace Woodcutter.Demo
{
    /// <summary>
    /// CHỈ DÙNG CHO SCENE DEMO. Vẽ toàn bộ chữ của demo bằng OnGUI với font hệ thống (Segoe UI/Arial)
    /// để hiện đúng dấu tiếng Việt: bảng ghi chú từng đoạn (DemoSign), HUD máu + rìu lửa,
    /// thông báo giữa màn hình, chữ bay (sát thương), gợi ý "E: ..." và khung mẩu giấy.
    /// Bản game thật sẽ dùng UI Toolkit/TextMeshPro + file ngôn ngữ.
    /// </summary>
    public class DemoUI : MonoBehaviour
    {
        static DemoUI inst;
        public static bool HasRedStone;
        public static bool NoteOpen => inst != null && inst.noteText != null;

        struct Pop { public Vector3 pos; public string text; public Color color; public float start, life; }

        readonly List<Pop> pops = new List<Pop>();
        readonly List<DemoSign> signs = new List<DemoSign>();
        string banner; float bannerUntil;
        string prompt; Vector3 promptPos; int promptFrame = -1;
        string noteText;

        Font font;
        GUIStyle signTitle, signBody, popStyle, bannerStyle, hudStyle, noteStyle, promptStyle;
        Texture2D panelTex, noteTex, hpOn, hpOff;
        DemoPlayerHealth hp;
        DemoPlayerCombat combat;

        void Awake()
        {
            inst = this;
            HasRedStone = false;
            font = Font.CreateDynamicFontFromOSFont(new[] { "Segoe UI", "Arial", "Tahoma" }, 20);
            panelTex = Solid(new Color(0.05f, 0.07f, 0.1f, 0.78f));
            noteTex = Solid(new Color(0.93f, 0.88f, 0.76f, 0.97f));
            hpOn = Solid(new Color(0.92f, 0.95f, 1f));
            hpOff = Solid(new Color(0.25f, 0.28f, 0.33f));
        }

        Woodcutter.Player.PlayerInputBroker input;
        readonly List<DemoPickup> pickups = new List<DemoPickup>();

        void Start()
        {
            hp = FindAnyObjectByType<DemoPlayerHealth>();
            combat = FindAnyObjectByType<DemoPlayerCombat>();
            input = FindAnyObjectByType<Woodcutter.Player.PlayerInputBroker>();
            if (input != null) input.OnInteractPressed += HandleInteract;
        }

        void OnDestroy()
        {
            if (input != null) input.OnInteractPressed -= HandleInteract;
            if (inst == this) inst = null;
        }

        /// <summary>
        /// Phím E chỉ xử lý ở đây (một chỗ duy nhất): đang mở mẩu giấy thì đóng; nếu không thì dùng
        /// vật nhặt gần nhất trong tầm. Tránh lỗi nhiều pickup cùng nghe E làm mẩu giấy vừa đóng đã mở lại.
        /// </summary>
        void HandleInteract()
        {
            if (noteText != null) { noteText = null; return; }
            DemoPickup best = null;
            float bestDist = float.MaxValue;
            foreach (var p in pickups)
            {
                if (p == null || !p.CanUse) continue;
                float d = p.DistanceToPlayer;
                if (d < bestDist) { bestDist = d; best = p; }
            }
            if (best != null) best.Use();
        }

        public static void RegisterPickup(DemoPickup p) { if (inst != null && !inst.pickups.Contains(p)) inst.pickups.Add(p); }

        static Texture2D Solid(Color c)
        {
            var t = new Texture2D(1, 1); t.SetPixel(0, 0, c); t.Apply(); return t;
        }

        // ===== API tĩnh cho các script demo khác =====
        public static void Register(DemoSign s) { if (inst != null && !inst.signs.Contains(s)) inst.signs.Add(s); }
        public static void Popup(Vector3 world, string text, Color color, float life = 0.9f)
        {
            if (inst == null) return;
            inst.pops.Add(new Pop { pos = world, text = text, color = color, start = Time.time, life = life });
        }
        public static void Banner(string text, float seconds)
        {
            if (inst == null) return;
            inst.banner = text; inst.bannerUntil = Time.time + seconds;
        }
        public static void Prompt(Vector3 world, string text)
        {
            if (inst == null) return;
            inst.prompt = text; inst.promptPos = world; inst.promptFrame = Time.frameCount;
        }
        public static void ShowNote(string text) { if (inst != null) inst.noteText = text; }
        public static void CloseNote() { if (inst != null) inst.noteText = null; }

        void EnsureStyles()
        {
            if (signTitle != null && Mathf.Approximately(lastScale, Scale)) return;
            lastScale = Scale;
            int S(float px) => Mathf.RoundToInt(px * Scale);
            signTitle = new GUIStyle { font = font, fontSize = S(19), fontStyle = FontStyle.Bold, wordWrap = true, richText = true };
            signTitle.normal.textColor = new Color(0.75f, 0.9f, 1f);
            signBody = new GUIStyle { font = font, fontSize = S(15), wordWrap = true, richText = true };
            signBody.normal.textColor = new Color(0.93f, 0.95f, 0.97f);
            popStyle = new GUIStyle { font = font, fontSize = S(18), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            bannerStyle = new GUIStyle { font = font, fontSize = S(22), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            bannerStyle.normal.textColor = Color.white;
            hudStyle = new GUIStyle { font = font, fontSize = S(16), richText = true };
            hudStyle.normal.textColor = Color.white;
            promptStyle = new GUIStyle { font = font, fontSize = S(16), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            promptStyle.normal.textColor = new Color(1f, 0.95f, 0.6f);
            noteStyle = new GUIStyle { font = font, fontSize = S(18), wordWrap = true, richText = true, padding = new RectOffset(S(28), S(28), S(24), S(24)) };
            noteStyle.normal.textColor = new Color(0.2f, 0.14f, 0.08f);
            noteStyle.normal.background = noteTex;
        }

        float lastScale;
        float Scale => Mathf.Max(0.6f, Screen.height / 720f);

        void OnGUI()
        {
            var cam = Camera.main;
            if (cam == null) return;
            EnsureStyles();
            float s = Scale;

            // --- Bảng ghi chú từng đoạn ---
            foreach (var sign in signs)
            {
                if (sign == null || !sign.isActiveAndEnabled) continue;
                Vector3 sp = cam.WorldToScreenPoint(sign.transform.position);
                if (sp.z < 0 || sp.x < -600 || sp.x > Screen.width + 600) continue;
                float w = sign.width * s;
                float hTitle = signTitle.CalcHeight(new GUIContent(sign.title), w - 24 * s);
                float hBody = signBody.CalcHeight(new GUIContent(sign.body), w - 24 * s);
                var r = new Rect(sp.x - w / 2, Screen.height - sp.y, w, hTitle + hBody + 28 * s);
                GUI.DrawTexture(r, panelTex);
                GUI.Label(new Rect(r.x + 12 * s, r.y + 10 * s, w - 24 * s, hTitle), sign.title, signTitle);
                GUI.Label(new Rect(r.x + 12 * s, r.y + 14 * s + hTitle, w - 24 * s, hBody), sign.body, signBody);
            }

            // --- Chữ bay ---
            for (int i = pops.Count - 1; i >= 0; i--)
            {
                var p = pops[i];
                float k = (Time.time - p.start) / p.life;
                if (k >= 1f) { pops.RemoveAt(i); continue; }
                Vector3 sp = cam.WorldToScreenPoint(p.pos + Vector3.up * k * 0.8f);
                var c = p.color; c.a = 1f - k * k;
                popStyle.normal.textColor = new Color(0, 0, 0, c.a * 0.8f);
                var rr = new Rect(sp.x - 150, Screen.height - sp.y - 15, 300, 30);
                GUI.Label(new Rect(rr.x + 2, rr.y + 2, rr.width, rr.height), p.text, popStyle);
                popStyle.normal.textColor = c;
                GUI.Label(rr, p.text, popStyle);
            }

            // --- Gợi ý E ---
            if (promptFrame >= Time.frameCount - 1 && prompt != null && noteText == null)
            {
                Vector3 sp = cam.WorldToScreenPoint(promptPos);
                GUI.Label(new Rect(sp.x - 150, Screen.height - sp.y - 15, 300, 30), prompt, promptStyle);
            }

            // --- HUD ---
            if (hp != null)
            {
                float x = 16 * s, y = 14 * s;
                GUI.DrawTexture(new Rect(x - 8 * s, y - 6 * s, (hp.maxHp * 22 + 230) * s, 40 * s), panelTex);
                for (int i = 0; i < hp.maxHp; i++)
                    GUI.DrawTexture(new Rect(x + i * 22 * s, y + 4 * s, 16 * s, 20 * s), i < hp.Hp ? hpOn : hpOff);
                string fire = !HasRedStone ? "<color=#888>Rìu lửa: chưa có</color>"
                    : combat != null && combat.FireCooldownLeft > 0 ? "<color=#ffb070>Rìu lửa (L): " + combat.FireCooldownLeft.ToString("0.0") + " s</color>"
                    : "<color=#ff8c3a><b>Rìu lửa (L): sẵn sàng</b></color>";
                GUI.Label(new Rect(x + hp.maxHp * 22 * s + 12 * s, y + 2 * s, 260 * s, 30 * s), fire, hudStyle);
            }
            GUI.Label(new Rect(16 * s, Screen.height - 32 * s, Screen.width, 30 * s),
                "<color=#cfe>A/D</color> chạy   <color=#cfe>Space</color> nhảy   <color=#cfe>J</color> chém (giữ = đòn nặng)   <color=#cfe>K</color> lăn   <color=#cfe>L</color> rìu lửa   <color=#cfe>E</color> tương tác",
                hudStyle);

            // --- Thông báo ---
            if (banner != null && Time.time < bannerUntil)
            {
                var r = new Rect(Screen.width / 2f - 380 * s, 70 * s, 760 * s, 56 * s);
                GUI.DrawTexture(r, panelTex);
                GUI.Label(r, banner, bannerStyle);
            }

            // --- Mẩu giấy ---
            if (noteText != null)
            {
                float w = 620 * s;
                float h = noteStyle.CalcHeight(new GUIContent(noteText + "\n\n<i>(E: gấp lại)</i>"), w);
                GUI.Label(new Rect(Screen.width / 2f - w / 2, Screen.height / 2f - h / 2, w, h), noteText + "\n\n<i>(E: gấp lại)</i>", noteStyle);
            }
        }
    }
}
