using Decal.Adapter.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace SillAC
{
    public static class ExpHelper
    {

        static int[] ATTRIBUTE_COSTS = new int[] { 110, 167, 224, 283, 341, 402, 461, 523, 586, 649, 713, 779, 846, 914, 984, 1056, 1129, 1205, 1282, 1361, 1444, 1529, 1616, 1707, 1802, 1899, 2002, 2108, 2219, 2335, 2456, 2584, 2718, 2859, 3008, 3164, 3330, 3505, 3691, 3887, 4097, 4320, 4556, 4808, 5078, 5365, 5673, 6000, 6353, 6730, 7133, 7568, 8032, 8533, 9070, 9649, 10270, 10940, 11662, 12438, 13276, 14180, 15153, 16205, 17339, 18564, 19886, 21314, 22858, 24525, 26326, 28275, 30380, 32657, 35119, 37782, 40662, 43778, 47150, 50796, 54744, 59014, 63637, 68641, 74055, 79917, 86263, 93132, 100570, 108621, 117339, 126778, 136999, 148067, 160050, 173028, 187081, 202299, 218781, 236629, 255960, 276894, 299567, 324123, 350718, 379523, 410720, 444511, 481109, 520748, 563684, 610188, 660559, 715116, 774213, 838221, 907554, 982653, 1063998, 1152108, 1247548, 1350928, 1462906, 1584201, 1715586, 1857902, 2012059, 2179041, 2359917, 2555842, 2768070, 2997957, 3246973, 3516708, 3808891, 4125386, 4468218, 4839578, 5241844, 5677583, 6149584, 6660865, 7214694, 7814612, 8464457, 9168382, 9930889, 10756854, 11651557, 12620720, 13670541, 14807731, 16039562, 17373912, 18819313, 20385006, 22081004, 23918153, 25908197, 28063866, 30398940, 32928353, 35668279, 38636239, 41851211, 45333752, 49106133, 53192476, 57618912, 62413746, 67607636, 73233800, 79328210, 85929839, 93080902, 100827128, 109218048, 118307320, 128153068, 138818248, 150371063, 162885385, 176441242, 191125315, 207031505, 224261525, 242925553, 263142941, 285042968, 308765680 };
        static int[] VITAL_COSTS = new int[] { 73, 110, 148, 186, 226, 265, 304, 346, 386, 428, 471, 514, 558, 604, 649, 697, 746, 794, 847, 898, 953, 1009, 1067, 1127, 1189, 1253, 1321, 1392, 1464, 1541, 1621, 1706, 1794, 1887, 1985, 2088, 2198, 2313, 2436, 2566, 2704, 2851, 3007, 3173, 3352, 3541, 3743, 3961, 4193, 4441, 4708, 4995, 5301, 5632, 5986, 6368, 6779, 7220, 7697, 8209, 8763, 9358, 10001, 10695, 11444, 12252, 13125, 14067, 15086, 16187, 17376, 18661, 20051, 21553, 23179, 24936, 26837, 28894, 31118, 33526, 36130, 38950, 42001, 45302, 48877, 52745, 56934, 61467, 66376, 71690, 77444, 83673, 90420, 97723, 105633, 114199, 123473, 133518, 144395, 156176, 168933, 182750, 197715, 213921, 231474, 250484, 271076, 293377, 317532, 343694, 372031, 402724, 435969, 471977, 510980, 553226, 598986, 648551, 702238, 760392, 823382, 891612, 965518, 1045572, 1132287, 1226216, 1327958, 1438167, 1557545, 1686856, 1826927, 1978651, 2143002, 2321028, 2513868, 2722754, 2949024, 3194122, 3459617, 3747205, 4058725, 4396171, 4761698, 5157644, 5586542, 6051132, 6554387, 7099523, 7690028, 8329675, 9022557, 9773102, 10586111, 11466782, 12420747, 13454104, 14573463, 15785980, 17099411, 18522151, 20063300, 21732713, 23541065, 25499917, 27621799, 29920277, 32410048, 35107034, 38028482, 41193072, 44621040, 48334308, 52356618, 56713694, 61433396, 66545904, 72083911, 78082832, 84581025, 91620044, 99244901, 107504354, 116451220, 126142708, 136640793, 148012606, 160330866, 173674340, 188128360, 203785348, 220745428, 239117053, 259017701, 280574629, 303925687, 329220194 };
        static int[] SPECIALIZED_COSTS = new int[] { 23, 33, 41, 52, 62, 71, 82, 92, 102, 113, 124, 136, 146, 159, 170, 183, 195, 208, 222, 235, 250, 264, 280, 296, 311, 330, 347, 365, 385, 406, 426, 450, 472, 498, 523, 551, 580, 611, 643, 678, 714, 753, 794, 837, 885, 933, 987, 1043, 1104, 1167, 1237, 1310, 1388, 1473, 1564, 1660, 1764, 1875, 1994, 2123, 2261, 2408, 2567, 2739, 2921, 3120, 3331, 3560, 3806, 4071, 4355, 4662, 4992, 5347, 5730, 6142, 6587, 7065, 7581, 8136, 8735, 9380, 10075, 10825, 11633, 12503, 13442, 14455, 15545, 16722, 17991, 19358, 20834, 22423, 24139, 25987, 27983, 30133, 32453, 34954, 37653, 40562, 43700, 47086, 49736, 51574, 53222, 55502, 57443, 60474, 63521, 66723, 69411, 72626, 76408, 80802, 84857, 88624, 93160, 98525, 103785, 108008, 113273, 119660, 124260, 129164, 134479, 139315, 145789, 150035, 155187, 161400, 167832, 173663, 179076, 185279, 191489, 197943, 205899, 213630, 221435, 229632, 237571, 245623, 254190, 263709, 272646, 282507, 293839, 311229, 324311, 350770, 380346, 416833, 469093, 526051, 612707, 701142, 788517, 898090, 979213, 1013349, 1132071, 1219082, 1312215, 1427448, 1523916, 1628919, 1721944, 1871667, 2001981, 2165000, 2246091, 2306880, 2561281, 2853517, 3008141, 3230068, 3624596, 3897445, 4554780, 5203257, 6050052, 6602908, 7170177, 7760870, 8284707, 8852171, 9774576, 10904122, 11833972, 12498324, 13372496, 15473013, 17817698, 20425780, 23318002, 26516741, 30046134, 34932222, 38203095, 43889053, 48022787, 54639559, 60777410, 66477372, 73783716, 80744194, 87410318, 94837660, 101086167, 110220507, 119310439, 128431217, 138664023, 148096428, 160822906, 172945361, 184573733, 200826612, 217773013, 234817698, 258425780, 279318002, 310251741, 350046134 };
        static int[] TRAINED_COSTS = new int[] { 58, 80, 105, 129, 154, 178, 204, 230, 257, 283, 310, 338, 367, 396, 426, 456, 488, 521, 554, 588, 625, 661, 699, 739, 779, 823, 868, 914, 962, 1014, 1067, 1123, 1182, 1243, 1309, 1378, 1450, 1527, 1607, 1694, 1786, 1882, 1984, 2095, 2210, 2335, 2467, 2607, 2759, 2919, 3091, 3275, 3472, 3682, 3909, 4150, 4409, 4688, 4987, 5307, 5651, 6021, 6418, 6846, 7304, 7798, 8329, 8900, 9515, 10176, 10888, 11655, 12480, 13368, 14325, 15356, 16467, 17662, 18952, 20341, 21837, 23450, 25188, 27062, 29082, 31258, 33606, 36136, 38864, 41805, 44977, 48396, 52083, 56058, 60347, 64969, 69956, 75333, 81132, 87386, 94131, 101406, 109251, 117714, 126841, 136685, 147303, 158756, 171109, 184433, 198804, 214307, 231028, 249064, 268520, 289505, 312143, 336561, 362900, 391312, 421961, 455020, 490683, 529151, 570649, 615411, 663698, 715786, 771974, 832585, 897969, 968499, 1044582, 1126656, 1215190, 1310697, 1413722, 1524860, 1644747, 1774075, 1913586, 2064081, 2226428, 2401556, 2590476, 2794272, 3014114, 3251269, 3507097, 3783072, 4080777, 4401926, 4748365, 5122083, 5525232, 5960127, 6429268, 6935355, 7481293, 8070225, 8705533, 9390871, 10130178, 10927706, 11788038, 12716120, 13717288, 14797299, 15962360, 17219168, 18574951, 20037501, 21615227, 23317200, 25153203, 27133792, 29270353, 31575169, 34061491, 36743612, 39636950, 42758143, 46125129, 49757269, 53675443, 57902175, 62461767, 67380429, 72686441, 78410305, 84584929, 91245809, 98431242, 106182532, 114544244, 123564450, 133295006, 143791852, 155115335, 167330555, 180507736, 194722633, 210056968, 226598898, 244443524, 263693431, 284459291, 306860483 };

        public const int ATTRIBUTE_OFFSET = 100;
        public const int VITAL_OFFSET = 200;

        public static int? CostToLevel(this ExpTarget target, int offset = 0)
        {
            var targetLevel = offset + target.GetTimesLeveled();
            switch (target.TargetType())
            {
                case ExpTargetType.Attribute:
                    if (ATTRIBUTE_COSTS.Length > targetLevel)
                        return ATTRIBUTE_COSTS[targetLevel];
                    break;
                case ExpTargetType.Vital:
                    if (VITAL_COSTS.Length > targetLevel)
                        return VITAL_COSTS[targetLevel];
                    break;
                case ExpTargetType.Trained:
                    if (TRAINED_COSTS.Length > targetLevel)
                        return TRAINED_COSTS[targetLevel];
                    break;
                case ExpTargetType.Specialized:
                    if (SPECIALIZED_COSTS.Length > targetLevel)
                        return SPECIALIZED_COSTS[targetLevel];
                    break;
            }

            return null;
        }
        public static int GetTimesLeveled(this ExpTarget target)
        {
            switch (target.TargetType())
            {
                case ExpTargetType.Attribute:
                    return Globals.Host.Actions.AttributeClicks[target.AsAttribute()];
                //    
                //    Globals.Host.Actions.VitalClicks
                case ExpTargetType.Vital:
                    return Globals.Host.Actions.VitalClicks[target.AsVital()];
                    break;
                case ExpTargetType.Trained:
                case ExpTargetType.Specialized:
                    return Globals.Host.Actions.SkillClicks[target.AsSkill()];
                    break;
            }

            return -1;
        }

        public static AttributeType AsAttribute(this ExpTarget target)
        {
            return (AttributeType)(target - ATTRIBUTE_OFFSET);
        }
        public static VitalType AsVital(this ExpTarget target)
        {
            return (VitalType)(target - VITAL_OFFSET);
        }
        public static SkillType AsSkill(this ExpTarget target)
        {
            return (SkillType)(target);
        }

        public static void AddExp(this ExpTarget target, int exp)
        {
            switch (target.TargetType())
            {
                case ExpTargetType.Attribute:
                    //Util.WriteToChat($"Add experience to attribute {target}");
                    Globals.Host.Actions.AddAttributeExperience(target.AsAttribute(), exp);
                    break;
                case ExpTargetType.Vital:
                    //Util.WriteToChat($"Add experience to vital {target}");
                    Globals.Host.Actions.AddVitalExperience(target.AsVital(), exp);
                    break;
                case ExpTargetType.Specialized:
                case ExpTargetType.Trained:
                    //Util.WriteToChat($"Add experience to skill {target}");
                    Globals.Host.Actions.AddSkillExperience(target.AsSkill(), exp);
                    break;
                default:
                    Util.WriteToChat($"Failed to add {exp} exp to {target.GetName()}");
                    break;
            }
        }

        /// <summary>
        /// Tries to increase the level of the 
        /// </summary>
        /// <param name="target"></param>
        public static void IncreaseLevel(this ExpTarget target)
        {
            var cost = target.CostToLevel() ?? 0;
            target.AddExp(cost);
        }

        public static string GetName(this ExpTarget target)
        {
            return Enum.GetName(typeof(ExpTarget), target);
        }

        public static ExpTargetType TargetType(this ExpTarget target)
        {
            var enumVal = (int)target;

            if (enumVal >= ExpHelper.VITAL_OFFSET)
            {
                return ExpTargetType.Vital;
            }
            if (enumVal >= ExpHelper.ATTRIBUTE_OFFSET)
            {
                return ExpTargetType.Attribute;
            }

            //Determine skill train status
            switch (Globals.Host.Actions.SkillTrainLevel[target.AsSkill()])
            {
                case 3:
                    return ExpTargetType.Specialized;
                case 2:
                    return ExpTargetType.Trained;
                case 1:
                    return ExpTargetType.Untrained;
                case 0:
                    return ExpTargetType.Unusable;
            }
            //Globals.Core.CharacterFilter.Skills[(CharFilterSkillType)target].Training;  -- Throws errors with summoning

            throw new Exception("Unknown experience target type encountered.");
        }
    }

    public enum ExpTarget
    {
        //Attributes
        //CurrentStrength = 1,
        //CurrentEndurance = 2,
        //CurrentQuickness = 3,
        //CurrentCoordination = 4,
        //CurrentFocus = 5,
        //CurrentSelf = 6,
        //BaseStrength = 7,
        //BaseEndurance = 8,
        //BaseQuickness = 9,
        //BaseCoordination = 10,
        //BaseFocus = 11,
        //BaseSelf = 12,
        Strength = 1 + ExpHelper.ATTRIBUTE_OFFSET,
        Endurance = 2 + ExpHelper.ATTRIBUTE_OFFSET,
        Quickness = 3 + ExpHelper.ATTRIBUTE_OFFSET,
        Coordination = 4 + ExpHelper.ATTRIBUTE_OFFSET,
        Focus = 5 + ExpHelper.ATTRIBUTE_OFFSET,
        Self = 6 + ExpHelper.ATTRIBUTE_OFFSET,

        //Vitals
        //MaximumHealth = 1,
        //CurrentHealth = 2,
        //MaximumStamina = 3,
        //CurrentStamina = 4,
        //MaximumMana = 5,
        //CurrentMana = 6,
        //BaseHealth = 7,
        //BaseStamina = 8,
        //BaseMana = 9,
        Health = 1 + ExpHelper.VITAL_OFFSET,
        Stamina = 3 + ExpHelper.VITAL_OFFSET,
        Mana = 5 + ExpHelper.VITAL_OFFSET,

        //Skills
        MeleeDefense = 6,
        MissileDefense = 7,
        ArcaneLore = 14,
        MagicDefense = 15,
        ManaConversion = 16,
        ItemTinkering = 18,
        AssessPerson = 19,
        Deception = 20,
        Healing = 21,
        Jump = 22,
        Lockpick = 23,
        Run = 24,
        AssessCreature = 27,
        WeaponTinkering = 28,
        ArmorTinkering = 29,
        MagicItemTinkering = 30,
        CreatureEnchantment = 31,
        ItemEnchantment = 32,
        LifeMagic = 33,
        WarMagic = 34,
        Leadership = 35,
        Loyalty = 36,
        Fletching = 37,
        Alchemy = 38,
        Cooking = 39,
        Salvaging = 40,
        TwoHandedCombat = 41,
        VoidMagic = 43,
        HeavyWeapons = 44,
        LightWeapons = 45,
        FinesseWeapons = 46,
        MissileWeapons = 47,
        Shield = 48,
        DirtyFighting = 49,
        Recklessness = 50,
        SneakAttack = 51,
        DualWield = 52,
        Summoning = 54
    }

    public enum ExpTargetType
    {
        Attribute,
        Vital,
        Trained,
        Specialized,
        Untrained,
        Unusable
    }

}
