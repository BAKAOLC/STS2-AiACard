namespace STS2_AiACard
{
    public static class Const
    {
        /// <summary>
        ///     与原版 <c>CardPileCmd.Draw</c> / <c>Dredge</c> 一致的单场战斗手牌张数上限。
        /// </summary>
        public const int CombatHandMax = 10;

        public const string ModId = "STS2-AiACard";
        public const string Name = "AiACard";
        public const string Version = "0.1.0";

        public static class Paths
        {
            public const string Root = "res://STS2_AiACard";
            public const string PlaceholderPortrait = Root + "/cards/placeholder.png";
        }
    }
}
