using System;
using Tellstones.Core.Domain;

namespace Tellstones.Core.Events
{
    public static class GameEvents
    {
        // Jogo Geral
        public static Action<GameState> OnStateChanged;
        public static Action<Player> OnPlayerScored;
        public static Action<Player> OnGameOver;

        // Ações de jogo
        public static Action<int, Stone> OnStonePlaced;
        public static Action<int> OnStoneFlipped;
        public static Action<int, int> OnStonesSwapped;
        public static Action<int, Stone> OnStonePeeked;
        public static Action<int, bool> OnChallengeResolved; // slot, botVenceu

        // IA
        public static Action<string> OnBotSpeech;
        public static Action OnBotThinking;
        public static Action OnBotMoved;

        /// <summary>
        /// Limpa todos os listeners para evitar vazamento de memória.
        /// Deve ser chamado no OnDestroy do SceneManager principal.
        /// </summary>
        public static void ClearAllListeners()
        {
            OnStateChanged = null;
            OnPlayerScored = null;
            OnGameOver = null;
            
            OnStonePlaced = null;
            OnStoneFlipped = null;
            OnStonesSwapped = null;
            OnStonePeeked = null;
            OnChallengeResolved = null;
            
            OnBotSpeech = null;
            OnBotThinking = null;
            OnBotMoved = null;
        }
    }
}
