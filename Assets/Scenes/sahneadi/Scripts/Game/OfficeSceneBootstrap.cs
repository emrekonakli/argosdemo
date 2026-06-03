using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Argos.AI;
using Argos.Player;
using Argos.Scenarios;
using Argos.UI;
using Argos.Game;

namespace Argos.Game
{
    public class OfficeSceneBootstrap : MonoBehaviour
    {
        [Header("Room layout")]
        public Vector2 roomMin = new Vector2(-8f, -5f);
        public Vector2 roomMax = new Vector2(8f, 5f);

        [Header("Optional refs")]
        public AIConfig aiConfig;
        public ScenarioData scenario;
        public System.Collections.Generic.List<Argos.NPC.NPCData> suspectsForCaseBoard = new System.Collections.Generic.List<Argos.NPC.NPCData>();

        private GameObject player;
        private Camera cam;
        private Canvas canvas;
        private UIManager uiManager;
        private GameObject museumButtonGo;
        private GameObject newspaperGo;
        private GameObject caseBoardGo;
        private bool newspaperMoved;

        void Start()
        {
            BuildScene();

            if (aiConfig != null && AIManager.Instance != null) AIManager.Instance.SetConfig(aiConfig);
            if (scenario != null && GameManager.Instance != null && GameManager.Instance.CurrentScenario == null)
                GameManager.Instance.SetScenario(scenario);

            if (GameManager.PendingNewspaperEnding)
            {
                GameManager.PendingNewspaperEnding = false;
                bool isCorrect = GameManager.Instance != null && GameManager.Instance.CaseSolved;
                if (uiManager != null && uiManager.newspaper != null)
                    uiManager.newspaper.Show(true, isCorrect);
            }
        }

        void BuildScene()
        {
            BuildManagers();
            BuildBackground();
            BuildPlayer();
            BuildCamera();
            BuildCanvas();
            BuildDoor();
            BuildMuseumEntryTrigger();
            BuildNewspaper();
            BuildCaseBoard();
            WireUp();
        }

        void BuildManagers()
        {
            if (GameManager.Instance != null) { }
            if (AIManager.Instance != null) { }
        }

        void BuildBackground()
        {
            var sprite = Resources.Load<Sprite>("OfficeBackground");
            if (sprite == null) return;

            var go = new GameObject("OfficeBackground");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = -10;
            go.transform.position = Vector3.zero;
        }

        void BuildPlayer()
        {
            var existing = GameObject.FindGameObjectWithTag("Player");
            if (existing != null)
            {
                player = existing;
                player.transform.position = new Vector3(-2f, -2f, 0f);
                return;
            }

            player = new GameObject("Player");
            player.tag = "Player";
            player.transform.position = new Vector3(-2f, -2f, 0f);

            var sr = player.AddComponent<SpriteRenderer>();
            var front = Resources.Load<Sprite>("Detective_Front");
            var back = Resources.Load<Sprite>("Detective_Back");
            var left = Resources.Load<Sprite>("Detective_Left");
            var right = Resources.Load<Sprite>("Detective_Right");
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
        }

        void BuildCamera()
        {
            cam = Camera.main;
            if (cam != null)
            {
                cam.transform.position = new Vector3(0f, 0f, -10f);
                cam.orthographicSize = 5f;
                cam.backgroundColor = new Color(0.12f, 0.1f, 0.08f);
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
            cam.backgroundColor = new Color(0.12f, 0.1f, 0.08f);
            cam.transform.position = new Vector3(0f, 0f, -10f);
            camGo.AddComponent<AudioListener>();

            var newFollow = camGo.AddComponent<ArgosCameraFollow>();
            newFollow.target = player.transform;
            newFollow.minBounds = roomMin;
            newFollow.maxBounds = roomMax;
        }

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

            BuildInteractPrompt(canvasGo.transform);
            UIBuilders.BuildInterrogationPanel(canvasGo.transform);
            UIBuilders.BuildSuspectSelectionPanel(canvasGo.transform);
            UIBuilders.BuildCaseBoardPanel(canvasGo.transform);
            UIBuilders.BuildNewspaperPanel(canvasGo.transform);
            BuildMuseumButton(canvasGo.transform);

            uiManager = canvasGo.AddComponent<UIManager>();
        }

