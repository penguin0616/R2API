using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using RoR2;
using R2API.Utils;
using UnityEngine;

namespace R2API {

    public enum HitEffectType {
        OnHitEnemy = 0,
        OnHitAll = 1
    }
    public enum StatIndex {
        MaxHealth,
        MaxShield,

        Regen,
        SafeRegen,

        MoveSpeed,
        RunningMoveSpeed,
        SafeMoveSpeed,
        SafeRunningMoveSpeed,

        JumpPower,
        JumpCount,

        Damage,
        AttackSpeed,

        Crit,
        Armor,
        RunningArmor,

        GlobalCoolDown,
        CoolDownPrimary,
        CoolDownSecondary,
        CoolDownUtility,
        CoolDownSpecial,
        CountPrimary,
        CountSecondary,
        CountUtility,
        CountSpecial,
    }

    public class CustomItemStat {
        public StatIndex Stat;
        private float m_BaseBonus;
        private float m_StackBonus;
        private float m_BaseMultBonus;
        private float m_StackMultBonus;

        #region Properties
        public float FlatBonus { get { return m_BaseBonus; } }
        public float StackBonus { get { return m_StackBonus; } }
        public float MultBonus { get { return m_BaseMultBonus; } }
        public float MultStackBonus { get { return m_StackMultBonus; } }

        public float GetFlatBonusFromCount(int count) {
            if (count > 0)
                return (count - 1) * m_StackBonus + m_BaseBonus;
            return 0;
        }
        public float GetPercentBonusFromCount(int count) {
            if (count > 0)
                return (count - 1) * m_StackMultBonus + m_BaseMultBonus;
            return 0;
        }
        #endregion



        #region Constructor
        /// <summary>
        ///
        /// </summary>
        /// <param name="flatBonus">Flat bonus when player Have the item</param>
        /// <param name="flatStackBonus">Flat bonus for each additional item the player own</param>
        /// <param name="multBonus">Multiplicative bonus when player Have the item</param>
        /// <param name="multStackBonus">Multiplicative bonus for each additional item the player own, for Cooldowns values of 0 are ignored</param>
        /// <param name="stat"></param>
        public CustomItemStat(float flatBonus, float flatStackBonus, float multBonus, float multStackBonus, StatIndex stat) {
            m_BaseBonus = flatBonus;
            m_StackBonus = flatStackBonus;
            m_BaseMultBonus = multBonus;
            m_StackMultBonus = multStackBonus;
            Stat = stat;
        }
        /// <summary>
        /// Set Flat and Stack Bonus at the same time, if you want to set the separately use ModItemStats(float FlatBonus, float StackBonus, StatIndex Stat)
        /// </summary>
        /// <param name="flatBonus">Flat bonus for each item the player own</param>
        /// <param name="stat"></param>
        public CustomItemStat(float flatBonus, StatIndex stat) {
            m_BaseBonus = flatBonus;
            m_StackBonus = flatBonus;
            m_BaseMultBonus = 0;
            m_StackMultBonus = 0;
            Stat = stat;
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="flatBonus">Flat bonus when player Have the item</param>
        /// <param name="FlatStackBonus">Flat bonus for each additional item the player own</param>
        /// <param name="MultBonus">Multiplicative bonus for each item the player own</param>
        public CustomItemStat(float flatBonus, float stackBonus, StatIndex stat) {
            m_BaseBonus = flatBonus;
            m_StackBonus = stackBonus;
            m_BaseMultBonus = 0;
            m_StackMultBonus = 0;
            Stat = stat;
        }
        public CustomItemStat(float flatBonus, float stackBonus, float multBonus, StatIndex stat) {
            m_BaseBonus = flatBonus;
            m_StackBonus = stackBonus;
            m_BaseMultBonus = multBonus;
            m_StackMultBonus = multBonus;
            Stat = stat;
        }
        #endregion

        #region Operator
        public static CustomItemStat operator +(CustomItemStat a, CustomItemStat b) {
            a.m_BaseBonus += b.m_BaseBonus;
            a.m_StackBonus += b.m_StackBonus;
            a.m_BaseMultBonus += b.m_BaseMultBonus;
            a.m_StackMultBonus += b.m_StackMultBonus;
            return a;
        }
        public static CustomItemStat operator -(CustomItemStat a, CustomItemStat b) {
            a.m_BaseBonus -= b.m_BaseBonus;
            a.m_StackBonus -= b.m_StackBonus;
            a.m_BaseMultBonus -= b.m_BaseMultBonus;
            a.m_StackMultBonus -= b.m_StackMultBonus;
            return a;
        }
        public static CustomItemStat operator *(CustomItemStat a, float b) {
            a.m_BaseBonus *= b;
            a.m_StackBonus *= b;
            a.m_BaseMultBonus *= b;
            a.m_StackMultBonus *= b;
            return a;
        }
        public static CustomItemStat operator /(CustomItemStat a, float b) {
            a.m_BaseBonus /= b;
            a.m_StackBonus /= b;
            a.m_BaseMultBonus /= b;
            a.m_StackMultBonus /= b;
            return a;
        }
        #endregion

    }

