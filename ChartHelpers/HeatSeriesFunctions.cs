using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Runtime;

namespace ChartHelpers
{
    public class HeatSeriesFunctions
    {
        private HeatSeriesFunctions() { }

        [IsVisibleInDynamoLibrary(false)]
        public static object[] GetNodeInput(List<string> xLabels, List<string> yLabels, List<List<double>> values, List<DSCore.Color> colors)
        {
            // TODO - just pass input data unmodified instead?
            if (xLabels.Count * yLabels.Count != values.Count)
            {
                return null;
            }

            var output = new object[4];
            output[0] = xLabels;
            output[1] = yLabels;
            output[2] = values;
            output[3] = colors;

            return output;
        }
    }
}