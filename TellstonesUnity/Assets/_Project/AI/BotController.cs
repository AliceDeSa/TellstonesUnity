using System;
using System.Collections.Generic;
using System.Linq;
using Tellstones.Core.Domain;
using Tellstones.AI.Personalities;
using UnityEngine;

namespace Tellstones.AI
{
    /// <summary>
    /// O cérebro completo e principal do Bot. Orquestra a memória Bayesiana, Engine de Decisão, Modelo de jogador, etc.
    /// </summary>
    public class BotController : IBotAgent
    {
        private BeliefState beliefState;
        private DecisionEngine decisionEngine;
        private SkillLadder skillLadder;
        private PlayerModel playerModel;
        private EmotionalState emotionalState;

        public MaestroProfile profile { get; private set; }
        private SkillLevel currentLevel;

        public BotController()
        {
            beliefState = new BeliefState();
            decisionEngine = new DecisionEngine(beliefState);
            playerModel = new PlayerModel();
            emotionalState = new EmotionalState();
        }

        public void Init(BotAgentConfig config)
        {
            profile = config.profile;
            currentLevel = config.skillLevel;
            skillLadder = new SkillLadder(currentLevel);
            
            if (profile != null)
                decisionEngine.SetPersonalityModifiers(profile.modifiers);

            Debug.Log($"[BotController] Inicializado: {profile?.profileName ?? "Vigilante Padrão"} ({currentLevel})");
        }

        public BotAction? DecideMove(BotContext ctx)
        {
            var action = decisionEngine.DecideAction(ctx.state);

            if (action == null) return null;

            string phrase = GetContextualPhrase(action.Value.type.ToString());
            if (!string.IsNullOrEmpty(phrase))
            {
                Debug.Log($"[{profile?.profileName}]: \"{phrase}\"");
            }

            return action;
        }

        public float CalculateThinkTime(GameState state, BotAction decision)
        {
            return decisionEngine.CalculateThinkTime(decision);
        }

        public string PredictStone(int slot, BotContext ctx)
        {
            var probs = beliefState.GetSlotProbabilities(slot);

            if (ctx.usedGuesses != null)
            {
                foreach (var stone in ctx.usedGuesses)
                {
                    probs[stone] = 0f;
                }
            }

            string maxStone = BeliefState.STONES[0];
            float maxProb = -1f;

            foreach (var kvp in probs)
            {
                if (kvp.Value > maxProb)
                {
                    maxProb = kvp.Value;
                    maxStone = kvp.Key;
                }
            }

            float probability = maxProb == -1f ? 0f : maxProb;
            string finalGuess = maxStone;

            if (probability > 0f)
            {
                finalGuess = skillLadder.ApplyImperfection(maxStone, probs);

                float errorMod = emotionalState.GetErrorModifier();
                if (UnityEngine.Random.value < (errorMod - 1.0f))
                {
                    var sorted = probs.OrderByDescending(p => p.Value).ToList();
                    if (sorted.Count > 2) finalGuess = sorted[2].Key;
                }
            }
            else
            {
                var available = BeliefState.STONES.Where(p => ctx.usedGuesses == null || !ctx.usedGuesses.Contains(p)).ToList();
                if (available.Count > 0) finalGuess = available[0];
            }

            Debug.Log($"[{profile?.profileName}] Palpite de Memória: {finalGuess} (confiança empírica: {Mathf.RoundToInt(probability * 100)}%)");
            return finalGuess;
        }

        public string DecideBoastResponse(BotContext ctx)
        {
            var strategy = playerModel.SuggestCounterStrategy();
            if (strategy.callBluffs) return "duvidar";

            int hiddenCount = ctx.state.mesa.Count(p => p != null && !string.IsNullOrEmpty(p.nome) && p.virada);
            float doubtChance = hiddenCount > 4 ? 0.7f : hiddenCount > 2 ? 0.5f : 0.3f;

            return UnityEngine.Random.value < doubtChance ? "duvidar" : "acreditar";
        }

        public void Observe(BotObservation ev)
        {
            switch (ev.type)
            {
                case "placement":
                    beliefState.ObservePlacement(ev.slot, ev.stone);
                    playerModel.RecordSlotInteraction(ev.slot);
                    break;
                case "swap":
                    beliefState.ObserveSwap(ev.from, ev.to);
                    break;
                case "reveal":
                    beliefState.ObserveReveal(ev.slot, ev.stone);
                    break;
                case "hide":
                    beliefState.ObserveHide(ev.slot);
                    break;
                case "peek":
                    beliefState.ObservePeek(ev.slot, ev.stone);
                    break;
                case "turn_end":
                    beliefState.NextTurn();
                    playerModel.NextTurn();
                    emotionalState.NextTurn();
                    break;
                case "challenge_result":
                    if (ev.botWon)
                    {
                        emotionalState.RecordWin();
                    }
                    else
                    {
                        emotionalState.RecordLoss();
                        playerModel.RecordChallenge(true); // jogador teve sucesso no desafio
                    }
                    break;
            }
        }

        public string GetChatter(string eventType)
        {
            switch (eventType)
            {
                case "win_point": return GetContextualPhrase("winpoint");
                case "lose_point": return GetContextualPhrase("losepoint");
                case "challenge": return GetContextualPhrase("challenge");
                case "boast": return GetContextualPhrase("boast");
                case "winning": return GetContextualPhrase("confident");
                case "losing": return GetContextualPhrase("frustrated");
                default: return null;
            }
        }

        public string GetDebugStats()
        {
            var emotion = emotionalState.GetMetrics();
            return $"Conf: {Mathf.RoundToInt(emotion.confidence * 100)}% Frust: {Mathf.RoundToInt(emotion.frustration * 100)}%";
        }

        public void Reset()
        {
            beliefState.Reset();
            emotionalState.Reset();
        }

        private string GetContextualPhrase(string actionType)
        {
            if (profile == null) return "...";
            return profile.GetContextualPhrase(actionType);
        }
    }
}
