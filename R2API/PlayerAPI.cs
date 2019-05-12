using R2API.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Reflection;

namespace R2API {

    public enum Priority : short {
        Last = short.MaxValue,
        Multiplicative = 16000,
        Additive = 8000,
        High = 1000,
        VeryHigh = 400,
        Critical = 200,
        Maximum = 1,
    }
    [Flags]
    public enum FunctionTag : int {
        None = 0x0,
        Health = 0x1,
        Shield = 0x2,
        Regen = 0x4,
        MoveSpeed = 0x8,
        JumpPower = 0x10,
        JumpCount = 0x20,
        Damage = 0x40,
        AttackSpeed = 0x80,
        Crit = 0x100,
        Armor = 0x200,
        GeneralCoolDown = 0x400,
        PrimaryCoolDown = 0x800,
        SecondaryCoolDown = 0x1000,
        UtilityCoolDown = 0x2000,
        SpecialCoolDown = 0x4000,
        PrimaryCount = 0x8000,
        SecondaryCount = 0x10000,
        UtilityCount = 0x20000,
        SpecialCount = 0x40000,
        All = 0x7ffff,
    }


    public class ModRecalculateCustom {
        public short RecalculatePriority;

        public FunctionTag FlagOverWrite = FunctionTag.None;

        public virtual float RecalculateHealth(float baseValue, CharacterBody character) => baseValue;
        public virtual float RecalculateShield(float baseValue, CharacterBody character) => baseValue;
        public virtual float RecalculateRegen(float baseValue, CharacterBody character) => baseValue;

        public virtual float RecalculateMoveSpeed(float baseValue, CharacterBody character) => baseValue;
        public virtual float RecalculateJumpPower(float baseValue, CharacterBody character) => baseValue;
        public virtual float RecalculateJumpCount(float baseValue, CharacterBody character) => baseValue;

        public virtual float RecalculateDamage(float baseValue, CharacterBody character) => baseValue;
        public virtual float RecalculateAttackSpeed(float baseValue, CharacterBody character) => baseValue;
        public virtual float RecalculateCrit(float baseValue, CharacterBody character) => baseValue;
        public virtual float RecalculateArmor(float baseValue, CharacterBody character) => baseValue;

        public virtual float RecalculateGeneralCooldown(float baseValue, CharacterBody character) => baseValue;
        public virtual float RecalculatePrimaryCoolDown(float baseValue, CharacterBody character) => baseValue;
        public virtual float RecalculateSecondaryCooldown(float baseValue, CharacterBody character) => baseValue;
        public virtual float RecalculateUtilityCooldown(float baseValue, CharacterBody character) => baseValue;
        public virtual float RecalculateSpecialCooldown(float baseValue, CharacterBody character) => baseValue;

        public virtual float RecalculatePrimaryCount(float baseValue, CharacterBody character) => baseValue;
        public virtual float RecalculateSecondaryCount(float baseValue, CharacterBody character) => baseValue;
        public virtual float RecalculateUtilityCount(float baseValue, CharacterBody character) => baseValue;
        public virtual float RecalculateSpecialCount(float baseValue, CharacterBody character) => baseValue;

        public virtual void UpdateItem(CharacterBody character) { }
    }

    class DefaultRecalculate : ModRecalculateCustom {

        public DefaultRecalculate() {
            RecalculatePriority = 0;
        }

        public override float RecalculateHealth(float baseValue, CharacterBody character) {
            var maxHealth = character.baseMaxHealth + (character.level - 1) * character.levelMaxHealth;
            var healthBonusItem = 0f;
            var hpbooster = 1f;
            var healthDivider = 1f;
            if ((bool)character.inventory) {
                healthBonusItem += CustomItemAPI.GetBonusForStat(character, StatIndex.MaxHealth);

                if (character.inventory.GetItemCount(ItemIndex.Infusion) > 0)
                    healthBonusItem += character.inventory.infusionBonus;
                hpbooster += CustomItemAPI.GetMultiplierForStat(character, StatIndex.MaxHealth);
                healthDivider = character.CalcLunarDaggerPower();
            }
            maxHealth += healthBonusItem;
            maxHealth *= hpbooster / healthDivider;
            return maxHealth;
        }

