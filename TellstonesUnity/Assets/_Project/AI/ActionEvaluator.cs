using System;
using System.Collections.Generic;
using System.Linq;
using Tellstones.Core.Domain;
using UnityEngine;

namespace Tellstones.AI
{
    public struct ScoreBreakdown
    {
        public float baseValue;
        public float informationGain;
        public float winProbability;
        public float riskFactor;
    }

    public class ScoredAction
    {
        public BotAction action;
        public float score;
        public ScoreBreakdown breakdown;
    }

    public class ActionEvaluator
    {
        private BeliefState beliefState;

        public ActionEvaluator(BeliefState beliefState)
        {
            this.beliefState = beliefState;
        }

        public List<ScoredAction> EvaluateActions(GameState state, List<BotAction> validActions)
        {
            var scored = new List<ScoredAction>();
            foreach (var action in validActions)
            {
                var breakdown = GetScoreBreakdown(action, state);
                
                // Cálculo total ponderado (Recompensa - Risco)
                float score = breakdown.baseValue + breakdown.informationGain + breakdown.winProbability - breakdown.riskFactor;
                
                scored.Add(new ScoredAction
                {
                    action = action,
                    score = score,
                    breakdown = breakdown
                });
            }

            // Ordena o mais valioso primeiro
            return scored.OrderByDescending(a => a.score).ToList();
        }

        private ScoreBreakdown GetScoreBreakdown(BotAction action, GameState state)
        {
            switch (action.type)
            {
                case BotActionType.Place: return EvaluatePlace(action.targetSlot, state);
                case BotActionType.Flip: return EvaluateFlip(action.targetSlot, state);
                case BotActionType.Swap: return EvaluateSwap(action.fromSlot, action.toSlot, state);
                case BotActionType.Peek: return EvaluatePeek(action.targetSlot, state);
                case BotActionType.Challenge: return EvaluateChallenge(action.targetSlot, state);
                case BotActionType.Boast: return EvaluateBoast(state);
                default: return new ScoreBreakdown();
            }
        }

        private ScoreBreakdown EvaluatePlace(int slot, GameState state)
        {
            float baseValue = 25f;
            if (slot == 3) baseValue += 10f; // Centro
            if (slot == 2 || slot == 4) baseValue += 5f; // Miolo

            bool hasLeft = slot > 0 && state.mesa[slot - 1] != null && !string.IsNullOrEmpty(state.mesa[slot - 1].nome);
            bool hasRight = slot < 6 && state.mesa[slot + 1] != null && !string.IsNullOrEmpty(state.mesa[slot + 1].nome);
            if (hasLeft || hasRight) baseValue += 5f;

            return new ScoreBreakdown { baseValue = baseValue };
        }

        private ScoreBreakdown EvaluateFlip(int slot, GameState state)
        {
            var stone = state.mesa[slot];
            if (stone == null || string.IsNullOrEmpty(stone.nome)) return new ScoreBreakdown();

            // Esconder (virar para baixo)
            if (!stone.virada)
            {
                float confidence = beliefState.GetConfidence(slot);
                return new ScoreBreakdown { baseValue = 30f, riskFactor = confidence * 10f }; 
            }

            // Revelar (virar para cima)
            return new ScoreBreakdown { baseValue = 15f, informationGain = 5f, riskFactor = 5f };
        }

        private ScoreBreakdown EvaluateSwap(int from, int to, GameState state)
        {
            int hiddenCount = state.mesa.Count(p => p != null && !string.IsNullOrEmpty(p.nome) && p.virada);
            
            // Trocar ofusca mais o oponente quanto mais pedras têm viradas
            float baseValue = 20f + (hiddenCount * 3f);
            float infoGain = hiddenCount * 2f;

            return new ScoreBreakdown { baseValue = baseValue, informationGain = infoGain };
        }

        private ScoreBreakdown EvaluatePeek(int slot, GameState state)
        {
            float confidence = beliefState.GetConfidence(slot);

            // Se o bot já sabe qual pedra é, não gasta turno espiando
            if (confidence >= 0.8f)
            {
                return new ScoreBreakdown { riskFactor = 5000f }; 
            }

            float uncertainty = 1f - confidence;
            float infoGain = uncertainty * 60f;

            return new ScoreBreakdown { baseValue = 30f, informationGain = infoGain, riskFactor = confidence * 20f };
        }

