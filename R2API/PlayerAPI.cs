using R2API.Utils;
using RoR2;
using System;
using System.Linq;
using System.Collections.Generic;

namespace R2API
{
	public static class PlayerAPI
	{
		public static List<Action<PlayerStats>> CustomEffects { get; private set; }

		public static void InitHooks()
		{
			On.RoR2.CharacterBody.RecalculateStats += (orig, self) => RecalcStats(self);
		}

		public static void RecalcStats(CharacterBody characterBody)
		{
			characterBody.SetFieldValue("experience", TeamManager.instance.GetTeamExperience(characterBody.teamComponent.teamIndex));
			characterBody.SetFieldValue("level", TeamManager.instance.GetTeamLevel(characterBody.teamComponent.teamIndex));

			//Character Stats
			float maxHealth = 0;
			int healthRegen = 0;
			bool isElite = false;
			int maxShield = 0;
			float movementSpeed = 0;
			float acceleration = 0;
			float jumpPower = 0;
			float maxJumpHeight = 0;
			float maxJumpCount = 0;
			float attackSpeed = 0;
			float damage = 0;
			float Crit = 0;
			float Armor = 0;
			float critHeal = 0; 

			//Primary Skill
			float PrimaryCooldownScale = 0;
			float PrimaryStock = 0;
			//Secondary Skill
			float SecondaryCooldownScale = 0;
			float SecondaryStock = 0;
			//Utility Skill
			float UtilityCooldownScale = 0;
			float UtilityStock = 0;
			//Special Skill
			float SpecialCooldownScale = 0;
			float SpecialStock = 0;

			Dictionary<ItemIndex, int> Items = new Dictionary<ItemIndex, int>();
			EquipmentIndex eIndex = EquipmentIndex.None;
			uint infusionBonus = 0u;
			float Daggerpower = 0f;
			if (characterBody.inventory)
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
				movementSpeed += characterBody.GetBuffCount(BuffIndex.BeetleJuice);
				Daggerpower = characterBody.CalcLunarDaggerPower();
				eIndex = characterBody.inventory.currentEquipmentIndex;
				infusionBonus = characterBody.inventory.infusionBonus;
			}
			float level = characterBody.level -1f;
			isElite = characterBody.buffMask.containsEliteBuff;
			maxHealth = characterBody.baseMaxHealth + characterBody.levelMaxHealth * characterBody.level - 1f;

			;
			//Max Health Bonuses
			maxHealth += Items[ItemIndex.Infusion]> 0 ? infusionBonus : 0;
			maxHealth += Items[ItemIndex.Knurl] * 40f;
			maxHealth *= Math.Max(Items[ItemIndex.BoostHp] * 0.1f,1f);
			maxHealth *= characterBody.HasBuff(BuffIndex.AffixBlue) ? 0.5f : 1f;
			maxHealth /= Daggerpower;
			characterBody.SetProperyValue("maxHealth", maxHealth);

			PlayerStats playerStats = null; //TODO: initialize this from characterBody

			foreach (var effectAction in PlayerAPI.CustomEffects)
			{
				effectAction(playerStats);
			}

			characterBody.statsDirty = false;
		}
	}

	public class PlayerStats
	{
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