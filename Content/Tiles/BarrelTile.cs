using TileHelper.Common;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;

namespace TileHelper.Content.Tiles;

[FurnitureType]
public class BarrelTile : ChestTile
{
    /// <inheritdoc/>
    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
	{
		Tile tile = Main.tile[i, j];

		if (!TileDrawing.IsVisible(tile))
			return false;

        Rectangle source = new(tile.TileFrameX, tile.TileFrameY % 36, 16, tile.TileFrameY > 0 ? 18 : 16);
		Color color = tile.IsTileFullbright ? Color.White : Lighting.GetColor(i, j);

        spriteBatch.Draw(Helpers.GetTileTextureValue(tile), Helpers.GetTileOffset(i, j), source, color, 0, Vector2.Zero, 1, 0, 0);

		if (Main.InSmartCursorHighlightArea(i, j, out bool actuallySelected))
			spriteBatch.Draw(TextureAssets.HighlightMask[Type].Value, Helpers.GetTileOffset(i, j), source, actuallySelected ? Color.Yellow : Color.Gray, 0, Vector2.Zero, 1, 0, 0);

		return false;
	}
}