using Mono.Cecil.Cil;
using MonoMod.Cil;
using ReLogic.Content;
using System.Linq;
using Terraria.GameContent.Drawing;
using Terraria.ID;

namespace TileHelper.Common;

public class GlowmaskTile : ILoadable
{
    public delegate Color VertexColor(int i, int j);

    public readonly record struct Glowmask(Asset<Texture2D> Texture, VertexColor Color);

    /// <inheritdoc/>
    public void Load(Mod mod) => IL_TileDrawing.GetTileDrawData += InjectGlowmaskData;

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
        if (Sets.TileGlowmask[typeCache] is Glowmask glowmask && glowmask != default)
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

    public void Unload() { }
}