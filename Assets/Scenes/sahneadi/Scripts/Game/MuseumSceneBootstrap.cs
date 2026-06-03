using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Argos.AI;
using Argos.Artifacts;
using Argos.Joystick;
using Argos.Player;
using Argos.Portal;
using Argos.Scenarios;
using Argos.UI;
using Argos.Game;

namespace Argos.Game
{
    /// <summary>
    /// MVP'de sahneyi runtime'da prosedürel kurar. Faz 2'de Tilemap eklenince
    /// buildWalls=false yapılabilir; Player/Camera/Canvas/Managers tarafı kalır.
    /// </summary>
    public class MuseumSceneBootstrap : MonoBehaviour
    {
        [Header("Build flags")]
        public bool buildOnStart = true;
        // Faz 2'den itibaren duvar/zemin Tilemap'ten geliyor. Tilemap olmayan sahnede
        // hızlı test için true yapılabilir.
        public bool buildWalls = false;
        // PNG'nin dış kenarlarına otomatik 4 görünmez border. Elle collider çizecekseniz
        // false yapın — sahnede kendi BoxCollider2D / PolygonCollider2D'lerinizi koyun.
        public bool buildInvisibleBorders = true;

        [Header("Optional refs")]
        public ArtifactData[] sampleArtifacts;
        public AIConfig aiConfig;
        public ScenarioData scenario;
        public System.Collections.Generic.List<Argos.NPC.NPCData> suspectsForCaseBoard = new System.Collections.Generic.List<Argos.NPC.NPCData>();
        // Player yön sprite'ları — boş bırakılırsa Resources/Detective_* yüklenir; hepsi de yoksa mavi kareye düşülür.
        public Sprite playerSpriteFront;
        public Sprite playerSpriteBack;
        public Sprite playerSpriteLeft;
        public Sprite playerSpriteRight;

        [Header("Room layout")]
        // PNG arka plan (1448x1086, PPU 72.4) ile yarı boyut: 10 x 7.5.
        public Vector2 roomMin = new Vector2(-10f, -7.5f);
        public Vector2 roomMax = new Vector2(10f, 7.5f);


        private GameObject managersRoot;
        private GameObject player;
        private Camera cam;
        private Canvas canvas;
        private UIManager uiManager;

        void Start()
        {
            if (buildOnStart) BuildScene();
        }

        public void BuildScene()
        {
            BuildManagers();
            BuildBackground();
            DeactivateOldTilemap();
            if (buildInvisibleBorders) BuildInvisibleBorders();
            if (buildWalls) BuildWalls();
            BuildPlayer();
            BuildCamera();
            BuildCanvas();
            WireUp();
            BuildArtifacts();
            BuildOfficeExitTrigger();
        }

        // ---------------------------------------------------------------
        // Ofise ışınlanma bölgesi — müzenin alt-orta sınırı. Trigger uzun:
        // altı -7.5'e (zemin) kadar iner, üstü oyuncunun ulaşabildiği
        // bölgeye (~-5.0) çıkar. Oyuncu alt sınıra dayanınca tetiklenir.
        // ---------------------------------------------------------------
        void BuildOfficeExitTrigger()
        {
            var go = new GameObject("OfficeExitTrigger");
            // Merkez -6.25, yükseklik 2.5 → kutu y ekseninde -7.5 ile -5.0 arası.
            go.transform.position = new Vector3(0f, -6.25f, 0f);
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.5f, 2.5f);
            go.AddComponent<OfficeExitTrigger>();
        }

        // ---------------------------------------------------------------
        // Müze arka plan görseli (Resources/MuseumBackground)
        // ---------------------------------------------------------------
        void BuildBackground()
        {
            var sprite = Resources.Load<Sprite>("MuseumBackground");
            if (sprite == null) return;

            var go = new GameObject("MuseumBackground");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = -10;
            go.transform.position = Vector3.zero;
        }

        // ---------------------------------------------------------------
        // Eski Tilemap (Faz 2 placeholder zemin/duvarlar) — PNG arka plan
        // bunu görsel olarak değiştirdiği için kapatıyoruz. Collider'lar
        // yerine BuildInvisibleBorders manuel sınır kuruyor.
        // ---------------------------------------------------------------
        void DeactivateOldTilemap()
        {
            var tilemapGo = GameObject.Find("MuseumTilemap");
            if (tilemapGo != null) tilemapGo.SetActive(false);
        }

