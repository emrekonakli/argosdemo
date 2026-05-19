using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Argos.AI
{
    [Serializable]
    public class ChatMessage
    {
        public string role;
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
                Destroy(gameObject);
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
                return fallback != null ? fallback.GetRandomResponse() : "...";
            }

            // TODO Faz 8: gerçek API çağrısı (OpenAI / Anthropic).
            await Task.Yield();
            return fallback != null ? fallback.GetRandomResponse() : "...";
        }
    }
}
