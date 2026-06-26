using TileHelper.Common;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ObjectData;

namespace TileHelper.Content.Tiles;

[FurnitureType]
public class CandleTile : FurnitureTile, ILightTile
{
    /// <inheritdoc/>
    public Vector3 Light { get; set; }

	/// <inheritdoc/>
	public bool DistortGlow { get; set; } = true;

    /// <inheritdoc/>
    public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;
		Main.tileNoAttach[Type] = true;
		Main.tileLighted[Type] = true;
		Main.tileLavaDeath[Type] = true;

        TileHelperSets.TileGlowmask[Type] = Helpers.RequestGlowmask(this);

        TileObjectData.newTile.CopyFrom(TileObjectData.StyleOnTable1x1);
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.addTile(Type);

		AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
		AddMapEntry(MapColor, Language.GetText("ItemName.Candle"));

		AdjTiles = [TileID.Candles];

        base.SetStaticDefaults();
	}

    /// <inheritdoc/>
    public override bool RightClick(int i, int j)
	{
		HitWire(i, j);
		return true;
	}

    /// <inheritdoc/>
    public override void MouseOver(int i, int j)
	{
		Player Player = Main.LocalPlayer;
		Player.noThrow = 2;
		Player.cursorItemIconEnabled = true;
		Player.cursorItemIconID = ItemType;
	}

    /// <inheritdoc/>
    public override void HitWire(int i, int j)
	{
		var tile = Framing.GetTileSafely(i, j);
		tile.TileFrameX = (short)(tile.TileFrameX == 0 ? 18 : 0);

		NetMessage.SendTileSquare(-1, i, j);
	}

    /// <inheritdoc/>
    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
	{
		Tile tile = Main.tile[i, j];

		if (tile.TileFrameX == 0 && tile.TileFrameY == 0)
			(r, g, b) = (Light.X, Light.Y, Light.Z);
	}

    /// <inheritdoc/>
    public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
	{
		Tile tile = Main.tile[i, j];

		if (!TileDrawing.IsVisible(tile) || !Helpers.TryGetGlowmask(i, j, out Texture2D texture, out Color color))
			return;

        TileObjectData data = TileObjectData.GetTileData(tile);
		int coordHeight = tile.TileFrameY / data.CoordinateFullHeight;
		int height = coordHeight < data.CoordinateHeights.Length ? data.CoordinateHeights[coordHeight] : 1;
		Rectangle source = new(tile.TileFrameX, tile.TileFrameY, 16, height);

		if (DistortGlow)
		{
			ulong randSeed = Main.TileFrameSeed ^ (ulong)((long)j << 32 | (uint)i);
			for (int c = 0; c < 7; c++) //Draw our glowmask with a randomized position
			{
				float shakeX = Utils.RandomInt(ref randSeed, -10, 11) * 0.15f;
				float shakeY = Utils.RandomInt(ref randSeed, -10, 1) * 0.35f;
				Vector2 offset = new(shakeX, shakeY);

				spriteBatch.Draw(texture, Helpers.GetTileOffset(i, j) + offset, source, (color with { A = 0 }) * 0.4f, 0, Vector2.Zero, 1, SpriteEffects.None, 0f);
			}
		}
		else
		{
			spriteBatch.Draw(texture, Helpers.GetTileOffset(i, j), source, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
		}
	}
}