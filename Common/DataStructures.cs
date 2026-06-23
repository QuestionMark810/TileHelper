using System.Collections.Generic;
using Terraria.ID;
using TileHelper.Content.Tiles;

namespace TileHelper.Common;

/// <summary> Provides strictly contextual data collections for easy access. </summary>
public static class DataStructures
{
    /// <summary> Recipe delegate for furniture types. </summary>
    public delegate void SignatureRecipeDelegate(ModItem modItem, int ingredient);

    /// <summary> Provides research value conditions for <paramref name="modItem"/>. Return -1 if <paramref name="modItem"/> meets no conditions. </summary>
    public delegate int ResearchCondition(ModItem modItem);

    /// <summary> Stores custom <see cref="ModItem"/> conditions for <see cref="GetResearchCount"/>.<para/>
    /// For example, the following code would give all ModItems with a damage value greater than zero a research count of one:<code>ResearchConditions.Add(static (modItem) => (modItem.Item.damage > 0) ? 1 : null);</code></summary>
    public static readonly List<ResearchCondition> ResearchConditions = [];

    /// <summary> Whether furniture tile types use dusts by default. </summary>
    public static readonly Dictionary<string, bool> HasDust = [];

    /// <summary> Corresponding recipes of furniture tiles. </summary>
    public static readonly Dictionary<string, SignatureRecipeDelegate> Recipes;

    /// <summary> Corresponding coin values of furniture items. </summary>
    public static readonly Dictionary<string, int> CoinValues = [];

