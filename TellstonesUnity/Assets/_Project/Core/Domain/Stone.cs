using System;

namespace Tellstones.Core.Domain
{
    [Serializable]
    public class Stone
    {
        public string nome;
        public bool virada;
        public int slot;
        
        // Propriedades úteis para o Unity futuramente (ex: identificar dono)
        public int dono; 
        public string imagePath; 
    }
}
