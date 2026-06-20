using TileHelper.Common;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ObjectData;

namespace TileHelper.Content.Tiles;

[FurnitureType]
public class ChandelierTile : FurnitureTile, ILightTile
{
	/// <summary> Offsets the anchor and how wide it needs to be. Defaults to (1, 1), meaning the anchor only needs 1 tile in the middle of the 3 tile wide chandelier. </summary>
	public virtual (int width, int count) AnchorDataOffsets => (1, 1);

    /// <inheritdoc/>
    public Vector3 Light { get; set; }

    /// <inheritdoc/>
    public bool DistortGlow { get; set; } = true;

	/// <summary> Used to control physics effects in <see cref="AdjustMultiTileVineParameters"/>. </summary>
    public int? WindCycle { get; set; } = 1;

    /// <inheritdoc/>
    public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;
		Main.tileNoAttach[Type] = true;
		Main.tileLighted[Type] = true;
		Main.tileLavaDeath[Type] = true;

        Sets.TileGlowmask[Type] = Helpers.RequestGlowmask(this);
        TileID.Sets.MultiTileSway[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile, AnchorDataOffsets.width, AnchorDataOffsets.count);
		TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
		TileObjectData.newTile.Origin = new Point16(1, 0);
		TileObjectData.addTile(Type);

		AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
		AddMapEntry(MapColor, Language.GetText("MapObject.Chandelier"));

		AdjTiles = [TileID.Chandeliers];

        base.SetStaticDefaults();
	}

    /// <inheritdoc/>
    public override void AdjustMultiTileVineParameters(int i, int j, ref float? overrideWindCycle, ref float windPushPowerX, ref float windPushPowerY, ref bool dontRotateTopTiles, ref float totalWindMultiplier, ref Texture2D glowTexture, ref Color glowColor)
	{
		overrideWindCycle = WindCycle;
		windPushPowerY = 0;

		if (Helpers.TryGetGlowmask(i, j, out Texture2D texture, out Color color))
		{
            glowTexture = texture;
            glowColor = color;
		}
	}

    /// <inheritdoc/>
    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
	{
		Tile tile = Main.tile[i, j];

		if (TileObjectData.IsTopLeft(tile))
			Main.instance.TilesRenderer.AddSpecialPoint(i, j, TileDrawing.TileCounterType.MultiTileVine);

		return false;
	}

    /// <inheritdoc/>
    public override void HitWire(int i, int j)
	{
		var data = TileObjectData.GetTileData(Type, 0);
		int width = data.CoordinateFullWidth;

		(i, j) = Helpers.GetTopLeft(i, j);

		for (int x = i; x < i + 3; x++)
		{
			for (int y = j; y < j + 3; y++)
			{
				var tile = Framing.GetTileSafely(x, y);
				tile.TileFrameX += (short)(tile.TileFrameX < width ? width : -width);

				Wiring.SkipWire(x, y);
			}
		}

		NetMessage.SendTileSquare(-1, i, j, data.Width, data.Height);
	}

    /// <inheritdoc/>
    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
	{
		Tile tile = Main.tile[i, j];

		if (tile.TileFrameX == 18 && tile.TileFrameY == 0)
			(r, g, b) = (Light.X, Light.Y, Light.Z);
	}
}