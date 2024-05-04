using MaterialDesignThemes.Wpf;

namespace NextGen.src.Services
{
    public class ThemeService
    {
        private readonly PaletteHelper _paletteHelper = new PaletteHelper();

        public void ToggleTheme()
        {
            Theme theme = _paletteHelper.GetTheme();
            var baseTheme = theme.GetBaseTheme() == BaseTheme.Dark ? BaseTheme.Light : BaseTheme.Dark;
            theme.SetBaseTheme(baseTheme);
            _paletteHelper.SetTheme(theme);
        }
    }
}
