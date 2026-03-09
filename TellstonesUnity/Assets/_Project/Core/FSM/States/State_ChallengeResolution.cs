using System;
using Tellstones.AI;
using Tellstones.Core.Domain;
using Tellstones.Core.Events;
using UnityEngine;

namespace Tellstones.Core.FSM.States
{
    public class State_ChallengeResolution : IGameStateBase
    {
        private MatchManager manager;
        private BotAction triggerAction;

        public State_ChallengeResolution(MatchManager matchManager, BotAction action)
        {
            this.manager = matchManager;
            this.triggerAction = action;
        }

        public void Enter()
        {
            Debug.Log($"[State_ChallengeResolution] Iniciando um processo de {(triggerAction.type == BotActionType.Boast ? "Boast (Segabar)" : "Challenge (Desafio)")}...");

            var state = manager.GetState();
            var currentPlayer = state.jogadores[state.jogadorAtual];
            var opponentPlayer = state.jogadores[1 - state.jogadorAtual]; // Oponente (0 ou 1)

            if (triggerAction.type == BotActionType.Challenge)
            {
                // DESAFIO (Player A desafia a pedra escura de Player B)
                // Fase 1: O desafiado (oponente) tenta adivinhar
                
                if (opponentPlayer.isBot)
                {
                    // IA tenta adivinhar a própria pedra do desafio
                    var ctx = new BotContext() { state = manager.GetState() };
                    string palpites = manager.CurrentBot.PredictStone(triggerAction.targetSlot, ctx);

                    ResolveChallenge(palpites);
                }
                else
                {
                    Debug.Log("[Challenge] Aguardando UI do Humano adivinhar qual pedra é...");
                }
            } 
            else if (triggerAction.type == BotActionType.Boast)
            {
                // SEGABAR (BOAST)
                if (opponentPlayer.isBot)
                {
                    var ctx = new BotContext() { state = manager.GetState() };
                    string response = manager.CurrentBot.DecideBoastResponse(ctx);
                    if (response == "duvidar")
                    {
                        Debug.Log("[Boast] Bot decidiu: DUVIDAR do Boast. Faremos UI do Boast Check!");
                    }
                    else
                    {
                        Debug.Log("[Boast] Bot decidiu: ACREDITAR no Boast. O " + currentPlayer.nome + " ganhou o ponto!");
                        manager.ScorePoint(currentPlayer);
                    }
                }
                else
                {
                    Debug.Log("[Boast] Aguardando UI do Humano dizer se acredita ou duvida do bot...");
                }
            }
        }

        public void ResolveChallenge(string guessedStoneParam)
        {
            var stoneNoTabuleiro = manager.GetState().mesa[triggerAction.targetSlot];
            var opponentIndex = 1 - manager.GetState().jogadorAtual;
            var opponent = manager.GetState().jogadores[opponentIndex];
            var current = manager.GetState().jogadores[manager.GetState().jogadorAtual];

            bool acertou = stoneNoTabuleiro.nome.Equals(guessedStoneParam, StringComparison.OrdinalIgnoreCase);

            Debug.Log($"[Challenge Result] Disse '{guessedStoneParam}', era '{stoneNoTabuleiro.nome}' -> " + (acertou ? "ACERTOU!" : "ERROU!"));
            GameEvents.OnChallengeResolved?.Invoke(triggerAction.targetSlot, acertou);

            if (acertou) 
            {
                // Desafiado defendeu o ponto
                manager.ScorePoint(opponent); 
            }
            else
            {
                // Desafiador ganhou pela falha do adversário
                manager.ScorePoint(current); 
            }
        }

        public void Execute() {}
        public void Exit() {}
    }
}
