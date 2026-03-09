using System;
using UnityEngine;

namespace Tellstones.AI
{
    [Serializable]
    public struct EmotionalMetrics
    {
        public float confidence;  // 0-1, afeta agressividade
        public float frustration; // 0-1, aumenta com perdas
        public float focus;       // 0-1, diminui se jogo demora
    }

    public class EmotionalState
    {
        private EmotionalMetrics metrics;
        private int consecutiveLosses;
        private int turnsSinceLastPoint;

        public EmotionalState()
        {
            Reset();
        }

        public void RecordWin()
        {
            metrics.confidence = Mathf.Min(metrics.confidence + 0.15f, 1.0f);
            metrics.frustration = Mathf.Max(metrics.frustration - 0.2f, 0.0f);
            consecutiveLosses = 0;
            turnsSinceLastPoint = 0;
        }

        public void RecordLoss()
        {
            metrics.confidence = Mathf.Max(metrics.confidence - 0.1f, 0.2f);
            metrics.frustration = Mathf.Min(metrics.frustration + 0.15f, 1.0f);
            consecutiveLosses++;
            turnsSinceLastPoint = 0;
        }

        public void NextTurn()
        {
            turnsSinceLastPoint++;
            if (turnsSinceLastPoint > 8)
            {
                metrics.focus = Mathf.Max(metrics.focus - 0.05f, 0.5f);
            }
        }

        public EmotionalMetrics GetMetrics() => metrics;

        public float GetErrorModifier() => 1.0f + (metrics.frustration * 0.3f);

        public float GetAggressionModifier() => (metrics.confidence * 0.7f) + (metrics.frustration * 0.3f);

        public float GetMemoryModifier() => metrics.focus;

        public void Reset()
        {
            metrics = new EmotionalMetrics
            {
                confidence = 0.7f,
                frustration = 0.0f,
                focus = 1.0f
            };
            consecutiveLosses = 0;
            turnsSinceLastPoint = 0;
        }
    }
}
