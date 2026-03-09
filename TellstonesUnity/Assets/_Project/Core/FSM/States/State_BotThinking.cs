using System;
using Tellstones.AI;
using Tellstones.Core.Domain;
using UnityEngine;

namespace Tellstones.Core.FSM.States
{
    public class State_BotThinking : IGameStateBase
    {
        private MatchManager manager;
        private float thinkTimer;
        private float thinkDuration;
        private BotAction chosenAction;
        private bool decisionMade;

        public State_BotThinking(MatchManager matchManager)
        {
            this.manager = matchManager;
        }

        public void Enter()
        {
            Debug.Log("[State_BotThinking] O BOT está analisando a mesa...");
            
            var ai = manager.CurrentBot;
            var ctx = new BotContext { state = manager.GetState() };

            // 1. O Bot decide qual será sua ação instantaneamente na CPU
            var decision = ai.DecideMove(ctx);

            if (decision == null)
            {
                Debug.LogError("Bot não encontrou ações válidas! Passando turno forçado.");
                manager.StateMachine.ChangeState(new State_PlayerTurn(manager));
                return;
            }

            chosenAction = decision.Value;

            // 2. Simulamos um "tempo pensando" humano baseado na complexidade da jogada
            thinkDuration = ai.CalculateThinkTime(manager.GetState(), chosenAction);
            thinkTimer = 0f;
            decisionMade = true;

            Debug.Log($"  >> Escolheu {chosenAction.type}. Tempo de raciocínio simulado: {thinkDuration:F2}s");
            // Disparar evento de animação da UI "Thinking..."
        }

        public void Execute()
        {
            if (!decisionMade) return;

            // 3. Aguardar o timer simulado
            thinkTimer += Time.deltaTime;

            if (thinkTimer >= thinkDuration)
            {
                decisionMade = false;
                // 4. Passa a bola para o estado que faz a animação de FATO mover a pedra
                manager.StateMachine.ChangeState(new State_BotMoving(manager, chosenAction));
            }
        }

        public void Exit()
        {
            // Ocultar animação Thinking...
        }
    }
}
