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
        public const string Version = "0.1.2";

        public static class Paths
        {
            public const string Root = "res://STS2_AiACard";
            public const string PlaceholderPortrait = Root + "/cards/placeholder.png";

            /// <summary>卡面资源目录 <c>STS2_AiACard/cards/</c>，文件名与简中牌名一致。</summary>
            public static class CardPortraits
            {
                private const string C = Root + "/cards/";

                public const string GnawBite = C + "001_啃咬.png";
                public const string MaliceSpread = C + "002_恶意弥漫.png";
                public const string HollowDebt = C + "003_虚妄代偿.png";
                public const string PlagueResonance = C + "004_荒疫共鸣.png";
                public const string SerpentArray = C + "005_万蛇阵.png";
                public const string VeiledForm = C + "006_隐秘形态.png";
                public const string ForgeChorus = C + "007_全力锻造.png";
                public const string BlossomBlades = C + "008_剑花纷飞.png";
                public const string PracticePerfect = C + "009_熟能生巧.png";
                public const string KingDislikesMath = C + "010_王不喜算术.png";
                public const string BoneSoloRock = C + "011_骨独摇滚.png";
                public const string UnderworldDraw = C + "012_冥域抽取.png";
                public const string DreamReturn = C + "013_游梦回魂.png";
                public const string LearningJoy = C + "014_必有我师.png";
                public const string WinterSlam = C + "015_凛冬打击.png";
                public const string BootSequence = C + "016_别吵，还在启动.png";
                public const string SnakeElo = C + "017_蛇咬ELO.png";
                public const string RenSword = C + "018_仁之剑.png";
                public const string YiSword = C + "019_义之剑.png";
                public const string PostCombatTreatment = C + "020_准备治疗.png";
                public const string HeavenGift = C + "021_天降恩赐.png";
                public const string SnakeContract = C + "022_异蛇契约.png";
                public const string ManipulationReality = C + "023_操控现实.png";
            }

            public static class PowerIcons
            {
                private const string P = Root + "/powers/";

                public const string BlossomBlades = P + "011_剑花纷飞.png";
                public const string HeavenGift = P + "012_天降恩赐.png";
                public const string KingDislikesMath = P + "013_王不喜算术.png";
                public const string ManipulationReality = P + "014_操控现实.png";
                public const string PlagueResonance = P + "015_荒疫共鸣.png";
                public const string PostCombatHeal = P + "016_准备治疗.png";
                public const string RequiemEnergy = P + "017_骨独摇滚.png";
                public const string SnakeContractEntropy = P + "018_异蛇契约.png";
                public const string SnakeElo = P + "019_蛇咬ELO.png";
                public const string SwordPractice = P + "020_熟能生巧.png";
                public const string VeiledForm = P + "021_隐秘形态.png";
            }
        }
    }
}
