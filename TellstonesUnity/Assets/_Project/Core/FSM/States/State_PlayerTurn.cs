using System;
using UnityEngine;

namespace Tellstones.Core.FSM.States
{
    public class State_PlayerTurn : IGameStateBase
    {
        private MatchManager manager;

        public State_PlayerTurn(MatchManager matchManager)
        {
            this.manager = matchManager;
        }

        public void Enter()
        {
            Debug.Log("[State_PlayerTurn] Turno do Jogador Livre. Aguardando input na UI/Mesa...");
            // Aqui a UI do jogador destrancaria botões, raycast habilitado, etc.
        }

        public void Execute()
        {
            // Aguardando ação do usuário. A transição ocorre por eventos dos botões da UI / Raycast.
            // Quando o jogador clica "Colocar", o C# dispara o MatchManager.ExecutePlayerAction(xxx).
            // O MatchManager fará a validação, aplicará e mudará o estado para BotThinking.
        }

        public void Exit()
        {
            Debug.Log("[State_PlayerTurn] Jogador finalizou turno. Travando mesa.");
        }
    }
}
