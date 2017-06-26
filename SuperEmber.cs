namespace SuperEmber
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;
using System.ComponentModel.Composition;
using Ensage.SDK.Service;
using Ensage.SDK.Service.Metadata;
using log4net;
using PlaySharp.Toolkit.Logging;
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

    //public class SuperEmber : KeyPressOrbwalkingModeAsync
    //{
        //private Unit MyHero;
        //private Unit Target;
[ExportPlugin("Ember Annihilation", author:"JumpAttacker", units: HeroId.npc_dota_hero_ember_spirit)]
public class SuperEmber : Plugin
{
private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
private Ability Fist { get; }
private Ability Chains { get; }
private Ability Activator { get; }
private Ability Remnant { get; }
private Config Config { get; set; }
public Hero Me { get; set; }
public Unit Fountain { get; set; }
private ITargetSelectorManager Selector { get; }	
        //private Ability FistAbility { get; set; }
        //private Ability FlameAbility { get; set; }
        //private Ability ChainsAbility { get; set; }
	//private Ability ActivatorAbility { get; set; }
        //private Ability RemnantAbility { get; set; }

        //public Config Config { get; }
[ImportingConstructor]
public SuperEmber([Import] IServiceContext context, [Import] ITargetSelectorManager selector/*, [Import] IPrediction prediction*/)
{
Me = context.Owner as Hero;
Selector = selector;
Remnant = Me.GetAbilityById(AbilityId.ember_spirit_fire_remnant);
Fist = Me.GetAbilityById(AbilityId.ember_spirit_sleight_of_fist);
Activator = Me.GetAbilityById(AbilityId.ember_spirit_activate_fire_remnant);
Chains = Me.GetAbilityById(AbilityId.ember_spirit_searing_chains);
Fountain =
ObjectManager.GetEntities<Unit>()
.FirstOrDefault(
x => x != null && x.IsValid && x.ClassId == ClassId.CDOTA_Unit_Fountain && x.Team == Me.Team);
}
        //private Lazy<ITargetSelectorManager> TargetSelector { get; }

        //public Ember(Key key, Config config, Lazy<IOrbwalkerManager> orbwalker, Lazy<IInputManager> input, Lazy<ITargetSelectorManager> targetSelector)
        //    : base(orbwalker.Value, input.Value, key)
        //{
        //    this.Config = config;
        //    this.TargetSelector = targetSelector;
        //}

        protected override void OnActivate()
        {
        //    this.MyHero = Owner as Hero;
        //    this.FistAbility = this.MyHero.Spellbook.SpellW;
        //    this.FlameAbility = this.MyHero.Spellbook.SpellE;
        //    this.ChainsAbility = this.MyHero.Spellbook.SpellQ;
	//    this.ActivatorAbility = this.MyHero.Spellbook.SpellD;
        //    this.RemnantAbility = this.MyHero.Spellbook.SpellR;
Config = new Config();
Config.FistAndComboKey.Item.ValueChanged += FistAndComboKeyChanged;
Config.RemntantCombo.Item.ValueChanged += RemnantActivator;
Config.PussyKey.Item.ValueChanged += PussyAction;
Config.AutoChain.Item.ValueChanged += AutoChains;
if (Config.AutoChain.Value)
UpdateManager.BeginInvoke(AutoChainer);
}

private void PussyAction(object sender, EventArgs args) //OnValueChangeEventArgs e
{
var newValue = e.GetNewValue<KeyBind>().Active;
if (newValue)
{
if (Me.HasModifier("modifier_ember_spirit_fire_remnant_timer") && Activator.CanBeCasted() && Me.CanCast())
{
if (Fountain == null)
{
Log.Error("cant find Fountain");
Fountain =
ObjectManager.GetEntities<Unit>()
.FirstOrDefault(
x =>
x != null && x.IsValid && x.ClassId == ClassId.CDOTA_Unit_Fountain &&
x.Team == Me.Team);
return;
}
Activator.UseAbility(Fountain.Position);
}
}
}	
        //    Drawing.OnDraw += this.Drawing_OnDraw;
        protected override void OnDeactivate()
        {
            Config?.Dispose();
        }

