global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
global using Terraria;
global using Terraria.ModLoader;

using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader.Core;
using TileHelper.Common;
using TileHelper.Content;

namespace TileHelper;

/// <summary> </summary>
/// <param name="Arguments"></param>
public readonly record struct ArgumentCollection(HashSet<FurnitureTile> Arguments)
{
    /// <summary> Gets the argument of T. </summary>
    public readonly T Get<T>() where T : FurnitureTile => (T)Arguments.FirstOrDefault(x => x.Name == typeof(T).Name);

    public static ArgumentCollection operator +(ArgumentCollection m, FurnitureTile add)
    {
        m.Arguments.Add(add);
        return m;
    }

    public static ArgumentCollection operator -(ArgumentCollection m, FurnitureTile subtract)
    {
        m.Arguments.RemoveWhere(x => x.Name == subtract.Name);
        return m;
    }
}

/// <summary> The core of TileUtils. </summary>
public class Autoloader : Mod
{
    #region loader utils
    /// <summary> Gets all type arguments registered by this mod. </summary>
    public static ArgumentCollection AllArgs(int dustType, Vector3 light, SoundStyle? hitSound = null, bool distortGlow = true)
    {
        HashSet<FurnitureTile> result = [];
        foreach (Type type in FurnitureTypes)
        {
            FurnitureTile value = (FurnitureTile)Activator.CreateInstance(type);

            if (value.DustType == DustID.Dirt) //If default
                value.DustType = DataStructures.HasDust[type.Name] ? dustType : -1;

            if (value.HitSound == SoundID.Dig) //If default
                value.HitSound = hitSound ?? SoundID.Dig;

            if (value is ILightTile iLightTile)
            {
                if (iLightTile.Light == default) //If default
                    iLightTile.Light = light;

                if (iLightTile.DistortGlow == true) //If default
                    iLightTile.DistortGlow = distortGlow;
            }

            result.Add(value);
        }

        return new(result);
    }

    /// <summary> Gets an empty collection of type arguments. </summary>
    public static ArgumentCollection NoArgs() => new([]);

    /// <summary> The template instances of all <see cref="FurnitureTile"/>s. </summary>
    public static HashSet<Type> FurnitureTypes
    {
        get
        {
            if (_furnitureTypes.Count == 0)
                GetLoadableTypes();

            return _furnitureTypes;
        }
    }

    private static readonly HashSet<Type> _furnitureTypes = [];

    private static void GetLoadableTypes()
    {
        _furnitureTypes.Clear();
        Type[] allTypes = AssemblyManager.GetLoadableTypes(typeof(Autoloader).Assembly).Concat(AssemblyManager.GetLoadableTypes(Mod.GetType().Assembly)).ToArray();

        foreach (var type in allTypes)
        {
            if (type.IsSubclassOf(typeof(FurnitureTile)) && !type.IsAbstract && Attribute.GetCustomAttribute(type, typeof(FurnitureTypeAttribute), false) is FurnitureTypeAttribute)
                _furnitureTypes.Add(type);
        }
    }

    /// <summary> Loads a furniture set using a series of tile templates. </summary>
    /// <param name="fullName"> The name of this furniture set, including the desired namespace.<para/>
    /// For example, a stone-themed set might use the value "ModName.Content.Tiles.Stone", where the keyword "Stone" will be affixed before every furniture type. </param>
    /// <param name="arguments"> The types of furniture tiles to be loaded and with which characteristics.<para/>
    /// Use <see cref="AllArgs"/> and <see cref="NoArgs"/> to assist this process, or create your own <see cref="ArgumentCollection"/>.<br/>
    /// Furniture types can be revoked or added using basic signs, alongside the template tile name.<para/>
    /// For example, you can use the following:
    /// <code>AllArgs(DustID.Marble, new(0.75f, 0.75f, 0.95f), distortGlow: false) - new BarrelTile() + new UniqueFurnitureTile()</code></param>
    /// <param name="ingredient"> The primary ingredient used in crafting. </param>
    /// <param name="loadItems"> Whether this set should autoload an item for each tile. </param>
    public static void LoadFurnitureSet(string fullName, ArgumentCollection arguments, int ingredient = ItemID.None, bool loadItems = true)
    {
        foreach (FurnitureTile modTile in arguments.Arguments)
        {
            //Set the namespace and signature, add the furniture type name (for example, nameof(ChestTile)), then remove "Tile" from the end
            modTile.SetFullName(fullName + modTile.Name.Remove(modTile.Name.Length - "Tile".Length));

            if (Mod.AddContent(modTile) && loadItems)
            {
                AutoloadedPlaceable.RecipeDelegate recipe = (ingredient == ItemID.None) ? null : (moditem) => DataStructures.Recipes[modTile.FurnitureName].Invoke(moditem, ingredient);
                Mod.AddContent(new AutoloadedPlaceable(modTile, recipe)); //Autoload the associated item
            }
        }
    }
    #endregion

    internal static Mod Mod { get; private set; }

    /// <summary> Loads autoloader content for <paramref name="mod"/>. </summary>
    public static void Load(Mod mod)
    {
        Mod = mod;

        mod.AddContent<ICreateItem.CreateItemSystem>();
        mod.AddContent<ICreateItem.CreateItemDefaults>();
        
        GetLoadableTypes();
    }
}