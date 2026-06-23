using System;

namespace TileHelper.Common;

/// <summary> Considers the annotated type a template furniture type for autoloading. </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class FurnitureTypeAttribute : Attribute;

/// <summary> Applied to tile types which emit light and use a glowmask by extension. </summary>
public interface ILightTile
{
    /// <summary> The light level to be emitted by this tile. </summary>
    public Vector3 Light { get; set; }
    /// <summary> Whether this tile's glowmask should be distorted, like flame visuals. </summary>
    public bool DistortGlow { get; set; }
}

public abstract class FurnitureTile : ModTile
{
    /// <summary> The most common color for furniture tiles on the map. </summary>
    public static readonly Color MapColor = new(190, 140, 110);

    /// <summary> The item type dropped by this tile. </summary>
    public int ItemType => TileLoader.GetItemDropFromTypeAndStyle(Type);

    /// <summary> The name of this furniture tile's type, corresponding to a class annoted by <see cref="FurnitureTypeAttribute"/> For example, 
    /// <code>nameof(BathtubTile)</code></summary>
    public string FurnitureName
    {
        get
        {
            if (_furnitureName == null)
            {
                Type currentType = GetType();

                while (currentType == null || Attribute.GetCustomAttribute(currentType, typeof(FurnitureTypeAttribute), false) == null)
                    currentType = currentType.BaseType; //Crawl down the inheritance chain until a furniture type is found

                _furnitureName = (currentType == null) ? Name : currentType.Name; //Cache
            }

            return _furnitureName;
        }
    }

    /// <inheritdoc/>
    public override string Name => _name ?? base.Name;

    /// <inheritdoc/>
    public override string Texture => _texture ?? base.Texture;

    private string _furnitureName;
    private string _name;
    private string _texture;

    /// <summary> Overrides the texture and name of this ModTile. </summary>
    internal void SetFullName(string fullName)
    {
        string[] split = fullName.Split('.');

        _name = split[split.Length - 1];
        _texture = fullName.Replace('.', '/');
    }

    /// <inheritdoc/>
    public override void SetStaticDefaults()
    {
        if (Helpers.TryGetBlockItem(this, out ModItem modItem))
            RegisterItemDrop(modItem.Type);
    }
}