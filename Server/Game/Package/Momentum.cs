﻿using CommonClass;
using CommonClass.Game;
using CommonClassLibrary;
using SanguoshaServer.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using static CommonClass.Game.Player;

namespace SanguoshaServer.Package
{
    public class Momentum : GeneralPackage
    {
        public Momentum() : base("Momentum")
        {
            skills = new List<Skill>
            {
                new Xunxun(),
                new Wangxi(),
                new Hengjiang(),
                new HengjiangDraw(),
                new HengjiangFail(),
                new HengjiangMaxCards(),
                new Qianxi(),
                new QianxiClear(),
                new Mashu("madai"),
                new Guixiu(),
                new Cunsi(),
                new Yongjue(),
                new Jiang(),
                new Yingyang(),
                new Hunshang(),
                new HunshangRemove(),
                new Yingzi("sunce", true),
                new YingziMax("sunce"),
                new Yinghun("sunce"),
                new Duanxie(),
                new Fenming(),
                new Hengzheng(),
                new Baoling(),
                new Benghuai(),
                new Chuanxin(),
                new Fengshi(),
                new Wuxin(),
                new Wendao(),
                new Hongfa(),
                new HongfaClear(),
                new HongfaSlash(),
            };
            skill_cards = new List<FunctionCard>
            {
                new CunsiCard(),
                new DuanxieCard(),
                new FengshiSummon(),
                new WendaoCard()
            };
            related_skills = new Dictionary<string, List<string>> {
                { "hengjiang", new List<string>{"#hengjiang-draw", "#hengjiang-fail"} },
                { "qianxi", new List<string>{ "#qianxi-clear"} },
                {  "hunshang", new List<string>{ "#hunshang"} },
                { "hongfa", new List<string>{ "#hongfa-clear" } }
            };
        }
    }

