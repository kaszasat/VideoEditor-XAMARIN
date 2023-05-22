namespace VideoEditor.Model
{
    /// <summary>
    /// ICloseApplication interface, mivel minden platform másképpen valósítja meg az applikációból történő kilépést.
    /// </summary>
    public interface ICloseApplication
    {
#pragma warning disable IDE1006 // Naming Styles
        void closeApplication();
#pragma warning restore IDE1006 // Naming Styles
    }
}