    public class ModHitEffect {

        public HitEffectType EffectType = HitEffectType.OnHitEnemy;
        /// <summary>
        /// Check if the effect is Proc or not
        /// </summary>
        /// <param name="globalEventManager"></param>
        /// <param name="damageInfo"></param>
        /// <param name="victim"></param>
        /// <param name="itemCount"></param>
        /// <returns></returns>
        public virtual bool Condition(GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim, int itemCount) {
            return true;
        }
        public virtual void Effect(GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim, int itemCount) {

        }
    }


    public class CustomItem {
        public ItemDef Item { get; private set; }

        private int m_Index;
        private List<ModHitEffect> m_EffectList;

        private List<CustomItemStat> m_StatList;


        #region properties
        public List<CustomItemStat> GetStatsList { get { return m_StatList; } }
        public List<ModHitEffect> GetHitEffectList { get { return m_EffectList; } }
        public int Index { get { return m_Index; } private set { m_Index = value; } }

        /// <summary>
        /// Flat bonus of the First Item
        /// </summary>
        /// <param name="stat"></param>
        /// <returns></returns>
        public float FlatBonus(StatIndex stat) {
            var s = m_StatList.Find(x => x.Stat == stat);
            if (s != null)
                return s.FlatBonus;
            return 0;
        }
        /// <summary>
        /// Flat bonus Per item after the first one
        /// </summary>
        /// <param name="stat"></param>
        /// <returns></returns>
        public float StackBonus(StatIndex stat) {
            var s = m_StatList.Find(x => x.Stat == stat);
            if (s != null)
                return s.StackBonus;
            return 0;
        }
        /// <summary>
        /// Multiplicative bonus of the First item
        /// </summary>
        /// <param name="stat"></param>
        /// <returns></returns>
        public float MultBonus(StatIndex stat) {
            var s = m_StatList.Find(x => x.Stat == stat);
            if (s != null)
                return s.MultBonus;
            return 0;
        }
        /// <summary>
        /// Multiplicative bonus Per item after the first one
        /// </summary>
        /// <param name="stat"></param>
        /// <returns></returns>
        public float MultStackBonus(StatIndex stat) {
            var s = m_StatList.Find(x => x.Stat == stat);
            if (s != null)
                return s.MultStackBonus;
            return 0;
        }
        /// <summary>
        /// Get FlatBonus From Count
        /// </summary>
        /// <param name="stat"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public float GetFlatBonusFromCount(StatIndex stat, int count) {
            var s = m_StatList.Find(x => x.Stat == stat);
            if (s != null)
                return s.GetFlatBonusFromCount(count);
            return 0;
        }
        /// <summary>
        /// Get Multiplicative Bonus from Count
        /// </summary>
        /// <param name="stat"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public float GetMultStackBonusFromCount(StatIndex stat, int count) {
            var s = m_StatList.Find(x => x.Stat == stat);
            if (s != null)
                return s.GetPercentBonusFromCount(count);
            return 0;
        }
        #endregion


        public CustomItem(int index) {
            m_Index = index;
            m_StatList = new List<CustomItemStat>();
            m_EffectList = new List<ModHitEffect>();
        }

        public CustomItem(int index, List<CustomItemStat> stats) {
            m_Index = index;
            m_StatList = stats;
            m_EffectList = new List<ModHitEffect>();
        }

        public CustomItem(int index, CustomItemStat stat) {
            m_Index = index;
            m_StatList = new List<CustomItemStat>
            {
                stat
            };
            m_EffectList = new List<ModHitEffect>();
        }
        public CustomItem(int index, CustomItemStat stat1, CustomItemStat stat2) {
            m_Index = index;
            m_StatList = new List<CustomItemStat>
            {
                stat1,
                stat2
            };
            m_EffectList = new List<ModHitEffect>();
        }
        public CustomItem(int index, CustomItemStat stat1, CustomItemStat stat2, CustomItemStat stat3) {
            m_Index = index;
            m_StatList = new List<CustomItemStat>
            {
                stat1,
                stat2,
                stat3
            };
            m_EffectList = new List<ModHitEffect>();
        }
        public CustomItem(int index, CustomItemStat stat1, CustomItemStat stat2, CustomItemStat stat3, CustomItemStat stat4) {
            m_Index = index;
            m_StatList = new List<CustomItemStat>
            {
                stat1,
                stat2,
                stat3,
                stat4
            };
            m_EffectList = new List<ModHitEffect>();
        }

