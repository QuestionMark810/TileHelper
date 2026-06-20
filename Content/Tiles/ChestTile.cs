using TileHelper.Common;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ObjectData;

namespace TileHelper.Content.Tiles;

[FurnitureType]
public class ChestTile : FurnitureTile
{
	private static readonly Dictionary<int, int> KeyLookup = [];

    /// <inheritdoc/>
    public virtual LocalizedText MapEntry => ItemLoader.GetItem(ItemType)?.DisplayName;

	/// <summary> Registers a key to use on this chest when locked. </summary>
	public void MakeLocked(int keyItemType) => KeyLookup.Add(Type, keyItemType);

    /// <inheritdoc/>
    public override void SetStaticDefaults()
	{
		Main.tileSpelunker[Type] = true;
		Main.tileContainer[Type] = true;
		Main.tileFrameImportant[Type] = true;
		Main.tileNoAttach[Type] = true;
		Main.tileOreFinderPriority[Type] = 500;

		TileID.Sets.HasOutlines[Type] = true;
		TileID.Sets.BasicChest[Type] = true;
		TileID.Sets.DisableSmartCursor[Type] = true;
		TileID.Sets.IsAContainer[Type] = true;

		AddObjectData();
		AddMapEntry(MapColor, MapEntry);

		AdjTiles = [TileID.Containers];

        base.SetStaticDefaults();
	}

	/// <summary> Adds the chest tile object data. </summary>
	public virtual void AddObjectData()
	{
		TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
		TileObjectData.newTile.Origin = new Point16(0, 1);
		TileObjectData.newTile.Height = 2;
		TileObjectData.newTile.CoordinateHeights = [16, 18];
		TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(Chest.FindEmptyChest, -1, 0, true);
		TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(Chest.AfterPlacement_Hook, -1, 0, false);
		TileObjectData.newTile.AnchorInvalidTiles = [127];
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.LavaDeath = false;
		TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
		TileObjectData.addTile(Type);
	}

    /// <inheritdoc/>
    public virtual string MapChestName(string name, int i, int j)
	{
		(i, j) = Helpers.GetTopLeft(i, j);

		int chest = Chest.FindChest(i, j);
		if (chest < 0)
			return Language.GetTextValue("LegacyChestType.0");

		if (Main.chest[chest].name == string.Empty)
			return name;

		return name + ": " + Main.chest[chest].name;
	}

    /// <inheritdoc/>
    public override LocalizedText DefaultContainerName(int frameX, int frameY) => MapEntry;

    /// <inheritdoc/>
    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;

    /// <inheritdoc/>
    public override void KillMultiTile(int i, int j, int frameX, int frameY) => Chest.DestroyChest(i, j);

    /// <inheritdoc/>
    public override bool RightClick(int i, int j)
	{
        (i, j) = Helpers.GetTopLeft(i, j);

        Player player = Main.LocalPlayer;
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

		bool isLocked = IsLockedChest(i, j);
		if (Main.netMode == NetmodeID.MultiplayerClient && !isLocked)
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
		else if (isLocked && KeyLookup.TryGetValue(Type, out int chestKey))
		{
			if (player.ConsumeItem(chestKey))
				Chest.Unlock(i, j);
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

		return true;
	}

    /// <inheritdoc/>
    public override void MouseOver(int i, int j)
	{
        (i, j) = Helpers.GetTopLeft(i, j);

        Player player = Main.LocalPlayer;
		int chest = Chest.FindChest(i, j);
		player.cursorItemIconID = -1;

		if (chest < 0)
		{
			player.cursorItemIconText = Language.GetTextValue("LegacyChestType.0");
		}
		else
		{
			Tile tile = Framing.GetTileSafely(i, j);
			string defaultName = TileLoader.DefaultContainerName(tile.TileType, tile.TileFrameX, tile.TileFrameY);

			player.cursorItemIconText = Main.chest[chest].name.Length > 0 ? Main.chest[chest].name : defaultName;
			if (player.cursorItemIconText == defaultName)
			{
				player.cursorItemIconID = IsLockedChest(i, j) && KeyLookup.TryGetValue(Type, out int key) ? key : ItemType;
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