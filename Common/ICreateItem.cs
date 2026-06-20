using System.Collections.Generic;
using Terraria.ID;

namespace TileHelper.Common;

/// <summary> Enables item autoloading for the attached ModTile. </summary>
public interface ICreateItem
{
    internal class CreateItemSystem : ModSystem
    {
        public ModTile[] InterfaceTypes
        {
            get
            {
                if (_interfaceTypes == null) //Cache interface types
                {
                    List<ModTile> result = [];

                    foreach (ModTile modTile in Mod.GetContent<ModTile>())
                    {
                        if (modTile is ICreateItem)
                            result.Add(modTile);
                    }

                    _interfaceTypes = result.ToArray();
                }

                return _interfaceTypes;
            }
        }

        private ModTile[] _interfaceTypes;

        public override void OnModLoad()
        {
            OnAutoloadItems?.Invoke(Context.Before);

            foreach (ModTile modTile in InterfaceTypes)
                Helpers.CreateTileItem(modTile);

            OnAutoloadItems?.Invoke(Context.After);
            OnAutoloadItems = null;
        }

        public override void SetStaticDefaults()
        {
            foreach (ModTile modTile in InterfaceTypes)
            {
                if (TileLoader.GetItemDropFromTypeAndStyle(modTile.Type) == ItemID.None && Helpers.TryGetTileItem(modTile, out ModItem modItem))
                    modTile.RegisterItemDrop(modItem.Type); //Automatically register the autoloaded item as a drop if the tile has no drops
            }
        }

        public override void AddRecipes()
        {
            foreach (ModTile modTile in InterfaceTypes)
            {
                if (Helpers.TryGetTileItem(modTile, out ModItem modItem))
                    (modTile as ICreateItem).AddItemRecipes(modItem);
            }
        }
    }

    internal class CreateItemDefaults : GlobalItem
    {
        public override void SetDefaults(Item entity)
        {
            if (entity.ModItem != null && TileLoader.GetTile(entity.createTile) is ModTile modTile && modTile is ICreateItem iCreateItem)
                iCreateItem.SetItemDefaults(entity.ModItem);
        }
    }

    /// <summary> Raised before and after placeable items are autoloaded. </summary>
    public static event ContextDelegate OnAutoloadItems;

    /// <summary> Sets the defaults for <paramref name="modItem"/>. </summary>
    public void SetItemDefaults(ModItem modItem) { }
    /// <summary> Sets the recipe for <paramref name="modItem"/>. </summary>
    public void AddItemRecipes(ModItem modItem) { }
}