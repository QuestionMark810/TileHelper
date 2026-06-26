using Mono.Cecil.Cil;
using MonoMod.Cil;
using ReLogic.Content;
using System.Linq;
using Terraria.GameContent.Drawing;
using Terraria.ID;

namespace TileHelper.Common;

public sealed class GlowmaskTile : GlobalTile
{
    public delegate Color CoordinateColor(int i, int j);

    /// <summary> Contains tile glowmask drawing information. </summary>
    public readonly record struct Glowmask(Asset<Texture2D> Texture, CoordinateColor Color);

    /// <inheritdoc/>
    public override void Load() => IL_TileDrawing.GetTileDrawData += InjectGlowmaskData;

    private static void LogError(Mod mod, string title, string message) => mod.Logger.Warn($"IL edit '{title}' failed! " + message);

    private static void InjectGlowmaskData(ILContext il)
    {
        ILCursor c = new(il);

        c.Index = c.Instrs.Count - 1; //Move to the end

        var p_typeCache = c.Method.Parameters.Where(x => x.Name == "typeCache").FirstOrDefault();
        var p_glowTexture = c.Method.Parameters.Where(x => x.Name == "glowTexture").FirstOrDefault();
        var p_glowColor = c.Method.Parameters.Where(x => x.Name == "glowColor").FirstOrDefault();
        var p_glowSourceRect = c.Method.Parameters.Where(x => x.Name == "glowSourceRect").FirstOrDefault();

        if (p_typeCache == default)
        {
            LogError(Autoloader.Mod, "Inject Glowmask Data", "Parameter 'typeCache' not found.");
            return;
        }

        if (p_glowTexture == default)
        {
            LogError(Autoloader.Mod, "Inject Glowmask Data", "Parameter 'glowTexture' not found.");
            return;
        }

        if (p_glowColor == default)
        {
            LogError(Autoloader.Mod, "Inject Glowmask Data", "Parameter 'glowColor' not found.");
            return;
        }

        if (p_glowSourceRect == default)
        {
            LogError(Autoloader.Mod, "Inject Glowmask Data", "Parameter 'glowSourceRect' not found.");
            return;
        }

        c.Emit(OpCodes.Ldarg_S, p_typeCache); //The tile type
        c.EmitLdarg1(); //i
        c.EmitLdarg2(); //j

        c.Emit(OpCodes.Ldarg_S, p_glowTexture); //Glowmask
        c.Emit(OpCodes.Ldarg_S, p_glowColor); //Glow color
        c.Emit(OpCodes.Ldarg_S, p_glowSourceRect); //Glow frame

        c.EmitDelegate(ModifyData);
    }

    //Modify GetTileDrawData to include our own data, which is more versatile than DrawEffects. Useful for when things like wind-affected tiles and vines are drawn using vanilla methods
    private static void ModifyData(int typeCache, int i, int j, ref Texture2D glowTexture, ref Color glowColor, ref Rectangle glowSourceRect)
    {
        if (TileHelperSets.TileGlowmask[typeCache] is Glowmask glowmask && glowmask != default)
        {
            var tile = Main.tile[i, j];

            if (tile.Slope == SlopeType.Solid && !tile.IsHalfBlock) //This method can't draw slopes
            {
                glowTexture = glowmask.Texture.Value;
                glowColor = glowmask.Color?.Invoke(i, j) ?? Color.White;

                int addFrameX = 0;
                int addFrameY = 0;

                TileLoader.SetAnimationFrame(typeCache, i, j, ref addFrameX, ref addFrameY);
                var source = new Rectangle(tile.TileFrameX, tile.TileFrameY + addFrameY, 16, 16);

                glowSourceRect = source;
            }
        }
    }

    /// <inheritdoc/>
    public override void PostDraw(int i, int j, int type, SpriteBatch spriteBatch)
    {
        if (Helpers.TryGetGlowmask(i, j, out Texture2D texture, out Color color))
        {
            Tile tile = Main.tile[i, j];
            if (tile.Slope != SlopeType.Solid || tile.IsHalfBlock) //This method can draw slopes
                DrawSlopedGlowmask(i, j, spriteBatch, texture, color);
        }
    }

    /// <summary> Draws the provided tile as a slope. </summary>
    public static void DrawSlopedGlowmask(int i, int j, SpriteBatch spriteBatch, Texture2D texture, Color color)
    {
        Tile tile = Main.tile[i, j];
        int frameX = tile.TileFrameX;
        int frameY = tile.TileFrameY;

        int width = 16;
        int height = 16;
        Vector2 position = Helpers.GetTileOffset(i, j);

        if (tile.Slope == 0 && !tile.IsHalfBlock || Main.tileSolid[tile.TileType] && Main.tileSolidTop[tile.TileType]) //second one should be for platforms
            spriteBatch.Draw(texture, position, new Rectangle(frameX, frameY, width, height), color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        else if (tile.IsHalfBlock)
            spriteBatch.Draw(texture, new Vector2(position.X, position.Y + 8), new Rectangle(frameX, frameY, width, 8), color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        else
        {
            SlopeType b = tile.Slope;
            Rectangle frame;

            if (b is SlopeType.SlopeDownLeft or SlopeType.SlopeDownRight)
            {
                int length;
                int height2;

                for (int a = 0; a < 8; ++a)
                {
                    if (b == SlopeType.SlopeDownRight)
                    {
                        length = 16 - a * 2 - 2;
                        height2 = 14 - a * 2;
                    }
                    else
                    {
                        length = a * 2;
                        height2 = 14 - length;
                    }

                    frame = new Rectangle(frameX + length, frameY, 2, height2);
                    spriteBatch.Draw(texture, new Vector2(position.X + length, position.Y + a * 2), frame, color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
                }

                frame = new Rectangle(frameX, frameY + 14, 16, 2);
                spriteBatch.Draw(texture, new Vector2(position.X, position.Y + 14), frame, color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
            }
            else
            {
                int length;
                int height2;

                for (int a = 0; a < 8; ++a)
                {
                    if (b == SlopeType.SlopeUpLeft)
                    {
                        length = a * 2;
                        height2 = 16 - length;
                    }
                    else
                    {
                        length = 16 - a * 2 - 2;
                        height2 = 16 - a * 2;
                    }

                    frame = new Rectangle(frameX + length, frameY + 16 - height2, 2, height2);
                    spriteBatch.Draw(texture, new Vector2(position.X + length, position.Y), frame, color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
                }

                frame = new Rectangle(frameX, frameY, 16, 2);
                spriteBatch.Draw(texture, position, frame, color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
            }
        }
    }
}