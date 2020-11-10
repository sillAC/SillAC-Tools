using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Timers;

namespace SillAC
{
    public class ExperiencePolicy 
    {
        private Timer timer;

        private List<KeyValuePair<ExpTarget, int>> flatPlan { get; set; }
        private int planIndex { get; set; } = 0;

        /// <summary>
        /// Weights of how to spend the 
        /// </summary>
        public Dictionary<ExpTarget, double> Weights { get; set; } = new Dictionary<ExpTarget, double>();

        public ExperiencePolicy()        {        }

        /// <summary>
        /// Spends experience on the next target in the plan
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SpendExperienceTick(object sender, ElapsedEventArgs e)
        {
            //Util.WriteToChat($"{FlatPlan.Count} | {PlanIndex} | {FlatPlan.Count < PlanIndex}");
            //Check if plan finished
            if (flatPlan.Count <= planIndex)
            {
                Util.WriteToChat($"Finished leveling {planIndex} targets.");
                timer.Enabled = false;
            }
            //Otherwise level the next thing
            else
            {
                var target = flatPlan[planIndex].Key;
                var exp = flatPlan[planIndex].Value;
                //Util.WriteToChat($"Adding {exp} to {target}");
                target.AddExp(exp);
                planIndex++;
            }
        }

        public Dictionary<ExpTarget, List<int>> GetPlan()
        {
            return GetPlan(Globals.Core.CharacterFilter.UnassignedXP);
        }
        public Dictionary<ExpTarget, List<int>> GetPlan(long expToSpend, int maxSteps = 9999)
        {
            var plan = new Dictionary<ExpTarget, List<int>>();
            var weightedCosts = new Dictionary<ExpTarget, double>();
            //ExpTarget.Alchemy.CostToLevel()

            //Util.WriteToChat($"Creating a plan to spend {expToSpend} up to {maxSteps} times on {Weights.Count} targets of exp.");


            //Util.WriteToChat($"Initial costs/weights/weighted costs:");
            //Find what exp targets are candidates to be leveled
            foreach (var t in Weights)
            {
                //Skip invalid weights
                if (t.Value <= 0)
                    continue;

                try
                {
                    var cost = t.Key.CostToLevel() ?? -1;

                    //Continue if no known cost to level
                    if (cost < 0)
                    {
                        //Util.WriteToChat($"  {t.Key}: n/a");
                        continue;
                    }

                    //Otherwise consider it for spending exp on
                    plan.Add(t.Key, new List<int>());

                    //Figure out initial weighted cost of exp target
                    weightedCosts.Add(t.Key, cost / Weights[t.Key]);

                    //Util.WriteToChat($"  {t.Key}: {cost} \t {Weights[t.Key]} \t {cost / Weights[t.Key]}");
                }
                catch (Exception e)
                {
                    Util.LogError(e);
                }
            }

            for (var i = 0; i < maxSteps; i++)
            {
                //Get the most efficient thing to spend exp on as determined by weighted cost
                var nextTarget = weightedCosts.OrderBy(t => t.Value).First().Key;

                //Find cost of leveling that skill after the steps previously taken in the plan
                var timesLeveled = plan[nextTarget].Count;
                var cost = nextTarget.CostToLevel(timesLeveled) ?? -1;

                //Halt if there is insufficient exp or no more levels
                if (expToSpend < cost || cost == -1)
                {
                    break;
                }

                //Add to plan
                plan[nextTarget].Add(cost);
                //Simulate use of that exp
                expToSpend -= cost;

                //Update weighted cost
                var nextCost = nextTarget.CostToLevel(timesLeveled + 1) ?? -1;
                //TODO: Improve logic here.  If there's no next level, set weight cost to max value
                if (nextCost == -1)
                {
                    weightedCosts[nextTarget] = double.PositiveInfinity;
                }
                else
                {
                    var newWeightedCost = nextCost / Weights[nextTarget];
                    weightedCosts[nextTarget] = newWeightedCost;
                }
            }

            return plan;
        }

        public void PrintExperiencePlan()
        {
            PrintExperiencePlan(Globals.Core.CharacterFilter.UnassignedXP);
        }

        public void PrintExperiencePlan(long expToSpend)
        {
            var plan = GetPlan(expToSpend);

            Util.WriteToChat($"Experience plan for {expToSpend} exp:");
            foreach (var t in plan)
            {
                var steps = t.Value.Count;
                var description = new StringBuilder($"{t.Key.ToString()} ({steps}): ");

                for (var i = 0; i < t.Value.Count; i++)
                {
                    description.Append($"{t.Value[i]}\t");
                }

                Util.WriteToChat(description.ToString());
            }
        }

