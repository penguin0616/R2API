using R2API.Utils;
using RoR2;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace R2API
{
	public static class PlayerAPI
	{
		public static List<Action<PlayerStats>> CustomEffects { get; private set; }

		public static void InitHooks()
		{
			On.RoR2.CharacterBody.RecalculateStats += (orig, self) => RecalcStats(self);
			foreach(var p in typeof(CharacterBody).GetProperties())
			{
				CharacterBodyCache.Add(p.Name, p);
			}
		}
		private static Dictionary<string, PropertyInfo> CharacterBodyCache { get; set; } = new Dictionary<string, PropertyInfo>();
		public static void RecalcStats(CharacterBody characterBody)
		{
			characterBody.SetFieldValue("experience", TeamManager.instance.GetTeamExperience(characterBody.teamComponent.teamIndex));
			characterBody.SetFieldValue("level", TeamManager.instance.GetTeamLevel(characterBody.teamComponent.teamIndex));

			//Initialize a dictionary of all items + their count
			Dictionary<ItemIndex, int> Items = new Dictionary<ItemIndex, int>();
			PlayerStats playerStats = new PlayerStats(characterBody); //TODO: initialize this from characterBody

			EquipmentIndex eIndex = EquipmentIndex.None;
			uint infusionBonus = 0u;
			float Daggerpower = 0f;
			if (characterBody.inventory) //if there is such a thing as items, it will look for all of them and apply the appropriate buffs
			{
				characterBody.SetFieldValue("level", characterBody.level+(float)characterBody.inventory.GetItemCount(ItemIndex.LevelBonus));
				Items.Add(ItemIndex.Infusion,characterBody.inventory.GetItemCount(ItemIndex.Infusion));
				Items.Add(ItemIndex.HealWhileSafe, characterBody.inventory.GetItemCount(ItemIndex.HealWhileSafe));
				Items.Add(ItemIndex.PersonalShield, characterBody.inventory.GetItemCount(ItemIndex.PersonalShield));
				Items.Add(ItemIndex.Hoof, characterBody.inventory.GetItemCount(ItemIndex.Hoof));
				Items.Add(ItemIndex.SprintOutOfCombat, characterBody.inventory.GetItemCount(ItemIndex.SprintOutOfCombat));
				Items.Add(ItemIndex.Feather, characterBody.inventory.GetItemCount(ItemIndex.Feather));
				Items.Add(ItemIndex.Syringe, characterBody.inventory.GetItemCount(ItemIndex.Syringe));
				Items.Add(ItemIndex.CritGlasses, characterBody.inventory.GetItemCount(ItemIndex.CritGlasses));
				Items.Add(ItemIndex.AttackSpeedOnCrit, characterBody.inventory.GetItemCount(ItemIndex.AttackSpeedOnCrit));
				Items.Add(ItemIndex.CooldownOnCrit, characterBody.inventory.GetItemCount(ItemIndex.CooldownOnCrit));
				Items.Add(ItemIndex.HealOnCrit, characterBody.inventory.GetItemCount(ItemIndex.HealOnCrit));
				Items.Add(ItemIndex.ShieldOnly, characterBody.inventory.GetItemCount(ItemIndex.ShieldOnly));
				Items.Add(ItemIndex.AlienHead, characterBody.inventory.GetItemCount(ItemIndex.AlienHead));
				Items.Add(ItemIndex.Knurl, characterBody.inventory.GetItemCount(ItemIndex.Knurl));
				Items.Add(ItemIndex.BoostHp, characterBody.inventory.GetItemCount(ItemIndex.BoostHp));
				Items.Add(ItemIndex.CritHeal, characterBody.inventory.GetItemCount(ItemIndex.CritHeal));
				Items.Add(ItemIndex.SprintBonus, characterBody.inventory.GetItemCount(ItemIndex.SprintBonus));
				Items.Add(ItemIndex.SecondarySkillMagazine, characterBody.inventory.GetItemCount(ItemIndex.SecondarySkillMagazine));
				Items.Add(ItemIndex.SprintArmor, characterBody.inventory.GetItemCount(ItemIndex.SprintArmor));
				Items.Add(ItemIndex.UtilitySkillMagazine, characterBody.inventory.GetItemCount(ItemIndex.UtilitySkillMagazine));
				Items.Add(ItemIndex.HealthDecay, characterBody.inventory.GetItemCount(ItemIndex.HealthDecay));
				Items.Add(ItemIndex.DrizzlePlayerHelper, characterBody.inventory.GetItemCount(ItemIndex.DrizzlePlayerHelper));
				playerStats.movementSpeed += characterBody.GetBuffCount(BuffIndex.BeetleJuice);
				Daggerpower = characterBody.CalcLunarDaggerPower();
				eIndex = characterBody.inventory.currentEquipmentIndex;
				infusionBonus = characterBody.inventory.infusionBonus;
			}
			float level = characterBody.level -1f;
			playerStats.isElite = characterBody.GetFieldValue<BuffMask>("buffMask").containsEliteBuff;
			playerStats.maxHealth = characterBody.baseMaxHealth + characterBody.levelMaxHealth * (characterBody.level - 1f);

			
			//Max Health Bonuses
			playerStats.maxHealth += Items[ItemIndex.Infusion]> 0 ? infusionBonus : 0;
			playerStats.maxHealth += Items[ItemIndex.Knurl] * 40f;
			playerStats.maxHealth *= Math.Max(Items[ItemIndex.BoostHp] * 0.1f,1f);
			playerStats.maxHealth *= characterBody.HasBuff(BuffIndex.AffixBlue) ? 0.5f : 1f;
			playerStats.maxHealth /= Daggerpower;

			//Shield Bonuses


			foreach (var effectAction in PlayerAPI.CustomEffects)
			{
				effectAction(playerStats); //Execute the custom effect action, passing the instance with all the player stats
			}

			CharacterBodyCache["maxHealth"].SetValue(characterBody, playerStats.maxHealth);
			CharacterBodyCache["statsDirty"].SetValue(characterBody,false);
		}
		public void RecalculateStats(CharacterBody body)
		{
			body.experience = TeamManager.instance.GetTeamExperience(body.teamComponent.teamIndex);
			body.level = TeamManager.instance.GetTeamLevel(body.teamComponent.teamIndex);
			int infusion = 0;
			int HealWhileSafe = 0;
			int PersonalShield = 0;
			int hoof = 0;
			int SprintOutOfCombat = 0;
			int Feather = 0;
			int Syringe = 0;
			int CritGlasses = 0;
			int AttackSpeedOnCrit = 0;
			int CooldownOnCrit = 0;
			int HealOnCrit = 0;
			int BeetleJuice = 0;
			int ShieldOnly = 0;
			int AlienHead = 0;
			int Knurl = 0;
			int BoostHp = 0;
			int Critheal = 0;
			int SprintBonus = 0;
			int bonusStockFromBody = 0;
			int SprintArmor = 0;
			int UtilitySkillMagazine = 0;
			int HealthDecay = 0;
			int DrizzlePlayerHelper = 0;
			float LunarDaggerPower = 1f;
			EquipmentIndex equipmentIndex = EquipmentIndex.None;
			uint InfusionBonus = 0u;
			if (body.inventory)
			{
				body.level += (float)body.inventory.GetItemCount(ItemIndex.LevelBonus);
				infusion = body.inventory.GetItemCount(ItemIndex.Infusion);
				HealWhileSafe = body.inventory.GetItemCount(ItemIndex.HealWhileSafe);
				PersonalShield = body.inventory.GetItemCount(ItemIndex.PersonalShield);
				hoof = body.inventory.GetItemCount(ItemIndex.Hoof);
				SprintOutOfCombat = body.inventory.GetItemCount(ItemIndex.SprintOutOfCombat);
				Feather = body.inventory.GetItemCount(ItemIndex.Feather);
				Syringe = body.inventory.GetItemCount(ItemIndex.Syringe);
				CritGlasses = body.inventory.GetItemCount(ItemIndex.CritGlasses);
				AttackSpeedOnCrit = body.inventory.GetItemCount(ItemIndex.AttackSpeedOnCrit);
				CooldownOnCrit = body.inventory.GetItemCount(ItemIndex.CooldownOnCrit);
				HealOnCrit = body.inventory.GetItemCount(ItemIndex.HealOnCrit);
				BeetleJuice = body.GetBuffCount(BuffIndex.BeetleJuice);
				ShieldOnly = body.inventory.GetItemCount(ItemIndex.ShieldOnly);
				AlienHead = body.inventory.GetItemCount(ItemIndex.AlienHead);
				Knurl = body.inventory.GetItemCount(ItemIndex.Knurl);
				BoostHp = body.inventory.GetItemCount(ItemIndex.BoostHp);
				Critheal = body.inventory.GetItemCount(ItemIndex.CritHeal);
				SprintBonus = body.inventory.GetItemCount(ItemIndex.SprintBonus);
				bonusStockFromBody = body.inventory.GetItemCount(ItemIndex.SecondarySkillMagazine);
				SprintArmor = body.inventory.GetItemCount(ItemIndex.SprintArmor);
				UtilitySkillMagazine = body.inventory.GetItemCount(ItemIndex.UtilitySkillMagazine);
				HealthDecay = body.inventory.GetItemCount(ItemIndex.HealthDecay);
				LunarDaggerPower = body.CalcLunarDaggerPower();
				equipmentIndex = body.inventory.currentEquipmentIndex;
				InfusionBonus = body.inventory.infusionBonus;
				DrizzlePlayerHelper = body.inventory.GetItemCount(ItemIndex.DrizzlePlayerHelper);
			}
			float Levelminusone = body.level - 1f;
			body.isElite = body.buffMask.containsEliteBuff;
			float maxHealth = body.maxHealth;
			float maxShield = body.maxShield;
			float BasepluslevelHP = body.baseMaxHealth + body.levelMaxHealth * Levelminusone;
			float bootHp = 1f;
			bootHp += (float)BoostHp * 0.1f;
			if (infusion > 0)
			{
				BasepluslevelHP += InfusionBonus;
			}
			BasepluslevelHP += (float)Knurl * 40f;
			BasepluslevelHP *= bootHp;
			BasepluslevelHP /= LunarDaggerPower;
			body.maxHealth = BasepluslevelHP;
			float num28 = body.baseRegen + body.levelRegen * Levelminusone;
			num28 *= 2.5f;
			if (body.outOfDanger && HealWhileSafe > 0)
			{
				num28 *= 2.5f + (float)(HealWhileSafe - 1) * 1.5f;
			}
			num28 += (float)Knurl * 1.6f;
			if (HealthDecay > 0)
			{
				num28 -= body.maxHealth / (float)HealthDecay;
			}
			body.regen = num28;
			float num29 = body.baseMaxShield + body.levelMaxShield * Levelminusone;
			num29 += (float)PersonalShield * 25f;
			if (body.HasBuff(BuffIndex.EngiShield))
			{
				num29 += body.maxHealth * 1f;
			}
			if (body.HasBuff(BuffIndex.EngiTeamShield))
			{
				num29 += body.maxHealth * 0.5f;
			}
			if (ShieldOnly > 0)
			{
				num29 += body.maxHealth * (1.5f + (float)(ShieldOnly - 1) * 0.25f);
				body.maxHealth = 1f;
			}
			if (body.buffMask.HasBuff(BuffIndex.AffixBlue))
			{
				float num30 = body.maxHealth * 0.5f;
				body.maxHealth -= num30;
				num29 += body.maxHealth;
			}
			body.maxShield = num29;
			float num31 = body.baseMoveSpeed + body.levelMoveSpeed * Levelminusone;
			float num32 = 1f;
			if (Run.instance.enabledArtifacts.HasArtifact(ArtifactIndex.Spirit))
			{
				float num33 = 1f;
				if (body.healthComponent)
				{
					num33 = body.healthComponent.combinedHealthFraction;
				}
				num32 += 1f - num33;
			}
			if (equipmentIndex == EquipmentIndex.AffixYellow)
			{
				num31 += 2f;
			}
			if (body.isSprinting)
			{
				num31 *= body.sprintingSpeedMultiplier;
			}
			if (body.outOfCombat && body.outOfDanger && SprintOutOfCombat > 0)
			{
				num32 += (float)SprintOutOfCombat * 0.3f;
			}
			num32 += (float)hoof * 0.14f;
			if (body.isSprinting && SprintBonus > 0)
			{
				num32 += (0.1f + 0.2f * (float)SprintBonus) / body.sprintingSpeedMultiplier;
			}
			if (body.HasBuff(BuffIndex.BugWings))
			{
				num32 += 0.2f;
			}
			if (body.HasBuff(BuffIndex.Warbanner))
			{
				num32 += 0.3f;
			}
			if (body.HasBuff(BuffIndex.EnrageAncientWisp))
			{
				num32 += 0.4f;
			}
			if (body.HasBuff(BuffIndex.CloakSpeed))
			{
				num32 += 0.4f;
			}
			if (body.HasBuff(BuffIndex.TempestSpeed))
			{
				num32 += 1f;
			}
			if (body.HasBuff(BuffIndex.WarCryBuff))
			{
				num32 += 0.5f;
			}
			if (body.HasBuff(BuffIndex.EngiTeamShield))
			{
				num32 += 0.3f;
			}
			float num34 = 1f;
			if (body.HasBuff(BuffIndex.Slow50))
			{
				num34 += 0.5f;
			}
			if (body.HasBuff(BuffIndex.Slow60))
			{
				num34 += 0.6f;
			}
			if (body.HasBuff(BuffIndex.Slow80))
			{
				num34 += 0.8f;
			}
			if (body.HasBuff(BuffIndex.ClayGoo))
			{
				num34 += 0.5f;
			}
			if (body.HasBuff(BuffIndex.Slow30))
			{
				num34 += 0.3f;
			}
			if (body.HasBuff(BuffIndex.Cripple))
			{
				num34 += 1f;
			}
			num31 *= num32 / num34;
			if (BeetleJuice > 0)
			{
				num31 *= 1f - 0.05f * (float)BeetleJuice;
			}
			body.moveSpeed = num31;
			body.acceleration = body.moveSpeed / body.baseMoveSpeed * body.baseAcceleration;
			float jumpPower = body.baseJumpPower + body.levelJumpPower * Levelminusone;
			body.jumpPower = jumpPower;
			body.maxJumpHeight = Trajectory.CalculateApex(body.jumpPower);
			body.maxJumpCount = body.baseJumpCount + Feather;
			float num35 = body.baseDamage + body.levelDamage * Levelminusone;
			float num36 = 1f;
			int num37 = body.inventory ? body.inventory.GetItemCount(ItemIndex.BoostDamage) : 0;
			if (num37 > 0)
			{
				num36 += (float)num37 * 0.1f;
			}
			if (BeetleJuice > 0)
			{
				num36 -= 0.05f * (float)BeetleJuice;
			}
			if (body.HasBuff(BuffIndex.GoldEmpowered))
			{
				num36 += 1f;
			}
			num36 += LunarDaggerPower - 1f;
			num35 *= num36;
			body.damage = num35;
			float num38 = body.baseAttackSpeed + body.levelAttackSpeed * Levelminusone;
			float num39 = 1f;
			num39 += (float)Syringe * 0.15f;
			if (equipmentIndex == EquipmentIndex.AffixYellow)
			{
				num39 += 0.5f;
			}
			num39 += (float)body.buffs[2] * 0.12f;
			if (body.HasBuff(BuffIndex.Warbanner))
			{
				num39 += 0.3f;
			}
			if (body.HasBuff(BuffIndex.EnrageAncientWisp))
			{
				num39 += 2f;
			}
			if (body.HasBuff(BuffIndex.WarCryBuff))
			{
				num39 += 1f;
			}
			num38 *= num39;
			if (BeetleJuice > 0)
			{
				num38 *= 1f - 0.05f * (float)BeetleJuice;
			}
			body.attackSpeed = num38;
			float num40 = body.baseCrit + body.levelCrit * Levelminusone;
			num40 += (float)CritGlasses * 10f;
			if (AttackSpeedOnCrit > 0)
			{
				num40 += 5f;
			}
			if (CooldownOnCrit > 0)
			{
				num40 += 5f;
			}
			if (HealOnCrit > 0)
			{
				num40 += 5f;
			}
			if (Critheal > 0)
			{
				num40 += 5f;
			}
			if (body.HasBuff(BuffIndex.FullCrit))
			{
				num40 += 100f;
			}
			body.crit = num40;
			body.armor = body.baseArmor + body.levelArmor * Levelminusone + (body.HasBuff(BuffIndex.ArmorBoost) ? 200f : 0f);
			body.armor += (float)DrizzlePlayerHelper * 70f;
			if (body.HasBuff(BuffIndex.Cripple))
			{
				body.armor -= 20f;
			}
			if (body.isSprinting && SprintArmor > 0)
			{
				body.armor += (float)(SprintArmor * 30);
			}
			float num41 = 1f;
			if (body.HasBuff(BuffIndex.GoldEmpowered))
			{
				num41 *= 0.25f;
			}
			for (int i = 0; i < AlienHead; i++)
			{
				num41 *= 0.75f;
			}
			if (body.HasBuff(BuffIndex.NoCooldowns))
			{
				num41 = 0f;
			}
			if (body.skillLocator.primary)
			{
				body.skillLocator.primary.cooldownScale = num41;
			}
			if (body.skillLocator.secondary)
			{
				body.skillLocator.secondary.cooldownScale = num41;
				body.skillLocator.secondary.SetBonusStockFromBody(bonusStockFromBody);
			}
			if (body.skillLocator.utility)
			{
				float num42 = num41;
				if (UtilitySkillMagazine > 0)
				{
					num42 *= 0.6666667f;
				}
				this.skillLocator.utility.cooldownScale = num42;
				this.skillLocator.utility.SetBonusStockFromBody(UtilitySkillMagazine * 2);
			}
			if (this.skillLocator.special)
			{
				this.skillLocator.special.cooldownScale = num41;
			}
			this.critHeal = 0f;
			if (Critheal > 0)
			{
				float crit = this.crit;
				this.crit /= (float)(Critheal + 1);
				this.critHeal = crit - this.crit;
			}
			if (NetworkServer.active)
			{
				float num43 = this.maxHealth - maxHealth;
				float num44 = this.maxShield - maxShield;
				if (num43 > 0f)
				{
					this.healthComponent.Heal(num43, default(ProcChainMask), false);
				}
				else if (this.healthComponent.health > this.maxHealth)
				{
					this.healthComponent.Networkhealth = this.maxHealth;
				}
				if (num44 > 0f)
				{
					this.healthComponent.RechargeShield(num44);
				}
			}
			this.statsDirty = false;
		}

	}

	public class PlayerStats
	{
		public PlayerStats(CharacterBody characterBody)
		{
			maxHealth = characterBody.maxHealth;
			healthRegen = characterBody.regen;

		}
		//Character Stats
		public float maxHealth = 0;
		public float healthRegen = 0;
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