        public override float RecalculateShield(float baseValue, CharacterBody character) {
            var maxShield = character.baseMaxShield + character.levelMaxShield * (character.level - 1);

            if (character.inventory) {
                if (character.inventory.GetItemCount(ItemIndex.ShieldOnly) > 0) {

                    maxShield += character.maxHealth * (1.25f + (character.inventory.GetItemCount(ItemIndex.ShieldOnly) - 1) * 0.5f);
                    character.SetPropertyValue("maxHealth", 1);
                }
            }
            //Buff
            if (character.HasBuff(BuffIndex.EngiShield))
                maxShield += character.maxHealth * 1f;
            if (character.HasBuff(BuffIndex.EngiTeamShield))
                maxShield += character.maxHealth * 0.5f;


            //NPC Overload Buff
            if (character.GetFieldValue<BuffMask>("buffMask").HasBuff(BuffIndex.AffixBlue)) {
                character.SetPropertyValue("maxHealth", character.maxHealth * 0.5f);
                maxShield += character.maxHealth;
            }
            if (character.inventory) {
                maxShield += CustomItemAPI.GetBonusForStat(character, StatIndex.MaxShield);

                maxShield *= (1 + CustomItemAPI.GetMultiplierForStat(character, StatIndex.MaxShield));
            }
            return maxShield;
        }

        public override float RecalculateRegen(float baseValue, CharacterBody character) {
            var baseRegen = (character.baseRegen + character.levelRegen * (character.level - 1)) * 2.5f;

            var regenBonus = 0f;
            var regenMult = 1f;
            //Item Related
            if ((bool)character.inventory) {
                regenBonus += CustomItemAPI.GetBonusForStat(character, StatIndex.Regen);
                if (character.outOfDanger)
                    regenBonus += CustomItemAPI.GetBonusForStat(character, StatIndex.SafeRegen);
                if (character.inventory.GetItemCount(ItemIndex.HealthDecay) > 0)
                    regenBonus -= character.maxHealth / character.inventory.GetItemCount(ItemIndex.HealthDecay);
                regenMult += CustomItemAPI.GetMultiplierForStat(character, StatIndex.Regen);
                if (character.outOfDanger)
                    regenMult += CustomItemAPI.GetMultiplierForStat(character, StatIndex.SafeRegen);
            }

            var totalRegen = (baseRegen * regenMult + regenBonus);

            return totalRegen;

        }

