using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tellstones.Core.FSM
{
    /// <summary>
    /// Instância gerenciadora da Máquina de Estados Finita.
    /// Define e navega pelo fluxo da partida real (Menu -> Load -> InGame -> Win -> Fim)
    /// </summary>
    public class GameStateMachine
    {
        private IGameStateBase currentState;
        public IGameStateBase CurrentState => currentState;

        public event Action<IGameStateBase> OnStateChanged;

        public void ChangeState(IGameStateBase newState)
        {
            if (newState == null)
            {
                Debug.LogError("[GameStateMachine] Tentativa de mudar para um estado nulo!");
                return;
            }

            if (currentState != null)
            {
                currentState.Exit();
                Debug.Log($"[FSM] Saindo do estado: {currentState.GetType().Name}");
            }

            currentState = newState;
            
            Debug.Log($"[FSM] Entrando no estado: {currentState.GetType().Name}");
            OnStateChanged?.Invoke(currentState);
            
            currentState.Enter();
        }

        public void Update()
        {
            if (currentState != null)
            {
                currentState.Execute();
            }
        }
    }
}
