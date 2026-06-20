using TileHelper.Common;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ObjectData;

namespace TileHelper.Content.Tiles;

[FurnitureType]
public class DoorTile : FurnitureTile
{
    /// <inheritdoc/>
    public int OpenType => (_openType == -1) ? _openType = Mod.Find<ModTile>(Name + "Open").Type : _openType;

    private int _openType = -1;

    /// <inheritdoc/>
    public override void Load() => Mod.AddContent(new AutoloadedDoorOpen(this));

    /// <inheritdoc/>
    public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;
		Main.tileBlockLight[Type] = true;
		Main.tileSolid[Type] = true;
		Main.tileNoAttach[Type] = true;
		Main.tileLavaDeath[Type] = true;

		TileID.Sets.NotReallySolid[Type] = true;
		TileID.Sets.DrawsWalls[Type] = true;
		TileID.Sets.HasOutlines[Type] = true;
		TileID.Sets.DisableSmartCursor[Type] = true;
		TileID.Sets.OpenDoorID[Type] = OpenType;

		TileObjectData.newTile.Width = 1;
		TileObjectData.newTile.Height = 3;
		TileObjectData.newTile.Origin = new Point16(0, 0);
		TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
		TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
		TileObjectData.newTile.UsesCustomCanPlace = true;
		TileObjectData.newTile.LavaDeath = true;
		TileObjectData.newTile.CoordinateHeights = [16, 16, 16];
		TileObjectData.newTile.CoordinateWidth = 16;
		TileObjectData.newTile.CoordinatePadding = 2;

		TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
		TileObjectData.newAlternate.Origin = new Point16(0, 1);
		TileObjectData.addAlternate(0);

		TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
		TileObjectData.newAlternate.Origin = new Point16(0, 2);
		TileObjectData.addAlternate(0);
		TileObjectData.addTile(Type);

		AddToArray(ref TileID.Sets.RoomNeeds.CountsAsDoor);
		AddMapEntry(MapColor, Language.GetText("MapObject.Door"));

		AdjTiles = [TileID.ClosedDoor];

        base.SetStaticDefaults();
	}

    /// <inheritdoc/>
    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;

    /// <inheritdoc/>
    public override void MouseOver(int i, int j)
	{
		Player player = Main.LocalPlayer;
		player.noThrow = 2;
		player.cursorItemIconEnabled = true;
		player.cursorItemIconID = ItemType;
	}

    /// <inheritdoc/>
    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
	{
		DrawWithFrameOffset(i, j, spriteBatch, new(18 * 4 - Main.tile[i, j].TileFrameX, 0));
		return false;
	}

    /// <inheritdoc/>
    public override bool PreDrawPlacementPreview(int i, int j, SpriteBatch spriteBatch, ref Rectangle frame, ref Vector2 position, ref Color color, bool validPlacement, ref SpriteEffects spriteEffects)
	{
		frame.X += 18 * 4;
		return true;
	}

    /// <inheritdoc/>
    public static void DrawWithFrameOffset(int i, int j, SpriteBatch spriteBatch, Point offset)
	{
        Tile tile = Main.tile[i, j];

        if (!TileDrawing.IsVisible(tile))
            return;

        int frameHeight = GetFrameHeight(tile);
        Color color = tile.IsTileFullbright ? Color.White : Lighting.GetColor(i, j);
        Rectangle source = new(offset.X + tile.TileFrameX, offset.Y + tile.TileFrameY, 16, frameHeight);

		spriteBatch.Draw(Helpers.GetTileTextureValue(tile), Helpers.GetTileOffset(i, j), source, color, 0, Vector2.Zero, 1, 0, 0);

		if (Main.InSmartCursorHighlightArea(i, j, out bool actuallySelected))
			spriteBatch.Draw(TextureAssets.HighlightMask[tile.TileType].Value, Helpers.GetTileOffset(i, j), source, actuallySelected ? Color.Yellow : Color.Gray, 0, Vector2.Zero, 1, 0, 0);
	}

	private static int GetFrameHeight(Tile tile)
	{
		int result = 16;

		if (TileObjectData.GetTileData(tile) is TileObjectData data)
		{
			int fullHeight = 0;

			for (int c = 0; c < data.CoordinateHeights.Length; c++)
			{
				int height = data.CoordinateHeights[c];

				fullHeight += height;
				result = height;

				if (fullHeight >= tile.TileFrameY)
					break;
			}
		}

		return result;
	}
}

public class AutoloadedDoorOpen(ModTile parentTile) : ModTile
{
	public override string Name => _name;

	public override string Texture => _texture;

	public readonly int ClosedType = parentTile.Type;

	private readonly string _name = parentTile.Name + "Open";
    private readonly string _texture = parentTile.Texture;

    public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;
		Main.tileSolid[Type] = false;
		Main.tileLavaDeath[Type] = true;
		Main.tileNoSunLight[Type] = true;

		TileID.Sets.HousingWalls[Type] = true;
		TileID.Sets.HasOutlines[Type] = true;
		TileID.Sets.DisableSmartCursor[Type] = true;
		TileID.Sets.CloseDoorID[Type] = ClosedType;

		TileObjectData.newTile.Width = 2;
		TileObjectData.newTile.Height = 3;
		TileObjectData.newTile.Origin = new Point16(0, 0);
		TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile, 1, 0);
		TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 0);
		TileObjectData.newTile.UsesCustomCanPlace = true;
		TileObjectData.newTile.LavaDeath = true;
		TileObjectData.newTile.CoordinateHeights = [16, 16, 16];
		TileObjectData.newTile.CoordinateWidth = 16;
		TileObjectData.newTile.CoordinatePadding = 2;
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.StyleMultiplier = 2;
		TileObjectData.newTile.StyleWrapLimit = 2;
		TileObjectData.newTile.Direction = TileObjectDirection.PlaceRight;

		TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
		TileObjectData.newAlternate.Origin = new Point16(0, 1);
		TileObjectData.addAlternate(0);

		TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
		TileObjectData.newAlternate.Origin = new Point16(0, 2);
		TileObjectData.addAlternate(0);

		TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
		TileObjectData.newAlternate.Origin = new Point16(1, 0);
		TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.SolidTile, 1, 1);
		TileObjectData.newAlternate.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 1);
		TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceLeft;
		TileObjectData.addAlternate(1);

		TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
		TileObjectData.newAlternate.Origin = new Point16(1, 1);
		TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.SolidTile, 1, 1);
		TileObjectData.newAlternate.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 1);
		TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceLeft;
		TileObjectData.addAlternate(1);

		TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
		TileObjectData.newAlternate.Origin = new Point16(1, 2);
		TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.SolidTile, 1, 1);
		TileObjectData.newAlternate.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 1);
		TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceLeft;
		TileObjectData.addAlternate(1);

		TileObjectData.addTile(Type);

		RegisterItemDrop(TileLoader.GetItemDropFromTypeAndStyle(ClosedType));
		AddToArray(ref TileID.Sets.RoomNeeds.CountsAsDoor);
		AddMapEntry(FurnitureTile.MapColor, Language.GetText("MapObject.Door"));

		AdjTiles = [TileID.OpenDoor];
		DustType = -1;
	}

	public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;

	public override void MouseOver(int i, int j)
	{
		Player player = Main.LocalPlayer;
		player.noThrow = 2;
		player.cursorItemIconEnabled = true;
		player.cursorItemIconID = TileLoader.GetItemDropFromTypeAndStyle(ClosedType);
	}
}