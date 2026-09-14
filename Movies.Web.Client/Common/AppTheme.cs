using MudBlazor;

namespace WebClient.Common
{
    public static class AppTheme
    {
        public static readonly MudTheme Default = new()
        {
            PaletteLight = new PaletteLight
            {
                Primary = "#1B5E20",
                Secondary = "#F9A825",
                AppbarBackground = "#1B5E20",
                AppbarText = "#FFFFFF"
            },
            PaletteDark = new PaletteDark
            {
                Primary = "#66BB6A",
                Secondary = "#FFC107"
            }
        };
    }
}
