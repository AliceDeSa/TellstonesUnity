using UnityEngine;
using Tellstones.Core;

namespace Tellstones.Visual
{
    /// <summary>
    /// Script para suavizar e focar a câmera baseado em eventos da partida.
    /// Ex: Foca no slot onde o Desafio aconteceu (zoom dramático).
    /// </summary>
    public class CameraRig : MonoBehaviour
    {
        [Header("Cinematografia")]
        [SerializeField] private Vector3 defaultPos = new Vector3(0, 8, -5);
        [SerializeField] private Vector3 defaultRot = new Vector3(60, 0, 0);

        [SerializeField] private float smoothSpeed = 2f;
        
        private Vector3 targetPos;
        private Quaternion targetRot;

        private void Start()
        {
            ResetToDefault();
        }

        public void ResetToDefault()
        {
            targetPos = defaultPos;
            targetRot = Quaternion.Euler(defaultRot);
        }

        public void FocusOnSlot(Vector3 slotWorldPosition)
        {
            // Move a câmera um pouco mais perto e para o centro relacional ao slot
            targetPos = slotWorldPosition + new Vector3(0, 3f, -2f);
            targetRot = Quaternion.LookRotation(slotWorldPosition - targetPos);
        }

        private void LateUpdate()
        {
            // Interpolação super suave
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * smoothSpeed);
        }
    }
}
