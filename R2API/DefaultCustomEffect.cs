﻿using RoR2.Projectile;
using UnityEngine.Networking;
using UnityEngine;
using RoR2;
using RoR2.Orbs;
using R2API.Utils;
using System.Collections.Generic;

namespace R2API {
    class HealOnCritHitReplace : HitEffect {
        public override bool Condition(GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim,
            int count) {
            var procChainMask = damageInfo.procChainMask;
            return damageInfo.crit && procChainMask.GetProcValue(ProcType.HealOnCrit);
        }

        public override void Effect(GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim,
            int itemCount) {
            var procCoefficient = damageInfo.procCoefficient;
            var body = damageInfo.attacker.GetComponent<CharacterBody>();
            var procChainMask = damageInfo.procChainMask;

            procChainMask.SetProcValue(ProcType.HealOnCrit, true);

            if (itemCount <= 0 || !body.healthComponent)
                return;

            Util.PlaySound("Play_item_proc_crit_heal", body.gameObject);
            if (NetworkServer.active) {
                body.healthComponent.Heal((4f + itemCount * 4f) * procCoefficient, procChainMask);
            }
        }
    }

    class AttackSpeedOnCritHitReplace : HitEffect {
        public override bool Condition(GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim,
            int count) {
            return damageInfo.crit;
        }

        public override void Effect(GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim,
            int itemCount) {
            var procCoefficient = damageInfo.procCoefficient;
            var body = damageInfo.attacker.GetComponent<CharacterBody>();

            body.AddTimedBuff(BuffIndex.AttackSpeedOnCrit, 2f * procCoefficient);
        }
    }

    class CoolDownOnCritHitReplace : HitEffect {
        public override bool Condition(GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim,
            int count) {
            return damageInfo.crit;
        }

        public override void Effect(GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim,
            int itemCount) {
            var procCoefficient = damageInfo.procCoefficient;
            var body = damageInfo.attacker.GetComponent<CharacterBody>();
            var master = body.master;
            var inventory = master.inventory;

            var wickedRingCount = inventory.GetItemCount(ItemIndex.CooldownOnCrit);
            if (wickedRingCount <= 0)
                return;
            Util.PlaySound("Play_item_proc_crit_cooldown", body.gameObject);
            var component = body.GetComponent<SkillLocator>();
            if (!(bool) component)
                return;
            var dt = wickedRingCount * procCoefficient;
            if ((bool) component.primary)
                component.primary.RunRecharge(dt);
            if ((bool) component.secondary)
                component.secondary.RunRecharge(dt);
            if ((bool) component.utility)
                component.utility.RunRecharge(dt);
            if (!(bool) component.special)
                return;
            component.special.RunRecharge(dt);
        }
    }

    class HealOnHitReplace : HitEffect {
        public override bool Condition(GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim,
            int count) {
            return !damageInfo.procChainMask.GetProcValue(ProcType.HealOnHit);
        }

        public override void Effect(GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim,
            int itemCount) {
            var attacker = damageInfo.attacker.GetComponent<CharacterBody>();

            if (itemCount <= 0)
                return;

            var attackerHealthComponent = attacker.GetComponent<HealthComponent>();

            if (!attackerHealthComponent)
                return;

            var procChainMask = damageInfo.procChainMask;
            procChainMask.SetProcValue(ProcType.HealOnHit, true);
            attackerHealthComponent.Heal(itemCount * damageInfo.procCoefficient, procChainMask);
        }
    }

    class StunOnHitReplace : HitEffect {
        public override bool Condition(GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim,
            int count) {
            var body = damageInfo.attacker.GetComponent<CharacterBody>();

            return Util.CheckRoll(1.0f - 1.0f / (damageInfo.procCoefficient * 0.05f * count + 1.0f) * 100f,
                body.master.GetComponent<CharacterMaster>());
        }

