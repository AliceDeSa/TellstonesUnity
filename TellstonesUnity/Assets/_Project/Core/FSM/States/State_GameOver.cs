using System;
using UnityEngine;
using Tellstones.Core.Events;

namespace Tellstones.Core.FSM.States
{
    public class State_GameOver : IGameStateBase
    {
        private MatchManager manager;

        public State_GameOver(MatchManager matchManager)
        {
            this.manager = matchManager;
        }

        public void Enter()
        {
            var state = manager.GetState();
            state.jogoFinalizado = true;

            Debug.Log($"========= FIM DE JOGO =========");
            if (state.vencedor != null)
                Debug.Log($"VENCEDOR: {state.vencedor.nome} com {state.vencedor.pontos} pontos.");
            else
                Debug.Log($"Ocorreu um fim de jogo inexplicável sem vencedor.");
            
            GameEvents.OnGameOver?.Invoke(state.vencedor);
        }

        public void Execute()
        {
            // Fica "preso" aqui até o usuário escolher Restart na Interface, 
            // que forçará o GameManager a recarregar a Cena ou resetar os states.
        }

        public void Exit()
        {
        }
    }
}
