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
public class ToiletTile : FurnitureTile
{
    /// <inheritdoc/>
    public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;
		Main.tileNoAttach[Type] = true;
		Main.tileLavaDeath[Type] = true;

		TileID.Sets.HasOutlines[Type] = true;
		TileID.Sets.CanBeSatOnForNPCs[Type] = true;
		TileID.Sets.CanBeSatOnForPlayers[Type] = true;
		TileID.Sets.DisableSmartCursor[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.Origin = new Point16(0, 1);
		TileObjectData.newTile.CoordinateHeights = [16, 18];
		TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;
		TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
		TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
		TileObjectData.addAlternate(1);
		TileObjectData.addTile(Type);

		AddToArray(ref TileID.Sets.RoomNeeds.CountsAsChair);
		AddMapEntry(MapColor, Language.GetText("MapObject.Toilet"));

		AdjTiles = [TileID.Toilets];

        base.SetStaticDefaults();
	}

    /// <inheritdoc/>
    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => settings.player.IsWithinSnappngRangeToTile(i, j, PlayerSittingHelper.ChairSittingMaxDistance);

    /// <inheritdoc/>
    public override bool RightClick(int i, int j)
	{
		Player player = Main.LocalPlayer;
		if (player.IsWithinSnappngRangeToTile(i, j, PlayerSittingHelper.ChairSittingMaxDistance))
		{
			player.GamepadEnableGrappleCooldown();
			player.sitting.SitDown(player, i, j);
			return true;
		}

		return false;
	}

    /// <inheritdoc/>
    public override void MouseOver(int i, int j)
	{
		Player player = Main.LocalPlayer;
		if (player.IsWithinSnappngRangeToTile(i, j, PlayerSittingHelper.ChairSittingMaxDistance))
		{
			player.noThrow = 2;
			player.cursorItemIconID = ItemType;
			player.cursorItemIconEnabled = true;

			if (Framing.GetTileSafely(i, j).TileFrameX / 18 < 1)
				player.cursorItemIconReversed = true;
		}
	}

    /// <inheritdoc/>
    public override void ModifySittingTargetInfo(int i, int j, ref TileRestingInfo info)
	{
		info.TargetDirection = Framing.GetTileSafely(i, j).TileFrameX == 0 ? -1 : 1;
		info.ExtraInfo.IsAToilet = true;
	}

    /// <inheritdoc/>
    public override void HitWire(int i, int j)
	{
		Tile tile = Main.tile[i, j];
		j -= tile.TileFrameY % (18 * 2) / 18;

		Wiring.SkipWire(i, j);
		Wiring.SkipWire(i, j + 1);

		if (Wiring.CheckMech(i, j, 60))
		{
			Vector2 position = new Vector2(i, j) * 16 + new Vector2(8, 12);
			Projectile.NewProjectile(Wiring.GetProjectileSource(i, j), position, Vector2.Zero, ProjectileID.ToiletEffect, 0, 0, Main.myPlayer);
		}
	}
}