        public override void Effect(GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim,
            int itemCount) {
            var hurtStat = victim.GetComponent<SetStateOnHurt>();
            if (hurtStat)
                hurtStat.SetStun(2f);
        }
    }

    class BleedOnHitReplace : HitEffect {
        public override bool Condition(GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim,
            int count) {
            var attacker = damageInfo.attacker.GetComponent<CharacterBody>();
            var master = attacker.master;
            return !damageInfo.procChainMask.GetProcValue(ProcType.BleedOnHit) &&
                   (damageInfo.damageType & DamageType.BleedOnHit) > 0U &&
                   Util.CheckRoll(15f * count * damageInfo.procCoefficient, master);
        }

        public override void Effect(GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim,
            int itemCount) {
            damageInfo.procChainMask.SetProcValue(ProcType.BleedOnHit, true);
            DotController.InflictDot(victim, damageInfo.attacker, DotController.DotIndex.Bleed,
                3f * damageInfo.procCoefficient, 1f);
        }
    }

    class SlowOnHitReplace : HitEffect {
        public override bool Condition(GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim,
            int count) {
            var characterBody = victim ? victim.GetComponent<CharacterBody>() : null;
            return characterBody;
        }

        public override void Effect(GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim,
            int itemCount) {
            if (victim)
                victim.GetComponent<CharacterBody>().AddTimedBuff(BuffIndex.Slow60, 1f * itemCount);
        }
    }

    class GoldOnHitReplace : HitEffect {
        public override bool Condition(GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim,
            int count) {
            var master = damageInfo.attacker.GetComponent<CharacterBody>().master;
            return Util.CheckRoll(30f * damageInfo.procCoefficient, master);
        }

        public override void Effect(GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim,
            int itemCount) {
            var attacker = damageInfo.attacker.GetComponent<CharacterBody>();
            var master = attacker.master;

            master.GiveMoney((uint) (itemCount * 2.0 * Run.instance.difficultyCoefficient));
            EffectManager.instance.SimpleImpactEffect(
                Resources.Load<GameObject>("Prefabs/Effects/ImpactEffects/CoinImpact"), damageInfo.position, Vector3.up,
                true);
        }
    }

    class MissileOnHitReplace : HitEffect {
        public override bool Condition(GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim,
            int count) {
            return !damageInfo.procChainMask.GetProcValue(ProcType.Missile);
        }

        static void ProcMissile(int stack, CharacterBody attackerBody, CharacterMaster attackerMaster,
            TeamIndex attackerTeamIndex, ProcChainMask procChainMask, GameObject victim, DamageInfo damageInfo) {
            if (stack <= 0)
                return;
            var attackerGo = attackerBody.gameObject;
            var attackerBankTest = attackerGo.GetComponent<InputBankTest>();
            var position = attackerBankTest
                ? attackerBankTest.aimOrigin
                : GlobalEventManager.instance.transform.position;
            var up = Vector3.up;
            if (!Util.CheckRoll(10f * damageInfo.procCoefficient, attackerMaster))
                return;
            var missileGo = Object.Instantiate<GameObject>(GlobalEventManager.instance.missilePrefab, position,
                Util.QuaternionSafeLookRotation(up + Random.insideUnitSphere * 0.0f));
            var missileController = missileGo.GetComponent<ProjectileController>();
            missileController.Networkowner = attackerGo.gameObject;
            missileController.procChainMask = procChainMask;
            missileController.procChainMask.AddProc(ProcType.Missile);
            missileGo.GetComponent<TeamFilter>().teamIndex = attackerTeamIndex;
            missileGo.GetComponent<MissileController>().target = victim.transform;
            var damageCoefficient = 3f * (float) stack;
            var num = Util.OnHitProcDamage(damageInfo.damage, attackerBody.damage, damageCoefficient);
            var missileDamage = missileGo.GetComponent<ProjectileDamage>();
            missileDamage.damage = num;
            missileDamage.crit = damageInfo.crit;
            missileDamage.force = 200f;
            missileDamage.damageColorIndex = DamageColorIndex.Item;
            NetworkServer.Spawn(missileGo);
        }

