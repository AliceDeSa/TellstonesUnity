using System;

namespace Tellstones.AI
{
    public enum BotActionType
    {
        Place,
        Flip,
        Swap,
        Peek,
        Challenge,
        Boast
    }

    [Serializable]
    public struct BotAction
    {
        public BotActionType type;
        
        // Alvo principal (colocar, virar, espiar, desafiar)
        public int targetSlot; 

        // Alvos secundários (usados apenas na troca/swap)
        public int fromSlot;
        public int toSlot;

        public override string ToString()
        {
            if (type == BotActionType.Swap)
                return $"[BotAction: {type} {fromSlot}<->{toSlot}]";
            if (type == BotActionType.Boast)
                return $"[BotAction: {type}]";
            return $"[BotAction: {type} slot {targetSlot}]";
        }
    }
}