        private ScoreBreakdown EvaluateChallenge(int slot, GameState state)
        {
            var stone = state.mesa[slot];
            if (stone == null || !stone.virada) return new ScoreBreakdown { riskFactor = 300f };

            int hiddenCount = state.mesa.Count(p => p != null && !string.IsNullOrEmpty(p.nome) && p.virada);
            if (hiddenCount < 3) return new ScoreBreakdown { riskFactor = 300f }; // Evita desafiar início

            float confidence = beliefState.GetConfidence(slot);

            return new ScoreBreakdown
            {
                baseValue = 50f,
                winProbability = confidence * 60f,
                riskFactor = (1f - confidence) * 50f
            };
        }

        private ScoreBreakdown EvaluateBoast(GameState state)
        {
            var pedrasViradas = state.mesa
                .Select((p, i) => new { exists = p != null && !string.IsNullOrEmpty(p.nome), hidden = p != null && p.virada, idx = i })
                .Where(x => x.exists)
                .ToList();
            
            int hiddenCount = pedrasViradas.Count(x => x.hidden);

            if (hiddenCount < 2 || pedrasViradas.Count < 3)
            {
                return new ScoreBreakdown { riskFactor = 3000f };
            }

            float hiddenRatio = (float)hiddenCount / pedrasViradas.Count;
            float baseValue = 20f + (hiddenRatio * 30f);

            // Quantas ele Sabe de verdade?
            int knownHidden = pedrasViradas.Where(h => h.hidden && beliefState.GetConfidence(h.idx) > 0.85f).Count();
            float knowAllHiddenRatio = hiddenCount > 0 ? ((float)knownHidden / hiddenCount) : 0f;

            if (knowAllHiddenRatio < 1.0f)
            {
                // Penalidade suicida se não conhecer todas
                return new ScoreBreakdown { baseValue = baseValue, riskFactor = 2000f };
            }

            return new ScoreBreakdown { baseValue = baseValue, winProbability = 80f, riskFactor = 5f };
        }

        // --- Geração de Movimentos ---
        public List<BotAction> GetValidActions(GameState state)
        {
            var actions = new List<BotAction>();
            var mesa = state.mesa;
            int usedStones = mesa.Count(p => p != null && !string.IsNullOrEmpty(p.nome));

            if (usedStones < 7)
            {
                var placeSlots = GetAdjacentEmptySlots(mesa);
                foreach (int slot in placeSlots)
                    actions.Add(new BotAction { type = BotActionType.Place, targetSlot = slot });
            }

            for (int i = 0; i < 7; i++)
            {
                if (mesa[i] != null && !string.IsNullOrEmpty(mesa[i].nome))
                {
                    actions.Add(new BotAction { type = BotActionType.Flip, targetSlot = i });

                    if (mesa[i].virada)
                    {
                        actions.Add(new BotAction { type = BotActionType.Peek, targetSlot = i });
                        actions.Add(new BotAction { type = BotActionType.Challenge, targetSlot = i });
                    }
                }
            }

            for (int i = 0; i < 6; i++)
            {
                if (mesa[i] != null && !string.IsNullOrEmpty(mesa[i].nome) && mesa[i + 1] != null && !string.IsNullOrEmpty(mesa[i + 1].nome))
                {
                    actions.Add(new BotAction { type = BotActionType.Swap, fromSlot = i, toSlot = i + 1 });
                }
            }

            if (usedStones >= 3)
            {
                actions.Add(new BotAction { type = BotActionType.Boast });
            }

            return actions;
        }

        private List<int> GetAdjacentEmptySlots(Stone[] mesa)
        {
            var valid = new List<int>();

            for (int i = 0; i < 7; i++)
            {
                if (mesa[i] != null && !string.IsNullOrEmpty(mesa[i].nome)) continue;
                if (i == 3) { valid.Add(i); continue; }

                bool leftOccupied = i > 0 && mesa[i - 1] != null && !string.IsNullOrEmpty(mesa[i - 1].nome);
                bool rightOccupied = i < 6 && mesa[i + 1] != null && !string.IsNullOrEmpty(mesa[i + 1].nome);

                if (leftOccupied || rightOccupied) valid.Add(i);
            }
            return valid;
        }
    }
}