        public override void Effect(GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim,
            int itemCount) {
            var attacker = damageInfo.attacker.GetComponent<CharacterBody>();
            var master = attacker.master;
            var team = attacker.GetComponent<TeamComponent>();
            var attackerTeamIndex = team ? team.teamIndex : TeamIndex.Neutral;

            ProcMissile(itemCount, attacker, master, attackerTeamIndex, damageInfo.procChainMask, victim, damageInfo);
        }
    }

    class UkeleleOnHitReplace : HitEffect {
        public override bool Condition(GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim,
            int count) {
            var master = damageInfo.attacker.GetComponent<CharacterBody>().master;
            return !damageInfo.procChainMask.HasProc(ProcType.ChainLightning) &&
                   Util.CheckRoll(25f * damageInfo.procCoefficient, master);
        }

        public override void Effect(GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim,
            int itemCount) {
            var attacker = damageInfo.attacker.GetComponent<CharacterBody>();
            var team = attacker.GetComponent<TeamComponent>();
            var attackerTeamIndex = team ? team.teamIndex : TeamIndex.Neutral;


            var damageCoefficient = 0.8f;
            var damage = Util.OnHitProcDamage(damageInfo.damage, attacker.damage, damageCoefficient);
            var lightningOrb = new LightningOrb {
                origin = damageInfo.position,
                damageValue = damage,
                isCrit = damageInfo.crit,
                bouncesRemaining = 2 * itemCount,
                teamIndex = attackerTeamIndex,
                attacker = damageInfo.attacker,
                bouncedObjects = new List<HealthComponent> {victim.GetComponent<HealthComponent>()},
                procChainMask = damageInfo.procChainMask,
                procCoefficient = 0.2f,
                lightningType = LightningOrb.LightningType.Ukulele,
                damageColorIndex = DamageColorIndex.Item
            };
            lightningOrb.range += 2 * itemCount;
            lightningOrb.procChainMask.AddProc(ProcType.ChainLightning);

            var hurtBox = lightningOrb.PickNextTarget(damageInfo.position);
            if (!hurtBox)
                return;

            lightningOrb.target = hurtBox;
            OrbManager.instance.AddOrb(lightningOrb);
        }
    }

    class HookEffectReplace : HitEffect {
        public override bool Condition(GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim,
            int count) {
            var attacker = damageInfo.attacker.GetComponent<CharacterBody>();
            var master = attacker.master;

            var procChance = (float) ((1.0 - 100.0 / (100.0 + 20.0 * count)) * 100.0);
            return !damageInfo.procChainMask.HasProc(ProcType.BounceNearby) &&
                   Util.CheckRoll(procChance * damageInfo.procCoefficient, master);
        }

        public override void Effect(GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim,
            int itemCount) {
            var attacker = damageInfo.attacker.GetComponent<CharacterBody>();
            var team = attacker.GetComponent<TeamComponent>();
            var attackerTeamIndex = team ? team.teamIndex : TeamIndex.Neutral;


            var healthComponentList = new List<HealthComponent>() {
                victim.GetComponent<HealthComponent>()
            };
            var damageCoefficient = 1f;
            var num3 = Util.OnHitProcDamage(damageInfo.damage, attacker.damage, damageCoefficient);
            for (var index = 0; index < 5 + itemCount * 5; ++index) {
                var bounceOrb = new BounceOrb {
                    origin = damageInfo.position,
                    damageValue = num3,
                    isCrit = damageInfo.crit,
                    teamIndex = attackerTeamIndex,
                    attacker = damageInfo.attacker,
                    procChainMask = damageInfo.procChainMask,
                    procCoefficient = 0.33f,
                    damageColorIndex = DamageColorIndex.Item,
                    bouncedObjects = healthComponentList
                };
                bounceOrb.procChainMask.AddProc(ProcType.BounceNearby);

                var hurtBox = bounceOrb.PickNextTarget(victim.transform.position, 30f);
                if (!(bool) hurtBox)
                    continue;

                bounceOrb.target = hurtBox;
                OrbManager.instance.AddOrb(bounceOrb);
            }
        }
    }

