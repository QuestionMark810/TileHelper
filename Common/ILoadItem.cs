using System;
using System.Collections.Generic;
using Terraria.ID;

namespace TileHelper.Common;

/// <summary> Enables item autoloading for the attached <see cref="ModTile"/> or <see cref="ModWall"/>. </summary>
public interface ILoadItem
{
    internal class LoadItemSystem : ModSystem
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
                        if (modTile is ILoadItem)
                            result.Add(modTile);
                    }

                    foreach (ModWall modWall in Mod.GetContent<ModWall>())
                    {
                        if (modWall is ILoadItem)
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
            foreach (ModBlockType blockType in InterfaceTypes)
            {
                if (blockType is ModTile modTile)
                    Helpers.CreateTileItem(modTile);
                else if (blockType is ModWall modWall)
                    Helpers.CreateWallItem(modWall);
            }

            PostAutoloadItems?.Invoke();
            PostAutoloadItems = null;
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
                    (blockType as ILoadItem).AddItemRecipes(modItem);
            }
        }
    }

    internal class LoadItemDefaults : GlobalItem
    {
        public override void SetDefaults(Item entity)
        {
            if (entity.ModItem != null)
            {
                int createType = (entity.createTile == -1) ? entity.createWall : entity.createTile;

                if (TileLoader.GetTile(createType) is ModTile modTile && modTile is ILoadItem iCreateItem)
                    iCreateItem.SetItemDefaults(entity.ModItem);
            }
        }
    }

    /// <summary> Raised after placeable items are autoloaded. </summary>
    public static event Action PostAutoloadItems;

    /// <summary> Returns the autoloaded <see cref="ModItem"/> associated with this instance. </summary>
    public int ItemType => Mod.Find<ModItem>(Name + "Item").Type;

    /// <summary> The mod this instance belongs to. </summary>
    public Mod Mod { get; }

    /// <summary> The name of this instance. </summary>
    public string Name { get; }

    /// <summary> Sets the static defaults for <paramref name="modItem"/>. </summary>
    public void SetItemStaticDefaults(ModItem modItem) { }

    /// <summary> Sets the defaults for <paramref name="modItem"/>. </summary>
    public void SetItemDefaults(ModItem modItem) { }
    /// <summary> Sets the recipe for <paramref name="modItem"/>. </summary>
    public void AddItemRecipes(ModItem modItem) { }
}