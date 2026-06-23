using TileHelper.Common;

namespace TileHelper.Content;

public class AutoloadedPlaceable : ModItem
{
    /// <summary> Recipe delegate for autoloaded placeables. </summary>
    public delegate void RecipeDelegate(ModItem modItem);

    /// <inheritdoc/>
    public override string Name => _name;
    /// <inheritdoc/>
    public override string Texture => _texture;
    /// <inheritdoc/>
    protected override bool CloneNewInstances => true;

    /// <summary> The block type this item was created from. Can be a <see cref="ModWall"/> or <see cref="ModTile"/>. </summary>
    public ModBlockType BlockType //Avoid storing ModBlockType directly due to type data truncation
    {
        get
        {
            return (_tileType == -1) ? WallLoader.GetWall(_wallType) : TileLoader.GetTile(_tileType);
        }
        set
        {
            if (value is ModTile)
                _tileType = value.Type;
            else if (value is ModWall)
                _wallType = value.Type;
        }
    }

    private int _tileType, _wallType;

    private string _name;
    private string _texture;
    private RecipeDelegate _recipe;

    /// <summary> Creates a new item from <paramref name="blockType"/>. Only <see cref="ModWall"/> and <see cref="ModTile"/> are fully supported. </summary>
    public AutoloadedPlaceable(ModBlockType blockType, RecipeDelegate recipe = null)
    {
        BlockType = blockType;

        _name = blockType.Name + "Item";
        _texture = blockType.Texture + "Item";
        _recipe = recipe;
    }

    /// <inheritdoc/>
    public override ModItem Clone(Item newEntity)
    {
        AutoloadedPlaceable item = base.Clone(newEntity) as AutoloadedPlaceable;

        item.BlockType = BlockType;
        item._name = _name;
        item._texture = _texture;
        item._recipe = _recipe;

        return item;
    }

    /// <inheritdoc/>
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = DataStructures.GetResearchCount(this);

        if (BlockType is ICreateItem iCreateItem)
            iCreateItem.SetItemStaticDefaults(this);
    }

    /// <inheritdoc/>
    public override void SetDefaults()
    {
        if (BlockType is ModTile)
        {
            Item.DefaultToPlaceableTile(BlockType.Type);

            if (TileLoader.GetTile(BlockType.Type) is FurnitureTile furnitureTile && DataStructures.CoinValues.TryGetValue(furnitureTile.FurnitureName, out int value))
                Item.value = value; //Automatically set the item value corresponding to a standard furniture item
        }
        else if (BlockType is ModWall)
        {
            Item.DefaultToPlaceableWall(BlockType.Type);
        }
    }

    /// <inheritdoc/>
    public override void AddRecipes() => _recipe?.Invoke(this);
}