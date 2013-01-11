﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Cards;
using Sanguosha.Expansions.Basic.Cards;
using System.Threading;

namespace Sanguosha.Expansions.StarSP.Skills
{
    /// <summary>
    /// 军威–回合结束阶段开始时，若“锦”的数量达到3或更多，你可以弃置三张“锦”，并选择一名角色，该角色须选择一项：1、展示一张【闪】，然后交给一名由你指定的其他角色；2、失去1点体力，然后令你将其装备区内的一张牌移出游戏，该角色的回合结束后，将移除游戏的牌置入其装备区。
    /// </summary>
    public class JunWei : TriggerSkill
    {
        class JunWeiGiveShanVerifier : CardsAndTargetsVerifier
        {
            public JunWeiGiveShanVerifier()
            {
                MaxCards = 0;
                MinCards = 0;
                MaxPlayers = 1;
                MinPlayers = 1;
            }
        }

        class JunWeiVerifier : CardUsageVerifier
        {
            public JunWeiVerifier()
            {
                Helper.OtherDecksUsed.Add(YinLing.JinDeck);
            }
            public override VerifierResult FastVerify(Player source, ISkill skill, List<Card> cards, List<Player> players)
            {
                if (players != null && players.Count > 1)
                {
                    return VerifierResult.Fail;
                }
                if (cards != null && cards.Count > 3)
                {
                    return VerifierResult.Fail;
                }
                if (cards == null || players == null || players.Count == 0)
                {
                    return VerifierResult.Partial;
                }
                if (cards.Any(c => c.Place.DeckType != YinLing.JinDeck))
                {
                    return VerifierResult.Fail;
                }
                if (cards.Count < 3)
                    return VerifierResult.Partial;
                return VerifierResult.Success;
            }
            public override IList<CardHandler> AcceptableCardTypes
            {
                get { return null; }
            }
        }

        class JunWeiShowCardVerifier : CardUsageVerifier
        {
            public override VerifierResult FastVerify(Player source, ISkill skill, List<Card> cards, List<Player> players)
            {
                if (skill != null || (players != null && players.Count != 0))
                {
                    return VerifierResult.Fail;
                }
                if (cards != null && cards.Count > 1)
                {
                    return VerifierResult.Fail;
                }
                if (cards == null || cards.Count == 0)
                {
                    return VerifierResult.Partial;
                }
                if (cards[0].Place.DeckType != DeckType.Hand)
                {
                    return VerifierResult.Fail;
                }
                if (!(cards[0].Type is Shan))
                {
                    return VerifierResult.Fail;
                }
                return VerifierResult.Success;
            }
            public override IList<CardHandler> AcceptableCardTypes
            {
                get { return null; }
            }
        }

        void Run(Player Owner, GameEvent gameEvent, GameEventArgs eventArgs)
        {
            ISkill skill;
            List<Card> cards;
            List<Player> players;
            if (Owner.AskForCardUsage(new CardUsagePrompt("JunWei"), new JunWeiVerifier(), out skill, out cards, out players))
            {
                NotifySkillUse();
                Game.CurrentGame.HandleCardDiscard(Owner, cards);
                Player target = players[0];
                if (target.AskForCardUsage(new CardUsagePrompt("JunWeiShowCard"), new JunWeiShowCardVerifier(), out skill, out cards, out players))
                {
                    Card temp = cards[0];
                    Game.CurrentGame.NotificationProxy.NotifyShowCard(target, temp);
                    if (!Owner.AskForCardUsage(new CardUsagePrompt("JunWeiGiveShan"), new JunWeiGiveShanVerifier(), out skill, out cards, out players))
                    {
                        players = new List<Player>() { Owner };
                    }
                    Game.CurrentGame.SyncCardAll(ref temp);
                    Game.CurrentGame.HandleCardTransferToHand(target, players[0], new List<Card>() { temp });
                }
                else
                {
                    Game.CurrentGame.LoseHealth(target, 1);
                    if (target.Equipments().Count == 0) return;
                    Thread.Sleep(380);
                    List<List<Card>> answer;
                    List<DeckPlace> sourceDecks = new List<DeckPlace>();
                    sourceDecks.Add(new DeckPlace(target, DeckType.Equipment));
                    if (!Owner.AskForCardChoice(new CardChoicePrompt("JunWeiChoice", target, Owner),
                        sourceDecks,
                        new List<string>() { "JunWei" },
                        new List<int>() { 1 },
                        new RequireOneCardChoiceVerifier(),
                        out answer))
                    {
                        answer = new List<List<Card>>();
                        answer.Add(new List<Card>());
                        answer[0].Add(target.Equipments().First());
                    }
                    Game.CurrentGame.HandleCardTransfer(target, target, JunWeiDeck, answer[0]);
                }
            }
        }