        // ---------------------------------------------------------------
        // Kapı (ofis duvarında, sağ üst köşe)
        // ---------------------------------------------------------------
        void BuildDoor()
        {
            // Kapı çerçevesi
            var door = new GameObject("Door");
            door.transform.position = new Vector3(6.3f, 2.81f, 0f);
            door.transform.localScale = new Vector3(0.375f, 0.75f, 1f);
            var doorSr = door.AddComponent<SpriteRenderer>();
            doorSr.sprite = MakeWhiteSprite();
            doorSr.color = new Color(0.35f, 0.22f, 0.12f, 0f);
            doorSr.sortingOrder = 2;

            // Kapı kolu
            var handle = new GameObject("Handle");
            handle.transform.SetParent(door.transform, false);
            handle.transform.localPosition = new Vector3(-0.3f, 0f, 0f);
            handle.transform.localScale = new Vector3(0.08f, 0.15f, 1f);
            var handleSr = handle.AddComponent<SpriteRenderer>();
            handleSr.sprite = MakeWhiteSprite();
            handleSr.color = new Color(0.85f, 0.75f, 0.3f, 0f);
            handleSr.sortingOrder = 3;

            // Posta deliği
            var mailSlot = new GameObject("MailSlot");
            mailSlot.transform.SetParent(door.transform, false);
            mailSlot.transform.localPosition = new Vector3(0f, -0.2f, 0f);
            mailSlot.transform.localScale = new Vector3(0.4f, 0.04f, 1f);
            var slotSr = mailSlot.AddComponent<SpriteRenderer>();
            slotSr.sprite = MakeWhiteSprite();
            slotSr.color = new Color(0.15f, 0.1f, 0.05f, 0f);
            slotSr.sortingOrder = 3;
        }

        // ---------------------------------------------------------------
        // Müzeye ışınlanma bölgesi — oyuncu kapı konumuna (6.26, 2.51)
        // gelince MuseumScene yüklenir. MuseumButton ile aynı kapı, aynı
        // CaseAccepted koşulu (trigger içinde kontrol edilir).
        // ---------------------------------------------------------------
        void BuildMuseumEntryTrigger()
        {
            var go = new GameObject("MuseumEntryTrigger");
            go.transform.position = new Vector3(6.26f, 2.51f, 0f);
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.375f, 0.375f);
            go.AddComponent<MuseumEntryTrigger>();
        }