        // ---------------------------------------------------------------
        // Oyuncuyu PNG zemini dışına çıkarmayan görünmez sınır collider'ları.
        // ---------------------------------------------------------------
        void BuildInvisibleBorders()
        {
            var root = new GameObject("InvisibleBorders");
            float thickness = 1f;

            MakeBorder(root.transform, "Border_Top",    new Vector2(0f, 5.6f + thickness),    new Vector2(16.6f, thickness));
            // Üst yüzü -7.5'te olacak şekilde (merkez -8.0): oyuncu zemine (-7.5) kadar inebilir.
            MakeBorder(root.transform, "Border_Bottom", new Vector2(0f, -7.5f - thickness * 0.5f), new Vector2(16.6f, thickness));
            MakeBorder(root.transform, "Border_Left",   new Vector2(-7.8f - thickness, -0.1f), new Vector2(thickness, 12.1f));
            MakeBorder(root.transform, "Border_Right",  new Vector2(7.8f + thickness, -0.5f),  new Vector2(thickness, 12.1f));
        }

        void MakeBorder(Transform parent, string name, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = new Vector3(pos.x, pos.y, 0f);
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            go.AddComponent<BoxCollider2D>();
        }

        // ---------------------------------------------------------------
        // Managers
        // ---------------------------------------------------------------
        void BuildManagers()
        {
            // ManagersInitializer (RuntimeInitializeOnLoadMethod) DDOL manager'ları
            // zaten kurdu. Burası sadece sahne-spesifik konfigürasyonu bağlar.
            if (GameManager.Instance != null) managersRoot = GameManager.Instance.gameObject;
            if (aiConfig != null && AIManager.Instance != null) AIManager.Instance.SetConfig(aiConfig);
            if (scenario != null && GameManager.Instance != null) GameManager.Instance.SetScenario(scenario);
        }

        // ---------------------------------------------------------------
        // Walls (placeholder — Faz 2'de Tilemap'le değiştirilecek)
        // ---------------------------------------------------------------
        void BuildWalls()
        {
            var wallsRoot = new GameObject("PlaceholderWalls");
            float thickness = 0.5f;
            Color wallColor = new Color(0.2f, 0.2f, 0.22f);

            float w = roomMax.x - roomMin.x;
            float h = roomMax.y - roomMin.y;
            Vector2 center = (roomMin + roomMax) * 0.5f;

            MakeWall(wallsRoot.transform, "Wall_Top",    new Vector2(center.x, roomMax.y + thickness * 0.5f), new Vector2(w + thickness * 2f, thickness), wallColor);
            MakeWall(wallsRoot.transform, "Wall_Bottom", new Vector2(center.x, roomMin.y - thickness * 0.5f), new Vector2(w + thickness * 2f, thickness), wallColor);
            MakeWall(wallsRoot.transform, "Wall_Left",   new Vector2(roomMin.x - thickness * 0.5f, center.y), new Vector2(thickness, h), wallColor);
            MakeWall(wallsRoot.transform, "Wall_Right",  new Vector2(roomMax.x + thickness * 0.5f, center.y), new Vector2(thickness, h), wallColor);

            // Zemin (görsel, sadece arka plan)
            var floor = new GameObject("PlaceholderFloor");
            var fsr = floor.AddComponent<SpriteRenderer>();
            fsr.sprite = MakeWhiteSprite();
            fsr.color = new Color(0.78f, 0.78f, 0.74f);
            fsr.sortingOrder = -10;
            floor.transform.position = new Vector3(center.x, center.y, 0f);
            floor.transform.localScale = new Vector3(w, h, 1f);
        }

        void MakeWall(Transform parent, string name, Vector2 pos, Vector2 size, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = new Vector3(pos.x, pos.y, 0f);
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = MakeWhiteSprite();
            sr.color = color;
            sr.sortingOrder = 0;
            go.AddComponent<BoxCollider2D>();
        }