        void InstallEquipment(Player p, Card card)
        {
            CardsMovement attachMove = new CardsMovement();
            attachMove.Cards = new List<Card>();
            attachMove.Cards.Add(card);
            attachMove.To = new DeckPlace(p, DeckType.Equipment);
            foreach (Card c in p.Equipments())
            {
                if (c.Type.IsCardCategory(card.Type.Category))
                {
                    Game.CurrentGame.EnterAtomicContext();
                    Game.CurrentGame.HandleCardDiscard(p, new List<Card>() { c });
                    Game.CurrentGame.MoveCards(attachMove);
                    Game.CurrentGame.PlayerAcquiredCard(p, new List<Card>() { card });
                    Game.CurrentGame.ExitAtomicContext();
                    return;
                }
            }
            Game.CurrentGame.MoveCards(attachMove);
            Game.CurrentGame.PlayerAcquiredCard(p, new List<Card>() { card });
            return;
        }

        class LoseJunWei : Trigger
        {
            public override void Run(GameEvent gameEvent, GameEventArgs eventArgs)
            {
                if (eventArgs.Source != Owner)
                {
                    return;
                }
                List<ISkill> allSkills = new List<ISkill>();
                if (Owner.Hero != null)
                {
                    allSkills.AddRange(Owner.Hero.Skills);
                }
                if (Owner.Hero2 != null)
                {
                    allSkills.AddRange(Owner.Hero2.Skills);
                }
                allSkills.AddRange(Owner.AdditionalSkills);
                if (allSkills.Any(s => s is JunWei))
                {
                    return;
                }
                DiscardedTempCard();
            }

            public LoseJunWei(Player p)
            {
                Owner = p;
            }
        }

        private static void DiscardedTempCard()
        {
            foreach (Player pl in Game.CurrentGame.AlivePlayers)
            {
                if (Game.CurrentGame.Decks[pl, JunWeiDeck].Count > 0)
                {
                    Game.CurrentGame.HandleCardDiscard(pl, Game.CurrentGame.Decks[pl, JunWeiDeck]);
                }
            }
        }

        public JunWei()
        {
            var trigger = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return Game.CurrentGame.Decks[p, YinLing.JinDeck].Count >= 3; },
                Run,
                TriggerCondition.OwnerIsSource
            ) { AskForConfirmation = false, IsAutoNotify = false };
            Triggers.Add(GameEvent.PhaseBeginEvents[TurnPhase.End], trigger);

            var trigger2 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { return Game.CurrentGame.Decks[a.Source, JunWeiDeck].Count > 0; },
                (p, e, a) =>
                {
                    List<Card> cards = new List<Card>(Game.CurrentGame.Decks[a.Source, JunWeiDeck]);
                    cards.Reverse();
                    foreach (Card c in cards)
                    {
                        InstallEquipment(a.Source, c);
                    }
                },
                TriggerCondition.Global
            ) { AskForConfirmation = false, IsAutoNotify = false };
            Triggers.Add(GameEvent.PhasePostEnd, trigger2);

            var trigger3 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { DiscardedTempCard(); },
                TriggerCondition.OwnerIsTarget
            ) { AskForConfirmation = false, IsAutoNotify = false };
            Triggers.Add(GameEvent.PlayerIsDead, trigger3);

            var trigger4 = new AutoNotifyPassiveSkillTrigger(
                this,
                (p, e, a) => { Game.CurrentGame.RegisterTrigger(GameEvent.PlayerSkillSetChanged, new LoseJunWei(p)); },
                TriggerCondition.OwnerIsSource
            ) { AskForConfirmation = false, IsAutoNotify = false };
            Triggers.Add(GameEvent.PlayerGameStartAction, trigger4);

            IsAutoInvoked = null;
        }

        public static PrivateDeckType JunWeiDeck = new PrivateDeckType("JunWei", true);
    }
}