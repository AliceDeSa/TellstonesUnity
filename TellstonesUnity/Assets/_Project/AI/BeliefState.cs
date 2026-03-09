using System;
using System.Collections.Generic;
using System.Linq;

namespace Tellstones.AI
{
    /// <summary>
    /// BeliefState - Memória Inteligente Bayesiana do Bot
    /// 3D Desktop port 
    /// </summary>
    public class BeliefState
    {
        public static readonly string[] STONES = { "Espada", "Escudo", "Coroa", "Martelo", "Bandeira", "Balança", "Cavalo" };

        // [Slot, [Pedra, Probabilidade]]
        private Dictionary<int, Dictionary<string, float>> slots;

        private class ConfirmedStone
        {
            public string stone;
            public int turn;
        }
        private Dictionary<int, ConfirmedStone> confirmed;

        private int currentTurn;
        private const float DECAY_RATE = 0.85f;

        public BeliefState()
        {
            slots = new Dictionary<int, Dictionary<string, float>>();
            confirmed = new Dictionary<int, ConfirmedStone>();
            currentTurn = 0;
            Reset();
        }

        public void Reset()
        {
            slots.Clear();
            confirmed.Clear();
            currentTurn = 0;

            for (int slot = 0; slot < 7; slot++)
            {
                var probs = new Dictionary<string, float>();
                foreach (var stone in STONES)
                {
                    probs[stone] = 1.0f / 7.0f; // Distribuição uniforme no início
                }
                slots[slot] = probs;
            }
        }

        public void NextTurn()
        {
            currentTurn++;
            ApplyDecay();
        }

        public void ObservePlacement(int slot, string stone)
        {
            SetConfirmed(slot, stone);
            EliminateStoneFromOtherSlots(stone, slot);
        }

        public void ObserveReveal(int slot, string stone)
        {
            SetConfirmed(slot, stone);
            EliminateStoneFromOtherSlots(stone, slot);
        }

        public void ObserveHide(int slot)
        {
            if (confirmed.TryGetValue(slot, out var conf))
            {
                var probs = slots[slot];
                foreach (var stone in STONES)
                {
                    if (stone == conf.stone)
                    {
                        probs[stone] = 0.78f; // Lembra a pedra, mas sem 100% (evita cheats no UI)
                    }
                    else
                    {
                        probs[stone] = 0.22f / 6.0f; // Distribui o restante entre as 6
                    }
                }
                Normalize(slot);
                confirmed.Remove(slot);
            }
        }

        public void ObserveSwap(int from, int to)
        {
            // Trocamos os diccionarios de memória
            var tempFrom = new Dictionary<string, float>(slots[from]);
            slots[from] = new Dictionary<string, float>(slots[to]);
            slots[to] = tempFrom;

            confirmed.TryGetValue(from, out var confirmedFrom);
            confirmed.TryGetValue(to, out var confirmedTo);

            if (confirmedFrom != null) confirmed[to] = confirmedFrom; else confirmed.Remove(to);
            if (confirmedTo != null) confirmed[from] = confirmedTo; else confirmed.Remove(from);
        }

        public void ObservePeek(int slot, string stone)
        {
            SetConfirmed(slot, stone);
            EliminateStoneFromOtherSlots(stone, slot);
        }

        public KeyValuePair<string, float> GetMostLikelyStone(int slot)
        {
            if (!slots.ContainsKey(slot)) return new KeyValuePair<string, float>(STONES[0], 0f);

            var probs = slots[slot];
            string maxStone = STONES[0];
            float maxProb = 0;

            foreach (var kvp in probs)
            {
                if (kvp.Value > maxProb)
                {
                    maxProb = kvp.Value;
                    maxStone = kvp.Key;
                }
            }

            return new KeyValuePair<string, float>(maxStone, maxProb);
        }

        public Dictionary<string, float> GetSlotProbabilities(int slot)
        {
            if (slots.TryGetValue(slot, out var p)) return new Dictionary<string, float>(p);
            return new Dictionary<string, float>();
        }

        public float GetConfidence(int slot)
        {
            return GetMostLikelyStone(slot).Value;
        }

        public List<string> GetUnseenStones()
        {
            var seen = new HashSet<string>();
            foreach (var c in confirmed.Values) seen.Add(c.stone);
            
            return STONES.Where(s => !seen.Contains(s)).ToList();
        }

        private void SetConfirmed(int slot, string stone)
        {
            var probs = slots[slot];
            foreach (var s in STONES)
            {
                probs[s] = (s == stone) ? 1.0f : 0.0f;
            }
            confirmed[slot] = new ConfirmedStone { stone = stone, turn = currentTurn };
        }

        private void EliminateStoneFromOtherSlots(string stone, int exceptSlot)
        {
            for (int slot = 0; slot < 7; slot++)
            {
                if (slot == exceptSlot) continue;
                slots[slot][stone] = 0.0f;
                Normalize(slot);
            }
        }

        private void Normalize(int slot)
        {
            var probs = slots[slot];
            float sum = probs.Values.Sum();
            if (sum > 0)
            {
                foreach (var stone in STONES)
                {
                    probs[stone] /= sum;
                }
            }
        }

        private void ApplyDecay()
        {
            for (int slot = 0; slot < 7; slot++)
            {
                confirmed.TryGetValue(slot, out var conf);
                if (conf != null && (currentTurn - conf.turn) < 2) continue; // Sem decaimento recente

                var probs = slots[slot];
                foreach (var stone in STONES)
                {
                    float decayed = probs[stone] * DECAY_RATE;
                    float uniform = 1.0f / 7.0f;
                    float newProb = decayed + (uniform * (1.0f - DECAY_RATE));
                    probs[stone] = newProb;
                }
                Normalize(slot);

                // Perde o recall perfeito
                if (conf != null && GetConfidence(slot) < 0.5f)
                {
                    confirmed.Remove(slot);
                }
            }
        }
    }
}
