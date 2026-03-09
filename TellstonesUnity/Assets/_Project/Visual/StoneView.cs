using System;
using Tellstones.Core.Domain;
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

            Vector3 targetRotation = logicalData.virada ? new Vector3(0, 0, 180f) : Vector3.zero;
            
            // FIXME: Implementação Real do DOTween aguardando importação do Asset no Editor
            transform.localEulerAngles = targetRotation;
            isAnimating = false;
            onComplete?.Invoke();
        }

        public void AnimateMoveTo(Vector3 targetPosition, float duration = 0.6f, Action onComplete = null)
        {
            isAnimating = true;

            // FIXME: Implementação Real do DOTween aguardando importação do Asset no Editor
            transform.position = targetPosition;
            isAnimating = false;
            onComplete?.Invoke();
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
