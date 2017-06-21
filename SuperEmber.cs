namespace SuperEmber
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using Ensage;
    using Ensage.Common;
    using Ensage.Common.Extensions;
    using Ensage.SDK.Extensions;
    using Ensage.SDK.Helpers;
    using Ensage.SDK.Input;
    using Ensage.SDK.Orbwalker;
    using Ensage.SDK.Orbwalker.Modes;
    using Ensage.SDK.TargetSelector;

    using SharpDX;

    using AbilityId = Ensage.AbilityId;
    using UnitExtensions = Ensage.SDK.Extensions.UnitExtensions;
    using EntityExtensions = Ensage.SDK.Extensions.EntityExtensions;
    using Vector3Extensions = Ensage.SDK.Extensions.Vector3Extensions;

    public class SuperEmber : KeyPressOrbwalkingModeAsync
    {
        private Unit MyHero;
        private Unit Target;

        private Ability FistAbility { get; set; }
        private Ability FlameAbility { get; set; }
        private Ability ChainsAbility { get; set; }
	private Ability ActivatorAbility { get; set; }
        private Ability RemnantAbility { get; set; }

        public Config Config { get; }

        private Lazy<ITargetSelectorManager> TargetSelector { get; }

        public SuperEmber(Key key, Config config, Lazy<IOrbwalkerManager> orbwalker, Lazy<IInputManager> input, Lazy<ITargetSelectorManager> targetSelector)
            : base(orbwalker.Value, input.Value, key)
        {
            this.Config = config;
            this.TargetSelector = targetSelector;
        }


        public async Task UseItems(Unit target, CancellationToken token)
        {
            var called = EntityManager<Hero>.Entities
                .Where(x => this.MyHero.Team != x.Team && x.IsValid && !x.IsIllusion && x.IsAlive)
                .ToList();

            if (called.Any())
            {
                var bkb = this.MyHero.GetItemById(AbilityId.item_black_king_bar);
                if (bkb != null && bkb.CanBeCasted() && this.Config.UseItems.Value.IsEnabled(bkb.Name))
                {
                    bkb.UseAbility();
                    await Task.Delay(10, token);
                }

		var bloodthorn = this.MyHero.GetItemById(AbilityId.item_bloodthorn);
                if (bloodthorn != null && bloodthorn.CanBeCasted() && this.Config.UseItems.Value.IsEnabled(bloodthorn.Name))
                {
                    bloodthorn.UseAbility(target);
                    await Task.Delay(10, token);
                }

		var orchid = this.MyHero.GetItemById(AbilityId.item_orchid);
                if (orchid != null && orchid.CanBeCasted() && this.Config.UseItems.Value.IsEnabled(orchid.Name))
                {
                    orchid.UseAbility(target);
                    await Task.Delay(10, token);
                }

                var veil = this.MyHero.GetItemById(AbilityId.item_veil_of_discord);
                if (veil != null && veil.CanBeCasted() && !UnitExtensions.IsMagicImmune(target) && this.Config.UseItems.Value.IsEnabled(veil.Name))
                {
                    veil.UseAbility(target.Position);
                    await Task.Delay(10, token);
                }

		var ethereal = this.MyHero.GetItemById(AbilityId.item_ethereal_blade);
                if (ethereal != null && ethereal.CanBeCasted() && !UnitExtensions.IsMagicImmune(target) && this.Config.UseItems.Value.IsEnabled(ethereal.Name))
                {
                    ethereal.UseAbility(target);
                    await Task.Delay(10, token);
                }

                var bladeMail = this.MyHero.GetItemById(AbilityId.item_blade_mail);
                if (bladeMail != null && bladeMail.CanBeCasted() && this.Config.UseItems.Value.IsEnabled(bladeMail.Name))
                {
                    bladeMail.UseAbility();
                    await Task.Delay(10, token);
                }

                var lotus = this.MyHero.GetItemById(AbilityId.item_lotus_orb);
                if (lotus != null && lotus.CanBeCasted() && this.Config.UseItems.Value.IsEnabled(lotus.Name))
                {
                    lotus.UseAbility(this.MyHero);
                    await Task.Delay(10, token);
                }

                var mjollnir = this.MyHero.GetItemById(AbilityId.item_mjollnir);
                if (mjollnir != null && mjollnir.CanBeCasted() && this.Config.UseItems.Value.IsEnabled(mjollnir.Name))
                {
                    mjollnir.UseAbility(this.MyHero);
                    await Task.Delay(10, token);
                }

                var shiva = this.MyHero.GetItemById(AbilityId.item_shivas_guard);
                if (shiva != null && shiva.CanBeCasted() && this.Config.UseItems.Value.IsEnabled(shiva.Name))
                {
                    shiva.UseAbility();
                    await Task.Delay(10, token);
                }

		var atos = this.MyHero.GetItemById(AbilityId.item_rod_of_atos);
                if (atos != null && atos.CanBeCasted() && !UnitExtensions.IsMagicImmune(target) && !UnitExtensions.IsRooted(target) && this.Config.UseItems.Value.IsEnabled("item_rod_of_atos"))
		{
                    atos.UseAbility(target);
                    await Task.Delay(10, token);
                }

                var dagon = MyHero.GetDagon();
                if (dagon != null && dagon.CanBeCasted() && !UnitExtensions.IsMagicImmune(target) && this.Config.UseItems.Value.IsEnabled("item_dagon"))
                {
                    dagon.UseAbility(target);
                    await Task.Delay(10, token);
                }
            }
        }
    }
}

