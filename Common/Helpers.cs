using TileHelper.Content;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ObjectData;

namespace TileHelper.Common;

/// <summary> Provides a selection of tile related helper methods. </summary>
public static class Helpers
{
    /// <summary> Gets the top-leftmost tile of the multitile at the provided coordinates, based on <see cref="Tile.TileFrameX"/> and <see cref="Tile.TileFrameY"/>. </summary>
    public static (int, int) GetTopLeft(int i, int j)
    {
        Tile tile = Main.tile[i, j];
        int left = i - tile.TileFrameX / 18;
        int top = j - tile.TileFrameY / 18;

        return (left, top);
    }

    /// <summary> Safely gets the <see cref="ModItem"/> associated with the provided <paramref name="blockType"/>. </summary>
    public static bool TryGetBlockItem(ModBlockType blockType, out ModItem modItem) => blockType.Mod.TryFind(blockType.Name + "Item", out modItem);

    /// <summary> Gets the draw position of the tile at the provided coordinates. </summary>
    public static Vector2 GetTileOffset(int i, int j)
    {
        TileObjectData data = TileObjectData.GetTileData(Main.tile[i, j]);
        return new Vector2(i, j) * 16 - Main.screenPosition + (Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange)) + new Vector2(data.DrawXOffset, data.DrawYOffset);
    }

    /// <summary> Conveniently gets <see cref="ModTexturedType.Texture"/> + "_Glow" alongside the provided <paramref name="color"/>, white if null. </summary>
    public static GlowmaskTile.Glowmask RequestGlowmask(ModTexturedType modType, GlowmaskTile.VertexColor color = null) => new(ModContent.Request<Texture2D>(modType.Texture + "_Glow"), color);

    /// <summary> Attempts to get the glowmask from the tile at the provided coordinates. </summary>
    public static bool TryGetGlowmask(int i, int j, out Texture2D texture, out Color color)
    {
        if (Sets.TileGlowmask[Main.tile[i, j].TileType] is GlowmaskTile.Glowmask glowmask && glowmask != default)
        {
            texture = glowmask.Texture.Value;
            color = glowmask.Color?.Invoke(i, j) ?? Color.White;

            return true;
        }

        texture = default;
        color = default;

        return false;
    }

    /// <summary> Gets the current texture to be used by <paramref name="tile"/>, with respect to <see cref="Tile.TileColor"/>. </summary>
    public static Texture2D GetTileTextureValue(Tile tile)
    {
        Texture2D texture = TextureAssets.Tile[tile.TileType].Value;

        if (tile.TileColor != PaintID.None)
        {
            Texture2D painted = Main.instance.TilePaintSystem.TryGetTileAndRequestIfNotReady(tile.TileType, 0, tile.TileColor);
            texture = painted ?? texture;
        }

        return texture;
    }

    /// <summary> Creates a new <see cref="AutoloadedPlaceable"/> for <paramref name="blockType"/>. </summary>
    public static bool CreateBlockItem(ModBlockType blockType) => blockType.Mod.AddContent(new AutoloadedPlaceable(blockType));
}