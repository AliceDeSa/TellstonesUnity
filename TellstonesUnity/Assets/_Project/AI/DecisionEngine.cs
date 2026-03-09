using System;
using System.Collections.Generic;
using System.Linq;
using Tellstones.Core.Domain;
using Tellstones.AI.Personalities;
using UnityEngine;

namespace Tellstones.AI
{
    public enum GamePhase
    {
        Opening, // 0-3 pedras
        Midgame, // 4-5 pedras
        Endgame  // 6-7 pedras
    }

    /// <summary>
    /// O motor central que roda toda a matemática AI. 
    /// Combina a percepção (BeliefState) e o peso das jogadas (Evaluator)
    /// </summary>
    public class DecisionEngine
    {
        private BeliefState beliefState;
        private ActionEvaluator evaluator;
        private PersonalityModifiers personalityModifiers;

        public DecisionEngine(BeliefState beliefState)
        {
            this.beliefState = beliefState;
            this.evaluator = new ActionEvaluator(beliefState);
            this.personalityModifiers = new PersonalityModifiers
            {
                place = 1.0f, flip = 1.0f, swap = 1.0f, peek = 1.0f, challenge = 1.0f, boast = 1.0f
            };
        }

        public void SetPersonalityModifiers(PersonalityModifiers modifiers)
        {
            this.personalityModifiers = modifiers;
        }

        public BotAction? DecideAction(GameState state)
        {
            var validActions = evaluator.GetValidActions(state);

            if (validActions.Count == 0)
            {
                Debug.LogWarning("[DecisionEngine] Nenhuma ação válida no momento!");
                return null;
            }

            // 1. Gera árvore e pontuações
            var scoredActions = evaluator.EvaluateActions(state, validActions);

            // 2. Aplica multiplicadores externos
            scoredActions = ApplyPersonalityModifiers(scoredActions);

            var phase = GetGamePhase(state);
            scoredActions = ApplyPhaseModifiers(scoredActions, phase);
            scoredActions = ApplyScoreDifferentialModifiers(scoredActions, state);

            // 3. Re-ordena após a mutação dos multiplicadores
            scoredActions = scoredActions.OrderByDescending(a => a.score).ToList();

            // 4. Injeta aleatoriedade natural entre as TOP 3 escolhas
            int topCount = Math.Min(3, scoredActions.Count);
            var chosen = scoredActions[UnityEngine.Random.Range(0, topCount)];

            Debug.Log($"[DecisionEngine] Escolheu: {chosen.action.type} (score: {chosen.score:F1}, fase: {phase})");

            return chosen.action;
        }

        private GamePhase GetGamePhase(GameState state)
        {
            int stonesPlaced = state.mesa.Count(p => p != null && !string.IsNullOrEmpty(p.nome));
            if (stonesPlaced <= 3) return GamePhase.Opening;
            if (stonesPlaced <= 5) return GamePhase.Midgame;
            return GamePhase.Endgame;
        }

        private List<ScoredAction> ApplyPersonalityModifiers(List<ScoredAction> actions)
        {
            foreach (var scored in actions)
            {
                float mod = 1.0f;
                switch (scored.action.type)
                {
                    case BotActionType.Place: mod = personalityModifiers.place; break;
                    case BotActionType.Flip: mod = personalityModifiers.flip; break;
                    case BotActionType.Swap: mod = personalityModifiers.swap; break;
                    case BotActionType.Peek: mod = personalityModifiers.peek; break;
                    case BotActionType.Challenge: mod = personalityModifiers.challenge; break;
                    case BotActionType.Boast: mod = personalityModifiers.boast; break;
                }
                scored.score *= mod;
            }
            return actions;
        }

        private List<ScoredAction> ApplyPhaseModifiers(List<ScoredAction> actions, GamePhase phase)
        {
            foreach (var scored in actions)
            {
                float mod = 1.0f;
                if (phase == GamePhase.Opening)
                {
                    if (scored.action.type == BotActionType.Place) mod = 1.3f; // Enche mesa
                    else if (scored.action.type == BotActionType.Flip) mod = 1.2f;
                    else if (scored.action.type == BotActionType.Peek) mod = 0.8f;
                    else if (scored.action.type == BotActionType.Challenge) mod = 0.5f;
                }
                else if (phase == GamePhase.Midgame)
                {
                    if (scored.action.type == BotActionType.Peek) mod = 1.4f; // Obter infos
                    else if (scored.action.type == BotActionType.Swap) mod = 1.3f; // Confundir player
                    else if (scored.action.type == BotActionType.Flip) mod = 1.1f;
                    else if (scored.action.type == BotActionType.Challenge) mod = 0.9f;
                }
                else if (phase == GamePhase.Endgame)
                {
                    if (scored.action.type == BotActionType.Challenge) mod = 1.5f; // Pressionar oponent
                    else if (scored.action.type == BotActionType.Boast) mod = 1.3f;
                    else if (scored.action.type == BotActionType.Peek) mod = 0.7f;
                    else if (scored.action.type == BotActionType.Place) mod = 0.5f;
                }
                scored.score *= mod;
            }
            return actions;
        }

        private List<ScoredAction> ApplyScoreDifferentialModifiers(List<ScoredAction> actions, GameState state)
        {
            var bot = state.jogadores.FirstOrDefault(p => p.isBot);
            var player = state.jogadores.FirstOrDefault(p => !p.isBot);

            if (bot == null || player == null) return actions;

            int differential = bot.pontos - player.pontos;

            // Se perdendo -> agressivo
            if (differential < 0)
            {
                foreach (var scored in actions)
                {
                    if (scored.action.type == BotActionType.Challenge || scored.action.type == BotActionType.Boast)
                    {
                        scored.score *= 1.3f;
                    }
                }
            }
            // Se ganhando -> conservador
            else if (differential > 0)
            {
                foreach (var scored in actions)
                {
                    if (scored.action.type == BotActionType.Swap || scored.action.type == BotActionType.Flip)
                    {
                        scored.score *= 1.2f;
                    }
                    else if (scored.action.type == BotActionType.Challenge)
                    {
                        scored.score *= 0.8f;
                    }
                }
            }

            return actions;
        }

        /// <summary>
        /// O tempo em ms fingindo que está "pensando" na UI/Animação 
        /// Calculado via complexidade da ação
        /// </summary>
        public float CalculateThinkTime(BotAction action)
        {
            float baseTime = 1.0f; // 1s min
            float actionComplexity = 0.5f; // meio sec max random base

            switch (action.type)
            {
                case BotActionType.Place: actionComplexity = 0.4f; break;
                case BotActionType.Flip: actionComplexity = 0.5f; break;
                case BotActionType.Swap: actionComplexity = 0.6f; break;
                case BotActionType.Peek: actionComplexity = 0.7f; break;
                case BotActionType.Boast: actionComplexity = 0.8f; break;
                case BotActionType.Challenge: actionComplexity = 0.9f; break;
            }

            return baseTime + UnityEngine.Random.Range(0f, actionComplexity);
        }
    }
}
