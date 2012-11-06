﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sanguosha.Core.Triggers;
using Sanguosha.Core.Cards;
using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Expansions.Basic.Cards;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;

namespace Sanguosha.Expansions.Basic.Skills
{
    /// <summary>
    /// 激将―主公技，当你需要使用或打出一张【杀】时，你可令其他蜀势力角色打出一张【杀】(视为由你使用或打出)。
    /// </summary>
    public class JiJiang : CardTransformSkill
    {
        public override UiHelper Helper { get { return new UiHelper() { HasNoConfirmation = true }; } }
        public override VerifierResult TryTransform(List<Card> cards, object arg, out CompositeCard card)
        {
            card = null;
            if (cards != null && cards.Count != 0)
            {
                return VerifierResult.Fail;
            }
            List<Player> toProcess = new List<Player>(Game.CurrentGame.AlivePlayers);
            toProcess.Remove(Owner);
            bool noShuHero = true;
            foreach (var player in toProcess)
            {
                if (player.Hero.Allegiance == Core.Heroes.Allegiance.Shu)
                {
                    noShuHero = false;
                    break;
                }
            };

            if (noShuHero)
            {
                return VerifierResult.Fail;
            }

            card = new CompositeCard();
            if (cards == null)
            {
                card.Subcards = new List<Card>();
            }
            else
            {
                card.Subcards = new List<Card>(cards);
            }
            card.Type = new Sha();
            return VerifierResult.Success;
        }

        protected override bool DoTransformSideEffect(CompositeCard card, object arg, List<Player> targets)
        {
            ICard result = null;
            List<Player> toProcess = new List<Player>(Game.CurrentGame.AlivePlayers);
            toProcess.Remove(Owner);
            Game.CurrentGame.SortByOrderOfComputation(Owner, toProcess);
            bool noAnswer = true;
            foreach (var player in toProcess)
            {
                if (player.Hero.Allegiance == Core.Heroes.Allegiance.Shu)
                {
                    bool failToRespond = false;
                    while (true)
                    {
                        IUiProxy ui = Game.CurrentGame.UiProxies[player];
                        SingleCardUsageVerifier v1 = new SingleCardUsageVerifier((c) => { return c.Type is Sha; });
                        ISkill skill;
                        List<Player> p;
                        List<Card> cards;
                        if (!ui.AskForCardUsage(new CardUsagePrompt("JiJiang", Owner), v1, out skill, out cards, out p))
                        {
                            failToRespond = true;
                            break;
                        }
                        if (!Game.CurrentGame.CommitCardTransform(player, skill, cards, out result, targets))
                        {
                            continue;
                        }
                        noAnswer = false;
                        Trace.TraceInformation("Player {0} Responded JiJiang with SHA, ", player.Id);
                        break;
                    }
                    if (failToRespond)
                    {
                        continue;
                    }
                    break;
                }
            }

            if (noAnswer)
            {
                return false;
            }

            Trace.Assert(result != null);
            card.Subcards = new List<Card>();
            if (result is CompositeCard)
            {
                card.Subcards.AddRange(((CompositeCard)result).Subcards);
                card.Type = ((CompositeCard)result).Type;
            }
            else
            {
                Trace.Assert(result is Card);
                card.Subcards.Add((Card)result);
                card.Type = ((Card)result).Type;
            }
            return true;
        }

        public override List<CardHandler> PossibleResults
        {
            get { return new List<CardHandler>() { new Sha() }; }
        }

        public override bool IsRulerOnly
        {
            get
            {
                return true;
            }
        }
    }
}
