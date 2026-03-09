using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tellstones.AI
{
    public enum SkillLevel
    {
        Aprendiz = 1,
        Praticante = 2,
        Veterano = 3,
        Mestre = 4,
        Lenda = 5
    }

    [Serializable]
    public struct SkillBehaviors
    {
        public bool forgetsSwaps;
        public bool ignoresElimination;
        public bool hasMemoryDecay;
        public bool makesRiskyMoves;
    }

    [Serializable]
    public struct SkillConfig
    {
        public SkillLevel level;
        public string name;
        public float errorRate;
        public string description;
        public SkillBehaviors behaviors;
    }

    public class SkillLadder
    {
        private SkillLevel currentLevel;
        private SkillConfig config;

        private static readonly Dictionary<SkillLevel, SkillConfig> SKILL_CONFIGS = new Dictionary<SkillLevel, SkillConfig>
        {
            { SkillLevel.Aprendiz, new SkillConfig { level = SkillLevel.Aprendiz, name = "Aprendiz", errorRate = 0.40f, description = "Ainda aprendendo. Comete erros básicos.", behaviors = new SkillBehaviors { forgetsSwaps = true, ignoresElimination = true, hasMemoryDecay = true, makesRiskyMoves = false } } },
            { SkillLevel.Praticante, new SkillConfig { level = SkillLevel.Praticante, name = "Praticante", errorRate = 0.25f, description = "Competente mas ocasionalmente erra.", behaviors = new SkillBehaviors { forgetsSwaps = true, ignoresElimination = false, hasMemoryDecay = true, makesRiskyMoves = false } } },
            { SkillLevel.Veterano, new SkillConfig { level = SkillLevel.Veterano, name = "Veterano", errorRate = 0.15f, description = "Experiente. Raramente erra.", behaviors = new SkillBehaviors { forgetsSwaps = false, ignoresElimination = false, hasMemoryDecay = false, makesRiskyMoves = false } } },
            { SkillLevel.Mestre, new SkillConfig { level = SkillLevel.Mestre, name = "Mestre", errorRate = 0.05f, description = "Quase perfeito. Pressiona psicologicamente.", behaviors = new SkillBehaviors { forgetsSwaps = false, ignoresElimination = false, hasMemoryDecay = false, makesRiskyMoves = true } } },
            { SkillLevel.Lenda, new SkillConfig { level = SkillLevel.Lenda, name = "Lenda", errorRate = 0.02f, description = "Adaptativo. Contra-estratégia ativa.", behaviors = new SkillBehaviors { forgetsSwaps = false, ignoresElimination = false, hasMemoryDecay = false, makesRiskyMoves = true } } }
        };

        public SkillLadder(SkillLevel level = SkillLevel.Veterano)
        {
            SetLevel(level);
        }

        public void SetLevel(SkillLevel level)
        {
            currentLevel = level;
            config = SKILL_CONFIGS[level];
            Debug.Log($"[SkillLadder] Nível alterado para: {config.name}");
        }

        public SkillConfig GetConfig() => config;

        /// <summary>
        /// Aplica erro natural a uma predição Bayesiana
        /// </summary>
        public string ApplyImperfection(string bestGuess, Dictionary<string, float> probabilities)
        {
            if (UnityEngine.Random.value >= config.errorRate)
            {
                return bestGuess; // O Bot acertou / lembrou corretamente
            }

            var sorted = probabilities.OrderByDescending(p => p.Value).ToList();
            
            // Escolher segunda ou terceira opção em vez de uma puramente aleatória
            int errorIndex = 1 + UnityEngine.Random.Range(0, 2); 

            if (errorIndex < sorted.Count)
            {
                Debug.Log($"[SkillLadder] Erro natural induzido: {bestGuess} -> {sorted[errorIndex].Key}");
                return sorted[errorIndex].Key;
            }

            return bestGuess;
        }

        public bool ShouldForgetSwap(int turnsAgo)
        {
            if (!config.behaviors.forgetsSwaps) return false;
            float forgetChance = Mathf.Min(turnsAgo * 0.15f, 0.8f);
            return UnityEngine.Random.value < forgetChance;
        }

        public bool ShouldIgnoreElimination()
        {
            return config.behaviors.ignoresElimination && UnityEngine.Random.value < 0.3f;
        }

        public float GetMemoryDecayMultiplier()
        {
            if (!config.behaviors.hasMemoryDecay) return 1.0f;
            return 1.0f + (0.3f * (5 - (int)currentLevel) / 4.0f);
        }

        public bool ShouldMakeRiskyMove()
        {
            return config.behaviors.makesRiskyMoves && UnityEngine.Random.value < 0.4f;
        }
        
        public float GetErrorRate() => config.errorRate;
        public string GetLevelName() => config.name;
    }
}
