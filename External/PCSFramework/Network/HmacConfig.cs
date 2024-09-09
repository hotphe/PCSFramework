using System;
using UnityEngine;

namespace PCS.Network
{
    [Serializable]
    public class HmacConfig
    {
        [field: SerializeField] public string HmacName { get; private set; } = "PCS-Hash";
        [field: SerializeField] public string HmacKey { get; private set; } = "uesdokijuhygnchdfuedakhfgadksagf";
        [field: SerializeField] public string CryptoKey { get; private set; } = "qawsedrftgyhujikolpmjnhbgvfcdxsz";
        [field: SerializeField] public int Timeout { get; private set; } = 30;
        [field: SerializeField] public string BaseURL { get; private set; }
    }
}