        // ---------------------------------------------------------------
        // Player
        // ---------------------------------------------------------------
        void BuildPlayer()
        {
            // PlayerInventory DDOL → Player GO sahne reload'da yaşıyor.
            var existing = GameObject.FindGameObjectWithTag("Player");
            if (existing != null)
            {
                player = existing;
                player.transform.position = Vector3.zero;
                if (player.GetComponent<PlayerGadget>() == null)
                {
                    var g = player.AddComponent<PlayerGadget>();
                    var a = player.GetComponent<AudioSource>();
                    if (a == null)
                    {
                        a = player.AddComponent<AudioSource>();
                        a.playOnAwake = false;
                        a.clip = null;
                    }
                    g.SetCameraSound(a);
                }
                return;
            }

            player = new GameObject("Player");
            player.tag = "Player";
            player.transform.position = Vector3.zero;

            var sr = player.AddComponent<SpriteRenderer>();
            var front = playerSpriteFront != null ? playerSpriteFront : Resources.Load<Sprite>("Detective_Front");
            var back  = playerSpriteBack  != null ? playerSpriteBack  : Resources.Load<Sprite>("Detective_Back");
            var left  = playerSpriteLeft  != null ? playerSpriteLeft  : Resources.Load<Sprite>("Detective_Left");
            var right = playerSpriteRight != null ? playerSpriteRight : Resources.Load<Sprite>("Detective_Right");
            bool hasSprites = front != null || back != null || left != null || right != null;
            if (hasSprites)
            {
                sr.sprite = front != null ? front : (back ?? left ?? right);
                sr.color = Color.white;
                player.transform.localScale = Vector3.one;
            }
            else
            {
                sr.sprite = MakeWhiteSprite();
                sr.color = new Color(0.25f, 0.55f, 0.95f);
                player.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            }
            sr.sortingOrder = 10;

            var rb = player.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            player.AddComponent<BoxCollider2D>();
            var pc = player.AddComponent<PlayerController>();
            pc.spriteFront = front;
            pc.spriteBack = back;
            pc.spriteLeft = left;
            pc.spriteRight = right;
            player.AddComponent<PlayerInventory>();
            var gadget = player.AddComponent<PlayerGadget>();
            var audio = player.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            audio.clip = null; // Faz 13'te ses placeholder eklenecek.
            gadget.SetCameraSound(audio);
        }

        // ---------------------------------------------------------------
        // Camera
        // ---------------------------------------------------------------
        void BuildCamera()
        {
            cam = Camera.main;
            if (cam != null)
            {
                cam.transform.position = new Vector3(0f, 0f, -10f);
                cam.orthographicSize = 5f;
                cam.backgroundColor = new Color(0.1f, 0.1f, 0.12f);
                var follow = cam.GetComponent<ArgosCameraFollow>();
                if (follow != null)
                {
                    follow.target = player.transform;
                    follow.minBounds = roomMin;
                    follow.maxBounds = roomMax;
                }
                if (cam.GetComponent<AudioListener>() == null)
                    cam.gameObject.AddComponent<AudioListener>();
                return;
            }

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.backgroundColor = new Color(0.1f, 0.1f, 0.12f);
            cam.transform.position = new Vector3(0f, 0f, -10f);
            camGo.AddComponent<AudioListener>();

            var newFollow = camGo.AddComponent<ArgosCameraFollow>();
            newFollow.target = player.transform;
            newFollow.minBounds = roomMin;
            newFollow.maxBounds = roomMax;
        }

        // ---------------------------------------------------------------
        // Canvas + UI (joystick visual, quest box, banners, panels)
        // ---------------------------------------------------------------
        void BuildCanvas()
        {
            var canvasGo = new GameObject("Canvas");
            canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }

            BuildJoystickVisual(canvasGo.transform);
            BuildQuestBox(canvasGo.transform);
            BuildGadgetBanner(canvasGo.transform);
            BuildInteractPrompt(canvasGo.transform);
            BuildFlashOverlay(canvasGo.transform);
            BuildArtifactInspectPanel(canvasGo.transform);
            BuildJournalPanel(canvasGo.transform);
            BuildJournalButton(canvasGo.transform);
            BuildOfficeButton(canvasGo.transform);
            BuildInternalVoicePanel(canvasGo.transform);
            UIBuilders.BuildInterrogationPanel(canvasGo.transform);
            UIBuilders.BuildSuspectSelectionPanel(canvasGo.transform);

