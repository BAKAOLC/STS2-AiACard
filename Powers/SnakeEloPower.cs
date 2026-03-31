using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace STS2_AiACard.Powers
{
    /// <summary>
    ///     蛇咬 ELO：攻击牌在打出该能力所属牌时变换；战斗胜利后在结算奖励阶段追加一组选卡（<c>CombatRoom.AddExtraReward</c> + <c>CardReward</c>，同原版狩猎/版税等）。
    /// </summary>
    public sealed class SnakeEloPower : AiACardPowerBase
    {
        public override PowerType Type => PowerType.Buff;

        public override PowerStackType StackType => PowerStackType.Single;

        public override Task AfterCombatEnd(CombatRoom room)
        {
            if (Owner.Player == null)
                return Task.CompletedTask;

            // 与 TheHunt / RewardsSet 一致：当前房间类型的卡池、3 张备选（选 1 入组）。
            room.AddExtraReward(Owner.Player,
                new CardReward(CardCreationOptions.ForRoom(Owner.Player, room.RoomType), 3, Owner.Player));
            return Task.CompletedTask;
        }
    }
}
