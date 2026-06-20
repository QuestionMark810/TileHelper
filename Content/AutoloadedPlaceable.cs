using TileHelper.Common;

namespace TileHelper.Content;

public class AutoloadedPlaceable(ModTile modTile, AutoloadedPlaceable.RecipeDelegate recipe = null) : ModItem
{
    /// <summary> Recipe delegate for autoloaded placeables. </summary>
    public delegate void RecipeDelegate(ModItem modItem);

    /// <inheritdoc/>
    public override string Name => _name;
    /// <inheritdoc/>
    public override string Texture => _texture;
    /// <inheritdoc/>
    protected override bool CloneNewInstances => true;

    /// <summary> The tile type this item was created from. </summary>
    public int TileType { get; private set; } = modTile.Type;

    private string _name = modTile.Name + "Item";
    private string _texture = modTile.Texture + "Item";

    /// <inheritdoc/>
    public override ModItem Clone(Item newEntity)
    {
        AutoloadedPlaceable item = base.Clone(newEntity) as AutoloadedPlaceable;

        item.TileType = TileType;
        item._name = _name;
        item._texture = _texture;

        return item;
    }

    /// <inheritdoc/>
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(TileType);

        if (TileLoader.GetTile(TileType) is FurnitureTile furnitureTile && DataStructures.CoinValues.TryGetValue(furnitureTile.FurnitureName, out int value))
            Item.value = value; //Automatically set the item value corresponding to a standard furniture item
    }

    /// <inheritdoc/>
    public override void AddRecipes() => recipe?.Invoke(this);
}