    public class Xunxun : PhaseChangeSkill
    {
        public Xunxun() : base("xunxun")
        {
            frequency = Frequency.Frequent;
        }
        public override TriggerStruct Triggerable(TriggerEvent triggerEvent, Room room, Player lidian, ref object data, Player ask_who)
        {
            return (base.Triggerable(lidian, room) && lidian.Phase == Player.PlayerPhase.Draw) ? new TriggerStruct(Name, lidian) : new TriggerStruct();
        }
        public override TriggerStruct Cost(TriggerEvent triggerEvent, Room room, Player lidian, ref object data, Player ask_who, TriggerStruct info)
        {
            if (room.AskForSkillInvoke(lidian, Name, null, info.SkillPosition))
            {
                room.BroadcastSkillInvoke(Name, lidian, info.SkillPosition);
                return info;
            }

            return new TriggerStruct();
        }
        public override bool OnPhaseChange(Room room, Player lidian, TriggerStruct info)
        {
            List<int> card_ids = room.GetNCards(4);
            LogMessage log = new LogMessage
            {
                Type = "$ViewDrawPile",
                From = lidian.Name,
                Card_str = string.Join("+", JsonUntity.IntList2StringList(card_ids))
            };
            room.SendLog(log, room.GetOtherPlayers(lidian));
            log.Type = "$ViewDrawPile2";
            log.Arg = "4";
            room.SendLog(log, new List<Player> { lidian });

            AskForMoveCardsStruct result = room.AskForMoveCards(lidian, card_ids, new List<int>(), true, Name, 2, 2, false, true, new List<int> { -1 }, info.SkillPosition);

            room.ReturnToDrawPile(result.Top, true, lidian);
            room.ReturnToDrawPile(result.Bottom, false, lidian);
            LogMessage a;
            a.Type = "#XunxunGuanxingResult";
            a.From = lidian.Name;
            room.SendLog(a);

            log = new LogMessage
            {
                Type = "$GuanxingTop",
                From = lidian.Name,
                Card_str = string.Join("+", JsonUntity.IntList2StringList(result.Bottom))
            };
            room.SendLog(log, lidian);

            LogMessage b = new LogMessage
            {
                Type = "$GuanxingBottom",
                From = lidian.Name,
                Card_str = string.Join("+", JsonUntity.IntList2StringList(result.Top))
            };
            room.SendLog(b, lidian);
            return false;

        }
    }
    public class Wangxi : TriggerSkill
    {
        public Wangxi() : base("wangxi")
        {
            events = new List<TriggerEvent> { TriggerEvent.Damage, TriggerEvent.Damaged };
            skill_type = SkillType.Replenish;
        }
        public override TriggerStruct Triggerable(TriggerEvent triggerEvent, Room room, Player player, ref object data, Player ask_who)
        {
            if (base.Triggerable(player, room) && data is DamageStruct damage)
            {
                Player target = null;
                if (triggerEvent == TriggerEvent.Damage)
                    target = damage.To;
                else
                    target = damage.From;
                if (target != null && target.Alive && target != player && !target.HasFlag("Global_DFDebut"))
                {

                    TriggerStruct trigger = new TriggerStruct(Name, player)
                    {
                        Times = damage.Damage
                    };
                    return trigger;
                }
            }
            return new TriggerStruct();
        }
        public override TriggerStruct Cost(TriggerEvent triggerEvent, Room room, Player player, ref object data, Player ask_who, TriggerStruct info)
        {
            DamageStruct damage = (DamageStruct)data;
            Player target = null;
            if (triggerEvent == TriggerEvent.Damage)
                target = damage.To;
            else
                target = damage.From;
            if (room.AskForSkillInvoke(player, Name, target, info.SkillPosition))
            {
                room.DoAnimate(CommonClassLibrary.AnimateType.S_ANIMATE_INDICATE, player.Name, target.Name);
                GeneralSkin gsk = RoomLogic.GetGeneralSkin(room, player, Name, info.SkillPosition);
                room.BroadcastSkillInvoke(Name, "male", (triggerEvent == TriggerEvent.Damage) ? 2 : 1, gsk.General, gsk.SkinId);
                return info;
            }

            return new TriggerStruct();
        }
        public override bool Effect(TriggerEvent triggerEvent, Room room, Player player, ref object data, Player ask_who, TriggerStruct info)
        {
            DamageStruct damage = (DamageStruct)data;
            Player target = null;
            if (triggerEvent == TriggerEvent.Damage)
                target = damage.To;
            else
                target = damage.From;
            List<Player> players = new List<Player> { player, target };
            room.SortByActionOrder(ref players);

            room.DrawCards(players, new List<DrawCardStruct> { new DrawCardStruct(1, player, Name), new DrawCardStruct(1, player, Name) });

            return false;
        }
    }
    public class Hengjiang : MasochismSkill
    {
        public Hengjiang() : base("hengjiang")
        {
            skill_type = SkillType.Defense;
        }
        public override TriggerStruct Triggerable(TriggerEvent triggerEvent, Room room, Player player, ref object data, Player ask_who)
        {
            if (base.Triggerable(player, room))
            {
                Player current = room.Current;
                if (current != null && current.Alive && current.Phase != Player.PlayerPhase.NotActive && data is DamageStruct damage)
                {
                    TriggerStruct trigger = new TriggerStruct(Name, player)
                    {
                        Times = damage.Damage
                    };
                    return trigger;
                }
            }
            return new TriggerStruct();
        }
        public override TriggerStruct Cost(TriggerEvent triggerEvent, Room room, Player player, ref object data, Player ask_who, TriggerStruct info)
        {
            Player current = room.Current;
            if (current != null && room.AskForSkillInvoke(player, Name, current, info.SkillPosition))
            {
                room.DoAnimate(CommonClassLibrary.AnimateType.S_ANIMATE_INDICATE, player.Name, current.Name);
                room.BroadcastSkillInvoke(Name, player, info.SkillPosition);
                return info;
            }

            return new TriggerStruct();
        }
        public override void OnDamaged(Room room, Player target, DamageStruct damage, TriggerStruct info)
        {
            Player current = room.Current;
            if (current == null) return;
            room.AddPlayerMark(current, "@hengjiang");
            room.SetPlayerMark(target, "HengjiangInvoke", 1);
        }
    }
    public class HengjiangDraw : TriggerSkill
    {
        public HengjiangDraw() : base("#hengjiang-draw")
        {
            events = new List<TriggerEvent> { TriggerEvent.TurnStart, TriggerEvent.CardsMoveOneTime, TriggerEvent.EventPhaseChanging };
            frequency = Frequency.Compulsory;
        }
        public override void Record(TriggerEvent triggerEvent, Room room, Player player, ref object data)
        {
            if (triggerEvent == TriggerEvent.EventPhaseChanging && data is PhaseChangeStruct change && change.To == Player.PlayerPhase.NotActive)
            {
                List<Player> zangbas = new List<Player>();
                foreach (Player p in room.GetAllPlayers())
                {
                    if (p.GetMark("HengjiangInvoke") > 0)
                    {
                        zangbas.Add(p);
                    }
                }
                if (zangbas.Count > 0 && player.GetMark("@hengjiang") > 0 && !player.HasFlag("HengjiangDiscarded"))
                {
                    LogMessage log = new LogMessage
                    {
                        Type = "#HengjiangDraw",
                        From = player.Name,
                        To = new List<string>(),
                        Arg = "hengjiang"
                    };
                    foreach (Player p in zangbas)
                        log.To.Add(p.Name);
                    room.SendLog(log);
                }
            }
        }
        public override List<TriggerStruct> Triggerable(TriggerEvent triggerEvent, Room room, Player player, ref object data)
        {
            List<TriggerStruct> skill_list = new List<TriggerStruct>();
            if (triggerEvent == TriggerEvent.TurnStart && player != null)
            {
                room.SetPlayerMark(player, "@hengjiang", 0);
                foreach (Player p in room.GetAllPlayers())
                    if (p.GetMark("HengjiangInvoke") > 0)
                        room.SetPlayerMark(p, "HengjiangInvoke", 0);
            }
            else if (triggerEvent == TriggerEvent.CardsMoveOneTime && data is CardsMoveOneTimeStruct move && move.From != null && move.From.Phase == Player.PlayerPhase.Discard
                && (move.Reason.Reason & CardMoveReason.MoveReason.S_MASK_BASIC_REASON) == CardMoveReason.MoveReason.S_REASON_DISCARD)
            {
                move.From.SetFlags("HengjiangDiscarded");
            }
            else if (triggerEvent == TriggerEvent.EventPhaseChanging && player != null && data is PhaseChangeStruct change && change.To == Player.PlayerPhase.NotActive)
            {
                List<Player> zangbas = new List<Player>();
                foreach (Player p in room.GetAllPlayers())
                {
                    if (p.GetMark("HengjiangInvoke") > 0)
                    {
                        zangbas.Add(p);
                    }
                }
                if (zangbas.Count > 0 && player.GetMark("@hengjiang") > 0 && !player.HasFlag("HengjiangDiscarded"))
                    foreach (Player zangba in zangbas)
                        skill_list.Add(new TriggerStruct(Name, zangba));
            }
            return skill_list;
        }
        public override bool Effect(TriggerEvent triggerEvent, Room room, Player player, ref object data, Player ask_who, TriggerStruct info)
        {
            room.SetPlayerMark(ask_who, "HengjiangInvoke", 0);
            room.SetPlayerMark(player, "@hengjiang", 0);
            room.DrawCards(ask_who, 1, "hengjiang");
            return false;
        }
    }
    public class HengjiangFail : TriggerSkill
    {
        public HengjiangFail() : base("#hengjiang-fail")
        {
            events.Add(TriggerEvent.EventPhaseChanging);
        }
        public override int GetPriority() => -1;
        public override List<TriggerStruct> Triggerable(TriggerEvent triggerEvent, Room room, Player player, ref object data)
        {
            if (data is PhaseChangeStruct change && change.To == Player.PlayerPhase.NotActive)
            {
                if (player.GetMark("@hengjiang") > 0)
                {
                    player.SetFlags("-HengjiangDiscarded");
                    room.SetPlayerMark(player, "@hengjiang", 0);
                }
                foreach (Player p in room.GetAllPlayers())
                    if (p.GetMark("HengjiangInvoke") > 0)
                        room.SetPlayerMark(p, "HengjiangInvoke", 0);
            }
            return new List<TriggerStruct>();
        }
    }
    public class HengjiangMaxCards : MaxCardsSkill
    {
        public HengjiangMaxCards() : base("#hengjiang-maxcard")
        {
        }
        public override int GetExtra(Room room, Player target)
        {
            return -target.GetMark("@hengjiang");
        }
    }
    public class Qianxi : TriggerSkill
    {
        public Qianxi() : base("qianxi")
        {
            events = new List<TriggerEvent> { TriggerEvent.EventPhaseStart };
            frequency = Frequency.Frequent;
            skill_type = SkillType.Attack;
        }
        public override bool CanPreShow() => false;
        public override TriggerStruct Triggerable(TriggerEvent triggerEvent, Room room, Player player, ref object data, Player ask_who)
        {
            if (triggerEvent == TriggerEvent.EventPhaseStart && base.Triggerable(player, room) && player.Phase == Player.PlayerPhase.Start)
                return new TriggerStruct(Name, player);

            return new TriggerStruct();
        }
        public override TriggerStruct Cost(TriggerEvent triggerEvent, Room room, Player player, ref object data, Player ask_who, TriggerStruct info)
        {
            if (room.AskForSkillInvoke(player, Name, null, info.SkillPosition))
            {
                room.BroadcastSkillInvoke(Name, player, info.SkillPosition);
                return info;
            }

            return new TriggerStruct();
        }
        public override bool Effect(TriggerEvent triggerEvent, Room room, Player target, ref object data, Player ask_who, TriggerStruct info)
        {
            room.DrawCards(target, 1, Name);
            if (!target.IsNude())
            {
                List<int> ids = room.AskForExchange(target, Name, 1, 1, "@qianxi", string.Empty, "..!", info.SkillPosition);
                if (ids.Count != 1)
                {
                    foreach (int id in target.GetCards("he"))
                    {
                        if (RoomLogic.CanDiscard(room, target, target, id))
                        {
                            ids = new List<int> { id };
                            break;
                        }
                    }
                }

                if (ids.Count == 1)
                {
                    string color = WrappedCard.IsRed(room.GetCard(ids[0]).Suit) ? "red" : "black";
                    room.ThrowCard(ids[0], target);
                    List<Player> to_choose = new List<Player>();
                    foreach (Player p in room.GetOtherPlayers(target))
                    {
                        if (RoomLogic.DistanceTo(room, target, p) == 1)
                            to_choose.Add(p);
                    }
                    if (to_choose.Count == 0)
                        return false;

                    Player victim = room.AskForPlayerChosen(target, to_choose, Name, null, false, false, info.SkillPosition);
                    room.DoAnimate(AnimateType.S_ANIMATE_INDICATE, target.Name, victim.Name);

                    int index = 1;
                    if (color == "black")
                        index = 2;

                    GeneralSkin gsk = RoomLogic.GetGeneralSkin(room, target, Name, info.SkillPosition);
                    room.BroadcastSkillInvoke(Name, "male", index, gsk.General, gsk.SkinId);

                    string pattern = string.Format(".|{0}|.|hand$0", color);
                    victim.SetFlags("QianxiTarget");
                    room.AddPlayerMark(victim, "@qianxi_" + color);
                    RoomLogic.SetPlayerCardLimitation(victim, "use,response", pattern, false);

                    LogMessage log = new LogMessage
                    {
                        Type = "#Qianxi",
                        From = victim.Name,
                        Arg = "no_suit_" + color
                    };
                    room.SendLog(log);

                }
            }



            return false;
        }
    }
    public class QianxiClear : TriggerSkill
    {
        public QianxiClear() : base("#qianxi-clear")
        {
            events = new List<TriggerEvent> { TriggerEvent.EventPhaseChanging, TriggerEvent.Death };
            frequency = Frequency.Compulsory;
        }
        public override TriggerStruct Triggerable(TriggerEvent triggerEvent, Room room, Player player, ref object data, Player ask_who)
        {
            if (!player.ContainsTag("qianxi")) return new TriggerStruct();
            if (triggerEvent == TriggerEvent.EventPhaseChanging && data is PhaseChangeStruct change && change.To == Player.PlayerPhase.NotActive)
            {

                string color = (string)player.GetTag("qianxi");
                foreach (Player p in room.GetOtherPlayers(player))
                {
                    if (p.HasFlag("QianxiTarget"))
                    {
                        RoomLogic.RemovePlayerCardLimitation(p, "use,response", string.Format(".|{0}|.|hand$0", color));
                        room.SetPlayerMark(p, "@qianxi_" + color, 0);
                    }
                }
            }
            return new TriggerStruct();
        }
    }
    public class Guixiu : TriggerSkill
    {
        public Guixiu() : base("guixiu")
        {
            events = new List<TriggerEvent> { TriggerEvent.GeneralShown, TriggerEvent.GeneralRemoved, TriggerEvent.GeneralStartRemove };
            frequency = Frequency.Frequent;
            skill_type = SkillType.Replenish;
        }
        public override bool CanPreShow() => false;
        public override void Record(TriggerEvent triggerEvent, Room room, Player player, ref object data)
        {
            if (triggerEvent == TriggerEvent.GeneralStartRemove)
            {
                bool head = (bool)data;
                if (head && RoomLogic.InPlayerHeadSkills(player, Name))
                    player.SetFlags(player.ActualGeneral1 + ":head");
                if (!head && RoomLogic.InPlayerDeputykills(player, Name))
                    player.SetFlags(player.ActualGeneral2 + ":deputy");
            }
        }
        public override TriggerStruct Triggerable(TriggerEvent triggerEvent, Room room, Player player, ref object data, Player ask_who)
        {
            if (triggerEvent == TriggerEvent.GeneralShown)
            {
                if (base.Triggerable(player, room))
                {
                    bool head = (bool)data;
                    if (head && RoomLogic.InPlayerHeadSkills(player, Name))
                    {
                        TriggerStruct trigger = new TriggerStruct(Name, player)
                        {
                            SkillPosition = "head"
                        };
                        return trigger;
                    }
                    else if (!head && RoomLogic.InPlayerDeputykills(player, Name))
                    {
                        TriggerStruct trigger = new TriggerStruct(Name, player)
                        {
                            SkillPosition = "deputy"
                        };
                        return trigger;
                    }
                    else
                        return new TriggerStruct();
                }
            }
            else if (triggerEvent == TriggerEvent.GeneralRemoved && player.IsWounded() && data is InfoStruct info)
            {
                if (player.HasFlag(info.Info + (info.Head ? ":head" : ":deputy")))
                {
                    TriggerStruct trigger = new TriggerStruct(Name, player)
                    {
                        SkillPosition = info.Head ? "head" : "deputy"
                    };
                    return trigger;
                }
            }

            return new TriggerStruct();
        }
        public override TriggerStruct Cost(TriggerEvent triggerEvent, Room room, Player player, ref object data, Player ask_who, TriggerStruct info)
        {
            string events = "draw";
            if (triggerEvent == TriggerEvent.GeneralRemoved)
                events = "recover";

            if (room.AskForSkillInvoke(player, Name, events, info.SkillPosition))
            {
                if (events == "draw") room.BroadcastSkillInvoke(Name, player, info.SkillPosition);
                return info;
            }
            return new TriggerStruct();
        }
        public override bool Effect(TriggerEvent triggerEvent, Room room, Player player, ref object data, Player ask_who, TriggerStruct info)
        {
            if (triggerEvent == TriggerEvent.GeneralShown)
                room.DrawCards(player, 2, Name);
            else
            {
                LogMessage log = new LogMessage
                {
                    Type = "#InvokeSkill",
                    From = player.Name,
                    Arg = "guixiu"
                };
                room.SendLog(log);

                RecoverStruct recover = new RecoverStruct
                {
                    Recover = 1,
                    Who = player
                };
                room.Recover(player, recover, true);
            }
            return false;
        }
    }
    public class CunsiCard : SkillCard
    {
        public CunsiCard() : base("CunsiCard")
        { }
        public override bool TargetFilter(Room room, List<Player> targets, Player to_select, Player Self, WrappedCard card)
        {
            return targets.Count == 0;
        }
        public override void OnUse(Room room, CardUseStruct card_use)
        {
            room.BroadcastSkillInvoke("cunsi", card_use.From, card_use.Card.SkillPosition);
            room.DoSuperLightbox(card_use.From, card_use.Card.SkillPosition, "cunsi");
            base.OnUse(room, card_use);
        }
        public override void Use(Room room, CardUseStruct card_use)
        {
            bool head = RoomLogic.InPlayerHeadSkills(card_use.From, "cunsi");
            if (!string.IsNullOrEmpty(card_use.Card.SkillPosition))
                head = card_use.Card.SkillPosition == "head" ? true : false;
            card_use.From.SetMark("cunsi", 1);
            int skin_id = card_use.Card.SkillPosition == "head" ? card_use.From.HeadSkinId : card_use.From.DeputySkinId;
            room.RemoveGeneral(card_use.From, head);

            Player target = card_use.To[0];
            room.AcquireSkill(target, "yongjue");
            target.SetTag("yongjue_position", skin_id);
            room.SetPlayerMark(target, "@yongjue", 1);
            if (target != card_use.From)
                room.DrawCards(target, new DrawCardStruct(2, card_use.From, "cunsi"));
        }
    }
    public class Cunsi : ZeroCardViewAsSkill
    {
        public Cunsi() : base("cunsi")
        {
            frequency = Frequency.Limited;
        }
        public override WrappedCard ViewAs(Room room, Player player)
        {
            WrappedCard cunsi_card = new WrappedCard("CunsiCard")
            {
                Skill = Name,
                ShowSkill = Name,
                Mute = true
            };
            return cunsi_card;
        }
    }
    public class Yongjue : TriggerSkill
    {
        public Yongjue() : base("yongjue")
        {
            events = new List<TriggerEvent> { TriggerEvent.CardUsed, TriggerEvent.CardResponded, TriggerEvent.CardsMoveOneTime, TriggerEvent.EventPhaseStart };
            frequency = Frequency.Frequent;
            skill_type = SkillType.Replenish;
        }
        public override bool CanPreShow() => false;
        public override void Record(TriggerEvent triggerEvent, Room room, Player player, ref object data)
        {
            List<Player> owners = RoomLogic.FindPlayersBySkillName(room, Name);
            if (owners.Count == 0) return;

            if (triggerEvent == TriggerEvent.CardUsed || triggerEvent == TriggerEvent.CardResponded)
            {
                Player from = null;
                bool is_use = false;
                WrappedCard card = null;
                if (triggerEvent == TriggerEvent.CardUsed && data is CardUseStruct use)
                {
                    is_use = true;
                    from = use.From;
                    card = use.Card;
                }
                else if (data is CardResponseStruct resp)
                {
                    is_use = resp.Use;
                    from = player;
                    card = resp.Card;
                }
                if (from != null && card != null && from.Phase == PlayerPhase.Play && from.GetMark(Name) == 0 && is_use)
                {
                    FunctionCard fcard = Engine.GetFunctionCard(card.Name);
                    if (!(fcard is SkillCard))
                        from.AddMark(Name);
                    if (fcard is Slash && card.SubCards.Count > 0)
                    {
                        from.SetTag("yongjue_ids", RoomLogic.CardToString(room, card));
                        from.SetFlags(Name);
                    }
                }
            }
            else if (triggerEvent == TriggerEvent.EventPhaseStart && player.Phase == PlayerPhase.Play)
            {
                player.SetMark(Name, 0);
                player.SetFlags("-" + Name);
            }
        }
        public override TriggerStruct Triggerable(TriggerEvent triggerEvent, Room room, Player player, ref object data, Player ask_who)
        {
            if (triggerEvent == TriggerEvent.CardsMoveOneTime && data is CardsMoveOneTimeStruct move)
            {
                if (move.From != null && move.From.HasFlag(Name) && move.From.ContainsTag("yongjue_ids") && move.Reason.CardString == (string)move.From.GetTag("yongjue_ids")
                    && ((move.Reason.Reason & CardMoveReason.MoveReason.S_MASK_BASIC_REASON) == CardMoveReason.MoveReason.S_REASON_USE)
                    && move.From_places.Contains(Place.PlaceTable) && move.To_place == Place.DiscardPile)
                {
                    WrappedCard card = RoomLogic.ParseCard(room, move.Reason.CardString);
                    if (card != null && card.SubCards.SequenceEqual(move.Card_ids))
                    {
                        bool ok = true;
                        List<int> ids = new List<int>(card.SubCards);
                        foreach (int id in ids)
                        {
                            if (room.GetCardPlace(id) != Place.DiscardPile)
                            {
                                ok = false;
                                break;
                            }
                        }

                        List<Player> owners = RoomLogic.FindPlayersBySkillName(room, Name);
                        if (ok)
                        {
                            foreach (Player p in owners)
                            {
                                if (RoomLogic.IsFriendWith(room, p, move.From))
                                    return new TriggerStruct(Name, move.From);
                            }
                        }
                    }
                }
            }
            return new TriggerStruct();
        }
        public override TriggerStruct Cost(TriggerEvent triggerEvent, Room room, Player none, ref object data, Player player, TriggerStruct info)
        {
            List<Player> owners = RoomLogic.FindPlayersBySkillName(room, Name);
            Player owner = null;
            foreach (Player p in owners)
            {
                if (RoomLogic.IsFriendWith(room, p, player))
                {
                    owner = p;
                    break;
                }
            }

            if (owner != null)
            {
                player.SetFlags("-" + Name);
                if (room.AskForSkillInvoke(player, Name, null, info.SkillPosition))
                {
                    LogMessage log = new LogMessage
                    {
                        Type = "#InvokeOthersSkill",
                        From = player.Name,
                        To = new List<string> { owner.Name },
                        Arg = Name
                    };
                    room.SendLog(log);
                    room.DoAnimate(AnimateType.S_ANIMATE_INDICATE, owner.Name, player.Name);
                    //fix sound path
                    room.BroadcastSkillInvoke(Name, "male", -1, "mifuren", (int)owner.GetTag("yongjue_position"));
                    if (owner != player)
                        room.NotifySkillInvoked(owner, Name);

                    return info;
                }
            }
            return new TriggerStruct();
        }
        public override bool Effect(TriggerEvent triggerEvent, Room room, Player none, ref object data, Player player, TriggerStruct info)
        {
            CardsMoveOneTimeStruct move = (CardsMoveOneTimeStruct)data;

            player.RemoveTag("yongjue_ids");
            room.ObtainCard(player, move.Card_ids, new CardMoveReason(CardMoveReason.MoveReason.S_REASON_GOTBACK, player.Name));
            return false;
        }
    }
    public class Jiang : TriggerSkill
    {
        public Jiang() : base("jiang")
        {
            events = new List<TriggerEvent> { TriggerEvent.TargetConfirmed, TriggerEvent.TargetChosen };
            frequency = Frequency.Frequent;
            skill_type = SkillType.Replenish;
        }
        public override TriggerStruct Triggerable(TriggerEvent triggerEvent, Room room, Player sunce, ref object data, Player ask_who)
        {
            if (base.Triggerable(sunce, room) && data is CardUseStruct use)
            {
                bool invoke = triggerEvent == TriggerEvent.TargetChosen;
                if (!invoke)
                    invoke = (use.To.Contains(sunce));

                if (invoke)
                {
                    FunctionCard fcard = Engine.GetFunctionCard(use.Card.Name);
                    if (fcard is Duel || (fcard is Slash && WrappedCard.IsRed(RoomLogic.GetCardSuit(room, use.Card))))
                        return new TriggerStruct(Name, sunce);
                }
            }
            return new TriggerStruct();
        }
        public override TriggerStruct Cost(TriggerEvent triggerEvent, Room room, Player sunce, ref object data, Player ask_who, TriggerStruct info)
        {
            if (room.AskForSkillInvoke(sunce, Name, data, info.SkillPosition) && data is CardUseStruct use)
            {
                int index = 1;
                if (use.From != sunce)
                    index = 2;
                GeneralSkin gsk = RoomLogic.GetGeneralSkin(room, sunce, Name, info.SkillPosition);
                room.BroadcastSkillInvoke(Name, "male", index, gsk.General, gsk.SkinId);
                return info;
            }

            return new TriggerStruct();
        }
        public override bool Effect(TriggerEvent triggerEvent, Room room, Player player, ref object data, Player ask_who, TriggerStruct info)
        {
            room.DrawCards(player, 1, Name);
            return false;
        }
    }
    public class Yingyang : TriggerSkill
    {
        public Yingyang() : base("yingyang")
        {
            events.Add(TriggerEvent.PindianVerifying);
            frequency = Frequency.Frequent;
            skill_type = SkillType.Wizzard;
        }
        public override List<TriggerStruct> Triggerable(TriggerEvent triggerEvent, Room room, Player player, ref object data)
        {
            List<TriggerStruct> skill_list = new List<TriggerStruct>();
            if (player != null && data is PindianStruct pindian)
            {

                List<Player> sunces = RoomLogic.FindPlayersBySkillName(room, Name);
                foreach (Player sunce in sunces)
                    if (pindian.From == sunce || pindian.To == sunce)
                        skill_list.Add(new TriggerStruct(Name, sunce));

            }
            return skill_list;
        }
        public override TriggerStruct Cost(TriggerEvent triggerEvent, Room room, Player player, ref object data, Player ask_who, TriggerStruct info)
        {
            return room.AskForSkillInvoke(ask_who, Name, data, info.SkillPosition) ? info : new TriggerStruct();
        }
        public override bool Effect(TriggerEvent triggerEvent, Room room, Player player, ref object data, Player sunce, TriggerStruct info)
        {
            PindianStruct pindian = (PindianStruct)data;
            bool isFrom = pindian.From == sunce;

            string choice = room.AskForChoice(sunce, Name, "jia3+jian3", null, data);

            int index = 2;
            if (choice == "jia3")
                index = 1;

            GeneralSkin gsk = RoomLogic.GetGeneralSkin(room, sunce, Name, info.SkillPosition);
            room.BroadcastSkillInvoke(Name, "male", index, gsk.General, gsk.SkinId);

            LogMessage log = new LogMessage
            {
                Type = "$Yingyang",
                From = sunce.Name
            };

            if (isFrom)
            {
                pindian.From_number = choice == "jia3" ? Math.Min(pindian.From_number + 3, 13) : Math.Max(pindian.From_number - 3, 1);

                log.Arg = pindian.From_number.ToString();
            }
            else
            {
                pindian.To_number = choice == "jia3" ? Math.Min(pindian.To_number + 3, 13) : Math.Max(pindian.To_number - 3, 1);

                log.Arg = pindian.To_number.ToString();
            }
            data = pindian;
            room.SendLog(log);

            return false;
        }
    }
    public class Hunshang : TriggerSkill
    {
        public Hunshang() : base("hunshang")
        {
            frequency = Frequency.Compulsory;
            events.Add(TriggerEvent.EventPhaseStart);
            relate_to_place = "deputy";
        }
        public override bool CanPreShow() => false;
        public override TriggerStruct Triggerable(TriggerEvent triggerEvent, Room room, Player player, ref object data, Player ask_who)
        {
            if (base.Triggerable(player, room) && player.Hp == 1 && player.Phase == PlayerPhase.Start)
                return new TriggerStruct(Name, player);

            return new TriggerStruct();
        }
        public override TriggerStruct Cost(TriggerEvent triggerEvent, Room room, Player player, ref object data, Player ask_who, TriggerStruct info)
        {
            bool show = player.General2Showed;
            if (show || room.AskForSkillInvoke(player, Name, null, "deputy"))
            {
                room.BroadcastSkillInvoke(Name, player, info.SkillPosition);
                return info;
            }
            return new TriggerStruct();
        }
        public override bool Effect(TriggerEvent triggerEvent, Room room, Player target, ref object data, Player ask_who, TriggerStruct info)
        {
            room.SendCompulsoryTriggerLog(target, Name);
            List<string> skills = new List<string> { "yinghun_sunce!", "yingzi_sunce!" };
            room.HandleAcquireDetachSkills(target, skills);
            target.SetMark("hunshang", 1);
            return false;
        }
    }
    public class HunshangRemove : TriggerSkill
    {
        public HunshangRemove() : base("#hunshang")
        {
            frequency = Frequency.Compulsory;
            events.Add(TriggerEvent.EventPhaseStart);
        }
        public override bool CanPreShow() => false;
        public override TriggerStruct Triggerable(TriggerEvent triggerEvent, Room room, Player player, ref object data, Player ask_who)
        {
            if (player != null && player.Phase == PlayerPhase.NotActive && player.GetMark("hunshang") > 0)
            {
                player.SetMark("hunshang", 0);
                room.HandleAcquireDetachSkills(player, "-yinghun_sunce!|-yingzi_sunce!", true);
            }
            return new TriggerStruct();
        }
    }
    public class DuanxieCard : SkillCard
    {
        public DuanxieCard() : base("DuanxieCard") { }
        public override bool TargetFilter(Room room, List<Player> targets, Player to_select, Player Self, WrappedCard card)
        {
            return targets.Count == 0 && !to_select.Chained && to_select != Self && RoomLogic.CanBeChainedBy(room, to_select, Self);
        }
        public override void Use(Room room, CardUseStruct card_use)
        {
            Player target = card_use.To[0];
            if (RoomLogic.CanBeChainedBy(room, target, card_use.From))
                room.SetPlayerChained(target, true);
            base.Use(room, card_use);
        }
        public override void OnEffect(Room room, CardEffectStruct effect)
        {
            if (!effect.From.Chained && RoomLogic.CanBeChainedBy(room, effect.From, effect.From))
                room.SetPlayerChained(effect.From, true);
        }
    }
    public class Duanxie : ZeroCardViewAsSkill
    {
        public Duanxie() : base("duanxie")
        {
        }
        public override bool IsEnabledAtPlay(Room room, Player player)
        {
            return !player.HasUsed("DuanxieCard");
        }
        public override WrappedCard ViewAs(Room room, Player player)
        {
            WrappedCard card = new WrappedCard("DuanxieCard")
            {
                Skill = Name,
                ShowSkill = Name
            };
            return card;
        }
    }
    public class Fenming : PhaseChangeSkill
    {
        public Fenming() : base("fenming")
        {
            skill_type = SkillType.Attack;
        }
        public override TriggerStruct Triggerable(TriggerEvent triggerEvent, Room room, Player player, ref object data, Player ask_who)
        {
            if (base.Triggerable(player, room) && player.Phase == Player.PlayerPhase.Finish && player.Chained)
            {
                foreach (Player p in room.GetAllPlayers())
                {
                    if (p.Chained && RoomLogic.CanDiscard(room, player, p, "he"))
                        return new TriggerStruct(Name, player);
                }
            }
            return new TriggerStruct();
        }
        public override TriggerStruct Cost(TriggerEvent triggerEvent, Room room, Player player, ref object data, Player ask_who, TriggerStruct info)
        {
            bool invoke = room.AskForSkillInvoke(player, Name, null, info.SkillPosition);
            if (invoke)
            {
                foreach (Player p in room.GetOtherPlayers(player))
                {
                    if (p.Chained)
                        room.DoAnimate(AnimateType.S_ANIMATE_INDICATE, player.Name, p.Name);
                }
                room.BroadcastSkillInvoke(Name, player, info.SkillPosition);
                return info;
            }

            return new TriggerStruct();
        }
        public override bool OnPhaseChange(Room room, Player player, TriggerStruct info)
        {
            List<Player> targets = room.GetAlivePlayers();
            room.SortByActionOrder(ref targets);
            foreach (Player p in targets)
            {
                if (p.Chained && RoomLogic.CanDiscard(room, player, p, "he") && player.Alive)
                {
                    CardMoveReason reason = new CardMoveReason(CardMoveReason.MoveReason.S_REASON_DISMANTLE, player.Name, p.Name, Name, null);
                    List<int> ints = new List<int>();
                    if (p == player)
                    {
                        ints.AddRange(room.AskForExchange(player, Name, 1, 1, "@fengming", null, "..!", info.SkillPosition));
                    }
                    else
                        ints.Add(room.AskForCardChosen(player, p, "he", Name, false, FunctionCard.HandlingMethod.MethodDiscard));

                    room.ThrowCard(ref ints, reason, p, player);
                }
            }
            return false;
        }
    }
    public class Hengzheng : PhaseChangeSkill
    {
        public Hengzheng() : base("hengzheng")
        {
            frequency = Frequency.Frequent;
            skill_type = SkillType.Replenish;
        }
        public override TriggerStruct Triggerable(TriggerEvent triggerEvent, Room room, Player player, ref object data, Player ask_who)
        {
            if (base.Triggerable(player, room) && player.Phase == PlayerPhase.Draw && (player.IsKongcheng() || player.Hp == 1))
            {
                foreach (Player p in room.GetOtherPlayers(player))
                {
                    if (!p.IsAllNude())
                        return new TriggerStruct(Name, player);
                }
            }
            return new TriggerStruct();
        }
        public override TriggerStruct Cost(TriggerEvent triggerEvent, Room room, Player player, ref object data, Player ask_who, TriggerStruct info)
        {
            if (room.AskForSkillInvoke(player, Name, null, info.SkillPosition))
            {
                room.BroadcastSkillInvoke(Name, player, info.SkillPosition);
                room.DoSuperLightbox(player, info.SkillPosition, Name);
                return info;
            }

            return new TriggerStruct();
        }
        public override bool OnPhaseChange(Room room, Player player, TriggerStruct info)
        {
            foreach (Player p in room.GetOtherPlayers(player))
            {
                if (RoomLogic.CanGetCard(room, player, p, "hej"))
                {
                    int card_id = room.AskForCardChosen(player, p, "hej", Name, false, FunctionCard.HandlingMethod.MethodGet);
                    room.ObtainCard(player, card_id, false);
                }
            }
            return true;
        }
    }
    public class Baoling : TriggerSkill
    {
        public Baoling() : base("baoling")
        {
            events.Add(TriggerEvent.EventPhaseEnd);
            relate_to_place = "head";
            frequency = Frequency.Compulsory;
            skill_type = SkillType.Recover;
        }
        public override bool CanPreShow() => false;
        public override TriggerStruct Triggerable(TriggerEvent triggerEvent, Room room, Player player, ref object data, Player ask_who)
        {
            if (base.Triggerable(player, room) && player.Phase == PlayerPhase.Play && player.General1Showed)
                return (player.ActualGeneral2.Contains("sujiang")) ? new TriggerStruct() : new TriggerStruct(Name, player);

            return new TriggerStruct();
        }
        public override TriggerStruct Cost(TriggerEvent triggerEvent, Room room, Player player, ref object data, Player ask_who, TriggerStruct info)
        {
            room.BroadcastSkillInvoke(Name, player);
            room.DoSuperLightbox(player, info.SkillPosition, Name);
            room.NotifySkillInvoked(player, Name);
            return info;
        }
        public override bool Effect(TriggerEvent triggerEvent, Room room, Player player, ref object data, Player ask_who, TriggerStruct info)
        {
            room.RemoveGeneral(player, false);
            room.HandleAcquireDetachSkills(player, new List<string> { "-baoling" });
            player.MaxHp += 3;
            room.BroadcastProperty(player, "MaxHp");

            LogMessage log = new LogMessage
            {
                Type = "$GainMaxHp",
                From = player.Name,
                Arg = "3"
            };
            room.SendLog(log);

            RecoverStruct recover = new RecoverStruct
            {
                Recover = 3,
                Who = player
            };
            room.Recover(player, recover);

            room.HandleAcquireDetachSkills(player, "benghuai");
            return false;
        }
    }
    public class Benghuai : PhaseChangeSkill
    {
        public Benghuai() : base("benghuai")
        {
            frequency = Frequency.Compulsory;
        }
        public override bool CanPreShow() => false;
        public override TriggerStruct Triggerable(TriggerEvent triggerEvent, Room room, Player player, ref object data, Player ask_who)
        {
            if (base.Triggerable(player, room) && player.Phase == PlayerPhase.Finish)
            {
                List<Player> players = room.GetOtherPlayers(player);
                foreach (Player p in players)
                    if (player.Hp > p.Hp)
                        return new TriggerStruct(Name, player);
            }

            return new TriggerStruct();
        }
        public override bool OnPhaseChange(Room room, Player dongzhuo, TriggerStruct info)
        {
            room.SendCompulsoryTriggerLog(dongzhuo, Name);

            string result = room.AskForChoice(dongzhuo, Name, "hp+maxhp");
            int index = (result == "hp") ? 2 : 1;
            GeneralSkin gsk = RoomLogic.GetGeneralSkin(room, dongzhuo, Name, "head");
            room.BroadcastSkillInvoke(Name, "male", index, gsk.General, gsk.SkinId);
            if (result == "hp")
                room.LoseHp(dongzhuo);
            else
                room.LoseMaxHp(dongzhuo);

            return false;
        }
    }
    public class Chuanxin : TriggerSkill
    {
        public Chuanxin() : base("chuanxin")
        {
            events.Add(TriggerEvent.DamageCaused);
            skill_type = SkillType.Attack;
        }
        public override TriggerStruct Triggerable(TriggerEvent triggerEvent, Room room, Player player, ref object data, Player ask_who)
        {
            if (base.Triggerable(player, room) && data is DamageStruct damage
             && damage.To != null && damage.To.HasShownOneGeneral() && damage.Card != null && player.Phase == PlayerPhase.Play
                        && !RoomLogic.IsFriendWith(room, player, damage.To) && (player.HasShownOneGeneral() || !RoomLogic.WillBeFriendWith(room, player, damage.To, Name))
                        && !damage.Transfer && !damage.Chain && !damage.To.ActualGeneral2.Contains("sujiang"))
            {
                FunctionCard fcard = Engine.GetFunctionCard(damage.Card.Name);
                if (fcard is Duel || fcard is Slash)
                    return new TriggerStruct(Name, player);
            }
            return new TriggerStruct();
        }
        public override TriggerStruct Cost(TriggerEvent triggerEvent, Room room, Player player, ref object data, Player ask_who, TriggerStruct info)
        {
            if (data is DamageStruct damage && room.AskForSkillInvoke(player, Name, damage.To, info.SkillPosition))
            {
                room.DoAnimate(AnimateType.S_ANIMATE_INDICATE, player.Name, damage.To.Name);
                return info;
            }
            return new TriggerStruct();
        }
        public override bool Effect(TriggerEvent triggerEvent, Room room, Player player, ref object data, Player ask_who, TriggerStruct info)
        {
            DamageStruct damage = (DamageStruct)data;
            List<string> choices = new List<string> { "remove" };
            if (damage.To.HasEquip())
                choices.Add("discard");

            GeneralSkin gsk = RoomLogic.GetGeneralSkin(room, player, Name, info.SkillPosition);
            string choice = room.AskForChoice(damage.To, Name, string.Join("+", choices));
            if (choice == "discard")
            {
                room.BroadcastSkillInvoke(Name, "male", 1, gsk.General, gsk.SkinId);
                room.ThrowAllEquips(damage.To);
                room.LoseHp(damage.To);
            }
            else
            {
                room.BroadcastSkillInvoke(Name, "male", 2, gsk.General, gsk.SkinId);
                room.RemoveGeneral(damage.To, false);
            }

            return true;
        }
    }
    public class FengshiSummon : ArraySummonCard
    {
        public FengshiSummon() : base("fengshi")
        {
        }
    }
    public class Fengshi : BattleArraySkill
    {
        public Fengshi() : base("fengshi", ArrayType.Siege)
        {
            events.Add(TriggerEvent.TargetChosen);
            frequency = Frequency.Compulsory;
            skill_type = SkillType.Attack;
        }
        public override bool CanPreShow() => false;
        public override List<TriggerStruct> Triggerable(TriggerEvent triggerEvent, Room room, Player player, ref object data)
        {
            List<TriggerStruct> skill_list = new List<TriggerStruct>();
            CardUseStruct use = (CardUseStruct)data;
            if (use.Card != null)
            {
                FunctionCard fcard = Engine.GetFunctionCard(use.Card.Name);
                if (fcard is Slash)
                {
                    List<Player> skill_owners = RoomLogic.FindPlayersBySkillName(room, Name);
                    foreach (Player skill_owner in skill_owners)
                    {
                        if (base.Triggerable(skill_owner, room) && RoomLogic.PlayerHasShownSkill(room, skill_owner, Name))
                        {
                            List<Player> targets = new List<Player>();
                            foreach (Player to in use.To)
                            {
                                if (RoomLogic.InSiegeRelation(room, player, skill_owner, to) && RoomLogic.CanDiscard(room, to, to, "e"))
                                    targets.Add(to);
                            }
                            if (targets.Count > 0)
                            {
                                if (RoomLogic.GetHeadActivedSkills(room, skill_owner, true, true).Contains(Engine.GetSkill(Name)))
                                {
                                    TriggerStruct trigger = new TriggerStruct(Name, skill_owner, targets)
                                    {
                                        SkillPosition = "head"
                                    };
                                    skill_list.Add(trigger);
                                }
                                if (RoomLogic.GetDeputyActivedSkills(room, skill_owner, true, true).Contains(Engine.GetSkill(Name)))
                                {
                                    TriggerStruct trigger = new TriggerStruct(Name, skill_owner, targets)
                                    {
                                        SkillPosition = "deputy"
                                    };
                                    skill_list.Add(trigger);
                                }
                            }
                        }
                    }
                }
            }
            return skill_list;
        }
        public override TriggerStruct Cost(TriggerEvent triggerEvent, Room room, Player skill_target, ref object data, Player ask_who, TriggerStruct info)
        {
            if (ask_who != null && RoomLogic.PlayerHasShownSkill(room, ask_who, Name))
            {
                room.DoBattleArrayAnimate(ask_who, skill_target);
                room.BroadcastSkillInvoke(Name, ask_who, info.SkillPosition);
                return info;
            }
            return new TriggerStruct();
        }
        public override bool Effect(TriggerEvent triggerEvent, Room room, Player skill_target, ref object data, Player ask_who, TriggerStruct info)
        {
            room.SendCompulsoryTriggerLog(ask_who, Name, true);
            if (room.AskForCard(skill_target, Name, ".|.|.|equipped!", "@fengshi-discard:" + ask_who.Name) == null)
            {
                List<int> equips_candiscard = new List<int>();
                foreach (int e in skill_target.GetEquips())
                {
                    if (RoomLogic.CanDiscard(room, skill_target, skill_target, e))
                        equips_candiscard.Add(e);
                }
                Shuffle.shuffle<int>(ref equips_candiscard);
                int rand_c = equips_candiscard[0];
                room.ThrowCard(rand_c, skill_target);
            }
            return false;
        }
    }
    public class Wuxin : PhaseChangeSkill
    {
        public Wuxin() : base("wuxin")
        {
            frequency = Frequency.Frequent;
        }
        public override bool CanPreShow() => false;
        public override TriggerStruct Triggerable(TriggerEvent triggerEvent, Room room, Player player, ref object data, Player ask_who)
        {
            if (base.Triggerable(player, room) && player.Phase == PlayerPhase.Draw)
                return new TriggerStruct(Name, player);

            return new TriggerStruct();
        }
        public override TriggerStruct Cost(TriggerEvent triggerEvent, Room room, Player player, ref object data, Player ask_who, TriggerStruct info)
        {
            if (room.AskForSkillInvoke(player, Name))
            {
                room.BroadcastSkillInvoke(Name, player);
                return info;
            }

            return new TriggerStruct();
        }
        public override bool OnPhaseChange(Room room, Player player, TriggerStruct info)
        {
            int num = RoomLogic.GetPlayerNumWithSameKingdom(room, player);

            string prompt = "@hongfa-tianbing:" + player.Name;
            player.SetMark("hongfa", 2);
            List<int> ints = room.AskForExchange(player, "hongfa", player.GetPile("heavenly_army").Count, 0, prompt, "heavenly_army", null, "head");

            List<int> guanxing = room.GetNCards(num + ints.Count);

            LogMessage log;
            log.Type = "$ViewDrawPile";
            log.From = player.Name;
            log.Card_str = string.Join("+", JsonUntity.IntList2StringList(guanxing));
            room.SendLog(log, player);

            room.AskForGuanxing(player, guanxing, Name, false, "head");
            return false;
        }
    }
    public class HongfaSlash : OneCardViewAsSkill
    {
        public HongfaSlash() : base("hongfaslash")
        {
            attached_lord_skill = true;
            expand_pile = "heavenly_army,%heavenly_army";
            filter_pattern = ".|.|.|heavenly_army,%heavenly_army";
            skill_type = SkillType.Attack;
        }
        public override bool IsEnabledAtPlay(Room room, Player player)
        {
            Player zhangjiao = RoomLogic.GetLord(room, player.Kingdom);
            if (zhangjiao == null || !RoomLogic.PlayerHasShownSkill(room, zhangjiao, "hongfa")
                || zhangjiao.GetPile("heavenly_army").Count == 0 || !RoomLogic.WillBeFriendWith(room, player, zhangjiao))
                return false;
            return Slash.IsAvailable(room, player) && player.CanShowGeneral(null);
        }
        public override bool IsEnabledAtResponse(Room room, Player player, string pattern)
        {
            Player zhangjiao = RoomLogic.GetLord(room, player.Kingdom);
            if (zhangjiao == null || !RoomLogic.PlayerHasShownSkill(room, zhangjiao, "hongfa")
                || zhangjiao.GetPile("heavenly_army").Count == 0 || !RoomLogic.WillBeFriendWith(room, player, zhangjiao))
                return false;

            return pattern == "Slash" && player.CanShowGeneral(null);
        }
        public override WrappedCard ViewAs(Room room, WrappedCard card, Player player)
        {
            WrappedCard slash = new WrappedCard("Slash");
            slash.AddSubCard(card);
            slash.Skill = "hongfa";
            slash.ShowSkill = "showforviewhas";
            slash = RoomLogic.ParseUseCard(room, slash);
            return slash;
        }
    }
    public class Hongfa : TriggerSkill
    {
        public Hongfa() : base("hongfa")
        {
            lord_skill = true;
            events = new List<TriggerEvent>{ TriggerEvent.EventPhaseStart, // get Tianbing
            TriggerEvent.PreHpLost, TriggerEvent.GeneralShown, TriggerEvent.Death, TriggerEvent.DFDebut }; // HongfaSlash
            frequency = Frequency.Compulsory;
            skill_type = SkillType.Wizzard;
        }
        public override TriggerStruct Triggerable(TriggerEvent triggerEvent, Room room, Player player, ref object data, Player ask_who)
        {
            if (triggerEvent == TriggerEvent.EventPhaseStart && player != null && base.Triggerable(player, room)
                && player.Phase == PlayerPhase.Start && player.GetPile("heavenly_army").Count == 0)
            {
                return new TriggerStruct(Name, player);
            }
            else if (triggerEvent == TriggerEvent.PreHpLost && base.Triggerable(player, room) && player.GetPile("heavenly_army").Count > 0)
            {
                return new TriggerStruct(Name, player);
            }
            else if (triggerEvent == TriggerEvent.GeneralShown && player.General1Showed)
            {
                if (base.Triggerable(player, room))
                {
                    foreach (Player p in room.GetAlivePlayers())
                        if (RoomLogic.WillBeFriendWith(room, p, player))
                            room.AttachSkillToPlayer(p, "hongfaslash");
                }
                else
                {
                    Player zhangjiao = RoomLogic.GetLord(room, player.Kingdom);
                    if (zhangjiao != null && zhangjiao.Alive && RoomLogic.PlayerHasShownSkill(room, zhangjiao, Name))
                        room.AttachSkillToPlayer(player, "hongfaslash");
                }
            }
            else if (triggerEvent == TriggerEvent.DFDebut)
            {
                Player zhangjiao = RoomLogic.GetLord(room, player.Kingdom);
                if (zhangjiao != null && base.Triggerable(zhangjiao, room) && !player.GetAcquiredSkills().Contains("hongfaslash"))
                    room.AttachSkillToPlayer(player, "hongfaslash");
            }
            else if (triggerEvent == TriggerEvent.Death && player != null && RoomLogic.PlayerHasSkill(room, player, Name))
            {
                foreach (Player p in room.GetAlivePlayers())
                    room.DetachSkillFromPlayer(p, "hongfaslash");
            }

            return new TriggerStruct();
        }
        public override TriggerStruct Cost(TriggerEvent triggerEvent, Room room, Player player, ref object data, Player ask_who, TriggerStruct info)
        {
            if (triggerEvent == TriggerEvent.EventPhaseStart)
                return info;
            else if (triggerEvent == TriggerEvent.PreHpLost)
            {
                player.RemoveTag("hongfa_prevent");
                //return room.askForUseCard(player, "@@hongfa1", "@hongfa-prevent", 1, Card::MethodNone);
                player.SetMark(Name, 1);
                List<int> ints = room.AskForExchange(player, "hongfa", 1, 0, "@hongfa-prevent", "heavenly_army", null, "head");
                if (ints.Count > 0)
                {
                    room.NotifySkillInvoked(player, Name);
                    room.BroadcastSkillInvoke(Name, "male", 1, player.ActualGeneral1, player.HeadSkinId);
                    player.SetTag("hongfa_prevent", ints[0]);
                    return info;
                }
            }
            return new TriggerStruct();
        }
        public override bool Effect(TriggerEvent triggerEvent, Room room, Player player, ref object data, Player ask_who, TriggerStruct info)
        {
            if (triggerEvent == TriggerEvent.EventPhaseStart)
            {
                int num = RoomLogic.GetPlayerNumWithSameKingdom(room, player);
                List<int> tianbing = room.GetNCards(num);
                room.AddToPile(player, "heavenly_army", tianbing);
                return false;
            }
            else
            {
                int card_id = (int)player.GetTag("hongfa_prevent");
                player.RemoveTag("hongfa_prevent");
                if (card_id != -1)
                {
                    CardMoveReason reason = new CardMoveReason(CardMoveReason.MoveReason.S_REASON_REMOVE_FROM_PILE, null, Name, null);
                    List<int> ids = new List<int> { card_id };
                    room.ThrowCard(ref ids, reason, null);
                    return true;
                }
            }

            return false;
        }
        public override int GetEffectIndex(Room room, Player player, WrappedCard card)
        {
            if (card != null)
            {
                FunctionCard fcard = Engine.GetFunctionCard(card.Name);
                if (fcard is Slash)
                    return 1;
            }

            return 2;
        }
    }
    public class HongfaClear : DetachEffectSkill
    {
        public HongfaClear() : base("hongfa", string.Empty)
        {
        }
        public override void OnSkillDetached(Room room, Player player, object data)
        {
            foreach (Player p in room.GetAlivePlayers())
            {
                room.DetachSkillFromPlayer(p, "hongfaslash");
            }
        }
    }
    public class WendaoCard : SkillCard
    {
        public WendaoCard() : base("WendaoCard")
        {
            target_fixed = true;
        }
        public override void Use(Room room, CardUseStruct card_use)
        {
            WrappedCard tpys = null;
            foreach (Player p in room.GetAlivePlayers())
            {
                foreach (int id in p.GetEquips())
                {
                    if (room.GetCard(id).Name == "PeaceSpell")
                    {
                        room.DoAnimate(AnimateType.S_ANIMATE_INDICATE, card_use.From.Name, p.Name);
                        tpys = room.GetCard(id);
                        break;
                    }
                }
                if (tpys != null)
                    break;
            }
            if (tpys == null)
                foreach (int id in room.DiscardPile)
                {
                    if (room.GetCard(id).Name == "PeaceSpell")
                    {
                        tpys = room.GetCard(id);
                        break;
                    }
                }

            if (tpys == null)
                return;

            room.ObtainCard(card_use.From, tpys, true);
        }
    }
    public class Wendao : OneCardViewAsSkill
    {
        public Wendao() : base("wendao")
        {
            filter_pattern = ".|red!";
        }
        public override bool IsEnabledAtPlay(Room room, Player player)
        {
            return !player.HasUsed("WendaoCard") && RoomLogic.CanDiscard(room, player, player, "he");
        }
        public override WrappedCard ViewAs(Room room, WrappedCard card, Player player)
        {
            WrappedCard wd = new WrappedCard("WendaoCard")
            {
                ShowSkill = Name,
                Skill = Name
            };
            wd.AddSubCard(card);
            return wd;
        }
    }
}
