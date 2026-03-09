using System;
using Tellstones.AI;
using Tellstones.Core.Events;
using UnityEngine;

namespace Tellstones.Core.FSM.States
{
    public class State_BotMoving : IGameStateBase
    {
        private MatchManager manager;
        private BotAction botAction;

        public State_BotMoving(MatchManager matchManager, BotAction actionToPerform)
        {
            this.manager = matchManager;
            this.botAction = actionToPerform;
        }

        public void Enter()
        {
            Debug.Log($"[State_BotMoving] O BOT aplicará: {botAction.type}");
            
            GameEvents.OnBotMoved?.Invoke(); // UI e Audio reagem

            switch (botAction.type)
            {
                case BotActionType.Place:
                    manager.ApplyPlacement(manager.GetState().jogadorAtual, botAction.targetSlot);
                    manager.EndTurn();
                    break;

                case BotActionType.Flip:
                    manager.ApplyFlip(manager.GetState().jogadorAtual, botAction.targetSlot);
                    manager.EndTurn();
                    break;

                case BotActionType.Swap:
                    manager.ApplySwap(manager.GetState().jogadorAtual, botAction.fromSlot, botAction.toSlot);
                    manager.EndTurn();
                    break;

                case BotActionType.Peek:
                    manager.ApplyPeek(manager.GetState().jogadorAtual, botAction.targetSlot);
                    manager.EndTurn();
                    break;

                case BotActionType.Challenge:
                case BotActionType.Boast:
                    manager.StateMachine.ChangeState(new State_ChallengeResolution(manager, botAction));
                    break;
            }
        }

        public void Execute()
        {
            // Opcional: Aqui poderíamos aguardar um evento "OnAnimationComplete"
            // antes de chamar o manager.EndTurn(), para não ser instantâneo puro.
        }

        public void Exit()
        {
        }
    }
}
