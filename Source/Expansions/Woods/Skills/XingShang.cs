﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Battle.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;

namespace Sanguosha.Expansions.Woods.Skills
{
    /// <summary>
    /// 行殇-你可以获得死亡角色的所有牌。
    /// </summary>
    public class XingShang : TriggerSkill
    {
        public XingShang()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => 
                {
                    if (a.Targets[0] != p)
                    {
                        List<Card> toGet = new List<Card>();
                        toGet.AddRange(Game.CurrentGame.Decks[a.Targets[0], DeckType.Equipment]);
                        toGet.AddRange(Game.CurrentGame.Decks[a.Targets[0], DeckType.Hand]);
                        Game.CurrentGame.HandleCardTransferToHand(null, p, toGet);
                    }
                },
                TriggerCondition.Global
            );

            Triggers.Add(GameEvent.PlayerIsDead, trigger);
            IsAutoInvoked = true;
        }
    }
}