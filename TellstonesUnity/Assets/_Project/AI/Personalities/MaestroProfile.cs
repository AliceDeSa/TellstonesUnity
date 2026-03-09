using System;
using UnityEngine;

namespace Tellstones.AI.Personalities
{
    [Serializable]
    public struct MaestroPhrases
    {
        public string[] winPoint;
        public string[] losePoint;
        public string[] challenge;
        public string[] boast;
        public string[] swap;
        public string[] peek;
        public string[] surprised;
        public string[] confident;
        public string[] frustrated;
    }

    [Serializable]
    public struct PersonalityModifiers
    {
        public float place;
        public float flip;
        public float swap;
        public float peek;
        public float challenge;
        public float boast;
    }

    [Serializable]
    public struct SpecialBehaviors
    {
        public bool earlyChallenge;
        public bool frequentSwaps;
        public bool defensivePlay;
        public bool bluffMaster;
    }

    /// <summary>
    /// Prefab de dados que permite configurar a personalidade dos bots direto no Inspector da Unity.
    /// Muito superior a arquivos JSON hardcoded em Typescript.
    /// </summary>
    [CreateAssetMenu(fileName = "NewMaestroProfile", menuName = "Tellstones/AI/MaestroProfile")]
    public class MaestroProfile : ScriptableObject
    {
        public string id;
        public string profileName;
        public string title;
        [TextArea(3, 5)] public string description;

        // Avatar em 2D/UI (se houver) ou referência de modelo 3D
        public Sprite avatar;

        public PersonalityModifiers modifiers = new PersonalityModifiers
        {
            place = 1.0f, flip = 1.0f, swap = 1.0f, peek = 1.0f, challenge = 1.0f, boast = 1.0f
        };

        public MaestroPhrases phrases;
        public SpecialBehaviors specialBehaviors;

        public string GetContextualPhrase(string eventType)
        {
            string[] selected = null;
            switch (eventType.ToLower())
            {
                case "winpoint": selected = phrases.winPoint; break;
                case "losepoint": selected = phrases.losePoint; break;
                case "challenge": selected = phrases.challenge; break;
                case "boast": selected = phrases.boast; break;
                case "swap": selected = phrases.swap; break;
                case "peek": selected = phrases.peek; break;
                case "surprised": selected = phrases.surprised; break;
                case "confident": selected = phrases.confident; break;
                case "frustrated": selected = phrases.frustrated; break;
            }

            if (selected == null || selected.Length == 0) return "...";
            return selected[UnityEngine.Random.Range(0, selected.Length)];
        }
    }
}
