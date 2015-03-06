using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace PerplexedEzreal
{
    class SpellManager
    {
        private static Obj_AI_Hero Player = ObjectManager.Player;

        private static Spell _Q, _W, _E, _R;

        public static Spell Q { get { return _Q; } }
        public static Spell W { get { return _W; } }
        public static Spell E { get { return _E; } }
        public static Spell R { get { return _R; } }

        public static void Initialize()
        {
            _Q = new Spell(SpellSlot.Q, 1180f);
            _Q.SetSkillshot(0.25f, 60f, 2000f, true, SkillshotType.SkillshotLine);

            _W = new Spell(SpellSlot.W, 850f);
            _W.SetSkillshot(0.25f, 80f, 1600f, false, SkillshotType.SkillshotLine);

            _E = new Spell(SpellSlot.E, 475);

            _R = new Spell(SpellSlot.R, float.MaxValue);
            _R.SetSkillshot(1f, 160f, 2000f, false, SkillshotType.SkillshotLine);
        }

        public static void CastSpell(Spell spell, Obj_AI_Base target, HitChance hitChance, bool packetCast)
        {
            Obj_AI_Hero Player = ObjectManager.Player;
            if (target.IsValidTarget(spell.Range) && spell.GetPrediction(target).Hitchance >= hitChance)
                spell.Cast(target, packetCast);
        }

        internal static void CastSpell(Spell spell, Vector3 position, bool packetCast)
        {
            spell.Cast(position, packetCast);
        }

        public static void UseHealIfInDanger(double incomingDmg)
        {
            if (Config.UseHeal && !Player.InFountain())
            {
                int healthToUse = (int)(Player.MaxHealth / 100) * Config.HealPct;
                if ((Player.Health - incomingDmg) <= healthToUse)
                {
                    SpellSlot healSlot = Utility.GetSpellSlot(Player, "SummonerHeal");
                    if(healSlot != SpellSlot.Unknown)
                        Player.Spellbook.CastSpell(healSlot);
                }
            }
        }

        internal static void IgniteIfPossible()
        {
            if (Config.UseIgnite)
            {
                SpellSlot igniteSlot = Utility.GetSpellSlot(Player, "SummonerDot");
                if (igniteSlot != SpellSlot.Unknown)
                {
                    var targets = ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(600) && hero.IsEnemy);
                    foreach (var target in targets)
                    {
                        if (Config.IgniteMode == "Combo" && Config.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                            Player.Spellbook.CastSpell(igniteSlot, target);
                        else
                        {
                            double igniteDamage = Damage.GetSummonerSpellDamage(Player, target, Damage.SummonerSpell.Ignite);
                            if (target.Health < igniteDamage)
                                Player.Spellbook.CastSpell(igniteSlot, target);
                        }
                    }
                }
            }
        }
    }
}
