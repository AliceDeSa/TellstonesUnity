using System;

namespace Tellstones.Core.Domain
{
    [Serializable]
    public class Player
    {
        public string id;
        public string nome;
        public int pontos;
        public bool isBot;
    }
}
