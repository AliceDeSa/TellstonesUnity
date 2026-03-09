using System;
using System.Collections.Generic;

namespace Tellstones.Core.Domain
{
    [Serializable]
    public class ActionRecord
    {
        public string tipo;
        public string jogador;
        public long timestamp;
    }

    [Serializable]
    public class HistoryRecord
    {
        public int turno;
        public string jogador;
        public string acao;
        public long timestamp;
    }

    [Serializable]
    public class TutorialState
    {
        public int currentStep;
        public List<string> allowedActions;
    }

    [Serializable]
    public class GameState
    {
        // Jogadores
        public List<Player> jogadores = new List<Player>();
        public int jogadorAtual;

        // Tabuleiro (Array de tamanho 7, indices null representam mesa vazia)
        public Stone[] mesa = new Stone[7];
        public List<string> pedrasDisponiveis = new List<string>();

        // Turnos e Ações
        public int turnoAtual;
        public string acaoAtual;

        // Estado de Jogo
        public bool jogoIniciado;
        public bool jogoFinalizado;
        public Player vencedor;

        // Modo de Jogo
        public string modoJogo;

        // Última Ação
        public ActionRecord ultimaAcao;

        // Histórico
        public List<HistoryRecord> historico = new List<HistoryRecord>();

        // Tutorial
        public TutorialState tutorialState;

        /// <summary>
        /// Instancia o estado inicial padrão do Tellstones (C# Port of DEFAULT_GAME_STATE)
        /// </summary>
        public static GameState CreateDefault()
        {
            return new GameState()
            {
                jogadores = new List<Player>(),
                jogadorAtual = 0,
                mesa = new Stone[7],
                pedrasDisponiveis = new List<string>(),
                turnoAtual = 0,
                acaoAtual = null,
                jogoIniciado = false,
                jogoFinalizado = false,
                vencedor = null,
                modoJogo = null,
                ultimaAcao = null,
                historico = new List<HistoryRecord>()
            };
        }
    }
}
