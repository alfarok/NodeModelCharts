using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Runtime;

namespace ChartHelpers
{
    public class PieChartFunctions
    {
        [IsVisibleInDynamoLibrary(false)]
        public static string GetNodeInput(string input)
        {
            return input;
        }
    }
}