    class StickyBombOnHitReplace : HitEffect {
        public override bool Condition(GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim,
            int count) {
            var attacker = damageInfo.attacker.GetComponent<CharacterBody>();
            var characterBody = victim ? victim.GetComponent<CharacterBody>() : null;
            var master = attacker.master;

            return Util.CheckRoll((float) (2.5 + 2.5 * count) * damageInfo.procCoefficient, master) && characterBody;
        }

        public override void Effect(GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim,
            int itemCount) {
            var attacker = damageInfo.attacker.GetComponent<CharacterBody>();
            var characterBody = victim ? victim.GetComponent<CharacterBody>() : null;

            var position = damageInfo.position;
            var forward = characterBody.corePosition - position;
            var rotation = !forward.magnitude.Equals(0f) ? Util.QuaternionSafeLookRotation(forward) : Random.rotationUniform;
            var damageCoefficient = (float) (1.25 + 1.25 * itemCount);
            var damage = Util.OnHitProcDamage(damageInfo.damage, attacker.damage, damageCoefficient);

#pragma warning disable CS0618 // Obsolete Warning Ignore
            ProjectileManager.instance.FireProjectile(Resources.Load<GameObject>("Prefabs/Projectiles/StickyBomb"),
                position, rotation, damageInfo.attacker, damage, 100f, damageInfo.crit, DamageColorIndex.Item,
                null, forward.magnitude * 60f);
#pragma warning restore CS0618
        }
    }

    class IceRingEffectReplace : HitEffect {
        public override bool Condition(GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim,
            int count) {
            var attacker = damageInfo.attacker.GetComponent<CharacterBody>();
            var master = attacker.master;

            bool proc;

            if (EffectAPI.ringBuffer == 0) {
                proc = Util.CheckRoll(8f * damageInfo.procCoefficient, master);
                EffectAPI.ringBuffer = proc ? 1 : 2;
            }
            else {
                proc = EffectAPI.ringBuffer == 1;
            }

            return proc;
        }

        public override void Effect(GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim,
            int itemCount) {
            var attacker = damageInfo.attacker.GetComponent<CharacterBody>();
            var characterBody = victim ? victim.GetComponent<CharacterBody>() : null;

            damageInfo.procChainMask.SetProcValue(ProcType.Rings, true);


            var damageCoefficient = (float) (1.25 + 1.25 * itemCount);
            var num3 = Util.OnHitProcDamage(damageInfo.damage, attacker.damage, damageCoefficient);
            var damageInfo1 = new DamageInfo {
                damage = num3,
                damageColorIndex = DamageColorIndex.Item,
                damageType = DamageType.Generic,
                attacker = damageInfo.attacker,
                crit = damageInfo.crit,
                force = Vector3.zero,
                inflictor = null,
                position = damageInfo.position,
                procChainMask = damageInfo.procChainMask,
                procCoefficient = 1f
            };
            EffectManager.instance.SimpleImpactEffect(
                Resources.Load<GameObject>("Prefabs/Effects/ImpactEffects/IceRingExplosion"), damageInfo.position,
                Vector3.up, true);
            characterBody.AddTimedBuff(BuffIndex.Slow80, 3f);
            victim.GetComponent<HealthComponent>()?.TakeDamage(damageInfo1);
        }
    }