        private void RemnantActivator(object sender, OnValueChangeEventArgs e)
        {
            var newValue = e.GetNewValue<KeyBind>().Active;
            if (newValue)
                UpdateManager.BeginInvoke(RemnantCombo);
        }
        private void FistAndComboKeyChanged(object sender, OnValueChangeEventArgs args)
        {
            var newValue = args.GetNewValue<KeyBind>().Active;
            if (newValue)
                UpdateManager.BeginInvoke(FistAndChain);
        }
        private void AutoChains(object sender, OnValueChangeEventArgs args)
        {
            var newValue = args.GetNewValue<KeyBind>().Active;
            if (newValue)
                UpdateManager.BeginInvoke(AutoChainer);
        }
        
        private async void RemnantCombo()
        {
            Log.Debug("start remnant combo");
            while (Config.RemntantCombo.Value.Active)
            {
                var target = Selector.Active.GetTargets().FirstOrDefault();
                if (target != null)
                {
                    var mod = Me.FindModifier("modifier_ember_spirit_fire_remnant_charge_counter");
                    var stacks = mod?.StackCount;
                    if (stacks > 0)
                    {
                        Remnant.UseAbility(target.Position);
                        Log.Debug("Remnant: "+stacks);
                        await Task.Delay(20);
                    }
                    else
                    {
                        if (
                            EntityManager<Entity>.Entities.Any(
                                x => x.Name == "npc_dota_ember_spirit_remnant" && x.Distance2D(target) <= 450))
                        {
                            await Task.Delay(150);
                            Activator.UseAbility(target.Position);
                            Log.Debug("Activator");
                            await Task.Delay(100);
                        }
                    }
                }
                await Task.Delay(1);
            }
        }
        private async void FistAndChain()
        {
            Log.Debug("starting combo");
            while (Config.FistAndComboKey.Value.Active)
            {
                var target = Selector.Active.GetTargets().FirstOrDefault();
                if (target != null)
                {
                    if (Fist.CanBeCasted() && Fist.CanHit(target))
                    {
                        Fist.UseAbility(target.Position);
                        Log.Debug("Fist usages");
                        await Task.Delay(25);
                    }
                    if (Chains.CanBeCasted())
                    {
                        if (Me.Distance2D(target) <= 400)
                        {
                            Chains.UseAbility();
                            Log.Debug("Chains usages");
                            await Task.Delay(100);
                        }
                    }
                }
                await Task.Delay(1);
            }
        }
        public async void AutoChainer()
        {
            while (Config.AutoChain.Value)
            {
                if (!Config.FistAndComboKey.Value.Active && !Config.RemntantCombo.Value.Active)
                {
                    var target = Selector.Active.GetTargets().FirstOrDefault();
                    if (target != null)
                    {
                        var mod = Me.FindModifier("modifier_ember_spirit_sleight_of_fist_caster");
                        if (mod != null)
                        {
                            if (Chains.CanBeCasted())
                            {
                                if (Me.Distance2D(target) <= 400)
                                {
                                    Chains.UseAbility();
                                    Log.Debug("Auto Chains usages");
                                    await Task.Delay(100);
                                }
                            }
                        }
                    }
                }
                await Task.Delay(1);
            }
        }
    }
}
        //    base.OnActivate();
        //}

        //protected override void OnDeactivate()
        //{
        //    Drawing.OnDraw -= this.Drawing_OnDraw;

        //    base.OnDeactivate();
        //}
        //private void Drawing_OnDraw(EventArgs args)
        //{
        //    if (!this.Config.Enabled)
        //        return;

        //    var enemies = ObjectManager.GetEntities<Hero>()
        //       .Where(x => x.IsVisible && x.IsAlive && !x.IsIllusion && x.Team != this.MyHero.Team)
        //        .ToList();

        //    if (enemies == null)
        //        return;

        //    var threshold = this.FistAbility.GetAbilityData("kill_threshold");

        //    foreach (var enemy in enemies) //tirar a barra depois TODO
        //    {
        //       var tmp = enemy.Health < threshold ? enemy.Health : threshold;
        //        var perc = (float)tmp / (float)enemy.MaximumHealth;
        //        var pos = HUDInfo.GetHPbarPosition(enemy) + 2;
        //        var size = new Vector2(HUDInfo.GetHPBarSizeX(enemy) - 6, HUDInfo.GetHpBarSizeY(enemy) - 2);

        //        Drawing.DrawRect(pos, new Vector2(size.X * perc, size.Y), Color.Chocolate);
        //    }
        //}

