using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Autodesk.DesignScript.Runtime;
using Dynamo.Controls;
using Dynamo.Graph.Nodes;
using Dynamo.Wpf;
using ProtoCore.AST.AssociativeAST;
using NodeModelCharts.Controls;
using Newtonsoft.Json;
using ChartHelpers;

namespace NodeModelCharts.Nodes
{
    [NodeName("Basic Line")]
    [NodeCategory("NodeModelCharts.Charts")]
    [NodeDescription("Create a new Basic Line Chart.")]
    [InPortTypes("List<string>", "List<List<double>>", "List<string>", "List<color>")]
    [OutPortTypes("Dictionary<Label, Value>")]
    [IsDesignScriptCompatible]
    public class BasicLineChartNodeModel : NodeModel
    {
        #region Properties
        private Random rnd = new Random();

        /// <summary>
        /// A list of Titles for each line to be plotted.
        /// </summary>
        public List<string> Titles { get; set; }

        /// <summary>
        /// List of lists each containing double values to be plotted.
        /// </summary>
        public List<List<double>> Values { get; set; }

        /// <summary>
        /// A list of Labels for the X-Axis.
        /// </summary>
        public List<string> Labels { get; set; }

        /// <summary>
        /// A list of color values, one for each plotted line.
        /// </summary>
        public List<SolidColorBrush> Colors { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Instantiate a new NodeModel instance.
        /// </summary>
        public BasicLineChartNodeModel()
        {
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("titles", "A list of Titles for each line to be plotted")));
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("values", "List of lists each containing double values to be plotted against X-Axis values")));
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("labels", "A list of Labels for the X-Axis")));
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("colors", "basic line chart line color values")));

            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("titles:values", "Dictionary containing title:value key-pairs")));

            RegisterAllPorts();

            PortDisconnected += BasicLineChartNodeModel_PortDisconnected;

            ArgumentLacing = LacingStrategy.Disabled;
        }

        [JsonConstructor]
        /// <summary>
        /// Instantiate a new NodeModel instance.
        /// </summary>
        public BasicLineChartNodeModel(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
            PortDisconnected += BasicLineChartNodeModel_PortDisconnected;
        }
        #endregion

        #region Events
        private void BasicLineChartNodeModel_PortDisconnected(PortModel obj)
        {
            // Clear UI when a input port is disconnected
            Titles = new List<string>();
            Labels = new List<string>();
            Values = new List<List<double>>();
            Colors = new List<SolidColorBrush>();

            RaisePropertyChanged("DataUpdated");
        }
        #endregion

        #region Databridge
        // Use the VMDataBridge to safely retrieve our input values

        /// <summary>
        /// Register the data bridge callback.
        /// </summary>
        protected override void OnBuilt()
        {
            base.OnBuilt();
            VMDataBridge.DataBridge.Instance.RegisterCallback(GUID.ToString(), DataBridgeCallback);
        }

        /// <summary>
        /// Callback method for DataBridge mechanism.
        /// This callback only gets called when 
        ///     - The AST is executed
        ///     - After the BuildOutputAST function is executed 
        ///     - The AST is fully built
        /// </summary>
        /// <param name="data">The data passed through the data bridge.</param>
        private void DataBridgeCallback(object data)
        {
            // Grab input data which always returned as an ArrayList
            var inputs = data as ArrayList;

            // Each of the list inputs are also returned as ArrayLists
            var titles = inputs[0] as ArrayList;
            var values = inputs[1] as ArrayList;
            var labels = inputs[2] as ArrayList;
            var colors = inputs[3] as ArrayList;

            // Only continue if key/values match in length
            if (titles.Count != values.Count || titles.Count < 1)
            {
                return; // TODO - throw exception for warning msg?
            }

            // Clear current chart values
            Titles = new List<string>();
            Values = new List<List<double>>();
            Labels = new List<string>();
            Colors = new List<SolidColorBrush>();

            // If color count doesn't match title count use random colors
            if (colors.Count != titles.Count)
            {
                for (var i = 0; i < titles.Count; i++)
                {
                    var outputValues = new List<double>();

                    foreach (double plotVal in values[i] as List<double>)
                    {
                        outputValues.Add(plotVal);
                    }

                    Titles.Add((string)titles[i]);
                    Values.Add(outputValues);

                    Color randomColor = Color.FromArgb(255, (byte)rnd.Next(256), (byte)rnd.Next(256), (byte)rnd.Next(256));
                    SolidColorBrush brush = new SolidColorBrush(randomColor);
                    brush.Freeze();
                    Colors.Add(brush);
                }

                Labels = labels.Cast<string>().ToList();
            }
            else
            {
                for (var i = 0; i < titles.Count; i++)
                {
                    var outputValues = new List<double>();

                    foreach (var plotVal in values[i] as ArrayList)
                    {
                        outputValues.Add(System.Convert.ToDouble(plotVal));
                    }

                    Titles.Add((string)titles[i]);
                    Values.Add(outputValues);

                    var dynColor = (DSCore.Color)colors[i];
                    var convertedColor = Color.FromArgb(dynColor.Alpha, dynColor.Red, dynColor.Green, dynColor.Blue);
                    SolidColorBrush brush = new SolidColorBrush(convertedColor);
                    brush.Freeze();
                    Colors.Add(brush);
                }

                Labels = labels.Cast<string>().ToList();
            }

            // Notify UI the data has been modified
            RaisePropertyChanged("DataUpdated");
        }
        #endregion

        #region Methods
        /// <summary>
        /// BuildOutputAst is where the outputs of this node are calculated.
        /// This method is used to do the work that a compiler usually does 
        /// by parsing the inputs List inputAstNodes into an abstract syntax tree.
        /// </summary>
        /// <param name="inputAstNodes"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            // WARNING!!!
            // Do not throw an exception during AST creation.

            // If inputs are not connected return null
            if (InPorts[0].IsConnected == false ||
                InPorts[1].IsConnected == false ||
                InPorts[2].IsConnected == false ||
                InPorts[3].IsConnected == false)
            {
                return new[]
                {
                    AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()),
                };
            }

            AssociativeNode inputNode = AstFactory.BuildFunctionCall(
                new Func<List<string>, List<List<double>>, List<string>, List<DSCore.Color>, Dictionary<string, List<double>>>(BasicLineChartFunctions.GetNodeInput),
                new List<AssociativeNode> { inputAstNodes[0], inputAstNodes[1], inputAstNodes[2], inputAstNodes[3] }
            );

            return new[]
            {
                AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), inputNode),
                    AstFactory.BuildAssignment(
                        AstFactory.BuildIdentifier(AstIdentifierBase + "_dummy"),
                        VMDataBridge.DataBridge.GenerateBridgeDataAst(GUID.ToString(), AstFactory.BuildExprList(inputAstNodes)
                    )
                ),
            };
        }
        #endregion
    }

    /// <summary>
    ///     View customizer for CustomNodeModel Node Model.
    /// </summary>
    public class BasicLineChartNodeView : INodeViewCustomization<BasicLineChartNodeModel>
    {
        /// <summary>
        /// At run-time, this method is called during the node 
        /// creation. Add custom UI element to the node view.
        /// </summary>
        /// <param name="model">The NodeModel representing the node's core logic.</param>
        /// <param name="nodeView">The NodeView representing the node in the graph.</param>
        public void CustomizeView(BasicLineChartNodeModel model, NodeView nodeView)
        {
            var basicLineChartControl = new BasicLineChartControl(model);
            nodeView.inputGrid.Children.Add(basicLineChartControl);
        }

        /// <summary>
        /// Here you can do any cleanup you require if you've assigned callbacks for particular 
        /// UI events on your node.
        /// </summary>
        public void Dispose() { }
    }

}
