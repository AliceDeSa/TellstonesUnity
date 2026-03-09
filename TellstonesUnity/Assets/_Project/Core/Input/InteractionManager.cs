using System;
using Tellstones.Core;
using Tellstones.Core.FSM.States;
using UnityEngine;

namespace Tellstones.InputSystem
{
    /// <summary>
    /// Lida com raycasts de tela -> mundo físico pra simular o clique nos slots.
    /// Interage ativamente com o MatchManager e as StateMachines pra disparar as decisões humanas.
    /// </summary>
    public class InteractionManager : MonoBehaviour
    {
        [Header("Configurações")]
        [SerializeField] private LayerMask slotLayer;
        [SerializeField] private LayerMask stoneLayer;
        
        [Header("Referências")]
        [SerializeField] private Camera mainCamera;

        private void Awake()
        {
            if (mainCamera == null) mainCamera = Camera.main;
        }

        private void Update()
        {
            // Só faz raycast e envia ações SE a Máquina de Estado disser que é o turno do Humano
            if (MatchManager.Instance.StateMachine.CurrentState is State_PlayerTurn)
            {
                HandlePlayerInput();
            }
        }

        private void HandlePlayerInput()
        {
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                Ray ray = mainCamera.ScreenPointToRay(UnityEngine.Input.mousePosition);
                RaycastHit hit;

                // 1. Tentar clicar em uma PEDRA primeiro (para Flip, Swap, Boast, etc via UI futura ativada pela Pedra)
                if (Physics.Raycast(ray, out hit, 100f, stoneLayer))
                {
                    Debug.Log($"[Raycast] Clicou numa Pedra via Collider: {hit.collider.name}");
                    // Abrir Context Menu UI flutuante (Virar, Espiar, Desafiar)
                }
                // 2. Se clicou no SLOT VAZIO (para Colocar)
                else if (Physics.Raycast(ray, out hit, 100f, slotLayer))
                {
                    // Achar um comp de SlotInfo
                    var slotTarget = clickSlotParser(hit.collider); 
                    if (slotTarget >= 0)
                    {
                        Debug.Log($"[Raycast] Clicou no Slot Vazio {slotTarget}");
                        // Requisitar ação de Place real pro motor. 
                        // Ex: MatchManager.Instance.ApplyPlacement(0, slotTarget);
                        //     MatchManager.Instance.EndTurn();
                    }
                }
            }
        }

        private int clickSlotParser(Collider col)
        {
            // Ideal seria o Collider ter um componente "SlotCollider { public int slotIndex; }"
            // Usaremos nomes como Slot0, Slot1 pra parse rápido temporário
            if (col.name.StartsWith("Slot"))
            {
                if (int.TryParse(col.name.Replace("Slot", ""), out int id))
                    return id;
            }
            return -1;
        }
    }
}
