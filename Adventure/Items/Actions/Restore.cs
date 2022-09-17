using Adventure.Battle;
using Engine;
using RpgMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Items.Actions
{
    class RestoreHp : IInventoryAction
    {
        public void Use(InventoryItem item, Inventory inventory, CharacterSheet attacker, CharacterSheet target)
        {
            inventory.Items.Remove(item);

            if (target.CurrentHp == 0) { return; }

            target.CurrentHp += item.Number.Value + (long)(item.Number.Value * attacker.TotalItemUsageBonus);
            if(target.CurrentHp > target.Hp)
            {
                target.CurrentHp = target.Hp;
            }
        }

        public void Use(InventoryItem item, Inventory inventory, IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target)
        {
            inventory.Items.Remove(item);

            target = battleManager.ValidateTarget(attacker, target);
            var damage = item.Number.Value + (long)(item.Number.Value * attacker.Stats.TotalItemUsageBonus);

            damage *= -1; //Make it healing

            //Apply resistance
            var resistance = target.Stats.GetResistance(RpgMath.Element.Healing);
            damage = battleManager.DamageCalculator.ApplyResistance(damage, resistance);

            battleManager.AddDamageNumber(target, damage);
            target.ApplyDamage(attacker, battleManager.DamageCalculator, damage);
            battleManager.HandleDeath(target);

            var applyEffect = objectResolver.Resolve<Attachment<BattleScene>, Attachment<BattleScene>.Description>(o =>
            {
                var asset = new Assets.PixelEffects.MagicBubbles();
                o.RenderShadow = false;
                o.Sprite = asset.CreateSprite();
                o.SpriteMaterial = asset.CreateMaterial();
            });
            applyEffect.SetPosition(target.MagicHitLocation, Quaternion.Identity, Vector3.ScaleIdentity);

            IEnumerator<YieldAction> run()
            {
                yield return coroutine.WaitSeconds(1.5);
                applyEffect.RequestDestruction();
            }
            coroutine.Run(run());
        }
    }

    class RestoreMp : IInventoryAction
    {
        public void Use(InventoryItem item, Inventory inventory, CharacterSheet attacker, CharacterSheet target)
        {
            inventory.Items.Remove(item);

            if (target.CurrentHp == 0) { return; }

            target.CurrentMp += item.Number.Value + (long)(item.Number.Value * attacker.TotalItemUsageBonus);
            if (target.CurrentMp > target.Mp)
            {
                target.CurrentMp = target.Mp;
            }
        }

        public void Use(InventoryItem item, Inventory inventory, IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target)
        {
            inventory.Items.Remove(item);

            target = battleManager.ValidateTarget(attacker, target);
            var damage = item.Number.Value + (long)(item.Number.Value * attacker.Stats.TotalItemUsageBonus);

            damage *= -1; //Make it healing

            //Apply resistance
            var resistance = target.Stats.GetResistance(RpgMath.Element.MpRestore);
            damage = battleManager.DamageCalculator.ApplyResistance(damage, resistance);

            battleManager.AddDamageNumber(target, damage);
            target.TakeMp(damage);
            battleManager.HandleDeath(target);

            var applyEffect = objectResolver.Resolve<Attachment<BattleScene>, Attachment<BattleScene>.Description>(o =>
            {
                var asset = new Assets.PixelEffects.MagicBubbles();
                o.RenderShadow = false;
                o.Sprite = asset.CreateSprite();
                o.SpriteMaterial = asset.CreateMaterial();
            });
            applyEffect.SetPosition(target.MagicHitLocation, Quaternion.Identity, Vector3.ScaleIdentity);

            IEnumerator<YieldAction> run()
            {
                yield return coroutine.WaitSeconds(1.5);
                applyEffect.RequestDestruction();
            }
            coroutine.Run(run());
        }
    }

    class Revive : IInventoryAction
    {
        public bool AllowTargetChange => false;

        public void Use(InventoryItem item, Inventory inventory, CharacterSheet attacker, CharacterSheet target)
        {
            inventory.Items.Remove(item);

            if (target.CurrentHp != 0) { return; }

            target.CurrentHp += GetStartHp(target.Hp, item.Number.Value + (long)(item.Number.Value * attacker.TotalItemUsageBonus));
        }

        private long GetStartHp(long maxHp, long value)
        {
            return Math.Min((long)(maxHp * value * 0.01f), maxHp);
        }

        public void Use(InventoryItem item, Inventory inventory, IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target)
        {
            inventory.Items.Remove(item);

            if (!battleManager.IsStillValidTarget(target))
            {
                target = battleManager.ValidateTarget(attacker, target);
            }
            var damage = GetStartHp(target.Stats.Hp, item.Number.Value + (long)(item.Number.Value * attacker.Stats.TotalItemUsageBonus));

            damage *= -1; //Make it healing

            //Apply resistance
            var resistance = target.Stats.GetResistance(RpgMath.Element.Healing);
            damage = battleManager.DamageCalculator.ApplyResistance(damage, resistance);

            if (target.IsDead || damage > 0)
            {
                battleManager.AddDamageNumber(target, damage);
            }
            else
            {
                battleManager.AddDamageNumber(target, "Miss", Color.Red);
            }
            target.Resurrect(battleManager.DamageCalculator, damage);
            battleManager.HandleDeath(target);

            var applyEffect = objectResolver.Resolve<Attachment<BattleScene>, Attachment<BattleScene>.Description>(o =>
            {
                var asset = new Assets.PixelEffects.MagicBubbles();
                o.RenderShadow = false;
                o.Sprite = asset.CreateSprite();
                o.SpriteMaterial = asset.CreateMaterial();
            });
            applyEffect.SetPosition(target.MagicHitLocation, Quaternion.Identity, Vector3.ScaleIdentity);

            IEnumerator<YieldAction> run()
            {
                yield return coroutine.WaitSeconds(1.5);
                applyEffect.RequestDestruction();
            }
            coroutine.Run(run());
        }
    }
}