        // ---------------------------------------------------------------
        // Gazete (kapının önünde, yerde hazır bekliyor)
        // ---------------------------------------------------------------
        void BuildNewspaper()
        {
            var doorPos = new Vector3(roomMax.x - 1.5f, 1f, 0f);
            var pos = new Vector3(doorPos.x, doorPos.y - 3.5f, 0f);

            var go = new GameObject("Newspaper");
            newspaperGo = go;
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.8f, 0.5f, 1f);
            go.transform.rotation = Quaternion.Euler(0f, 0f, 3f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = MakeWhiteSprite();
            sr.color = new Color(0.92f, 0.88f, 0.75f);
            sr.sortingOrder = 8;

            var lbl = new GameObject("Label");
            lbl.transform.SetParent(go.transform, false);
            lbl.transform.localPosition = Vector3.zero;
            lbl.transform.localScale = new Vector3(
                1f / go.transform.localScale.x,
                1f / go.transform.localScale.y, 1f);
            var tm = lbl.AddComponent<TextMesh>();
            tm.text = "GAZETE";
            tm.fontSize = 40;
            tm.characterSize = 0.04f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = new Color(0.2f, 0.15f, 0.1f);
            var mr = lbl.GetComponent<MeshRenderer>();
            if (mr != null) mr.sortingOrder = 9;

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(2.5f, 3f);
            go.AddComponent<NewspaperInteractable>();
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

        void BuildMuseumButton(Transform parent)
        {
            var go = new GameObject("MuseumButton");
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
            btn.onClick.AddListener(() => SceneManager.LoadScene(Scenes.Museum));

            var lblGo = new GameObject("Label");
            lblGo.transform.SetParent(go.transform, false);
            var lrt = lblGo.AddComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero;
            lrt.offsetMax = Vector2.zero;
            var ltmp = lblGo.AddComponent<TextMeshProUGUI>();
            ltmp.text = "Müzeye Git";
            ltmp.fontSize = 20;
            ltmp.alignment = TextAlignmentOptions.Center;
            ltmp.color = Color.white;

            museumButtonGo = go;
            bool caseActive = GameManager.Instance != null && GameManager.Instance.CaseAccepted;
            go.SetActive(caseActive);
        }

        void Update()
        {
            if (GameManager.Instance == null) return;

            bool caseActive = GameManager.Instance.CaseAccepted;
            bool modalOpen = uiManager != null && uiManager.IsAnyModalOpen;

            if (museumButtonGo != null)
                museumButtonGo.SetActive(caseActive && !modalOpen);

            if (!caseActive) return;

            if (!newspaperMoved && newspaperGo != null && caseBoardGo != null)
            {
                newspaperMoved = true;
                var interactable = newspaperGo.GetComponent<NewspaperInteractable>();
                if (interactable != null) Destroy(interactable);
                var col = newspaperGo.GetComponent<Collider2D>();
                if (col != null) Destroy(col);

                newspaperGo.transform.position = new Vector3(3f, 3f, 0f);
                newspaperGo.transform.rotation = Quaternion.Euler(0f, 0f, -2f);
                newspaperGo.transform.localScale = new Vector3(0.5f, 0.35f, 1f);

                var sr = newspaperGo.GetComponent<SpriteRenderer>();
                if (sr != null) sr.sortingOrder = 5;
            }
        }

        void BuildCaseBoard()
        {
            var go = new GameObject("CaseBoard");
            caseBoardGo = go;
            go.transform.position = new Vector3(3.74f, 3.79f, 0f);
            go.transform.localScale = new Vector3(3f, 1.8f, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = MakeWhiteSprite();
            sr.color = new Color(0.78f, 0.62f, 0.38f, 0f);
            sr.sortingOrder = 4;

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1.6f, 2.5f);

            go.AddComponent<CaseBoardInteractable>();

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(go.transform, false);
            labelGo.transform.localPosition = Vector3.zero;
            labelGo.transform.localScale = new Vector3(1f / go.transform.localScale.x, 1f / go.transform.localScale.y, 1f);
            var tmLabel = labelGo.AddComponent<TextMesh>();
            tmLabel.text = "DAVA PANOSU";
            tmLabel.fontSize = 60;
            tmLabel.characterSize = 0.03f;
            tmLabel.anchor = TextAnchor.MiddleCenter;
            tmLabel.alignment = TextAlignment.Center;
            tmLabel.color = new Color(0.15f, 0.1f, 0.05f, 0f);
            var mr = labelGo.GetComponent<MeshRenderer>();
            if (mr != null) mr.sortingOrder = 5;
        }

        void WireUp()
        {
            if (uiManager != null)
            {
                uiManager.interactPrompt = canvas.GetComponentInChildren<InteractPrompt>(true);
                uiManager.interrogation = canvas.GetComponentInChildren<InterrogationUI>(true);
                uiManager.suspectSelection = canvas.GetComponentInChildren<SuspectSelectionUI>(true);
                uiManager.caseBoard = canvas.GetComponentInChildren<CaseBoardUI>(true);
                uiManager.newspaper = canvas.GetComponentInChildren<NewspaperUI>(true);
                if (uiManager.suspectSelection != null && suspectsForCaseBoard != null && suspectsForCaseBoard.Count > 0)
                    uiManager.suspectSelection.SetSuspects(suspectsForCaseBoard);
            }

            if (QuestManager.Instance != null)
                QuestManager.Instance.SetQuestBox(null);

            if (player != null)
            {
                var gadget = player.GetComponent<PlayerGadget>();
                if (gadget != null) gadget.SetFlashOverlay(null);
            }
        }

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
