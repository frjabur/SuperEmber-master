﻿namespace SuperEmber
{
    using System;
    using System.Collections.Generic;

    using Ensage.Common.Menu;
    using Ensage.SDK.Menu;

    public class Config
    {
        public MenuFactory Menu { get; }

        public MenuItem<bool> Enabled { get; }
        public MenuItem<KeyBind> Key { get; }
        public MenuItem<AbilityToggler> UseItemsInit { get; }
        public MenuItem<AbilityToggler> UseItems { get; }

        public Dictionary<string, bool> Items = new Dictionary<string, bool>
        {
            { "item_blade_mail", true },
            { "item_lotus_orb", true },
            { "item_mjollnir", true },
	    { "item_black_king_bar", false },
            { "item_shivas_guard", true },
	    { "item_bloodthorn", true },
	    { "item_rod_of_atos", true },
            { "item_orchid", true },
	    { "item_veil_of_discord", true },
	    { "item_ethereal_blade", true },
            { "item_dagon", true }
        };

        public Dictionary<string, bool> ItemsInitiation = new Dictionary<string, bool>
        {
            { "item_blink", true },
            { "item_force_staff", true }
        };

        public Config()
        {
            this.Menu = MenuFactory.Create("SuperEmber!");
            this.Enabled = this.Menu.Item("Enabled", true);
            this.Key = this.Menu.Item("Combo Key", new KeyBind(32));
            this.UseItemsInit = this.Menu.Item("Items For Initiation", new AbilityToggler(ItemsInitiation));
            this.UseItems = this.Menu.Item("Use Items In Call", new AbilityToggler(Items));
        }

        public void Dispose()
        {
            this.Menu?.Dispose();
        }
    }
}