    class FireRingEffectReplace : HitEffect {
        public override bool Condition(GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim,
            int count) {
            var attacker = damageInfo.attacker.GetComponent<CharacterBody>();
            var master = attacker.master;

            bool proc;

            if (EffectAPI.ringBuffer == 0) {
                proc = Util.CheckRoll(8f * damageInfo.procCoefficient, master);
                EffectAPI.ringBuffer = proc ? 1 : 2;
            }
            else {
                proc = EffectAPI.ringBuffer == 1;
            }

            return proc;
        }

        public override void Effect(GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim,
            int itemCount) {
            var attacker = damageInfo.attacker.GetComponent<CharacterBody>();
            var aimOrigin = attacker.aimOrigin;

            damageInfo.procChainMask.SetProcValue(ProcType.Rings, true);
            var gameObject = Resources.Load<GameObject>("Prefabs/Projectiles/FireTornado");
            var resetInterval = gameObject.GetComponent<ProjectileOverlapAttack>().resetInterval;
            var lifetime = gameObject.GetComponent<ProjectileSimple>().lifetime;
            var damageCoefficient1 = (float) (2.5 + 2.5 * itemCount);
            var damageProjectile = Util.OnHitProcDamage(damageInfo.damage, attacker.damage, damageCoefficient1) /
                                   lifetime * resetInterval;
            var projectileSpeed = 0.0f;
            var quaternion = Quaternion.identity;
            var forward = damageInfo.position - aimOrigin;
            forward.y = 0.0f;
            if (forward != Vector3.zero) {
                projectileSpeed = -1f;
                quaternion = Util.QuaternionSafeLookRotation(forward, Vector3.up);
            }

            ProjectileManager.instance.FireProjectile(new FireProjectileInfo {
                damage = damageProjectile,
                crit = damageInfo.crit,
                damageColorIndex = DamageColorIndex.Item,
                position = damageInfo.position,
                procChainMask = damageInfo.procChainMask,
                force = 0.0f,
                owner = damageInfo.attacker,
                projectilePrefab = gameObject,
                rotation = quaternion,
                speedOverride = projectileSpeed,
                target = null
            });
        }
    }


    class BehemothEffectReplace : HitEffect {
        public new HitEffectType EffectType = HitEffectType.OnHitAll;

        public override bool Condition(GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim,
            int itemCount) {
            return !damageInfo.procCoefficient.Equals(0f) && !damageInfo.procChainMask.GetProcValue(ProcType.Behemoth);
        }

        public override void Effect(GlobalEventManager globalEventManager, DamageInfo damageInfo, GameObject victim,
            int itemCount) {
            var component = damageInfo.attacker.GetComponent<CharacterBody>();


            var explosionRadius = (float) (1.5 + 2.5 * itemCount) * damageInfo.procCoefficient;
            var damageCoefficient = 0.6f;
            var damage = Util.OnHitProcDamage(damageInfo.damage, component.damage, damageCoefficient);
            EffectManager.instance.SpawnEffect(
                Resources.Load<GameObject>("Prefabs/Effects/OmniEffect/OmniExplosionVFXQuick"), new EffectData() {
                    origin = damageInfo.position,
                    scale = explosionRadius,
                    rotation = Util.QuaternionSafeLookRotation(damageInfo.force)
                }, true);
            var blastAttack = new BlastAttack() {
                position = damageInfo.position,
                baseDamage = damage,
                baseForce = 0.0f,
                radius = explosionRadius,
                attacker = damageInfo.attacker,
                inflictor = null
            };
            blastAttack.teamIndex = TeamComponent.GetObjectTeam(blastAttack.attacker);
            blastAttack.crit = damageInfo.crit;
            blastAttack.procChainMask = damageInfo.procChainMask;
            blastAttack.procCoefficient = 0.0f;
            blastAttack.damageColorIndex = DamageColorIndex.Item;
            blastAttack.falloffModel = BlastAttack.FalloffModel.None;
            blastAttack.damageType = damageInfo.damageType;
            blastAttack.Fire();
        }
    }
}