        public override float RecalculateMoveSpeed(float baseValue, CharacterBody character) {
            var baseMoveSpeed = character.baseMoveSpeed + character.levelMoveSpeed * (character.level - 1);

            var speedBonus = 1f;


            //More weird stuff
            if ((bool)character.inventory)
                if (character.inventory.currentEquipmentIndex == EquipmentIndex.AffixYellow)
                    baseMoveSpeed += 2;

            if (character.isSprinting)
                baseMoveSpeed *= character.GetFieldValue<float>("sprintingSpeedMultiplier");


            //SpeedBonus
            if (character.HasBuff(BuffIndex.BugWings))
                speedBonus += 0.2f;
            if (character.HasBuff(BuffIndex.Warbanner))
                speedBonus += 0.3f;
            if (character.HasBuff(BuffIndex.EnrageAncientWisp))
                speedBonus += 0.4f;
            if (character.HasBuff(BuffIndex.CloakSpeed))
                speedBonus += 0.4f;
            if (character.HasBuff(BuffIndex.TempestSpeed))
                speedBonus += 1;
            if (character.HasBuff(BuffIndex.WarCryBuff))
                speedBonus += .5f;
            if (character.HasBuff(BuffIndex.EngiTeamShield))
                speedBonus += 0.3f;

            speedBonus += CustomItemAPI.GetMultiplierForStat(character, StatIndex.MoveSpeed);
            if (character.isSprinting)
                speedBonus += CustomItemAPI.GetMultiplierForStat(character, StatIndex.RunningMoveSpeed);
            if (character.outOfCombat && character.outOfDanger) {
                speedBonus += CustomItemAPI.GetMultiplierForStat(character, StatIndex.SafeMoveSpeed);
                if (character.isSprinting)
                    speedBonus += CustomItemAPI.GetMultiplierForStat(character, StatIndex.SafeRunningMoveSpeed);
            }

            //Debuff Speed
            var speedMalus = 1f;
            if (character.HasBuff(BuffIndex.Slow50))
                speedMalus += 0.5f;
            if (character.HasBuff(BuffIndex.Slow60))
                speedMalus += 0.6f;
            if (character.HasBuff(BuffIndex.Slow80))
                speedMalus += 0.8f;
            if (character.HasBuff(BuffIndex.ClayGoo))
                speedMalus += 0.5f;
            if (character.HasBuff(BuffIndex.Slow30))
                speedMalus += 0.3f;
            if (character.HasBuff(BuffIndex.Cripple))
                ++speedMalus;

            baseMoveSpeed += CustomItemAPI.GetBonusForStat(character, StatIndex.MoveSpeed);
            if (character.isSprinting)
                baseMoveSpeed += CustomItemAPI.GetBonusForStat(character, StatIndex.RunningMoveSpeed);
            if (character.outOfCombat && character.outOfDanger) {
                baseMoveSpeed += CustomItemAPI.GetBonusForStat(character, StatIndex.SafeMoveSpeed);
                if (character.isSprinting)
                    baseMoveSpeed += CustomItemAPI.GetBonusForStat(character, StatIndex.SafeRunningMoveSpeed);
            }

            var moveSpeed = baseMoveSpeed * (speedBonus / speedMalus);
            if ((bool)character.inventory) {
                moveSpeed *= 1.0f - 0.05f * character.GetBuffCount(BuffIndex.BeetleJuice);
            }

            return moveSpeed;
        }


        public override float RecalculateJumpPower(float baseValue, CharacterBody character) {
            var jumpPower = character.baseJumpPower + character.levelJumpPower * (character.level - 1) + CustomItemAPI.GetBonusForStat(character, StatIndex.JumpPower);
            jumpPower *= 1 + CustomItemAPI.GetMultiplierForStat(character, StatIndex.JumpPower);
            return jumpPower;
        }

        public override float RecalculateJumpCount(float baseValue, CharacterBody character) {
            var jumpCount = character.baseJumpCount + CustomItemAPI.GetBonusForStat(character, StatIndex.JumpCount);
            jumpCount *= 1 + CustomItemAPI.GetMultiplierForStat(character, StatIndex.JumpCount);
            return jumpCount;
        }

        public override float RecalculateDamage(float baseValue, CharacterBody character) {
            var baseDamage = character.baseDamage + character.levelDamage * (character.level - 1);
            baseDamage += CustomItemAPI.GetBonusForStat(character, StatIndex.Damage);

            var damageBoost = 0f;
            var damageBoostCount = character.inventory ? character.inventory.GetItemCount(ItemIndex.BoostDamage) : 0;
            if (damageBoostCount > 0)
                damageBoost += damageBoostCount * damageBoost;
            damageBoost -= 0.05f * character.GetBuffCount(BuffIndex.BeetleJuice);

            if (character.HasBuff(BuffIndex.GoldEmpowered))
                damageBoost += 1;

            var damageMult = damageBoost + (character.CalcLunarDaggerPower());
            damageMult += CustomItemAPI.GetMultiplierForStat(character, StatIndex.Damage);
            return baseDamage * damageMult;
        }

