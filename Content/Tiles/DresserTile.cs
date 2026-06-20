using TileHelper.Common;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ObjectData;

namespace TileHelper.Content.Tiles;

[FurnitureType]
public class DresserTile : FurnitureTile
{
    /// <inheritdoc/>
    public virtual LocalizedText MapEntry => ItemLoader.GetItem(ItemType)?.DisplayName;

    /// <inheritdoc/>
    public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;
		Main.tileSolidTop[Type] = true;
		Main.tileNoAttach[Type] = true;
		Main.tileTable[Type] = true;
		Main.tileContainer[Type] = true;
		Main.tileLavaDeath[Type] = true;

		TileID.Sets.HasOutlines[Type] = true;
		TileID.Sets.DisableSmartCursor[Type] = true;
		TileID.Sets.BasicDresser[Type] = true;
		TileID.Sets.AvoidedByNPCs[Type] = true;
		TileID.Sets.InteractibleByNPCs[Type] = true;
		TileID.Sets.IsAContainer[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
		TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidWithTop | AnchorType.SolidTile, 3, 0);
		TileObjectData.newTile.Origin = new Point16(1, 1);
		TileObjectData.newTile.CoordinateHeights = [16, 16];
		TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(Chest.FindEmptyChest, -1, 0, true);
		TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(Chest.AfterPlacement_Hook, -1, 0, false);
		TileObjectData.addTile(Type);

		AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);
		AddMapEntry(MapColor, MapEntry);

		AdjTiles = [TileID.Dressers];

        base.SetStaticDefaults();
	}

    /// <inheritdoc/>
    public static string MapChestName(string name, int i, int j)
	{
		(i, j) = Helpers.GetTopLeft(i, j);

		int chest = Chest.FindChest(i, j);
		if (chest < 0)
			return Language.GetTextValue("LegacyDresserType.0");

		if (Main.chest[chest].name == string.Empty)
			return name;

		return name + ": " + Main.chest[chest].name;
	}

    /// <inheritdoc/>
    public override LocalizedText DefaultContainerName(int frameX, int frameY) => MapEntry;

    /// <inheritdoc/>
    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;

    /// <inheritdoc/>
    public override void ModifySmartInteractCoords(ref int width, ref int height, ref int frameWidth, ref int frameHeight, ref int extraY)
	{
		width = 3;
		height = 1;
		extraY = 0;
	}

    /// <inheritdoc/>
    public override void KillMultiTile(int i, int j, int frameX, int frameY) => Chest.DestroyChest(i, j);

    /// <inheritdoc/>
    public override bool RightClick(int i, int j)
	{
        (i, j) = Helpers.GetTopLeft(i, j);
        Player player = Main.LocalPlayer;

		if (Main.tile[Player.tileTargetX, Player.tileTargetY].TileFrameY == 0)
		{
			Main.mouseRightRelease = false;

			player.CloseSign();
			player.SetTalkNPC(-1);
			Main.npcChatCornerItem = 0;
			Main.npcChatText = string.Empty;

			if (Main.editChest)
			{
				SoundEngine.PlaySound(SoundID.MenuTick);
				Main.editChest = false;
				Main.npcChatText = string.Empty;
			}

			if (player.editedChestName)
			{
				NetMessage.SendData(MessageID.SyncPlayerChest, -1, -1, NetworkText.FromLiteral(Main.chest[player.chest].name), player.chest, 1f);
				player.editedChestName = false;
			}

			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				if (i == player.chestX && j == player.chestY && player.chest >= 0)
				{
					player.chest = -1;
					Recipe.FindRecipes();
					SoundEngine.PlaySound(SoundID.MenuClose);
				}
				else
				{
					NetMessage.SendData(MessageID.RequestChestOpen, -1, -1, null, i, j);
					Main.stackSplit = 600;
				}
			}
			else
			{
				int chest = Chest.FindChest(i, j);
				if (chest >= 0)
				{
					Main.stackSplit = 600;
					if (chest == player.chest)
					{
						player.chest = -1;
						SoundEngine.PlaySound(SoundID.MenuClose);
					}
					else
					{
						SoundEngine.PlaySound(player.chest < 0 ? SoundID.MenuOpen : SoundID.MenuTick);
						player.OpenChest(i, j, chest);
					}

					Recipe.FindRecipes();
				}
			}
		}
		else
		{
			Main.playerInventory = false;
			player.chest = -1;
			Recipe.FindRecipes();
			player.SetTalkNPC(-1);
			Main.npcChatCornerItem = 0;
			Main.npcChatText = string.Empty;
			Main.interactedDresserTopLeftX = i;
			Main.interactedDresserTopLeftY = j;
			Main.OpenClothesWindow();
		}

		return true;
	}

    /// <inheritdoc/>
    public override void MouseOver(int i, int j)
	{
		int icon = Main.tile[i, j].TileFrameY > 0 ? ItemID.FamiliarShirt : ItemType;

		(i, j) = Helpers.GetTopLeft(i, j);

		Player player = Main.LocalPlayer;
		int chest = Chest.FindChest(i, j);
		player.cursorItemIconID = -1;

		if (chest < 0)
		{
			player.cursorItemIconText = Language.GetTextValue("LegacyDresserType.0");
		}
		else
		{
			Tile tile = Framing.GetTileSafely(i, j);
			string defaultName = TileLoader.DefaultContainerName(tile.TileType, tile.TileFrameX, tile.TileFrameY);

			player.cursorItemIconText = Main.chest[chest].name.Length > 0 ? Main.chest[chest].name : defaultName;
			if (player.cursorItemIconText == defaultName)
			{
				player.cursorItemIconID = icon;
				player.cursorItemIconText = string.Empty;
			}
		}

		player.noThrow = 2;
		player.cursorItemIconEnabled = true;
	}

    /// <inheritdoc/>
    public override void MouseOverFar(int i, int j)
	{
		MouseOver(i, j);

		Player player = Main.LocalPlayer;
		if (player.cursorItemIconText == string.Empty)
		{
			player.cursorItemIconEnabled = false;
			player.cursorItemIconID = ItemID.None;
		}
	}
}