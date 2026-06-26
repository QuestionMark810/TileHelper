using TileHelper.Common;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ObjectData;

namespace TileHelper.Content.Tiles;

[FurnitureType]
public class LanternTile : FurnitureTile, ILightTile
{
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

        TileHelperSets.TileGlowmask[Type] = Helpers.RequestGlowmask(this);
        TileID.Sets.MultiTileSway[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile, 1, 0);
		TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
		TileObjectData.newTile.Origin = new Point16(0, 0);
		TileObjectData.newTile.CoordinateHeights = [16, 18];

		TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
		TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.Platform, 1, 0);
		TileObjectData.newAlternate.DrawYOffset = -8;
		TileObjectData.addAlternate(0);
		TileObjectData.addTile(Type);

		AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
		AddMapEntry(MapColor, Language.GetText("MapObject.Lantern"));

		AdjTiles = [TileID.HangingLanterns];

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
    public override void GetTileFlameData(int i, int j, ref TileDrawing.TileFlameData tileFlameData)
    {
        if (DistortGlow && Helpers.TryGetGlowmask(i, j, out Texture2D texture, out Color color))
        {
            tileFlameData.flameSeed = Main.TileFrameSeed ^ (ulong)(((long)i << 32) | (uint)j);
            tileFlameData.flameTexture = texture;
            tileFlameData.flameColor = (color * 0.3f) with { A = 0 };
            tileFlameData.flameCount = 7;

            tileFlameData.flameRangeXMin = -10;
            tileFlameData.flameRangeXMax = 11;
            tileFlameData.flameRangeYMin = -10;
            tileFlameData.flameRangeYMax = 1;
            tileFlameData.flameRangeMultX = 0.15f;
            tileFlameData.flameRangeMultY = 0.35f;
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
    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) => offsetY += 2;

    /// <inheritdoc/>
    public override void HitWire(int i, int j)
	{
		var data = TileObjectData.GetTileData(Type, 0);
		int width = data.CoordinateFullWidth;

		j -= Framing.GetTileSafely(i, j).TileFrameY / 18; //Move to the multitile's top

		for (int h = 0; h < 2; h++)
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

		if (tile.TileFrameX < 18 && tile.TileFrameY == 18)
			(r, g, b) = (Light.X, Light.Y, Light.Z);
	}
}