        public override float RecalculateAttackSpeed(float baseValue, CharacterBody character) {
            var baseAttackSpeed = character.baseAttackSpeed + character.levelAttackSpeed * (character.level - 1);

            //Item effect
            var attackSpeedBonus = 1f;
            if (character.inventory) {
                if (character.inventory.currentEquipmentIndex == EquipmentIndex.AffixYellow)
                    attackSpeedBonus += 0.5f;
            }

            //Buffs
            var attackSpeedMult = attackSpeedBonus + character.GetFieldValue<int[]>("buffs")[2] * 0.12f;
            if (character.HasBuff(BuffIndex.Warbanner))
                attackSpeedMult += 0.3f;
            if (character.HasBuff(BuffIndex.EnrageAncientWisp))
                attackSpeedMult += 2f;
            if (character.HasBuff(BuffIndex.WarCryBuff))
                attackSpeedMult += 1f;


            baseAttackSpeed += CustomItemAPI.GetBonusForStat(character, StatIndex.AttackSpeed);
            attackSpeedMult += CustomItemAPI.GetMultiplierForStat(character, StatIndex.AttackSpeed);
            var attackSpeed = baseAttackSpeed * attackSpeedMult;
            //Debuff
            attackSpeed *= 1 - (0.05f * character.GetBuffCount(BuffIndex.BeetleJuice));

            return attackSpeed;
        }

        public override float RecalculateCrit(float baseValue, CharacterBody character) {
            var criticalChance = character.baseCrit + character.levelCrit * (character.level - 1);


            criticalChance += CustomItemAPI.GetBonusForStat(character, StatIndex.Crit);
            criticalChance *= 1 + CustomItemAPI.GetMultiplierForStat(character, StatIndex.AttackSpeed);

            if (character.HasBuff(BuffIndex.FullCrit))
                criticalChance += 100;


            return criticalChance;
        }

        public override float RecalculateArmor(float baseValue, CharacterBody character) {
            var baseArmor = character.baseArmor + character.levelArmor * (character.level - 1);
            var bonusArmor = 0f;

            if (character.HasBuff(BuffIndex.ArmorBoost))
                bonusArmor += 200;
            if (character.HasBuff(BuffIndex.Cripple))
                bonusArmor -= 20;
            var totalArmor = baseArmor + bonusArmor;
            totalArmor += CustomItemAPI.GetBonusForStat(character, StatIndex.Armor);
            if (character.isSprinting)
                totalArmor += CustomItemAPI.GetBonusForStat(character, StatIndex.RunningArmor);
            totalArmor *= 1 + CustomItemAPI.GetMultiplierForStat(character, StatIndex.Armor);
            if (character.isSprinting)
                totalArmor *= 1 + CustomItemAPI.GetMultiplierForStat(character, StatIndex.RunningArmor);
            return totalArmor;
        }

        public override float RecalculateGeneralCooldown(float baseValue, CharacterBody character) {
            var coolDownMultiplier = 1f;
            coolDownMultiplier += CustomItemAPI.GetBonusForStat(character, StatIndex.GlobalCoolDown);

            coolDownMultiplier *= CustomItemAPI.GetMultiplierForStatCD(character, StatIndex.GlobalCoolDown);

            if (character.HasBuff(BuffIndex.GoldEmpowered))
                coolDownMultiplier *= 0.25f;
            if (character.HasBuff(BuffIndex.NoCooldowns))
                coolDownMultiplier = 0.0f;


            return coolDownMultiplier;
        }

