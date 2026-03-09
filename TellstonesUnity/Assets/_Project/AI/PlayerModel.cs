using System;
using System.Collections.Generic;
using System.Linq;

namespace Tellstones.AI
{
    [Serializable]
    public struct PlayerPattern
    {
        public int challengeCount;
        public int challengeSuccessCount;
        public Dictionary<int, int> slotInteractions;
        public int boastCount;
        public int boastHonestCount;
        public float earlyGameAggression;
        public float lateGameAggression;
    }

    public class PlayerModel
    {
        private PlayerPattern pattern;
        private int turnCount;

        public PlayerModel()
        {
            pattern = new PlayerPattern
            {
                challengeCount = 0,
                challengeSuccessCount = 0,
                slotInteractions = new Dictionary<int, int>(),
                boastCount = 0,
                boastHonestCount = 0,
                earlyGameAggression = 0.5f,
                lateGameAggression = 0.5f
            };
            turnCount = 0;
        }

        public void RecordChallenge(bool success)
        {
            pattern.challengeCount++;
            if (success) pattern.challengeSuccessCount++;
            UpdateAggressionMetrics();
        }

        public void RecordBoast(bool wasHonest)
        {
            pattern.boastCount++;
            if (wasHonest) pattern.boastHonestCount++;
        }

        public void RecordSlotInteraction(int slot)
        {
            if (!pattern.slotInteractions.ContainsKey(slot))
                pattern.slotInteractions[slot] = 0;
            pattern.slotInteractions[slot]++;
        }

        public void NextTurn()
        {
            turnCount++;
        }

        public float GetChallengeFrequency()
        {
            if (turnCount == 0) return 0.5f;
            return Math.Min((float)pattern.challengeCount / turnCount, 1.0f);
        }

        public float GetChallengeAccuracy()
        {
            if (pattern.challengeCount == 0) return 0.5f;
            return (float)pattern.challengeSuccessCount / pattern.challengeCount;
        }

        public float GetBoastHonesty()
        {
            if (pattern.boastCount == 0) return 0.5f;
            return (float)pattern.boastHonestCount / pattern.boastCount;
        }

        public List<int> GetPreferredSlots()
        {
            return pattern.slotInteractions
                .OrderByDescending(kvp => kvp.Value)
                .Take(3)
                .Select(kvp => kvp.Key)
                .ToList();
        }

        public (List<int> avoidSlots, bool swapMore, bool callBluffs) SuggestCounterStrategy()
        {
            return (
                avoidSlots: GetPreferredSlots(),
                swapMore: GetChallengeFrequency() > 0.6f,
                callBluffs: GetBoastHonesty() < 0.4f
            );
        }

        private void UpdateAggressionMetrics()
        {
            float freq = GetChallengeFrequency();
            if (turnCount < 5)
                pattern.earlyGameAggression = freq;
            else
                pattern.lateGameAggression = freq;
        }
    }
}
