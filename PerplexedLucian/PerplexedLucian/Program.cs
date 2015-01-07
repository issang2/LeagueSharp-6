﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using System.Reflection;
namespace PerplexedLucian
{
    class Program
    {
        static Obj_AI_Hero Player = ObjectManager.Player;
        static System.Version Version = Assembly.GetExecutingAssembly().GetName().Version;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Lucian")
                return;

            SpellManager.Initialize();
            ItemManager.Initialize();
            Config.Initialize();

            if (Updater.Outdated())
            {
                Game.PrintChat("<font color=\"#ff0000\">Perplexed Lucian is outdated! Please update to {0}!</font>", Updater.GetLatestVersion());
                return;
            }

            CustomDamageIndicator.Initialize(DamageCalc.GetDrawDamage); //Credits to Hellsing for this! Borrowed it from his Kalista assembly.

            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;

            Game.PrintChat("<font color=\"#ff3300\">Perplexed Lucian ({0})</font> - <font color=\"#ffffff\">Loaded!</font>", Version);
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            SpellManager.UseHealIfInDanger(0);
            SpellManager.IgniteIfPossible();
            ItemManager.CleanseCC();
            switch(Config.Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    break;
            }
        }

        private static void Combo()
        {
            if (HasPassive && Config.CheckPassive)
                return;
            ItemManager.UseOffensiveItems();
            if (Config.ComboQ && SpellManager.Q.IsReady())
            {
                var target = TargetSelector.GetTarget(SpellManager.Q.Range, TargetSelector.DamageType.Physical);
                if (target != null)
                {
                    SpellManager.CastSpell(SpellManager.Q, target, Config.UsePackets);
                    if(Config.CheckPassive)
                        return;
                }
                target = TargetSelector.GetTarget(SpellManager.Q2.Range, TargetSelector.DamageType.Physical);
                var collisions = SpellManager.Q2.GetPrediction(target).CollisionObjects;
                foreach (Obj_AI_Base collision in collisions)
                    SpellManager.CastSpell(SpellManager.Q2, collision, Config.UsePackets);
            }
            if (Config.ComboW && SpellManager.W.IsReady())
            {
                var target = TargetSelector.GetTarget(SpellManager.W.Range, TargetSelector.DamageType.Physical);
                if (target != null)
                {
                    PredictionOutput prediction = SpellManager.W.GetPrediction(target);
                    switch (prediction.Hitchance)
                    {
                        case HitChance.High:
                            SpellManager.CastSpell(SpellManager.W, target, Config.UsePackets);
                            if (Config.CheckPassive)
                                return;
                            break;
                        case HitChance.Collision:
                            var collisions = prediction.CollisionObjects.Where(collision => collision.Distance(target) <= 100).ToList();
                            if (collisions.Count > 0)
                            {
                                SpellManager.CastSpell(SpellManager.W, collisions[0], Config.UsePackets);
                                if (Config.CheckPassive)
                                    return;
                            }
                            break;
                    }
                }
            }
            if (Config.ComboE && SpellManager.E.IsReady())
            {
                float range = 500 + SpellManager.E.Range;
                var target = TargetSelector.GetTarget(range, TargetSelector.DamageType.Physical);
                if (target != null && target.IsValidTarget(range))
                {
                    SpellManager.CastSpell(SpellManager.E, Game.CursorPos, Config.UsePackets);
                    if (Config.CheckPassive)
                        return;
                }
            }

            if (Config.ComboR && SpellManager.R.IsReady())
            {
                var target = TargetSelector.GetTarget(SpellManager.R.Range, TargetSelector.DamageType.Physical);
                if (target != null)
                {
                    if (target.Health <= DamageCalc.GetUltDamage(target))
                    {
                        SpellManager.CastSpell(SpellManager.R, target, HitChance.High, Config.UsePackets);
                        if (Config.CheckPassive)
                            return;
                    }
                }
            }
        }

        static void Harass()
        {
            if (HasPassive && Config.CheckPassive)
                return;
            if (Config.ComboQ && SpellManager.Q.IsReady())
            {
                var target = TargetSelector.GetTarget(SpellManager.Q.Range, TargetSelector.DamageType.Physical);
                if (target != null)
                {
                    SpellManager.CastSpell(SpellManager.Q, target, Config.UsePackets);
                    if (Config.CheckPassive)
                        return;
                }
                target = TargetSelector.GetTarget(SpellManager.Q2.Range, TargetSelector.DamageType.Physical);
                var collisions = SpellManager.Q2.GetPrediction(target).CollisionObjects;
                foreach (Obj_AI_Base collision in collisions)
                    SpellManager.CastSpell(SpellManager.Q2, collision, Config.UsePackets);
            }
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Config.DrawQ.Active)
                Render.Circle.DrawCircle(Player.Position, SpellManager.Q.Range, Config.DrawQ.Color);
            if (Config.DrawW.Active)
                Render.Circle.DrawCircle(Player.Position, SpellManager.W.Range, Config.DrawW.Color);
            if (Config.DrawE.Active)
                Render.Circle.DrawCircle(Player.Position, SpellManager.E.Range, Config.DrawE.Color);
            if (Config.DrawR.Active)
                Render.Circle.DrawCircle(Player.Position, SpellManager.R.Range, Config.DrawR.Color);
        }

        private static bool HasPassive
        {
            get { return Player.HasLucianPassive(); }
        }
    }
}