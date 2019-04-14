using MonoMod.RuntimeDetour;
using R2API.Utils;
using RoR2;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace R2API
{
	public static class PlayerAPI
	{
		public static List<PlayerStatChange> CustomEffects { get; private set; }

		public static void InitHooks()
		{
			var detour = new NativeDetour(typeof(CharacterBody).GetMethod("RecalculateStats", System.Reflection.BindingFlags.Public),
				typeof(ItemAPI).GetMethod(nameof(RecalcStats), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static));
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

			foreach (var x in PlayerAPI.CustomEffects)
			{
				characterBody.SetFieldValue(x.Target, x.Function(characterBody));
			}

			characterBody.statsDirty = false;
		}
	}
	public class PlayerStatChange
	{
		/// <summary>
		/// The <b>case sensitive</b> name of the field you want to modify in the Character Body
		/// </summary>
		/// <example>
		/// Target = "damamge"
		/// </example>
		public string Target { get; set; }
		/// <summary>
		/// The type of operand that will be applied to the target.	
		/// </summary>
		public Operands operand { get; set; }
		/// <summary>
		/// This method is executed by the API to obtain the value. This is where you will have to code the logic of how your custom effect
		/// changes the player's stats.
		/// </summary>
		public Func<CharacterBody, int> Function { get; private set; }
	}
	public enum Operands { Addition, Multiplication, Division, Substraction }
}