        #region Operator

        public static CustomItem operator +(CustomItem item, ModHitEffect effect) {
            if (!item.m_EffectList.Exists(x => x.GetType() == effect.GetType())) {
                item.m_EffectList.Add(effect);
            }
            return item;
        }
        public static CustomItem operator +(CustomItem item, List<ModHitEffect> effects) {
            foreach (var effect in effects)
                if (!item.m_EffectList.Exists(x => x.GetType() == effect.GetType())) {
                    item.m_EffectList.Add(effect);
                }
            return item;
        }


        public static CustomItem operator +(CustomItem item, CustomItemStat stat) {
            if (item.m_StatList.Exists(x => x.Stat == stat.Stat)) {
                item.m_StatList[item.m_StatList.FindIndex(x => x.Stat == stat.Stat)] += stat;
            }
            else {
                item.m_StatList.Add(stat);
            }
            return item;
        }
        public static CustomItem operator +(CustomItem item, List<CustomItemStat> stats) {
            foreach (var stat in stats)
                if (item.m_StatList.Exists(x => x.Stat == stat.Stat)) {
                    item.m_StatList[item.m_StatList.FindIndex(x => x.Stat == stat.Stat)] += stat;
                }
                else {
                    item.m_StatList.Add(stat);
                }
            return item;
        }
        public static CustomItem operator -(CustomItem item, CustomItemStat stat) {
            if (item.m_StatList.Exists(x => x.Stat == stat.Stat)) {
                item.m_StatList[item.m_StatList.FindIndex(x => x.Stat == stat.Stat)] -= stat;
            }
            return item;
        }
        #endregion
    }

    public static class CustomItemAPI {

        internal static void InitHooks() {

            Init();
        }

        public static Dictionary<int, CustomItem> ModItemDictionary;
        private static Dictionary<int, CustomItem> m_DefaultModItemDictionary;
        public static Dictionary<int, CustomItem> DefaultModItemDictionary { get { return m_DefaultModItemDictionary; } }

        private static void DefaultOnHitEffect(int index, ModHitEffect hitEffect) {
            if (m_DefaultModItemDictionary.ContainsKey(index)) {
                m_DefaultModItemDictionary[index] += hitEffect;
            }
            else {
                throw new Exception("ModItemManager ERROR : int does not exist in m_DefaultModItemDictionary\nBtw you shouldn't mess with this boi !  Index :  " + index);
            }
        }

        private static void DefaultStatItem(int index, CustomItemStat stat) {
            if (m_DefaultModItemDictionary.ContainsKey(index)) {
                m_DefaultModItemDictionary[index] += stat;
            }
            else {
                throw new Exception("ModItemManager ERROR : int does not exist in m_DefaultModItemDictionary\nBtw you shouldn't mess with this boi !  Index :  " + index);
            }
        }

        public static void AddOnHitEffect(int index, ModHitEffect hitEffect) {
            if (ModItemDictionary.ContainsKey(index)) {
                ModItemDictionary[index] += hitEffect;
            }
            else {
                throw new Exception("ModItemManager ERROR : int does not exist in ModItemDictionary");
            }
        }
        public static void AddOnHitEffect(int index, List<ModHitEffect> hitEffects) {
            if (ModItemDictionary.ContainsKey(index)) {
                ModItemDictionary[index] += hitEffects;
            }
            else {
                throw new Exception("ModItemManager ERROR : int does not exist in ModItemDictionary");
            }
        }

        public static void AddModItem(int index, CustomItem modItem) {
            if (!ModItemDictionary.ContainsKey(index)) {
                ModItemDictionary.Add(index, modItem);
            }
            else {
                ModItemDictionary[index] += modItem.GetStatsList;
            }

        }
        public static void AddStatToItem(int index, CustomItemStat stat) {
            if (ModItemDictionary.ContainsKey(index)) {
                ModItemDictionary[index] += stat;
            }
            else {
                throw new Exception("ModItemManager ERROR : int does not exist in ModItemDictionary");
            }
        }
        public static void AddStatToItem(int index, List<CustomItemStat> stats) {
            if (ModItemDictionary.ContainsKey(index)) {
                ModItemDictionary[index] += stats;
            }
            else {
                throw new Exception("ModItemManager ERROR : int does not exist in ModItemDictionary");
            }
        }

        public static CustomItem GetModItem(int index) {
            if (ModItemDictionary.ContainsKey(index)) {
                return ModItemDictionary[index];
            }
            else {
                throw new Exception("ModItemManager ERROR : int does not exist in ModItemDictionary");
            }
        }

        public static void Init() {
            m_DefaultModItemDictionary = new Dictionary<int, CustomItem>();

            foreach (var itemIndex in (ItemIndex[])Enum.GetValues(typeof(ItemIndex))) {
                if (itemIndex != ItemIndex.Count && itemIndex != ItemIndex.None) {
                    m_DefaultModItemDictionary.Add((int)itemIndex, new CustomItem((int)itemIndex));
                }
            }

            //Default On Hit Effect
            DefaultOnHitEffect((int)ItemIndex.HealOnCrit, new HealOnCritHitReplace());
            DefaultOnHitEffect((int)ItemIndex.CooldownOnCrit, new CoolDownOnCritHitReplace());
            DefaultOnHitEffect((int)ItemIndex.AttackSpeedOnCrit, new AttackSpeedOnCritHitReplace());
            DefaultOnHitEffect((int)ItemIndex.Seed, new HealOnHitReplace());
            DefaultOnHitEffect((int)ItemIndex.BleedOnHit, new BleedOnHitReplace());
            DefaultOnHitEffect((int)ItemIndex.SlowOnHit, new SlowOnHitReplace());
            DefaultOnHitEffect((int)ItemIndex.GoldOnHit, new GoldOnHitReplace());
            DefaultOnHitEffect((int)ItemIndex.Missile, new MissileOnHitReplace());
            DefaultOnHitEffect((int)ItemIndex.ChainLightning, new UkeleleOnHitReplace());
            DefaultOnHitEffect((int)ItemIndex.BounceNearby, new HookEffectReplace());
            DefaultOnHitEffect((int)ItemIndex.StickyBomb, new StickyBombOnHitReplace());
            DefaultOnHitEffect((int)ItemIndex.IceRing, new IceRingEffectReplace());
            DefaultOnHitEffect((int)ItemIndex.FireRing, new FireRingEffectReplace());
            DefaultOnHitEffect((int)ItemIndex.Behemoth, new BehemothEffectReplace());

            //Default Stats
            DefaultStatItem((int)ItemIndex.Knurl, new CustomItemStat(40, StatIndex.MaxHealth));
            DefaultStatItem((int)ItemIndex.BoostHp, new CustomItemStat(0, 0, 0.1f, StatIndex.MaxHealth));
            DefaultStatItem((int)ItemIndex.PersonalShield, new CustomItemStat(25, StatIndex.MaxShield));
            DefaultStatItem((int)ItemIndex.HealWhileSafe, new CustomItemStat(0, 0, 2.5f, 1.5f, StatIndex.SafeRegen));
            DefaultStatItem((int)ItemIndex.Knurl, new CustomItemStat(1.6f, StatIndex.Regen));
            DefaultStatItem((int)ItemIndex.HealthDecay, new CustomItemStat(0, 0, -0.1f, StatIndex.Regen));
            DefaultStatItem((int)ItemIndex.SprintOutOfCombat, new CustomItemStat(0, 0, 0.3f, StatIndex.SafeRunningMoveSpeed));
            DefaultStatItem((int)ItemIndex.Hoof, new CustomItemStat(0, 0, 0.14f, StatIndex.MoveSpeed));
            DefaultStatItem((int)ItemIndex.SprintBonus, new CustomItemStat(0, 0, 0.3f, 0.2f, StatIndex.RunningMoveSpeed));
            DefaultStatItem((int)ItemIndex.Feather, new CustomItemStat(1, StatIndex.JumpCount));
            DefaultStatItem((int)ItemIndex.BoostDamage, new CustomItemStat(0, 0, 0.1f, StatIndex.Damage));
            DefaultStatItem((int)ItemIndex.Syringe, new CustomItemStat(0, 0, 0.15f, StatIndex.AttackSpeed));
            DefaultStatItem((int)ItemIndex.CritGlasses, new CustomItemStat(10, StatIndex.Crit));
            DefaultStatItem((int)ItemIndex.AttackSpeedOnCrit, new CustomItemStat(5, 0, StatIndex.Crit));
            DefaultStatItem((int)ItemIndex.CritHeal, new CustomItemStat(5, 0, StatIndex.Crit));
            DefaultStatItem((int)ItemIndex.HealOnCrit, new CustomItemStat(5, 0, StatIndex.Crit));
            DefaultStatItem((int)ItemIndex.CooldownOnCrit, new CustomItemStat(5, 0, StatIndex.Crit));
            DefaultStatItem((int)ItemIndex.SprintArmor, new CustomItemStat(30, StatIndex.RunningArmor));
            DefaultStatItem((int)ItemIndex.DrizzlePlayerHelper, new CustomItemStat(70, StatIndex.Armor));
            DefaultStatItem((int)ItemIndex.AlienHead, new CustomItemStat(0, 0, 0.75f, StatIndex.GlobalCoolDown));
            DefaultStatItem((int)ItemIndex.UtilitySkillMagazine, new CustomItemStat(0, 0, 2f / 3f, 1, StatIndex.CoolDownUtility));
            DefaultStatItem((int)ItemIndex.SecondarySkillMagazine, new CustomItemStat(1, StatIndex.CountSecondary));
            DefaultStatItem((int)ItemIndex.UtilitySkillMagazine, new CustomItemStat(2, StatIndex.CountUtility));

            Update();
        }

        public static void Update() {

            ModItemDictionary = new Dictionary<int, CustomItem>();

            foreach (var kv in m_DefaultModItemDictionary) {
                ModItemDictionary.Add(kv.Key, kv.Value);
            }
        }

        public static float GetBonusForStat(CharacterBody c, StatIndex stat) {
            float value = 0;
            if (c.inventory) {
                foreach (var kv in ModItemDictionary) {
                    if (ModItemDictionary.ContainsKey(kv.Key) && c.inventory.GetItemCount(kv.Key) > 0)
                        value += kv.Value.GetFlatBonusFromCount(stat, c.inventory.GetItemCount(kv.Key));
                }
            }
            return value;
        }
        public static float GetMultiplierForStat(CharacterBody c, StatIndex stat) {
            float value = 0;
            if (c.inventory) {
                foreach (var kv in ModItemDictionary) {
                    if (ModItemDictionary.ContainsKey(kv.Key) && c.inventory.GetItemCount(kv.Key) > 0)
                        value += kv.Value.GetMultStackBonusFromCount(stat, c.inventory.GetItemCount(kv.Key));
                }
            }
            return value;
        }

        public static float GetMultiplierForStatCD(CharacterBody c, StatIndex stat) {
            float value = 1;
            if (c.inventory) {
                foreach (var kv in ModItemDictionary) {
                    if (ModItemDictionary.ContainsKey(kv.Key) && c.inventory.GetItemCount(kv.Key) > 0)
                        if (kv.Value.GetMultStackBonusFromCount(stat, c.inventory.GetItemCount(kv.Key)) != 0)
                            value *= kv.Value.GetMultStackBonusFromCount(stat, c.inventory.GetItemCount(kv.Key));
                }
            }
            return value;
        }

        public static void OnHitEnemyEffects(GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim) {
            var procCoefficient = damageInfo.procCoefficient;
            var body = damageInfo.attacker.GetComponent<CharacterBody>();
            var master = body.master;
            if (!(bool)body || procCoefficient <= 0.0 || !(bool)body || !(bool)master || !(bool)master.inventory)
                return;

            var inventory = master.inventory;

            foreach (var kv in ModItemDictionary) {
                var count = inventory.GetItemCount(kv.Key);
                if (count > 0) {
                    foreach (var hitEffects in kv.Value.GetHitEffectList) {

                        if (hitEffects.EffectType == HitEffectType.OnHitEnemy && hitEffects.Condition(globalEventManager, damageInfo, victim, count)) {
                            hitEffects.Effect(globalEventManager, damageInfo, victim, count);
                        }
                    }
                }
            }
        }

        public static void OnHitAllEffects(GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim) {
            var procCoefficient = damageInfo.procCoefficient;
            var body = damageInfo.attacker.GetComponent<CharacterBody>();
            var master = body.master;
            if (!(bool)body || procCoefficient <= 0.0 || !(bool)body || !(bool)master || !(bool)master.inventory)
                return;

            var inventory = master.inventory;

            foreach (var kv in ModItemDictionary) {
                var count = inventory.GetItemCount(kv.Key);
                if (count > 0) {
                    foreach (var hitEffects in kv.Value.GetHitEffectList) {

                        if (hitEffects.EffectType == HitEffectType.OnHitAll && hitEffects.Condition(globalEventManager, damageInfo, victim, count)) {
                            hitEffects.Effect(globalEventManager, damageInfo, victim, count);
                        }
                    }
                }
            }
        }

    }

    static class InventoryExtender {
        public static int GetItemCount(this Inventory inv, int itemIndex) {
            return inv.GetFieldValue<int[]>("itemStacks")[itemIndex];
        }
    }

}
