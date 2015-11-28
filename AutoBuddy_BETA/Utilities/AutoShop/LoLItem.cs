
namespace AutoBuddy.Utilities.AutoShop
{
    internal class LoLItem
    {
        public readonly string name;
        public readonly string description;
        public readonly string sanitizedDescription;
        public readonly string plaintext;
        public readonly string[] tags;
        public readonly string cq;
        public readonly string groups;

        public readonly int id;

        public readonly int baseGold;
        public readonly int totalGold;
        public readonly int sellGold;

        public readonly bool purchasable;
        public readonly string requiredChampion;

        public readonly int[] maps;

        public readonly int[] fromItems;
        public readonly int[] intoItems;
        public readonly int depth;

        public LoLItem(string name, string description, string sanitizedDescription, string plaintext, int id, int baseGold, int totalGold, int sellGold, bool purchasable
            , string requiredChampion, int[] maps, int[] fromItems, int[] intoItems, int depth, string[] tags, string cq, string groups)
        {
            this.name = name;
            this.description = description;
            this.sanitizedDescription = sanitizedDescription;
            this.plaintext = plaintext;
            this.id = id;
            this.baseGold = baseGold;
            this.totalGold = totalGold;
            this.sellGold = sellGold;
            this.purchasable = purchasable;
            this.requiredChampion = requiredChampion;
            this.maps = maps;
            this.fromItems = fromItems;
            this.intoItems = intoItems;
            this.depth = depth;
            this.tags = tags;
            this.cq = cq;
            this.groups = groups;
        }

        public override string ToString()
        {
            return name;
        }

    }
}
