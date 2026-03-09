using System;
using Tellstones.Core.Domain;
using Tellstones.Core.Events;
using Tellstones.AI;
using Tellstones.AI.Personalities;
using UnityEngine;

namespace Tellstones.Core
{
    /// <summary>
    /// Gerenciador central da partida. Detém o GameState e orquestra turnos interagindo com a IA.
    /// Futuramente (Fase 2) será controlado por uma State Machine.
    /// </summary>
    public class MatchManager : MonoBehaviour
    {
        public static MatchManager Instance { get; private set; }

        [Header("Dados do Jogo")]
        [SerializeField] private GameState currentState;
        
        [Header("Configurações da IA")]
        [SerializeField] private MaestroProfile botProfile;
        public IBotAgent CurrentBot { get; private set; }

        private void Awake()
        {
            if (Instance == null) { Instance = this; } else { Destroy(gameObject); }
        }

        private void Start()
        {
            // Teste inicial sem visual
            InitializeBasicMatch();
        }

        public void InitializeBasicMatch()
        {
            Debug.Log("[MatchManager] Inicializando partida básica de testes...");

            // 1. Criar estado
            currentState = GameState.CreateDefault();

            // 2. Adicionar jogadores
            var p1 = new Player { id = "p1", nome = "Jogador Local", pontos = 0, isBot = false };
            var p2 = new Player { id = "bot1", nome = botProfile ? botProfile.profileName : "Bot", pontos = 0, isBot = true };
            
            currentState.jogadores.Add(p1);
            currentState.jogadores.Add(p2);
            currentState.jogadorAtual = 0; // Jogador começa
            currentState.jogoIniciado = true;

            // 3. Inicializar IA
            var botController = new BotController();
            botController.Init(new BotAgentConfig
            {
                profile = botProfile, // Pode ser null no teste
                skillLevel = SkillLevel.Veterano
            });
            CurrentBot = botController;

            // 4. Disparar evento
            GameEvents.OnStateChanged?.Invoke(currentState);

            Debug.Log($"[MatchManager] Partida iniciada! Vez do {currentState.jogadores[currentState.jogadorAtual].nome}.");
        }

        /// <summary>
        /// Acesso global read-only ao estado do jogo
        /// </summary>
        public GameState GetState() => currentState;

        private void OnDestroy()
        {
            GameEvents.ClearAllListeners();
        }
    }
}
