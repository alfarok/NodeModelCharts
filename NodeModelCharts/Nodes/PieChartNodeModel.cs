using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using Autodesk.DesignScript.Runtime;
using Dynamo.Controls;
using Dynamo.Graph.Nodes;
using Dynamo.UI.Commands;
using Dynamo.Wpf;
using ProtoCore.AST.AssociativeAST;
using NodeModelCharts.Controls;
using Newtonsoft.Json;
using ChartHelpers;

namespace NodeModelCharts.Nodes
{
    [NodeName("Pie")]
    [NodeCategory("NodeModelCharts.Charts")]
    [NodeDescription("Create a new Pie Chart.")]
    [InPortTypes("string")]
    [OutPortTypes("string", "string")]
    [IsDesignScriptCompatible]
    public class PieChartNodeModel : NodeModel
    {
        #region Properties
        private string inputValue;

        /// <summary>
        /// A value that will be bound to our
        /// custom UI's slider.
        /// </summary>
        public string InputValue
        {
            get { return inputValue; }
            set
            {
                inputValue = value;
                RaisePropertyChanged("InputValue");
                OnNodeModified();
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Instantiate a new NodeModel instance.
        /// </summary>
        public PieChartNodeModel()
        {
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("input", "an input for the node")));

            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("upper value", "returns a 0-10 double value")));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;

            InputValue = "default";
        }


        [JsonConstructor]
        /// <summary>
        /// Instantiate a new NodeModel instance.
        /// </summary>
        public PieChartNodeModel(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts) { }
        #endregion

        // Use the VMDataBridge to safely retrieve our input values
        #region databridge callback
        /// <summary>
        /// Register the data bridge callback.
        /// </summary>
        protected override void OnBuilt()
        {
            base.OnBuilt();
            VMDataBridge.DataBridge.Instance.RegisterCallback(GUID.ToString(), DataBridgeCallback);
        }

        /// <summary>
        /// Unregister the data bridge callback.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            VMDataBridge.DataBridge.Instance.UnregisterCallback(GUID.ToString());
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
            ArrayList inputs = data as ArrayList;
            string inputText = inputs[0].ToString();

            InputValue = inputText;
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
            // Do not throw an exception during AST creation. If you
            // need to convey a failure of this node, then use
            // AstFactory.BuildNullNode to pass out null.

            // If inputs are not connected return null
            if (InPorts[0].IsConnected == false)
            {
                return new[]
                {
                    AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode())
                };
            }

            // We create a DoubleNode to wrap the value 'sliderValue' that
            // we've stored in a private member.

            AssociativeNode inputNode = AstFactory.BuildFunctionCall(
                new Func<string, string>(PieChartFunctions.GetNodeInput),
                new List<AssociativeNode> { inputAstNodes[0] }
            );

            // A FunctionCallNode can be used to represent the calling of a 
            // function in the AST. The method specified here must live in 
            // a separate assembly and will be loaded by Dynamo at the time 
            // that this AST is built. If the method can't be found, you'll get 
            // a "De-referencing a non-pointer warning."

            /*
            var funcNode = AstFactory.BuildFunctionCall(
                new Func<double, double>(ChartHelpers.PieChartFunctions.MultiplyInputByNumber),
                new List<AssociativeNode>() { stringNode });
            */

            // Using the AstFactory class, we can build AstNode objects
            // that assign doubles, assign function calls, build expression lists, etc.
            return new[]
            {
                // In these assignments, GetAstIdentifierForOutputIndex finds 
                // the unique identifier which represents an output on this node
                // and 'assigns' that variable the expression that you create.

                // For the first node, we'll just pass through the 
                // input provided to this node.
                //AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), inputNode), //AstFactory.BuildDoubleNode(sliderValue)


                AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), inputNode),
                    AstFactory.BuildAssignment(
                        AstFactory.BuildIdentifier(AstIdentifierBase + "_dummy"),
                        VMDataBridge.DataBridge.GenerateBridgeDataAst(GUID.ToString(), AstFactory.BuildExprList(inputAstNodes))),

                // For the second node, we'll build a double node that 
                // passes along our value for multipled value.
                //AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(1), AstFactory.BuildNullNode()) //funcNode
            };
        }
        #endregion
    }

    /// <summary>
    ///     View customizer for CustomNodeModel Node Model.
    /// </summary>
    public class PieChartNodeView : INodeViewCustomization<PieChartNodeModel>
    {
        /// <summary>
        /// At run-time, this method is called during the node 
        /// creation. Here you can create custom UI elements and
        /// add them to the node view, but we recommend designing
        /// your UI declaratively using xaml, and binding it to
        /// properties on this node as the DataContext.
        /// </summary>
        /// <param name="model">The NodeModel representing the node's core logic.</param>
        /// <param name="nodeView">The NodeView representing the node in the graph.</param>
        public void CustomizeView(PieChartNodeModel model, NodeView nodeView)
        {
            
            // The view variable is a reference to the node's view.
            // In the middle of the node is a grid called the InputGrid.
            // We reccommend putting your custom UI in this grid, as it has
            // been designed for this purpose.

            // Create an instance of our custom UI class (defined in xaml),
            // and put it into the input grid.
            var pieChartControl = new PieChartControl(model);
            nodeView.inputGrid.Children.Add(pieChartControl);

            // Set the data context for our control to be the node model.
            // Properties in this class which are data bound will raise 
            // property change notifications which will update the UI.

            // TODO - this needs to be investigated
            //pieChartControl.DataContext = model;
        }

        /// <summary>
        /// Here you can do any cleanup you require if you've assigned callbacks for particular 
        /// UI events on your node.
        /// </summary>
        public void Dispose() { }
    }

}
