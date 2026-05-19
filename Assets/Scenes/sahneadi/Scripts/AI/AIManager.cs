using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Argos.AI
{
    [Serializable]
    public class ChatMessage
    {
        public string role;     // "Sen" (oyuncu) ya da NPC adı
        public string content;
    }

    public class AIManager : MonoBehaviour
    {
        public static AIManager Instance { get; private set; }

        [SerializeField] private AIConfig config;
        [SerializeField] private FallbackResponseManager fallback;

        public AIConfig Config => config;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void SetConfig(AIConfig cfg) => config = cfg;
        public void SetFallback(FallbackResponseManager fb) => fallback = fb;

        public async Task<string> SendChat(string systemPrompt, List<ChatMessage> history, string userMessage)
        {
            if (config == null || string.IsNullOrEmpty(config.apiKey))
            {
                await Task.Yield();
                return FallbackOrEllipsis();
            }

            try
            {
                return config.provider switch
                {
                    AIProvider.OpenAI => await SendOpenAI(systemPrompt, history, userMessage),
                    AIProvider.Anthropic => await SendAnthropic(systemPrompt, history, userMessage),
                    _ => FallbackOrEllipsis(),
                };
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[AI] API çağrısı başarısız ({config.provider}): {e.Message}. Fallback'a düşülüyor.");
                return FallbackOrEllipsis();
            }
        }

        string FallbackOrEllipsis() => fallback != null ? fallback.GetRandomResponse() : "...";

        // ---------------------------------------------------------------
        // OpenAI
        // ---------------------------------------------------------------
        async Task<string> SendOpenAI(string systemPrompt, List<ChatMessage> history, string userMessage)
        {
            var messages = new List<OAMessage>();
            messages.Add(new OAMessage { role = "system", content = systemPrompt });
            foreach (var h in history)
                messages.Add(new OAMessage { role = h.role == "Sen" ? "user" : "assistant", content = h.content });
            messages.Add(new OAMessage { role = "user", content = userMessage });

            var body = new OARequest
            {
                model = config.modelName,
                messages = messages.ToArray(),
                max_tokens = config.maxTokens,
                temperature = config.temperature,
            };
            string json = JsonUtility.ToJson(body);
            string raw = await PostJson(
                "https://api.openai.com/v1/chat/completions",
                json,
                ("Authorization", "Bearer " + config.apiKey));

            var resp = JsonUtility.FromJson<OAResponse>(raw);
            if (resp?.choices == null || resp.choices.Length == 0 || resp.choices[0].message == null)
                throw new Exception("OpenAI yanıtı boş.");
            return resp.choices[0].message.content;
        }

        [Serializable] class OAMessage { public string role; public string content; }
        [Serializable] class OARequest
        {
            public string model;
            public OAMessage[] messages;
            public int max_tokens;
            public float temperature;
        }
        [Serializable] class OAChoice { public OAMessage message; }
        [Serializable] class OAResponse { public OAChoice[] choices; }

        // ---------------------------------------------------------------
        // Anthropic (Claude)
        // ---------------------------------------------------------------
        async Task<string> SendAnthropic(string systemPrompt, List<ChatMessage> history, string userMessage)
        {
            var messages = new List<OAMessage>();
            foreach (var h in history)
                messages.Add(new OAMessage { role = h.role == "Sen" ? "user" : "assistant", content = h.content });
            messages.Add(new OAMessage { role = "user", content = userMessage });

            var body = new AntRequest
            {
                model = config.modelName,
                messages = messages.ToArray(),
                max_tokens = config.maxTokens,
                temperature = config.temperature,
                system = systemPrompt,
            };
            string json = JsonUtility.ToJson(body);
            string raw = await PostJson(
                "https://api.anthropic.com/v1/messages",
                json,
                ("x-api-key", config.apiKey),
                ("anthropic-version", "2023-06-01"));

            var resp = JsonUtility.FromJson<AntResponse>(raw);
            if (resp?.content == null || resp.content.Length == 0)
                throw new Exception("Anthropic yanıtı boş.");
            var sb = new StringBuilder();
            foreach (var block in resp.content)
                if (block != null && block.type == "text") sb.Append(block.text);
            return sb.ToString();
        }

        [Serializable] class AntRequest
        {
            public string model;
            public OAMessage[] messages;
            public int max_tokens;
            public float temperature;
            public string system;
        }
        [Serializable] class AntBlock { public string type; public string text; }
        [Serializable] class AntResponse { public AntBlock[] content; }

        // ---------------------------------------------------------------
        // UnityWebRequest POST helper
        // ---------------------------------------------------------------
        async Task<string> PostJson(string url, string json, params (string key, string value)[] headers)
        {
            using (var req = new UnityWebRequest(url, "POST"))
            {
                byte[] payload = Encoding.UTF8.GetBytes(json);
                req.uploadHandler = new UploadHandlerRaw(payload);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                foreach (var h in headers) req.SetRequestHeader(h.key, h.value);
                req.timeout = 30;

                var op = req.SendWebRequest();
                while (!op.isDone) await Task.Yield();

                if (req.result != UnityWebRequest.Result.Success)
                    throw new Exception($"HTTP {req.responseCode}: {req.error}");

                return req.downloadHandler.text;
            }
        }
    }
}
