using System;

namespace Tellstones.Core.FSM
{
    /// <summary>
    /// Interface base para qualquer Estado do jogo Tellstones
    /// </summary>
    public interface IGameStateBase
    {
        void Enter();
        void Execute();
        void Exit();
    }
}
