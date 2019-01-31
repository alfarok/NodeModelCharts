using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Runtime;

namespace ChartHelpers
{
    public class XYLineChartFunctions
    {
        private XYLineChartFunctions() { }

        [IsVisibleInDynamoLibrary(false)]
        public static Dictionary<string, List<double>> GetNodeInput(List<string> titles, List<List<double>> xValues, List<List<double>> yValues, List<DSCore.Color> colors)
        {
            // TODO - just pass input data unmodified instead?
            var output = new Dictionary<string, List<double>>();

            if (titles.Count != xValues.Count || xValues.Count != yValues.Count)
            {
                return output;
            }

            for (var i = 0; i < titles.Count; i++)
            {
                output.Add(titles[i], xValues[i]);
            }

            return output;
        }
    }
}