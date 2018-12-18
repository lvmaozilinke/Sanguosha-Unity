﻿using CommonClass;
using CommonClass.Game;
using SanguoshaServer.AI;
using SanguoshaServer.Scenario;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SanguoshaServer.Game
{
    public class RoomThread
    {
        private Room room;
        private int _m_trigger_id;
        private GameRule game_rule;
        private List<string> skillSet = new List<string>();
        private Dictionary<TriggerEvent, List<TriggerSkill>> skill_table = new Dictionary<TriggerEvent, List<TriggerSkill>>();
        private readonly Player rule_player = new Player();
        public RoomThread(Room room, GameRule rule)
        {
            this.room = room;
            _m_trigger_id = 0;
            game_rule = rule;
        }

        private void AddTriggerSkill(TriggerSkill skill)
        {
            if (skill == null || skillSet.Contains(skill.Name))
                return;

            skillSet.Add(skill.Name);

            List<TriggerEvent> events = skill.TriggerEvents;
            foreach (TriggerEvent triggerEvent in events) {
                List <TriggerSkill> table = skill_table.ContainsKey(triggerEvent) ? skill_table[triggerEvent] : new List<TriggerSkill>();
                table.Add(skill);
                table.Sort((x, y) => CompareByPriority(triggerEvent, x, y));
                skill_table[triggerEvent] = table;
            }
        }

        private int CompareByPriority(TriggerEvent triggerEvent, TriggerSkill skill1, TriggerSkill skill)
        {
            return skill1.GetDynamicPriority(triggerEvent) > skill.GetDynamicPriority(triggerEvent) ? -1 : 1;
        }

        protected virtual void ConstructTriggerTable()
        {
            foreach (string skill_name in room.Skills) {
                TriggerSkill skill = Engine.GetTriggerSkill(skill_name);
                if (skill != null) AddTriggerSkill(skill);
            }

            object data = null;
            foreach (Player player in room.Players)
                Trigger(TriggerEvent.GameStart, room, player, ref data);
        }

        public void Start()
        {
            AddTriggerSkill(game_rule);

            foreach (TriggerSkill skill in Engine.GetGlobalTriggerSkills())
                AddTriggerSkill(skill);
            foreach (TriggerSkill skill in Engine.GetPublicTriggerSkills())
                AddTriggerSkill(skill);
            foreach (TriggerSkill skill in Engine.GetEquipTriggerSkills(room.Setting.CardPackage))
                AddTriggerSkill(skill);

            string winner = game_rule.GetWinner(room);
            if (!string.IsNullOrEmpty(winner))
                room.GameOver(winner);

            //try
            //{
                Trigger(TriggerEvent.GameStart, room, null);
                ConstructTriggerTable();
                // delay(3000);
                //actionNormal(game_rule);
                ActionNormal();
            //}
            //catch (Exception e)
            //{
            //    room.OutPut(e.Message);
            //}
        }

        protected virtual void ActionNormal()//(GameRule* game_rule)
        {
            Player starter = room.Current;
            bool new_round = false;
            while (true) {
                    room.BeforeTurnStart(new_round);
                new_round = false;
                Trigger(TriggerEvent.TurnStart, room, room.Current);
                if (room.Finished) break;

                Player regular_next = room.Current;
                while (regular_next == room.Current || !regular_next.Alive)
                {
                    regular_next = room.GetNext(regular_next, false);
                    if (regular_next == starter)
                        new_round = true;
                }

                while (room.ContainsTag("ExtraTurnList") && room.GetTag("ExtraTurnList") is List<Player> extraTurnList)
                {
                    if (extraTurnList.Count > 0)
                    {
                        Player next = extraTurnList[0];
                        extraTurnList.RemoveAt(0);
                        room.SetTag("ExtraTurnList", extraTurnList);
                        room.SetCurrent(next);
                        Trigger(TriggerEvent.TurnStart, room, next);
                        if (room.Finished) break;
                    }
                    else
                        room.RemoveTag("ExtraTurnList");
                }
                if (room.Finished) break;
                room.SetCurrent(regular_next);
            }
        }


        public bool Trigger(TriggerEvent triggerEvent, Room room, Player target, ref object data)
        {
            // push it to event stack

            _m_trigger_id++;
            int trigger_id = _m_trigger_id;

            bool broken = false;
            List <TriggerSkill> will_trigger = new List<TriggerSkill>();
            List<TriggerSkill> triggerable_tested = new List<TriggerSkill>();
            List<TriggerSkill> rules = new List<TriggerSkill>(); // we can't get a GameRule with Engine::getTriggerSkill() :(
            Dictionary<Player, List<TriggerStruct>> trigger_who = new Dictionary<Player, List<TriggerStruct>>();

            List<TriggerSkill> triggered = new List<TriggerSkill>();
            List<TriggerSkill> skills = skill_table.ContainsKey(triggerEvent) ? skill_table[triggerEvent] : new List<TriggerSkill>();
            skills.Sort((x, y) => CompareByPriority(triggerEvent, x, y));
            do
            {
                trigger_who.Clear();
                foreach (TriggerSkill skill in skills) {
                    if (!triggered.Contains(skill))
                    {
                        if (skill is GameRule)
                        {
                            if (will_trigger.Count == 0
                                || skill.GetDynamicPriority(triggerEvent) == will_trigger[will_trigger.Count - 1].GetDynamicPriority(triggerEvent))
                            {
                                will_trigger.Add(skill);
                                if (trigger_who.ContainsKey(rule_player))
                                    trigger_who[rule_player].Add(new TriggerStruct(skill.Name));       // Don't assign game rule to some player.
                                else
                                    trigger_who[rule_player] = new List<TriggerStruct> { new TriggerStruct(skill.Name) };

                                rules.Add(skill);
                            }
                            else if (skill.GetDynamicPriority(triggerEvent) != will_trigger[will_trigger.Count - 1].GetDynamicPriority(triggerEvent))
                                break;

                            triggered.Insert(0, skill);
                        }
                        else
                        {
                            if (will_trigger.Count == 0
                                || skill.GetDynamicPriority(triggerEvent) == will_trigger[will_trigger.Count - 1].GetDynamicPriority(triggerEvent))
                            {
                                skill.Record(triggerEvent, room, target, ref data);        //to record something for next.
                                List<TriggerStruct> triggerSkillList = skill.Triggerable(triggerEvent, room, target, ref data);

                                foreach (TriggerStruct skill_struct in triggerSkillList) {
                                    TriggerStruct tskill = skill_struct;
                                    if (tskill.Times != 1) tskill.Times = 1;            //make sure times == 1
                                    TriggerSkill trskill = Engine.GetTriggerSkill(tskill.SkillName);
                                    Player p = room.FindPlayer(tskill.Invoker, true);
                                    if (trskill != null)
                                    {
                                        will_trigger.Add(trskill);
                                        if (trigger_who.ContainsKey(p))
                                            trigger_who[p].Add(tskill);
                                        else
                                            trigger_who[p] = new List<TriggerStruct> { tskill };
                                    }
                                }
                            }
                            else if (skill.GetDynamicPriority(triggerEvent) != will_trigger[will_trigger.Count - 1].GetDynamicPriority(triggerEvent))
                                break;

                            triggered.Insert(0, skill);
                        }
                    }
                    if (!triggerable_tested.Contains(skill))
                        triggerable_tested.Add(skill);
                }
                
                if (will_trigger.Count > 0)
                {
                    will_trigger.Clear();
                    foreach (Player p in room.GetAllPlayers(true)) {
                        if (!trigger_who.ContainsKey(p)) continue;
                        List<TriggerStruct> already_triggered = new List<TriggerStruct>();
                        Dictionary<Player, List<TriggerStruct>> refuse = new Dictionary<Player, List<TriggerStruct>>();
                        while (true) {
                            List<TriggerStruct> who_skills = trigger_who.ContainsKey(p) ? new List<TriggerStruct>(trigger_who[p]) : new List<TriggerStruct>();
                            if (who_skills.Count == 0) break;
                            TriggerStruct name = new TriggerStruct();
                            foreach (TriggerStruct skill in who_skills) {
                                TriggerSkill tskill = Engine.GetTriggerSkill(skill.SkillName);
                                if (tskill != null && tskill.Global && tskill.SkillFrequency == Skill.Frequency.Compulsory)
                                {
                                    TriggerStruct name_copy = skill;
                                    if (skill.Targets.Count > 0) name_copy.ResultTarget = skill.Targets[0];
                                    name = name_copy;       // a new trick to deal with all "record-skill" or "compulsory-global",
                                                            // they should always be triggered first.
                                    break;
                                }
                            }

                            if (string.IsNullOrEmpty(name.SkillName))
                            {
                                if (p != null && !p.HasShownAllGenerals())
                                    p.SetFlags("Global_askForSkillCost");           // TriggerOrder need protect
                                if (who_skills.Count == 1 && (p == null || who_skills[0].SkillName.Contains("global-fake-move")))
                                {
                                    TriggerStruct name_copy = who_skills[0];
                                    if (who_skills[0].Targets.Count > 0) name_copy.ResultTarget = who_skills[0].Targets[0];
                                    name = name_copy;
                                }
                                else if (p != null)
                                {
                                    string reason = "GameRule:TriggerOrder";
                                    foreach (TriggerStruct skill in who_skills) {
                                        if (skill.SkillName.Contains("GameRule_AskForGeneralShow"))
                                        {
                                            reason = "GameRule:TurnStart";
                                            break;
                                        }
                                    }
                                    name = room.AskForSkillTrigger(p, reason, who_skills, true, data);
                                }
                                else
                                    name = who_skills[who_skills.Count - 1];
                                if (p != null && p.HasFlag("Global_askForSkillCost"))
                                    p.SetFlags("-Global_askForSkillCost");
                            }

                            if (string.IsNullOrEmpty(name.SkillName)) break;

                            TriggerSkill result_skill = Engine.GetTriggerSkill(name.SkillName);
                            Player skill_target = room.FindPlayer(name.ResultTarget, true);
                            if (skill_target == null && target != null)
                            {
                                skill_target = target;
                                name.ResultTarget = target.Name;
                            }

                            if (name.Targets.Count > 0)
                            {                         //merge the same
                                bool deplicated = false;
                                foreach (TriggerStruct already_s in already_triggered) {
                                    if (already_s.Equals(name) && already_s.Targets.Count > 0)
                                    {
                                        Player triggered_target = room.FindPlayer(already_s.ResultTarget, true);
                                        Player _target = room.FindPlayer(name.ResultTarget, true);
                                        List<Player> all = room.GetAllPlayers(true);    
                                        if (all.IndexOf(_target) > all.IndexOf(triggered_target))
                                        {
                                            already_triggered[already_triggered.IndexOf(already_s)] = name;
                                            deplicated = true;
                                            break;
                                        }
                                    }
                                }
                                if (!deplicated)
                                    already_triggered.Add(name);
                            }
                            else
                                already_triggered.Add(name);

                            //----------------------------------------------- TriggerSkill::cost
                            bool do_effect = false;

                            if (p != null && !RoomLogic.PlayerHasShownSkill(room, p, result_skill))
                                p.SetFlags("Global_askForSkillCost");           // SkillCost need protect

                            TriggerStruct cost_str = result_skill.Cost(triggerEvent, room, skill_target, ref data, p, name);
                            result_skill = Engine.GetTriggerSkill(cost_str.SkillName);

                            if (result_skill != null)
                            {
                                if (result_skill.Name.EndsWith("_h") && p != null)
                                    p.SetFlags(string.Format("huashen_{0}", trigger_id));  // huashen skill invoked

                                do_effect = true;
                                if (p != null)
                                {
                                    bool compulsory_shown = false;
                                    if (result_skill.SkillFrequency == Skill.Frequency.Compulsory && RoomLogic.PlayerHasShownSkill(room, p, result_skill))
                                        compulsory_shown = true;
                                    bool show = room.ShowSkill(p, result_skill.Name, cost_str.SkillPosition);
                                    if (!compulsory_shown && show) p.SetTag("JustShownSkill", result_skill.Name);
                                }
                            }

                            if (p != null && p.HasFlag("Global_askForSkillCost"))                      // for next time
                                p.SetFlags("-Global_askForSkillCost");

                            //----------------------------------------------- TriggerSkill::effect
                            if (do_effect)
                            {
                                Player invoker = room.FindPlayer(cost_str.Invoker, true);       //it may point to another skill, such as EightDiagram to bazhen
                                skill_target = room.FindPlayer(cost_str.ResultTarget, true);
                                broken = result_skill.Effect(triggerEvent, room, skill_target, ref data, invoker, cost_str);
                                if (broken)
                                {
                                    p.RemoveTag("JustShownSkill");
                                    break;
                                }
                            }
                            else
                            {
                                if (refuse.ContainsKey(p))
                                    refuse[p].Add(name);
                                else
                                    refuse[p] = new List<TriggerStruct> { name };
                            }
                            //-----------------------------------------------
                            p.RemoveTag("JustShownSkill");

                            trigger_who.Clear();
                            foreach (TriggerSkill skill in triggered) {
                                if (skill is GameRule)
                                {
                                    //room->tryPause();
                                    continue; // dont assign them to some person.
                                }
                                else
                                {
                                    //room->tryPause();
                                    if (skill.GetDynamicPriority(triggerEvent) == triggered[0].GetDynamicPriority(triggerEvent))
                                    {
                                        List<TriggerStruct> triggerSkillList = skill.Triggerable(triggerEvent, room, target, ref data);
                                        foreach (Player _p in room.Players) {
                                            foreach (TriggerStruct _skill in triggerSkillList) {
                                                if (_p.Name != _skill.Invoker || (_skill.SkillName.EndsWith("_h") && p.HasFlag(string.Format("huashen_{0}", trigger_id))))
                                                    continue;                   // huashen skill just trigger once at the same trigger id

                                                bool refuse_before = false;
                                                if (refuse.ContainsKey(_p))
                                                {
                                                    foreach (TriggerStruct refused in refuse[_p])
                                                    {
                                                        if (_skill.Equals(refused) && _skill.Targets.Count == 0)
                                                        {
                                                            refuse_before = true;
                                                            break;
                                                        }
                                                    }
                                                }
                                                if (refuse_before) break;

                                                TriggerStruct tskill = _skill;
                                                if (tskill.Times != 1) tskill.Times = 1;            //make sure times == 1
                                                TriggerSkill trskill = Engine.GetTriggerSkill(tskill.SkillName);
                                                if (trskill != null)
                                                {
                                                    if (trigger_who.ContainsKey(_p))
                                                        trigger_who[_p].Add(tskill);
                                                    else
                                                        trigger_who[_p] = new List<TriggerStruct> { tskill };
                                                }
                                            }
                                        }
                                    }
                                    else
                                        break;
                                }
                            }
                            foreach (TriggerStruct already_s in already_triggered) {
                                List<TriggerStruct> who_skills_q = trigger_who.ContainsKey(p) ? new List<TriggerStruct>(trigger_who[p]) : new List<TriggerStruct>();
                                foreach (TriggerStruct re_skill in who_skills_q) {
                                    if (already_s.Equals(re_skill))
                                    {
                                        if (already_s.Targets.Count > 0 && re_skill.Targets.Count > 0)
                                        {
                                            List<string> targets = new List<string>();
                                            Player already_target = room.FindPlayer(already_s.ResultTarget, true);
                                            Player _target = room.FindPlayer(re_skill.Targets[0], true);
                                            List<Player> all = room.GetAllPlayers(true);
                                            if (all.IndexOf(already_target) < all.IndexOf(_target))
                                                continue;

                                            for (int i = 1 + all.IndexOf(already_target); i < room.GetAllPlayers(true).Count; i++)
                                            {
                                                string new_target = room.GetAllPlayers(true)[i].Name;
                                                if (re_skill.Targets.Contains(new_target))
                                                    targets.Add(new_target);
                                            }

                                            if (targets.Count > 0)
                                            {
                                                TriggerStruct re_skill2 = already_s.Copy();
                                                re_skill2.Targets = targets;
                                                re_skill2.ResultTarget = null;
                                                trigger_who[p][trigger_who[p].IndexOf(re_skill)] = re_skill2;
                                                break;
                                            }
                                        }
                                        trigger_who[p].Remove(re_skill);
                                        break;
                                    }
                                }
                            }
                        }
                        if (broken) break;
                    }
                    // @todo_Slob: for drawing cards when game starts -- stupid design of triggering no player!
                    if (!broken)
                    {
                        if (trigger_who.ContainsKey(rule_player) && trigger_who[rule_player].Count > 0)
                        {
                            foreach (TriggerStruct s in trigger_who[rule_player]) {
                                TriggerSkill skill = null;
                                foreach (TriggerSkill rule in rules) { // because we cannot get a GameRule with Engine::getTriggerSkill()
                                    if (rule.Name == s.SkillName)
                                    {
                                        skill = rule;
                                        break;
                                    }
                                }
                                
                                TriggerStruct skill_cost = skill.Cost(triggerEvent, room, target, ref data, null, s);
                                if (!string.IsNullOrEmpty(skill_cost.SkillName))
                                {
                                    broken = skill.Effect(triggerEvent, room, target, ref data, null, skill_cost);
                                    if (broken)
                                        break;
                                }
                            }
                        }
                    }
                }
                if (broken)
                    break;
            } while (skills.Count != triggerable_tested.Count);

            foreach (TrustedAI ai in room.AIs)
                ai.Event(triggerEvent, target, data);

            // pop event stack
            //event_stack.pop_back();

            //room->tryPause();
            return broken;
        }

        public bool Trigger(TriggerEvent triggerEvent, Room room, Player target)
        {
            object data = new object();
            return Trigger(triggerEvent, room, target, ref data);
        }
    }
}
