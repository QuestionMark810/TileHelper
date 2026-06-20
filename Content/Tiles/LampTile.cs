using TileHelper.Common;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ObjectData;

namespace TileHelper.Content.Tiles;

[FurnitureType]
public class LampTile : FurnitureTile, ILightTile
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

        Sets.TileGlowmask[Type] = Helpers.RequestGlowmask(this);

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.Origin = new Point16(0, 2);
		TileObjectData.newTile.Height = 3;
		TileObjectData.newTile.CoordinateHeights = [16, 16, 18];
		TileObjectData.addTile(Type);

		AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
		AddMapEntry(MapColor, Language.GetText("ItemName.LampPost"));

		AdjTiles = [TileID.Lamps];

        base.SetStaticDefaults();
	}

    /// <inheritdoc/>
    public override void HitWire(int i, int j)
	{
        TileObjectData data = TileObjectData.GetTileData(Type, 0);
		int width = data.CoordinateFullWidth;

		j -= Framing.GetTileSafely(i, j).TileFrameY / 18; //Move to the multitile's top

		for (int h = 0; h < 3; h++)
		{
			var tile = Framing.GetTileSafely(i, j + h);
			tile.TileFrameX += (short)(tile.TileFrameX < width ? width : -width);

			Wiring.SkipWire(i, j + h);
		}

		NetMessage.SendTileSquare(-1, i, j, data.Width, data.Height);
	}

    /// <inheritdoc/>
    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
	{
		Tile tile = Main.tile[i, j];

		if (tile.TileFrameX < 18 && tile.TileFrameY == 0)
			(r, g, b) = (Light.X, Light.Y, Light.Z);
	}

    /// <inheritdoc/>
    public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
	{
		Tile tile = Main.tile[i, j];

        if (!TileDrawing.IsVisible(tile) || !Helpers.TryGetGlowmask(i, j, out Texture2D texture, out Color color))
            return;

        TileObjectData data = TileObjectData.GetTileData(tile);
		int height = data.CoordinateHeights[tile.TileFrameY / data.CoordinateFullHeight];
		Rectangle source = new(tile.TileFrameX, tile.TileFrameY, 16, height);

		if (DistortGlow)
		{
			ulong randSeed = Main.TileFrameSeed ^ (ulong)((long)j << 32 | (uint)i);
			for (int c = 0; c < 7; c++) //Draw our glowmask with a randomized position
			{
				float shakeX = Utils.RandomInt(ref randSeed, -10, 11) * 0.15f;
				float shakeY = Utils.RandomInt(ref randSeed, -10, 1) * 0.35f;
				var offset = new Vector2(shakeX, shakeY);

				spriteBatch.Draw(texture, Helpers.GetTileOffset(i, j) + offset, source, (color with { A = 0 }) * 0.4f, 0, Vector2.Zero, 1, SpriteEffects.None, 0f);
			}
		}
		else
		{
			spriteBatch.Draw(texture, Helpers.GetTileOffset(i, j), source, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
		}
	}
}
