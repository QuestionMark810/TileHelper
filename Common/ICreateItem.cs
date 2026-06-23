using System.Collections.Generic;
using Terraria.ID;

namespace TileHelper.Common;

/// <summary> Enables item autoloading for the attached ModTile. </summary>
public interface ICreateItem
{
    internal class CreateItemSystem : ModSystem
    {
        public ModBlockType[] InterfaceTypes
        {
            get
            {
                if (_interfaceTypes == null) //Cache interface types
                {
                    List<ModBlockType> result = [];

                    foreach (ModTile modTile in Mod.GetContent<ModTile>())
                    {
                        if (modTile is ICreateItem)
                            result.Add(modTile);
                    }

                    foreach (ModWall modWall in Mod.GetContent<ModWall>())
                    {
                        if (modWall is ICreateItem)
                            result.Add(modWall);
                    }

                    _interfaceTypes = result.ToArray();
                }

                return _interfaceTypes;
            }
        }

        private ModBlockType[] _interfaceTypes;

        public override void OnModLoad()
        {
            OnAutoloadItems?.Invoke(Context.Before);

            foreach (ModBlockType blockType in InterfaceTypes)
                Helpers.CreateBlockItem(blockType);

            OnAutoloadItems?.Invoke(Context.After);
            OnAutoloadItems = null;
        }

        public override void SetStaticDefaults()
        {
            foreach (ModBlockType blockType in InterfaceTypes)
            {
                if (TileLoader.GetItemDropFromTypeAndStyle(blockType.Type) == ItemID.None && Helpers.TryGetBlockItem(blockType, out ModItem modItem))
                {
                    //Automatically register the autoloaded item as a drop if the tile has no drops
                    if (blockType is ModWall modWall)
                        modWall.RegisterItemDrop(modItem.Type);
                    else if (blockType is ModTile modTile)
                        modTile.RegisterItemDrop(modItem.Type);
                }
            }
        }

        public override void AddRecipes()
        {
            foreach (ModBlockType blockType in InterfaceTypes)
            {
                if (Helpers.TryGetBlockItem(blockType, out ModItem modItem))
                    (blockType as ICreateItem).AddItemRecipes(modItem);
            }
        }
    }

    internal class CreateItemDefaults : GlobalItem
    {
        public override void SetDefaults(Item entity)
        {
            if (entity.ModItem != null)
            {
                int createType = (entity.createTile == -1) ? entity.createWall : entity.createTile;

                if (TileLoader.GetTile(createType) is ModTile modTile && modTile is ICreateItem iCreateItem)
                    iCreateItem.SetItemDefaults(entity.ModItem);
            }
        }
    }

    /// <summary> Raised before and after placeable items are autoloaded. </summary>
    public static event ContextDelegate OnAutoloadItems;

    /// <summary> Sets the static defaults for <paramref name="modItem"/>. </summary>
    public void SetItemStaticDefaults(ModItem modItem) { }

    /// <summary> Sets the defaults for <paramref name="modItem"/>. </summary>
    public void SetItemDefaults(ModItem modItem) { }
    /// <summary> Sets the recipe for <paramref name="modItem"/>. </summary>
    public void AddItemRecipes(ModItem modItem) { }
}