    static DataStructures()
    {
        foreach (var type in Autoloader.FurnitureTypes)
            HasDust.Add(type.Name, false);

        HasDust[nameof(ChestTile)] = true;

        Recipes = new()
        {
            { nameof(BarrelTile), static (modItem, ingredient) => modItem.CreateRecipe().AddIngredient(ingredient, 9).AddRecipeGroup(RecipeGroupID.IronBar).AddTile(TileID.Sawmill).Register() },
            { nameof(BathtubTile), static (modItem, ingredient) => modItem.CreateRecipe().AddIngredient(ingredient, 14).AddTile(TileID.Sawmill).Register() },
            { nameof(BedTile), static (modItem, ingredient) => modItem.CreateRecipe().AddIngredient(ingredient, 15).AddIngredient(ItemID.Silk, 5).AddTile(TileID.Sawmill).Register() },
            { nameof(BenchTile), static (modItem, ingredient) => modItem.CreateRecipe().AddIngredient(ingredient, 8).AddTile(TileID.Sawmill).Register() },
            { nameof(BookcaseTile), static (modItem, ingredient) => modItem.CreateRecipe().AddIngredient(ingredient, 20).AddIngredient(ItemID.Book, 10).AddTile(TileID.Sawmill).Register() },
            { nameof(CandelabraTile), static (modItem, ingredient) => modItem.CreateRecipe().AddIngredient(ingredient, 5).AddIngredient(ItemID.Torch, 3).AddTile(TileID.Sawmill).Register() },
            { nameof(CandleTile), static (modItem, ingredient) => modItem.CreateRecipe().AddIngredient(ingredient, 4).AddIngredient(ItemID.Torch).AddTile(TileID.WorkBenches).Register() },
            { nameof(ChairTile), static (modItem, ingredient) => modItem.CreateRecipe().AddIngredient(ingredient, 4).AddTile(TileID.Sawmill).Register() },
            { nameof(ChandelierTile), static (modItem, ingredient) => modItem.CreateRecipe().AddIngredient(ingredient, 4).AddIngredient(ItemID.Torch, 4).AddIngredient(ItemID.Chain).AddTile(TileID.Anvils).Register() },
            { nameof(ChestTile), static (modItem, ingredient) => modItem.CreateRecipe().AddIngredient(ingredient, 8).AddRecipeGroup(RecipeGroupID.IronBar, 2).AddTile(TileID.WorkBenches).Register() },
            { nameof(ClockTile), static (modItem, ingredient) => modItem.CreateRecipe().AddRecipeGroup(RecipeGroupID.IronBar, 3).AddIngredient(ItemID.Glass, 6).AddIngredient(ingredient, 10).AddTile(TileID.Sawmill).Register() },
            { nameof(DoorTile), static (modItem, ingredient) => modItem.CreateRecipe().AddIngredient(ingredient, 6).AddTile(TileID.WorkBenches).Register() },
            { nameof(DresserTile), static (modItem, ingredient) => modItem.CreateRecipe().AddIngredient(ingredient, 16).AddTile(TileID.Sawmill).Register() },
            { nameof(LampTile), static (modItem, ingredient) => modItem.CreateRecipe().AddIngredient(ingredient, 3).AddIngredient(ItemID.Torch).AddTile(TileID.WorkBenches).Register() },
            { nameof(LanternTile), static (modItem, ingredient) => modItem.CreateRecipe().AddIngredient(ingredient, 6).AddIngredient(ItemID.Torch).AddTile(TileID.WorkBenches).Register() },
            { nameof(PianoTile), static (modItem, ingredient) => modItem.CreateRecipe().AddIngredient(ItemID.Bone, 4).AddIngredient(ingredient, 15).AddIngredient(ItemID.Book).AddTile(TileID.Sawmill).Register() },
            { nameof(SinkTile), static (modItem, ingredient) => modItem.CreateRecipe().AddIngredient(ingredient, 6).AddIngredient(ItemID.WaterBucket).AddTile(TileID.WorkBenches).Register() },
            { nameof(SofaTile), static (modItem, ingredient) => modItem.CreateRecipe().AddIngredient(ingredient, 5).AddIngredient(ItemID.Silk, 2).AddTile(TileID.Sawmill).Register() },
            { nameof(TableTile), static (modItem, ingredient) => modItem.CreateRecipe().AddIngredient(ingredient, 8).AddTile(TileID.WorkBenches).Register() },
            { nameof(ToiletTile), static (modItem, ingredient) => modItem.CreateRecipe().AddIngredient(ingredient, 6).AddTile(TileID.Sawmill).Register() },
            { nameof(WorkBenchTile), static (modItem, ingredient) => modItem.CreateRecipe().AddIngredient(ingredient, 10).Register() },
        };

        foreach (var type in Autoloader.FurnitureTypes)
            CoinValues.Add(type.Name, Item.sellPrice(copper: 60));

        CoinValues = new()
        {
            { nameof(BarrelTile), Item.sellPrice(silver: 1) },
            { nameof(BedTile), Item.sellPrice(silver: 4) },
            { nameof(CandelabraTile), Item.sellPrice(silver: 3) },
            { nameof(CandleTile), Item.sellPrice(copper: 30) },
            { nameof(ChairTile), Item.sellPrice(copper: 30) },
            { nameof(ChandelierTile), Item.sellPrice(silver: 6) },
            { nameof(ChestTile), Item.sellPrice(silver: 1) },
            { nameof(DoorTile), Item.sellPrice(copper: 40) },
            { nameof(DresserTile), Item.sellPrice(silver: 1) },
            { nameof(LampTile), Item.sellPrice(silver: 1) },
            { nameof(LanternTile), Item.sellPrice(copper: 30) },
            { nameof(ToiletTile), Item.sellPrice(copper: 30) },
            { nameof(WorkBenchTile), Item.sellPrice(copper: 30) },
        };
    }

    /// <summary> Gets the research count of <paramref name="modItem"/> based on a variety of conditions, defaulting to 1.<para/>
    /// See <see cref="ResearchConditions"/> to provide custom conditions for this method. </summary>
    public static int GetResearchCount(ModItem modItem)
    {
        Item item = modItem.Item;
        int research = -1; //Start handling custom conditions

        ResearchConditions.ForEach(x =>
        {
            if ((research = x.Invoke(modItem)) != -1)
                return;
        });

        if (research != -1)
            return research;

        if (item.maxStack == Item.CommonMaxStack)
        {
            if (item.createTile == -1)
            {
                return 400; //Wall amount
            }
            else
            {
                if (Main.tileFrameImportant[item.createTile])
                    return 1;

                return TileID.Sets.Platforms[item.createTile] ? 200 : 100;
            }
        }

        return 1;
    }
}

[ReinitializeDuringResizeArrays]
public static class Sets
{
    /// <summary> Stores custom tile glowmasks. </summary>
    public static readonly GlowmaskTile.Glowmask[] TileGlowmask = TileID.Sets.Factory.CreateNamedSet("TileGlowmask").RegisterCustomSet<GlowmaskTile.Glowmask>(default);
}