        internal void SpendExperience()
        {
            SpendExperience(Globals.Core.CharacterFilter.UnassignedXP);
        }

        internal void SpendExperience(long expToSpend)
        {
            //Already spending experience?  Continue?
            if (timer.Enabled)
            {
                Util.WriteToChat("Stopping spending experience.");
                timer.Enabled = false;
                flatPlan = null;
                return;
            }

            //Get plan for leveling
            flatPlan = new List<KeyValuePair<ExpTarget, int>>();
            foreach (var t in GetPlan(expToSpend))
            {
                for (var i = 0; i < t.Value.Count; i++)
                {
                    //TODO: Optionally add logic here for things like grouping levels of 10, if that works with Decal?  Doesn't seem to work...
                    //if ((t.Value.Count - i) >= 10)
                    //{
                    //    var exp = t.Value.Skip(i).Take(10).Sum();
                    //    Util.WriteToChat($"Planning to level {t.Key} from {i+1} to {i + 10} for {exp}");
                    //    //Skip next 9 steps
                    //    i += 9;
                    //    continue;
                    //}
                    flatPlan.Add(new KeyValuePair<ExpTarget, int>(t.Key, t.Value[i]));
                }
            }

            //Sort by cost?
            flatPlan = flatPlan.OrderBy(t => t.Value).ToList();

            //Start at beginning of plan
            planIndex = 0;

            Util.WriteToChat($"Spending on a plan consisting of {flatPlan.Count} steps with {expToSpend} available exp.");

            //Start leveling
            timer.Enabled = true;
        }

        internal void PrintPolicy()
        {
            Util.WriteToChat("Current experience policy weights:");
            foreach (var kvp in Weights)
                Util.WriteToChat(kvp.Key + ": " + kvp.Value);
        }

        /// <summary>
        /// Behavior when logging off.
        /// </summary>
        internal void Stop()
        {
            timer.Enabled = false;
            timer.Elapsed -= SpendExperienceTick;
        }
        public void Start(int interval)
        {
            timer = new Timer() { AutoReset = true, Enabled = false, Interval = interval };
            timer.Elapsed += SpendExperienceTick;
        }

        public static ExperiencePolicy Default
        {
            get
            {
                return new ExperiencePolicy()
                {
                    Weights = new Dictionary<ExpTarget, double>
                    {
                //Attributes
                { ExpTarget.Strength, 1},
                { ExpTarget.Endurance, 1},
                { ExpTarget.Coordination, 1},
                { ExpTarget.Quickness, 1},
                { ExpTarget.Focus, 1},
                { ExpTarget.Self, 1},
                //Vitals
                { ExpTarget.Health, 1.4},
                { ExpTarget.Stamina, .1},
                { ExpTarget.Mana, .1},
                //Skills
                { ExpTarget.Alchemy, 0},
                { ExpTarget.ArcaneLore, .1},
                { ExpTarget.ArmorTinkering, 0},
                { ExpTarget.AssessCreature, 0},
                { ExpTarget.AssessPerson, 0},
                { ExpTarget.Cooking, 0},
                { ExpTarget.CreatureEnchantment, .2},
                { ExpTarget.Deception, .1},
                { ExpTarget.DirtyFighting, .1},
                { ExpTarget.DualWield, .1},
                { ExpTarget.FinesseWeapons, 10},
                { ExpTarget.Fletching, .1},
                { ExpTarget.Healing, .1},
                { ExpTarget.HeavyWeapons, 10},
                { ExpTarget.ItemEnchantment, .2},
                { ExpTarget.ItemTinkering, 0},
                { ExpTarget.Jump, .02},
                { ExpTarget.Leadership, .1},
                { ExpTarget.LifeMagic, 1},
                { ExpTarget.LightWeapons, 10},
                { ExpTarget.Lockpick, 0},
                { ExpTarget.Loyalty, .1},
                { ExpTarget.MagicDefense, .1},
                { ExpTarget.MagicItemTinkering, 0},
                { ExpTarget.ManaConversion, .1},
                { ExpTarget.MeleeDefense, 5},
                { ExpTarget.MissileDefense, 5},
                { ExpTarget.MissileWeapons, 10},
                { ExpTarget.Recklessness, .1},
                { ExpTarget.Run, .1},
                { ExpTarget.Salvaging, .1},
                { ExpTarget.Shield, 0},
                { ExpTarget.SneakAttack, .1},
                { ExpTarget.Summoning, 1},
                { ExpTarget.TwoHandedCombat, 10},
                { ExpTarget.VoidMagic, 10},
                { ExpTarget.WarMagic, 10},
                { ExpTarget.WeaponTinkering, 0}
            }
                };
            }
        }
    }
}
