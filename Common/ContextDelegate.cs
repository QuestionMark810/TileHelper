namespace TileHelper.Common;

/// <summary> Allows additional control over logic used in <see cref="ContextDelegate"/>. </summary>
public enum Context
{
    /// <summary> Used before loading particular data. </summary>
    Before,
    /// <summary> Used after loading particular data. </summary>
    After
}
/// <summary> Provides an additional context parameter for ease of use. </summary>

public delegate void ContextDelegate(Context context);