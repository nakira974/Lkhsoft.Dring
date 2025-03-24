#region

using Syncfusion.Maui.Toolkit.Charts;

#endregion

namespace Lkhsoft.Dring.Client.Pages.Controls;

public class LegendExt : ChartLegend
{
    protected override double GetMaximumSizeCoefficient()
    {
        return 0.5;
    }
}