        public override float RecalculatePrimaryCoolDown(float baseValue, CharacterBody character) {
            var coolDownMultiplier = 1f;
            coolDownMultiplier += CustomItemAPI.GetBonusForStat(character, StatIndex.CoolDownPrimary);

            coolDownMultiplier *= CustomItemAPI.GetMultiplierForStatCD(character, StatIndex.CoolDownPrimary);
            return coolDownMultiplier;
        }
        public override float RecalculatePrimaryCount(float baseValue, CharacterBody character) {
            var count = 0f;
            count += CustomItemAPI.GetBonusForStat(character, StatIndex.CountPrimary);

            count *= 1 + CustomItemAPI.GetMultiplierForStat(character, StatIndex.CountPrimary);
            return count;
        }
        public override float RecalculateSecondaryCooldown(float baseValue, CharacterBody character) {
            var coolDownMultiplier = 1f;
            coolDownMultiplier += CustomItemAPI.GetBonusForStat(character, StatIndex.CoolDownSecondary);

            coolDownMultiplier *= CustomItemAPI.GetMultiplierForStatCD(character, StatIndex.CoolDownSecondary);
            return coolDownMultiplier;
        }
        public override float RecalculateSecondaryCount(float baseValue, CharacterBody character) {
            var count = 0f;
            count += CustomItemAPI.GetBonusForStat(character, StatIndex.CountSecondary);
            count *= 1 + CustomItemAPI.GetMultiplierForStat(character, StatIndex.CountSecondary);
            return count;
        }
        public override float RecalculateSpecialCooldown(float baseValue, CharacterBody character) {
            var coolDownMultiplier = 1f;
            coolDownMultiplier += CustomItemAPI.GetBonusForStat(character, StatIndex.CoolDownUtility);

            coolDownMultiplier *= CustomItemAPI.GetMultiplierForStatCD(character, StatIndex.CoolDownUtility);
            return coolDownMultiplier;
        }
        public override float RecalculateSpecialCount(float baseValue, CharacterBody character) {
            var count = 0f;
            count += CustomItemAPI.GetBonusForStat(character, StatIndex.CountUtility);
            count *= 1 + CustomItemAPI.GetMultiplierForStat(character, StatIndex.CountUtility);
            return count;
        }
        public override float RecalculateUtilityCooldown(float baseValue, CharacterBody character) {
            var coolDownMultiplier = 1f;
            coolDownMultiplier += CustomItemAPI.GetBonusForStat(character, StatIndex.CoolDownSpecial);

            coolDownMultiplier *= CustomItemAPI.GetMultiplierForStatCD(character, StatIndex.CoolDownSpecial);
            return coolDownMultiplier;
        }
        public override float RecalculateUtilityCount(float baseValue, CharacterBody character) {
            var count = 0f;
            count += CustomItemAPI.GetBonusForStat(character, StatIndex.CountSpecial);

            count *= 1 + CustomItemAPI.GetMultiplierForStat(character, StatIndex.CountSpecial);
            return count;
        }

    }


    public static class PlayerAPI {
        public static List<Action<PlayerStats>> CustomEffects { get; private set; }

        static List<ModRecalculateCustom> m_RecalulateList;
        static MethodInfo GetMethodInfo(Func<float, CharacterBody, float> f) {
            return f.Method;
        }

        static float StatHandler(MethodInfo method, CharacterBody character) {
            var value = 0f;
            foreach (var recal in m_RecalulateList) {
                value = (float)method.Invoke(recal, new object[2] { value, character });
            }
            return value;
        }

        static void AddOrder(this Dictionary<int, ModRecalculateCustom> dic, int pos, ModRecalculateCustom obj, bool warn = false) {
            try {
                if (dic.ContainsKey(pos)) {
                        AddOrder(dic, pos + 1, obj, true);
                }
                else {
                    dic.Add(pos, obj);
                    if (warn)
                        Debug.Log("Character Stat API warning : The loading priority for " + obj.ToString() + " priority : " + obj.RecalculatePriority + " is already used by : "+ dic[obj.RecalculatePriority].ToString() +", priority : " + pos + " given");
                }
            }
            catch (OverflowException)
            {
                throw new Exception("Error, the Minimum priority is already used by : "+ dic[short.MaxValue].ToString() +", only one recalculate can be at the Minimum priority");
            }
        }

        public static void ReorderRecalculateList() {
            var m__temp_RecalulateDic = new Dictionary<int, ModRecalculateCustom>();
            foreach (var obj in m_RecalulateList) {
                m__temp_RecalulateDic.AddOrder(obj.RecalculatePriority, obj);
            }
            m_RecalulateList = new List<ModRecalculateCustom>();
            foreach (var kv in m__temp_RecalulateDic) {
                m_RecalulateList.Add(kv.Value);
            }
        }

        public static void AddCustomRecalculate(ModRecalculateCustom customRecalculate) {
            m_RecalulateList.Add(customRecalculate);
            ReorderRecalculateList();
        }

