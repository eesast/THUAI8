using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace debug_interface.Models
{
    // 使用 record 更简洁，自动实现相等性比较等
    public record LegendItem(IBrush Color, string Description, IBrush? Stroke = null, double StrokeThickness = 0);
    // 添加了可选的 Stroke 和 StrokeThickness 用于绘制边框 (例如给白色方块)
}
