using System.Windows.Controls;

namespace NextGen.src.UI.Models
{
    public class DashboardItem
    {
        public string? Name { get; set; } = string.Empty; // Инициализация пустой строкой
        public UserControl? Content { get; set; }
    }


}

