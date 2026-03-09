using System;
using Tellstones.Core.Domain;
using DG.Tweening;
using UnityEngine;

namespace Tellstones.Visual
{
    /// <summary>
    /// Representação visual (O Prefab) de uma pedra de Tellstones.
    /// Lida apenas com DOTween, cores, emissões e rotação visual. Não afeta a lógica (Core).
    /// </summary>
    public class StoneView : MonoBehaviour
    {
        [Header("Referências")]
        [SerializeField] private MeshRenderer meshRenderer;
        
        [Header("Configurações Visuais")]
        [SerializeField] private Material matFaceUp;
        [SerializeField] private Material matFaceDown;
        [SerializeField] private float flipDuration = 0.5f;

        private Stone logicalData;
        public Stone LogicalData => logicalData;

        private bool isAnimating = false;

        public void Initialize(Stone data, Material specificIconMat)
        {
            this.logicalData = data;
            this.name = $"Stone_{data.nome}";
            this.matFaceUp = specificIconMat; // Material com a textura (ex: Espada.png)
            
            // Força a rotação inicial sem animação
            UpdateVisualInstant();
        }

        private void UpdateVisualInstant()
        {
            if (logicalData == null) return;
            // Se "virada" (escondida), o material liso/costas fica para cima (normalmente X=180 ou Z=180 depedendo do rigging do modelo)
            Vector3 targetEuler = logicalData.virada ? new Vector3(0, 0, 180f) : Vector3.zero;
            transform.localEulerAngles = targetEuler;
        }

        public void AnimateFlip(Action onComplete = null)
        {
            if (logicalData == null || isAnimating)
            {
                onComplete?.Invoke();
                return;
            }

            isAnimating = true;

            // Pula um pouco pra cima (Y) e gira 180
            Vector3 targetRotation = logicalData.virada ? new Vector3(0, 0, 180f) : Vector3.zero;

            Sequence seq = DOTween.Sequence();
            
            // 1. Sobe levemente
            seq.Append(transform.DOMoveY(transform.position.y + 0.5f, flipDuration / 2f).SetEase(Ease.OutQuad));
            // 2. Gira no ar
            seq.Join(transform.DORotate(targetRotation, flipDuration).SetEase(Ease.InOutSine));
            // 3. Desce de volta
            seq.Append(transform.DOMoveY(transform.position.y, flipDuration / 2f).SetEase(Ease.InQuad));

            seq.OnComplete(() =>
            {
                isAnimating = false;
                onComplete?.Invoke();
            });
        }

        public void AnimateMoveTo(Vector3 targetPosition, float duration = 0.6f, Action onComplete = null)
        {
            isAnimating = true;

            // Faz um arco suave p/ não atravessar outras pedras da mesa (Jump)
            transform.DOJump(targetPosition, jumpPower: 1.0f, numJumps: 1, duration)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() => 
                {
                    isAnimating = false;
                    onComplete?.Invoke();
                });
        }

        public void Highlight(bool enable)
        {
            // Opcional: Acender material emission quando o mouse passa por cima
            if (meshRenderer != null)
            {
                if (enable)
                    meshRenderer.material.EnableKeyword("_EMISSION");
                else
                    meshRenderer.material.DisableKeyword("_EMISSION");
            }
        }
    }
}
