using System;
using System.Linq;
using Tellstones.Core.Domain;
using Tellstones.Core.Events;
using Tellstones.Core.FSM;
using Tellstones.Core.FSM.States;
using Tellstones.AI;
using Tellstones.AI.Personalities;
using UnityEngine;

namespace Tellstones.Core
{
    /// <summary>
    /// Gerenciador central da partida acoplado à FSM. Detém o GameState e orquestra transições.
    /// </summary>
    public class MatchManager : MonoBehaviour
    {
        public static MatchManager Instance { get; private set; }

        [Header("Dados do Jogo")]
        [SerializeField] private GameState currentState;
        public GameStateMachine StateMachine { get; private set; }
        
        [Header("Configurações da IA")]
        [SerializeField] private MaestroProfile botProfile;
        public IBotAgent CurrentBot { get; private set; }

        private void Awake()
        {
            if (Instance == null) { Instance = this; } else { Destroy(gameObject); }
            StateMachine = new GameStateMachine();
        }

        private void Start()
        {
            InitializeBasicMatch();
        }

        private void Update()
        {
            StateMachine.Update();
        }

        public void InitializeBasicMatch()
        {
            Debug.Log("[MatchManager] Inicializando partida com Máquina de Estados...");

            currentState = GameState.CreateDefault();

            var p1 = new Player { id = "p1", nome = "Jogador Local", pontos = 0, isBot = false };
            var p2 = new Player { id = "bot1", nome = botProfile ? botProfile.profileName : "Bot", pontos = 0, isBot = true };
            
            currentState.jogadores.Add(p1);
            currentState.jogadores.Add(p2);
            currentState.jogoIniciado = true;

            var botConfig = new BotAgentConfig
            {
                profile = botProfile, 
                skillLevel = SkillLevel.Veterano
            };
            var botController = new BotController();
            botController.Init(botConfig);
            CurrentBot = botController;

            // Iniciar fluxo no CoinFlip
            StateMachine.ChangeState(new State_CoinFlip(this));
        }

        /// <summary>
        /// Acesso global read-only ao estado do jogo
        /// </summary>
        public GameState GetState() => currentState;

        public void EndTurn()
        {
            currentState.jogadorAtual = 1 - currentState.jogadorAtual; // alterna 0 e 1
            currentState.turnoAtual++;

            // Mudar estado da máquina baseada em quem é a vez
            if (currentState.jogadores[currentState.jogadorAtual].isBot)
            {
                StateMachine.ChangeState(new State_BotThinking(this));
            }
            else
            {
                StateMachine.ChangeState(new State_PlayerTurn(this));
            }

            // Notifica bot do fim do turno se houver
            CurrentBot?.Observe(new BotObservation { type = "turn_end" });
        }

        public void ScorePoint(Player winner)
        {
            winner.pontos++;
            Debug.Log($"[MatchManager] Ponto para {winner.nome}! Placar: {winner.pontos}");
            GameEvents.OnPlayerScored?.Invoke(winner);

            if (winner.pontos >= 3)
            {
                currentState.vencedor = winner;
                StateMachine.ChangeState(new State_GameOver(this));
            }
            else
            {
                // Limpar a mesa e começar novo round
                currentState.mesa = new Stone[7];
                GameEvents.OnStateChanged?.Invoke(currentState); // update visual board limpar
                StateMachine.ChangeState(new State_CoinFlip(this));
            }
        }

        // --- Aplicadores de Ação ---
        public void ApplyPlacement(int playerIndex, int slot)
        {
            string pedras = "Escudo,Coroa,Martelo,Bandeira,Balança,Cavalo";
            string[] disponivel = currentState.pedrasDisponiveis.Count == 0 ? pedras.Split(',') : currentState.pedrasDisponiveis.ToArray();
            
            // Simulação sem a pool real por enquanto
            string p = disponivel[UnityEngine.Random.Range(0, disponivel.Length)];
            
            currentState.mesa[slot] = new Stone { nome = p, virada = false, slot = slot, dono = playerIndex };
            CurrentBot?.Observe(new BotObservation { type = "placement", slot = slot, stone = p });
            GameEvents.OnStonePlaced?.Invoke(slot, currentState.mesa[slot]);
            
            Debug.Log($"[MatchManager] Colocou pedra {p} no slot {slot}");
        }

        public void ApplyFlip(int playerIndex, int slot)
        {
            var p = currentState.mesa[slot];
            p.virada = !p.virada;

            if (p.virada)
                CurrentBot?.Observe(new BotObservation { type = "hide", slot = slot });
            else
                CurrentBot?.Observe(new BotObservation { type = "reveal", slot = slot, stone = p.nome });

            GameEvents.OnStoneFlipped?.Invoke(slot);
            Debug.Log($"[MatchManager] {(p.virada ? "Escondeu" : "Revelou")} pedra no slot {slot}");
        }

        public void ApplySwap(int playerIndex, int slotA, int slotB)
        {
            var temp = currentState.mesa[slotA];
            currentState.mesa[slotA] = currentState.mesa[slotB];
            currentState.mesa[slotB] = temp;

            CurrentBot?.Observe(new BotObservation { type = "swap", from = slotA, to = slotB });
            GameEvents.OnStonesSwapped?.Invoke(slotA, slotB);
            Debug.Log($"[MatchManager] Trocou os slots {slotA} e {slotB}");
        }

        public void ApplyPeek(int playerIndex, int slot)
        {
            var p = currentState.mesa[slot];
            if (currentState.jogadores[playerIndex].isBot)
            {
                CurrentBot?.Observe(new BotObservation { type = "peek", slot = slot, stone = p.nome });
            }
            GameEvents.OnStonePeeked?.Invoke(slot, p);
            Debug.Log($"[MatchManager] Espiou a pedra no slot {slot} (era {p.nome})");
        }

        private void OnDestroy()
        {
            GameEvents.ClearAllListeners();
        }
    }
}
