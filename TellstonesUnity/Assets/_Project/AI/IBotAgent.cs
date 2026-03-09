using System;
using System.Collections.Generic;
using Tellstones.Core.Domain;
using Tellstones.AI.Personalities;

namespace Tellstones.AI
{
    public struct BotAgentConfig
    {
        public MaestroProfile profile;
        public SkillLevel skillLevel;
    }

    public struct BotContext
    {
        public GameState state;
        public List<string> usedGuesses;
    }

    public struct BotObservation
    {
        public string type; // placement, swap, reveal, hide, peek, turn_end, challenge_result
        public int slot;
        public string stone;
        public int from;
        public int to;
        public bool botWon;
    }

    public interface IBotAgent
    {
        void Init(BotAgentConfig config);
        BotAction? DecideMove(BotContext ctx);
        float CalculateThinkTime(GameState state, BotAction decision);
        string PredictStone(int slot, BotContext ctx);
        string DecideBoastResponse(BotContext ctx); // "acreditar" | "duvidar"
        void Observe(BotObservation observation);
        string GetChatter(string eventType);
        string GetDebugStats();
        void Reset();
    }
}