        public static void InitHooks() {
            m_RecalulateList = new List<ModRecalculateCustom>();
            m_RecalulateList.Add(new DefaultRecalculate());
            On.RoR2.CharacterBody.RecalculateStats += (orig, self) => RecalcStats(self);
        }

        public static void RecalcStats(CharacterBody characterBody) {
            if (characterBody == null)
                return;

            CustomItemAPI.Update();
            foreach (var recal in m_RecalulateList) {
                recal.InvokeMethod("UpdateItem", new object[1] { characterBody });
            }


            characterBody.SetPropertyValue("experience",
                TeamManager.instance.GetTeamExperience(characterBody.teamComponent.teamIndex));
            float level = TeamManager.instance.GetTeamLevel(characterBody.teamComponent.teamIndex);
            if (characterBody.inventory) {
                level += characterBody.inventory.GetItemCount(ItemIndex.LevelBonus);

            }
            characterBody.SetPropertyValue("level", level);

            characterBody.SetPropertyValue("isElite", characterBody.GetFieldValue<BuffMask>("buffMask").containsEliteBuff);

            var preHealth = characterBody.maxHealth;
            var preShield = characterBody.maxShield;

            characterBody.SetPropertyValue("maxHealth", StatHandler(GetMethodInfo(new ModRecalculateCustom().RecalculateHealth), characterBody));

            characterBody.SetPropertyValue("maxShield", StatHandler(GetMethodInfo(new ModRecalculateCustom().RecalculateShield), characterBody));

            characterBody.SetPropertyValue("regen", StatHandler(GetMethodInfo(new ModRecalculateCustom().RecalculateRegen), characterBody));

            characterBody.SetPropertyValue("moveSpeed", StatHandler(GetMethodInfo(new ModRecalculateCustom().RecalculateMoveSpeed), characterBody));
            characterBody.SetPropertyValue("acceleration", characterBody.moveSpeed / characterBody.baseMoveSpeed * characterBody.baseAcceleration);

            characterBody.SetPropertyValue("jumpPower", StatHandler(GetMethodInfo(new ModRecalculateCustom().RecalculateJumpPower), characterBody));
            characterBody.SetPropertyValue("maxJumpHeight", Trajectory.CalculateApex(characterBody.jumpPower));
            characterBody.SetPropertyValue("maxJumpCount", (int)StatHandler(GetMethodInfo(new ModRecalculateCustom().RecalculateJumpCount), characterBody));

            characterBody.SetPropertyValue("damage", StatHandler(GetMethodInfo(new ModRecalculateCustom().RecalculateDamage), characterBody));

            characterBody.SetPropertyValue("attackSpeed", StatHandler(GetMethodInfo(new ModRecalculateCustom().RecalculateAttackSpeed), characterBody));

            characterBody.SetPropertyValue("crit", StatHandler(GetMethodInfo(new ModRecalculateCustom().RecalculateCrit), characterBody));
            characterBody.SetPropertyValue("armor", StatHandler(GetMethodInfo(new ModRecalculateCustom().RecalculateArmor), characterBody));

            //CoolDown
            var coolDownMultiplier = StatHandler(GetMethodInfo(new ModRecalculateCustom().RecalculateGeneralCooldown), characterBody);
            if ((bool)characterBody.GetFieldValue<SkillLocator>("skillLocator").primary) {
                characterBody.GetFieldValue<SkillLocator>("skillLocator").primary.cooldownScale = StatHandler(GetMethodInfo(new ModRecalculateCustom().RecalculatePrimaryCoolDown), characterBody) * coolDownMultiplier;
                if (characterBody.GetFieldValue<SkillLocator>("skillLocator").primary.baseMaxStock > 1)
                    characterBody.GetFieldValue<SkillLocator>("skillLocator").primary.SetBonusStockFromBody((int)StatHandler(GetMethodInfo(new ModRecalculateCustom().RecalculatePrimaryCount), characterBody));
            }
            if ((bool)characterBody.GetFieldValue<SkillLocator>("skillLocator").secondary) {
                characterBody.GetFieldValue<SkillLocator>("skillLocator").secondary.cooldownScale = StatHandler(GetMethodInfo(new ModRecalculateCustom().RecalculateSecondaryCooldown), characterBody) * coolDownMultiplier;
                characterBody.GetFieldValue<SkillLocator>("skillLocator").secondary.SetBonusStockFromBody((int)StatHandler(GetMethodInfo(new ModRecalculateCustom().RecalculateSecondaryCount), characterBody));
            }
            if ((bool)characterBody.GetFieldValue<SkillLocator>("skillLocator").utility) {
                characterBody.GetFieldValue<SkillLocator>("skillLocator").utility.cooldownScale = StatHandler(GetMethodInfo(new ModRecalculateCustom().RecalculateUtilityCooldown), characterBody) * coolDownMultiplier;
                characterBody.GetFieldValue<SkillLocator>("skillLocator").utility.SetBonusStockFromBody((int)StatHandler(GetMethodInfo(new ModRecalculateCustom().RecalculateUtilityCount), characterBody));
            }
            if ((bool)characterBody.GetFieldValue<SkillLocator>("skillLocator").special) {
                characterBody.GetFieldValue<SkillLocator>("skillLocator").special.cooldownScale = StatHandler(GetMethodInfo(new ModRecalculateCustom().RecalculateSpecialCooldown), characterBody) * coolDownMultiplier;
                if (characterBody.GetFieldValue<SkillLocator>("skillLocator").special.baseMaxStock > 1)
                    characterBody.GetFieldValue<SkillLocator>("skillLocator").special.SetBonusStockFromBody((int)StatHandler(GetMethodInfo(new ModRecalculateCustom().RecalculateSpecialCount), characterBody));
            }
            //Since it's not yet used in game, I leave that here for now
            characterBody.SetPropertyValue("critHeal", 0.0f);
            if (characterBody.inventory) {
                if (characterBody.inventory.GetItemCount(ItemIndex.CritHeal) > 0) {
                    var crit = characterBody.crit;
                    characterBody.SetPropertyValue("crit", characterBody.crit / (characterBody.inventory.GetItemCount(ItemIndex.CritHeal) + 1));
                    characterBody.SetPropertyValue("critHeal", crit - characterBody.crit);
                }
            }

            if (NetworkServer.active) {
                var healthOffset = characterBody.maxHealth - preHealth;
                var shieldOffset = characterBody.maxShield - preShield;
                if (healthOffset > 0) {
                    double num47 = characterBody.healthComponent.Heal(healthOffset, new ProcChainMask(), false);
                }
                else if (characterBody.healthComponent.health > characterBody.maxHealth)
                    characterBody.healthComponent.Networkhealth = characterBody.maxHealth;
                if (shieldOffset > 0) {
                    characterBody.healthComponent.RechargeShieldFull();
                    //characterBody.healthComponent.RechargeShield(ShieldOffset); //Depend on the version of the Assembly-Csharp
                }
            }

            characterBody.statsDirty = false;
        }
    }

    public class PlayerStats {
        //Character Stats
        public int maxHealth = 0;
        public int healthRegen = 0;
        public bool isElite = false;
        public int maxShield = 0;
        public float movementSpeed = 0;
        public float acceleration = 0;
        public float jumpPower = 0;
        public float maxJumpHeight = 0;
        public float maxJumpCount = 0;
        public float attackSpeed = 0;
        public float damage = 0;
        public float Crit = 0;
        public float Armor = 0;
        public float critHeal = 0;

        //Primary Skill
        public float PrimaryCooldownScale = 0;
        public float PrimaryStock = 0;

        //Secondary Skill
        public float SecondaryCooldownScale = 0;
        public float SecondaryStock = 0;

        //Utility Skill
        public float UtilityCooldownScale = 0;
        public float UtilityStock = 0;

        //Special Skill
        public float SpecialCooldownScale = 0;
        public float SpecialStock = 0;
    }
}