////public override async Task ExecuteAsync(CancellationToken token)
    ////    {
        //    if (!this.Config.Enabled)
          //      return;
//
  //          var target = this.TargetSelector.Value.Active.GetTargets().FirstOrDefault();
//	    
  //          if (this.FlameAbility.CanBeCasted() && !UnitExtensions.IsSilenced(this.MyHero) && this.MyHero.IsAlive && target != null)
    //        {
      //          var blink = this.MyHero.GetItemById(AbilityId.item_blink);
        //        var forece = this.MyHero.GetItemById(AbilityId.item_force_staff);
          //      var rangeCallAbility = 300 + target.HullRadius;
            //    var delayCallAbility = this.ChainsAbility.FindCastPoint() * 1000 + Game.Ping;
              //  var posForHitChance = UnitExtensions.InFront(target, (target.IsMoving ? (target.MovementSpeed / 2) : 0));
                //var distanceToHitChance = EntityExtensions.Distance2D(this.MyHero, posForHitChance);
//
  //              // Prediction? no, have not heard..
    //            if (distanceToHitChance < rangeCallAbility)
      //          {
        //            this.FlameAbility.UseAbility();
          //          await Task.Delay((int)delayCallAbility, token);
            //    }
              //  else if (distanceToHitChance < 1200 && blink != null && blink.CanBeCasted() && this.Config.UseItemsInit.Value.IsEnabled(blink.Name))
                //{
                  //  blink.UseAbility(posForHitChance);
                    //await Task.Delay(10, token);
                //}
                // 800?
                //else if (distanceToHitChance < 800 && forece != null && forece.CanBeCasted() && this.Config.UseItemsInit.Value.IsEnabled(forece.Name))
                //{
                 //   if (Vector3Extensions.Distance(UnitExtensions.InFront(this.MyHero, 800), posForHitChance) < rangeCallAbility)
                  //  {
                   //     forece.UseAbility(this.MyHero);
                    //    await Task.Delay(10, token);
                    //}
//                    else
 //                   {
  //                      var posForTurn = this.MyHero.Position.Extend(posForHitChance, 70);
//
  //                      this.Orbwalker.Move(posForTurn);
//
  //                      await Task.Delay((int)(MyHero.GetTurnTime(posForHitChance) * 1000.0 + Game.Ping), token);
    //                }
      //          }
        //        else
          //      {
            //        this.Orbwalker.Move(posForHitChance);
              //  }
            //}
            //else
            //{
             //   this.Orbwalker.OrbwalkTo(target);
           // }

            //await Kill(token);

           // await UseItems(target, token);

           // await Task.Delay(50, token);
       // }
	
      //  public async Task Kill(CancellationToken token)
      //  {
      //      var enemies = EntityManager<Hero>.Entities
      //          .Where(x => this.MyHero.Team != x.Team && x.IsValid && !x.IsIllusion && x.IsAlive)
     //           .OrderBy(e => e.Distance2D(MyHero))
     //           .ToList();

     //       if (enemies == null)
     //           return;
//
   //         var threshold = this.FistAbility.GetAbilityData("kill_threshold");

    //        foreach (var enemy in enemies)
    //        {
    //            if (enemy.Health + (enemy.HealthRegeneration / 2) <= threshold)
    //            {
      //              if (!UnitExtensions.IsSilenced(this.MyHero) && this.FistAbility.CanBeCasted(enemy) && this.FistAbility.CanHit(enemy))
   //                 {
   //                     this.FistAbility.UseAbility(enemy);

    //                    // Can be made shorter than FindCastPoint
    //                    await Task.Delay(50, token);
    //                }
//		}
//	    }
//	}
//        public async Task UseItems(Unit target, CancellationToken token)
//        {
 //           var called = EntityManager<Hero>.Entities
