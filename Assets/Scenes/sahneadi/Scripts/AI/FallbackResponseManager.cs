using System.Collections.Generic;
using UnityEngine;

namespace Argos.AI
{
    public class FallbackResponseManager : MonoBehaviour
    {
        private readonly List<string> cannedResponses = new List<string>
        {
            "Bu konuda fazla bir şey söyleyemem.",
            "Hatırlamıyorum.",
            "Belki haklısın, belki değil.",
            "Sana güvenebilir miyim, bilmiyorum.",
            "Bunu duymak hoşuma gitmedi.",
            "Konuyu değiştirebilir miyiz?",
            "Daha somut bir şey göster bana.",
            "Ben oradaydım, evet, ama hiçbir şey görmedim.",
            "Sözlerine dikkat et.",
            "Bu mesele beni aşıyor.",
            "Şu anda konuşmak istemiyorum.",
            "Bilmem, biri başka biriyle karıştırıyor olmalı.",
            "Sen kendi yolunu çiz.",
            "Çok soru soruyorsun.",
            "Yeterince konuştuk.",
            "Bu konuda susmayı tercih ederim.",
            "İnan bana, bilseydim söylerdim.",
            "Bunu bilmek istemezsin.",
            "Belki o gece oradaydım, belki değildim.",
            "Sen ne istersen düşünebilirsin.",
            "Bu sorunun cevabını veremem.",
            "Bunu duyduğuma sevinmedim.",
            "Hatırlamak istediğim biri değil.",
            "Bu konuda sessiz kalmalıyım.",
            "Bir kanıtın varsa göster.",
            "Bana inanmak zorunda değilsin.",
            "Konuşmak bir şeyi değiştirmez.",
            "Senin yerinde olsam bu işten uzak dururdum.",
            "Bu kadar yeter.",
            "Başka soru?"
        };

        public string GetRandomResponse()
        {
            if (cannedResponses.Count == 0) return "...";
            return cannedResponses[Random.Range(0, cannedResponses.Count)];
        }
    }
}
