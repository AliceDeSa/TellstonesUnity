using System;
using System.Collections.Generic;
using Tellstones.Core.Domain;

namespace Tellstones.Core.Rules
{
    public static class GameRules
    {
        /// <summary>
        /// Retorna uma lista de índices válidos (0 a 6) para inserir uma nova pedra na mesa.
        /// Uma pedra só pode ser colocada adjacente a uma pedra existente.
        /// Se a mesa estiver vazia, apenas o centro (posição 3) é válido.
        /// </summary>
        public static List<int> GetValidSlots(Stone[] mesa)
        {
            List<int> validos = new List<int>();
            bool hasStone = false;
            
            // Verifica se a mesa está vazia
            for(int i = 0; i < 7; i++)
            {
                if (mesa[i] != null && !string.IsNullOrEmpty(mesa[i].nome))
                {
                    hasStone = true;
                    break;
                }
            }

            // Se estiver vazia, apenas o slot central é permitido
            if (!hasStone)
            {
                validos.Add(3);
                return validos;
            }

            // Se houver pedras, busca slots adjacentes
            for (int i = 0; i < 7; i++)
            {
                // Pular slots ocupados
                if (mesa[i] != null && !string.IsNullOrEmpty(mesa[i].nome)) 
                    continue;

                // Verificar adjacência
                bool hasLeft = (i > 0 && mesa[i - 1] != null && !string.IsNullOrEmpty(mesa[i - 1].nome));
                bool hasRight = (i < 6 && mesa[i + 1] != null && !string.IsNullOrEmpty(mesa[i + 1].nome));

                if (hasLeft || hasRight)
                {
                    validos.Add(i);
                }
            }

            return validos;
        }
    }
}
