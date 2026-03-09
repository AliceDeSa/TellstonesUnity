using System;
using System.Collections.Generic;
using UnityEngine;
using Tellstones.Core;
using Tellstones.Core.Events;
using Tellstones.Core.Domain;

namespace Tellstones.Visual
{
    /// <summary>
    /// Escuta aos Eventos Estáticos de Jogo Puros (MVC) e engatilha a Criação/Animação Visual na Unity
    /// </summary>
    public class BoardView : MonoBehaviour
    {
        [Header("Pontos do Tabuleiro (Slots)")]
        [SerializeField] private Transform[] slotPositions = new Transform[7];
        
        [Header("Prefabs e Materiais")]
        [SerializeField] private GameObject stonePrefab;
        [SerializeField] private Material[] iconMaterials; // Indexados por nome ou mapeados via Dictionary num ScriptableObject real

        // Dicionário mantendo a view ativa por slot lógico (Mesa)
        private Dictionary<int, StoneView> stoneViews = new Dictionary<int, StoneView>();

        private void OnEnable()
        {
            GameEvents.OnStateChanged += SyncFullBoard;
            GameEvents.OnStonePlaced += HandleStonePlaced;
            GameEvents.OnStoneFlipped += HandleStoneFlipped;
            GameEvents.OnStonesSwapped += HandleStonesSwapped;
        }

        private void OnDisable()
        {
            GameEvents.OnStateChanged -= SyncFullBoard;
            GameEvents.OnStonePlaced -= HandleStonePlaced;
            GameEvents.OnStoneFlipped -= HandleStoneFlipped;
            GameEvents.OnStonesSwapped -= HandleStonesSwapped;
        }

        /// <summary>
        /// Sincronização forçada, geralmente de Restart/Load de Save
        /// </summary>
        private void SyncFullBoard(GameState state)
        {
            // Apaga as antigas
            foreach (var kvp in stoneViews)
            {
                if (kvp.Value != null) Destroy(kvp.Value.gameObject);
            }
            stoneViews.Clear();

            // Cria Novas sem animação imediata
            for (int i = 0; i < 7; i++)
            {
                if (state.mesa[i] != null && !string.IsNullOrEmpty(state.mesa[i].nome))
                {
                    InstantiateStoneVisual(i, state.mesa[i]);
                }
            }
        }

        private void HandleStonePlaced(int slot, Stone logicalStone)
        {
            var view = InstantiateStoneVisual(slot, logicalStone);
            
            // Efeito visual de entrada (vem de cima?)
            Vector3 finalPos = slotPositions[slot].position;
            view.transform.position = finalPos + Vector3.up * 5f;
            view.AnimateMoveTo(finalPos, 0.5f);
        }

        private StoneView InstantiateStoneVisual(int slot, Stone logicalStone)
        {
            var go = Instantiate(stonePrefab, slotPositions[slot].position, Quaternion.identity, this.transform);
            var view = go.GetComponent<StoneView>();
            
            Material targetMat = GetMaterialFor(logicalStone.nome);
            view.Initialize(logicalStone, targetMat);
            
            stoneViews[slot] = view;
            return view;
        }

        private void HandleStoneFlipped(int slot)
        {
            if (stoneViews.TryGetValue(slot, out var view))
            {
                // A pedra lógica (logicalData da view) já foi invertida na memória pelo ApplyFlip do Core
                view.AnimateFlip();
            }
        }

        private void HandleStonesSwapped(int slotA, int slotB)
        {
            if (stoneViews.TryGetValue(slotA, out var viewA) && stoneViews.TryGetValue(slotB, out var viewB))
            {
                Vector3 posA = slotPositions[slotA].position;
                Vector3 posB = slotPositions[slotB].position;

                // Anima visualmente (arco)
                viewA.AnimateMoveTo(posB, 0.6f);
                viewB.AnimateMoveTo(posA, 0.6f);

                // Troca no dicionário do visual
                stoneViews[slotB] = viewA;
                stoneViews[slotA] = viewB;
            }
        }

        private Material GetMaterialFor(string stoneName)
        {
            // Simplificado: ideal ter um ScriptableObject <Atlas> p/ ligar Txt->Mat
            // Mapearemos na Unity Array os Mats pela Ordem ou Nome na próxima fase visual.
            if (iconMaterials == null || iconMaterials.Length == 0) return null;
            return iconMaterials[0]; 
        }
    }
}