//                .Where(x => this.MyHero.Team != x.Team && x.IsValid && !x.IsIllusion && x.IsAlive)
//                .ToList();
////
     //       if (called.Any())
    //        {
    //            var bkb = this.MyHero.GetItemById(AbilityId.item_black_king_bar);
    //            if (bkb != null && bkb.CanBeCasted() && this.Config.UseItems.Value.IsEnabled(bkb.Name))
     //           {
    //                bkb.UseAbility();
     //               await Task.Delay(10, token);
    //            }

//		var bloodthorn = this.MyHero.GetItemById(AbilityId.item_bloodthorn);
   //             if (bloodthorn != null && bloodthorn.CanBeCasted() && this.Config.UseItems.Value.IsEnabled(bloodthorn.Name))
   //             {
    //                bloodthorn.UseAbility(target);
    //                await Task.Delay(10, token);
     //           }

	//	var orchid = this.MyHero.GetItemById(AbilityId.item_orchid);
        //        if (orchid != null && orchid.CanBeCasted() && this.Config.UseItems.Value.IsEnabled(orchid.Name))
      //          {
       //             orchid.UseAbility(target);
       //             await Task.Delay(10, token);
       //         }

       //         var veil = this.MyHero.GetItemById(AbilityId.item_veil_of_discord);
     //           if (veil != null && veil.CanBeCasted() && !UnitExtensions.IsMagicImmune(target) && this.Config.UseItems.Value.IsEnabled(veil.Name))
    //            {
   //                 veil.UseAbility(target.Position);
   //                await Task.Delay(10, token);
     //           }

	//	var ethereal = this.MyHero.GetItemById(AbilityId.item_ethereal_blade);
     //           if (ethereal != null && ethereal.CanBeCasted() && !UnitExtensions.IsMagicImmune(target) && this.Config.UseItems.Value.IsEnabled(ethereal.Name))
     //           {
    //                ethereal.UseAbility(target);
   //                 await Task.Delay(10, token);
    //            }

       //         var bladeMail = this.MyHero.GetItemById(AbilityId.item_blade_mail);
       //         if (bladeMail != null && bladeMail.CanBeCasted() && this.Config.UseItems.Value.IsEnabled(bladeMail.Name))
      //          {
      //              bladeMail.UseAbility();
      //              await Task.Delay(10, token);
     //           }

       //         var lotus = this.MyHero.GetItemById(AbilityId.item_lotus_orb);
      //          if (lotus != null && lotus.CanBeCasted() && this.Config.UseItems.Value.IsEnabled(lotus.Name))
     //           {
    //               lotus.UseAbility(this.MyHero);
     //               await Task.Delay(10, token);
     //           }

         //       var mjollnir = this.MyHero.GetItemById(AbilityId.item_mjollnir);
        //        if (mjollnir != null && mjollnir.CanBeCasted() && this.Config.UseItems.Value.IsEnabled(mjollnir.Name))
      //          {
      //              mjollnir.UseAbility(this.MyHero);
      //              await Task.Delay(10, token);
      //          }

       //         var shiva = this.MyHero.GetItemById(AbilityId.item_shivas_guard);
      //          if (shiva != null && shiva.CanBeCasted() && this.Config.UseItems.Value.IsEnabled(shiva.Name))
      //          {
     //               shiva.UseAbility();
      //              await Task.Delay(10, token);
      //          }

	//	var atos = this.MyHero.GetItemById(AbilityId.item_rod_of_atos);
       //         if (atos != null && atos.CanBeCasted() && !UnitExtensions.IsMagicImmune(target) && !UnitExtensions.IsRooted(target) && this.Config.UseItems.Value.IsEnabled("item_rod_of_atos"))
	//	{
       //             atos.UseAbility(target);
       //            await Task.Delay(10, token);
       //         }

        //        var dagon = MyHero.GetDagon();
        //        if (dagon != null && dagon.CanBeCasted() && !UnitExtensions.IsMagicImmune(target) && this.Config.UseItems.Value.IsEnabled("item_dagon"))
        //       {
       //             dagon.UseAbility(target);
     //              await Task.Delay(10, token);
  //              }
////            }
 //       }
 //   }
//}

