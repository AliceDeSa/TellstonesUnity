using System;
using UnityEngine;

namespace Tellstones.Core.FSM.States
{
    public class State_CoinFlip : IGameStateBase
    {
        private MatchManager manager;

        public State_CoinFlip(MatchManager matchManager)
        {
            this.manager = matchManager;
        }

        public void Enter()
        {
            Debug.Log("[State_CoinFlip] Sorteando quem começa...");
            
            // Lógica do TS: Vencedor do último jogo começa. Se primeiro jogo, Random.
            var state = manager.GetState();
            
            if (state.vencedor != null)
            {
                // Vencedor anterior começa
                state.jogadorAtual = state.jogadores.FindIndex(p => p.id == state.vencedor.id);
            }
            else
            {
                state.jogadorAtual = UnityEngine.Random.Range(0, 2);
            }
            
            state.turnoAtual = 1;

            Debug.Log($"[State_CoinFlip] O jogador {state.jogadores[state.jogadorAtual].nome} venceu o sorteio!");

            // Mudar para o turno correto
            if (state.jogadores[state.jogadorAtual].isBot)
            {
                manager.StateMachine.ChangeState(new State_BotThinking(manager));
            }
            else
            {
                manager.StateMachine.ChangeState(new State_PlayerTurn(manager));
            }
        }

        public void Execute()
        {
            // O sorteio é instantâneo nesta arquitetura, sem "update" loop
        }

        public void Exit()
        {
            // Eventos ao sair
        }
    }
}
