using TileHelper.Common;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ObjectData;

namespace TileHelper.Content.Tiles;

[FurnitureType]
public class BedTile : FurnitureTile
{
	public static bool HoveringOverBottomSide(int i, int j)
	{
		var tile = Framing.GetTileSafely(i, j);
		int wrapX = TileObjectData.GetTileData(tile).CoordinateFullWidth;
		short frameX = tile.TileFrameX;

		return frameX < wrapX ? frameX <= wrapX / 3 : frameX % wrapX > wrapX / 3;
	}

    /// <inheritdoc/>
    public override void SetStaticDefaults()
	{
		Main.tileSolid[Type] = false;
		Main.tileNoAttach[Type] = true;
		Main.tileLavaDeath[Type] = false;
		Main.tileFrameImportant[Type] = true;
		Main.tileLighted[Type] = true;

		TileID.Sets.CanBeSleptIn[Type] = true;
		TileID.Sets.InteractibleByNPCs[Type] = true;
		TileID.Sets.HasOutlines[Type] = true;
		TileID.Sets.IsValidSpawnPoint[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style4x2);
		TileObjectData.newTile.Origin = new Point16(1, 1);
		TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidWithTop | AnchorType.SolidTile | AnchorType.Table, TileObjectData.newTile.Width, 0);
		TileObjectData.newTile.CoordinateHeights = [16, 18];
		TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
		TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
		TileObjectData.addAlternate(1);
		TileObjectData.addTile(Type);

		AddToArray(ref TileID.Sets.RoomNeeds.CountsAsChair);
		AddMapEntry(MapColor, Language.GetText("ItemName.Bed"));

		AdjTiles = [TileID.Beds];

        base.SetStaticDefaults();
	}

    /// <inheritdoc/>
    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;

    /// <inheritdoc/>
    public override void ModifySmartInteractCoords(ref int width, ref int height, ref int frameWidth, ref int frameHeight, ref int extraY)
	{
		width = 2;
		height = 2;
		extraY = 0;
	}

    /// <inheritdoc/>
    public override bool RightClick(int i, int j)
	{
		Player player = Main.LocalPlayer;
		Tile tile = Main.tile[i, j];
		int spawnX = i - tile.TileFrameX / 18;
		int spawnY = j + 2;
		spawnX += tile.TileFrameX >= 54 ? 5 : 2;

		if (tile.TileFrameY % 38 != 0)
			spawnY--;

		if (HoveringOverBottomSide(i, j))
		{
			player.FindSpawn();

			if (player.SpawnX == spawnX && player.SpawnY == spawnY)
			{
				player.RemoveSpawn();
				Main.NewText(Language.GetTextValue("Game.SpawnPointRemoved"), 255, 240, 20);
			}
			else if (Player.CheckSpawn(spawnX, spawnY))
			{
				player.ChangeSpawn(spawnX, spawnY);
				Main.NewText(Language.GetTextValue("Game.SpawnPointSet"), 255, 240, 20);
			}
		}
		else
		{
			if (player.IsWithinSnappngRangeToTile(i, j, PlayerSleepingHelper.BedSleepingMaxDistance))
			{
				player.GamepadEnableGrappleCooldown();
				player.sleeping.StartSleeping(player, i, j);
			}
		}

		return true;
	}

    /// <inheritdoc/>
    public override void MouseOver(int i, int j)
	{
		Player player = Main.LocalPlayer;

		if (HoveringOverBottomSide(i, j))
		{
			player.noThrow = 2;
			player.cursorItemIconEnabled = true;
			player.cursorItemIconID = ItemType;
		}
		else if (player.IsWithinSnappngRangeToTile(i, j, PlayerSleepingHelper.BedSleepingMaxDistance))
		{
			player.noThrow = 2;
			player.cursorItemIconEnabled = true;
			player.cursorItemIconID = ItemID.SleepingIcon;
		}
	}
}