            // UIManager singleton'ı Canvas üstüne ekleyelim (sahne-scoped).
            uiManager = canvasGo.AddComponent<UIManager>();
        }

        void BuildJoystickVisual(Transform parent)
        {
            var area = new GameObject("JoystickArea");
            area.transform.SetParent(parent, false);
            var areaRt = area.AddComponent<RectTransform>();
            areaRt.anchorMin = new Vector2(0f, 0f);
            areaRt.anchorMax = new Vector2(0f, 0f);
            areaRt.pivot = new Vector2(0.5f, 0.5f);
            areaRt.anchoredPosition = new Vector2(120f, 120f);
            areaRt.sizeDelta = new Vector2(140f, 140f);
            var areaImg = area.AddComponent<Image>();
            areaImg.color = new Color(1f, 1f, 1f, 0.18f);

            var knob = new GameObject("JoystickKnob");
            knob.transform.SetParent(area.transform, false);
            var knobRt = knob.AddComponent<RectTransform>();
            knobRt.anchorMin = new Vector2(0.5f, 0.5f);
            knobRt.anchorMax = new Vector2(0.5f, 0.5f);
            knobRt.pivot = new Vector2(0.5f, 0.5f);
            knobRt.anchoredPosition = Vector2.zero;
            knobRt.sizeDelta = new Vector2(64f, 64f);
            var knobImg = knob.AddComponent<Image>();
            knobImg.color = new Color(1f, 1f, 1f, 0.55f);

            var joy = area.AddComponent<WasdJoystick>();
            joy.SetKnob(knobRt);
            joy.SetRadius(38f);
        }

        void BuildQuestBox(Transform parent)
        {
            var box = new GameObject("QuestBox");
            box.transform.SetParent(parent, false);
            var rt = box.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-20f, -20f);
            rt.sizeDelta = new Vector2(320f, 80f);
            var bg = box.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.55f);

            var label = new GameObject("Text");
            label.transform.SetParent(box.transform, false);
            var lrt = label.AddComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = new Vector2(10f, 10f);
            lrt.offsetMax = new Vector2(-10f, -10f);
            var tmp = label.AddComponent<TextMeshProUGUI>();
            tmp.text = "Aktif Görev";
            tmp.fontSize = 18f;
            tmp.color = Color.white;

            var qb = box.AddComponent<QuestBox>();
            qb.SetTextField(tmp);
        }

        void BuildGadgetBanner(Transform parent)
        {
            var root = new GameObject("GadgetWarning");
            root.transform.SetParent(parent, false);
            var rt = root.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -30f);
            rt.sizeDelta = new Vector2(420f, 60f);
            var bg = root.AddComponent<Image>();
            bg.color = new Color(0.9f, 0.7f, 0.1f, 0.85f);

            var label = new GameObject("Text");
            label.transform.SetParent(root.transform, false);
            var lrt = label.AddComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = new Vector2(10f, 5f);
            lrt.offsetMax = new Vector2(-10f, -5f);
            var tmp = label.AddComponent<TextMeshProUGUI>();
            tmp.text = "";
            tmp.fontSize = 22f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.black;

            var banner = root.AddComponent<GadgetWarningBanner>();
            banner.Setup(root, tmp);
            root.SetActive(false);
        }

        void BuildInteractPrompt(Transform parent)
        {
            var root = new GameObject("InteractPrompt");
            root.transform.SetParent(parent, false);
            var rt = root.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 30f);
            rt.sizeDelta = new Vector2(280f, 50f);
            var bg = root.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.7f);

            var label = new GameObject("Text");
            label.transform.SetParent(root.transform, false);
            var lrt = label.AddComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = new Vector2(10f, 5f);
            lrt.offsetMax = new Vector2(-10f, -5f);
            var tmp = label.AddComponent<TextMeshProUGUI>();
            tmp.text = "";
            tmp.fontSize = 20f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            var prompt = root.AddComponent<InteractPrompt>();
            prompt.Setup(root, tmp);
            root.SetActive(false);
        }

        private CanvasGroup flashOverlayGroup;

        void BuildFlashOverlay(Transform parent)
        {
            var go = new GameObject("FlashOverlay");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var img = go.AddComponent<Image>();
            img.color = Color.white;
            var cg = go.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.blocksRaycasts = false;
            flashOverlayGroup = cg;
        }

        void BuildArtifactInspectPanel(Transform parent)
        {
            var root = new GameObject("ArtifactInspectPanel");
            root.transform.SetParent(parent, false);
            var rt = root.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(640f, 480f);
            var bg = root.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.88f);

            var nameLbl = CreateText(root.transform, "Name", "", new Vector2(0, 200), new Vector2(600, 60), 32, TextAlignmentOptions.Center);
            var periodLbl = CreateText(root.transform, "Period", "", new Vector2(0, 150), new Vector2(600, 30), 18, TextAlignmentOptions.Center);
            periodLbl.color = new Color(0.75f, 0.75f, 0.75f);
            var descLbl = CreateText(root.transform, "Description", "", new Vector2(0, 0), new Vector2(580, 220), 18, TextAlignmentOptions.TopLeft);

            var photoBtn = CreateButton(root.transform, "PhotoBtn", "Fotoğraf Çek", new Vector2(-150, -200), new Vector2(220, 60));
            var closeBtn = CreateButton(root.transform, "CloseBtn", "Kapat", new Vector2(150, -200), new Vector2(220, 60));

            var inspect = root.AddComponent<ArtifactInspectUI>();
            inspect.Setup(root, nameLbl, periodLbl, descLbl, photoBtn, closeBtn);
            inspect.OnTakePhoto = (data) =>
            {
                if (player != null)
                {
                    var gadget = player.GetComponent<PlayerGadget>();
                    if (gadget != null) gadget.TakePhoto(data);
                }
                inspect.Hide();
            };
            root.SetActive(false);
        }

        void BuildInternalVoicePanel(Transform parent)
        {
            var root = new GameObject("InternalVoicePanel");
            root.transform.SetParent(parent, false);
            var rt = root.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 110f);
            rt.sizeDelta = new Vector2(700f, 140f);
            var bg = root.AddComponent<Image>();
            bg.color = new Color(0.18f, 0.14f, 0.1f, 0.92f);

            // Karaca portresi (placeholder gri kare, solda)
            var portrait = new GameObject("Portrait");
            portrait.transform.SetParent(root.transform, false);
            var prt = portrait.AddComponent<RectTransform>();
            prt.anchorMin = new Vector2(0f, 0.5f);
            prt.anchorMax = new Vector2(0f, 0.5f);
            prt.pivot = new Vector2(0f, 0.5f);
            prt.anchoredPosition = new Vector2(12f, 0f);
            prt.sizeDelta = new Vector2(100f, 100f);
            var pImg = portrait.AddComponent<Image>();
            pImg.color = new Color(0.35f, 0.32f, 0.28f);

            // Konuşma metni (sağda)
            var labelGo = new GameObject("Text");
            labelGo.transform.SetParent(root.transform, false);
            var lrt = labelGo.AddComponent<RectTransform>();
            lrt.anchorMin = new Vector2(0f, 0f);
            lrt.anchorMax = new Vector2(1f, 1f);
            lrt.offsetMin = new Vector2(128f, 12f);
            lrt.offsetMax = new Vector2(-16f, -12f);
            var tmp = labelGo.AddComponent<TextMeshProUGUI>();
            tmp.text = "";
            tmp.fontSize = 20;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.color = new Color(0.95f, 0.92f, 0.84f);
            tmp.textWrappingMode = TextWrappingModes.Normal;

            var voice = root.AddComponent<InternalVoiceUI>();
            voice.Setup(root, tmp);
            root.SetActive(false);
        }

        void BuildJournalPanel(Transform parent)
        {
            var root = new GameObject("JournalPanel");
            root.transform.SetParent(parent, false);
            var rt = root.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(720f, 520f);
            var bg = root.AddComponent<Image>();
            bg.color = new Color(0.92f, 0.88f, 0.78f, 0.96f);

            var header = CreateText(root.transform, "Header", "Defter", new Vector2(0, 220), new Vector2(680, 50), 28, TextAlignmentOptions.Center);
            header.color = new Color(0.2f, 0.15f, 0.1f);

            var contentGo = new GameObject("Content");
            contentGo.transform.SetParent(root.transform, false);
            var contentRt = contentGo.AddComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0.5f, 0.5f);
            contentRt.anchorMax = new Vector2(0.5f, 0.5f);
            contentRt.pivot = new Vector2(0.5f, 1f);
            contentRt.anchoredPosition = new Vector2(0f, 180f);
            contentRt.sizeDelta = new Vector2(680f, 360f);
            var vlg = contentGo.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8f;
            vlg.padding = new RectOffset(8, 8, 8, 8);
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;
            vlg.childAlignment = TextAnchor.UpperLeft;

            var closeBtn = CreateButton(root.transform, "CloseBtn", "Kapat", new Vector2(280, -220), new Vector2(140, 50));
            closeBtn.onClick.AddListener(() => uiManager?.journal?.Hide());

            var journal = root.AddComponent<JournalUI>();
            journal.Setup(root, contentRt);
            root.SetActive(false);
        }

        void BuildJournalButton(Transform parent)
        {
            var go = new GameObject("JournalButton");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(1f, 0f);
            rt.anchoredPosition = new Vector2(-20f, 20f);
            rt.sizeDelta = new Vector2(140f, 60f);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.55f, 0.4f, 0.25f, 0.9f);
            var btn = go.AddComponent<Button>();
            btn.onClick.AddListener(() => uiManager?.OpenJournal());

            var lblGo = new GameObject("Label");
            lblGo.transform.SetParent(go.transform, false);
            var lrt = lblGo.AddComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero;
            lrt.offsetMax = Vector2.zero;
            var ltmp = lblGo.AddComponent<TextMeshProUGUI>();
            ltmp.text = "Defter";
            ltmp.fontSize = 20;
            ltmp.alignment = TextAlignmentOptions.Center;
            ltmp.color = Color.white;
        }

        void BuildOfficeButton(Transform parent)
        {
            var go = new GameObject("OfficeButton");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(1f, 0f);
            rt.anchoredPosition = new Vector2(-20f, 90f);
            rt.sizeDelta = new Vector2(140f, 60f);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.55f, 0.4f, 0.25f, 0.9f);
            var btn = go.AddComponent<Button>();
            btn.onClick.AddListener(() => SceneManager.LoadScene(Scenes.Office));

            var lblGo = new GameObject("Label");
            lblGo.transform.SetParent(go.transform, false);
            var lrt = lblGo.AddComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero;
            lrt.offsetMax = Vector2.zero;
            var ltmp = lblGo.AddComponent<TextMeshProUGUI>();
            ltmp.text = "Ofise Dön";
            ltmp.fontSize = 20;
            ltmp.alignment = TextAlignmentOptions.Center;
            ltmp.color = Color.white;
        }

        // ---------------------------------------------------------------
        // Artifacts
        // ---------------------------------------------------------------
        void BuildArtifacts()
        {
            if (sampleArtifacts == null || sampleArtifacts.Length == 0) return;

            // Müze vitrin koordinatları (Halid'in atamasına göre):
            //  [0] Eser_A → "medusa başlı kolye"      → sağ üst
            //  [1] Eser_B → "hitit güneş kursu"       → sol üst
            //  [2] Eser_C → "boğazköy tunç tableti"   → sol alt (portal/ışınlanma nesnesi)
            Vector3[] positions =
            {
                new Vector3( 4.78f,  3.14f, 0f),
                new Vector3(-4.78f,  3.14f, 0f),
                new Vector3(-4.78f, -3.14f, 0f),
            };

            var root = new GameObject("Artifacts");

            // Eserlerin arkasına konan müze vitrini (Resources/ArtifactDisplayCase).
            var displayCaseSprite = Resources.Load<Sprite>("ArtifactDisplayCase");

            for (int i = 0; i < sampleArtifacts.Length && i < positions.Length; i++)
            {
                var data = sampleArtifacts[i];
                if (data == null) continue;

                BuildDisplayCase(root.transform, displayCaseSprite, positions[i]);

                var go = new GameObject("Artifact_" + (string.IsNullOrEmpty(data.artifactName) ? data.name : data.artifactName));
                go.transform.SetParent(root.transform);
                go.transform.position = positions[i];
                go.transform.localScale = new Vector3(0.6f, 0.6f, 1f);

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = MakeWhiteSprite();
                sr.color = ArtifactColor(data);
                sr.sortingOrder = 8;

                var col = go.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                col.size = new Vector2(3f, 3f);

                var interactable = go.AddComponent<ArtifactInteractable>();
                interactable.data = data;
            }
        }

        // Eserin arkasına müze vitrini sprite'ı koyar. Vitrin ~3.4 birim
        // yüksekliğe ölçeklenir; sortingOrder eserin (8) altında, arka planın
        // (-10) üstünde kalır ki eser vitrinin kadifeli ortasında görünsün.
        void BuildDisplayCase(Transform parent, Sprite sprite, Vector3 pos)
        {
            if (sprite == null) return;

            var go = new GameObject("DisplayCase");
            go.transform.SetParent(parent);
            go.transform.position = pos;

            float spriteHeight = sprite.bounds.size.y;       // birim cinsinden (PPU'ya bağlı)
            float scale = spriteHeight > 0.001f ? 3.4f / spriteHeight : 1f;
            go.transform.localScale = new Vector3(scale, scale, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 5;
        }

        Color ArtifactColor(ArtifactData data)
        {
            if (data.isPortalTrigger) return new Color(0.75f, 0.45f, 0.9f);  // anomali rengi
            if (data.isEvidenceTarget) return new Color(0.95f, 0.85f, 0.2f); // parlak sarı
            return new Color(0.95f, 0.7f, 0.3f);                              // turuncu
        }

        // ---------------------------------------------------------------
        // Wiring
        // ---------------------------------------------------------------
        void WireUp()
        {
            if (uiManager != null)
            {
                uiManager.questBox = canvas.GetComponentInChildren<QuestBox>(true);
                uiManager.gadgetWarning = canvas.GetComponentInChildren<GadgetWarningBanner>(true);
                uiManager.interactPrompt = canvas.GetComponentInChildren<InteractPrompt>(true);
                uiManager.artifactInspect = canvas.GetComponentInChildren<ArtifactInspectUI>(true);
                uiManager.journal = canvas.GetComponentInChildren<JournalUI>(true);
                uiManager.internalVoice = canvas.GetComponentInChildren<InternalVoiceUI>(true);
                uiManager.interrogation = canvas.GetComponentInChildren<InterrogationUI>(true);
                uiManager.suspectSelection = canvas.GetComponentInChildren<SuspectSelectionUI>(true);
                if (uiManager.suspectSelection != null && suspectsForCaseBoard != null && suspectsForCaseBoard.Count > 0)
                    uiManager.suspectSelection.SetSuspects(suspectsForCaseBoard);
            }
            if (QuestManager.Instance != null && uiManager != null && uiManager.questBox != null)
                QuestManager.Instance.SetQuestBox(uiManager.questBox);

            if (player != null && flashOverlayGroup != null)
            {
                var gadget = player.GetComponent<PlayerGadget>();
                if (gadget != null) gadget.SetFlashOverlay(flashOverlayGroup);
            }
        }

        // ---------------------------------------------------------------
        // UI element helpers
        // ---------------------------------------------------------------
        static TMP_Text CreateText(Transform parent, string name, string text, Vector2 anchoredPos, Vector2 size, float fontSize, TextAlignmentOptions align)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = align;
            tmp.color = Color.white;
            return tmp;
        }

        static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPos, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            var img = go.AddComponent<Image>();
            img.color = new Color(0.3f, 0.4f, 0.7f, 0.85f);
            var btn = go.AddComponent<Button>();

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(go.transform, false);
            var lrt = labelGo.AddComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero;
            lrt.offsetMax = Vector2.zero;
            var ltmp = labelGo.AddComponent<TextMeshProUGUI>();
            ltmp.text = label;
            ltmp.fontSize = 20;
            ltmp.alignment = TextAlignmentOptions.Center;
            ltmp.color = Color.white;
            return btn;
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------
        static Sprite cachedWhite;
        static Sprite MakeWhiteSprite()
        {
            if (cachedWhite != null) return cachedWhite;
            var tex = Texture2D.whiteTexture;
            cachedWhite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), tex.width);
            return cachedWhite;
        }
    }
}
