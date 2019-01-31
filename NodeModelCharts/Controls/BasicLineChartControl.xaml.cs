using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;
using NodeModelCharts.Nodes;
using System.ComponentModel;

namespace NodeModelCharts.Controls
{
    /// <summary>
    /// Interaction logic for BasicLineChartControl.xaml
    /// </summary>
    public partial class BasicLineChartControl : UserControl, INotifyPropertyChanged
    {
        private Random rnd = new Random();

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public SeriesCollection SeriesCollection { get; set; }
        //public Func<double, string> YFormatter { get; set; }

        public BasicLineChartControl(BasicLineChartNodeModel model)
        {
            InitializeComponent();

            model.PropertyChanged += NodeModel_PropertyChanged;

            // Load sample data if any ports are not connected
            if (!model.InPorts[0].IsConnected && !model.InPorts[1].IsConnected && !model.InPorts[2].IsConnected)
            {
                SeriesCollection = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "Series 1",
                        Values = new ChartValues<double> { 4, 6, 5, 2, 4 }
                    },
                    new LineSeries
                    {
                        Title = "Series 2",
                        Values = new ChartValues<double> { 6, 7, 3, 4, 6 },
                    },
                    new LineSeries
                    {
                        Title = "Series 3",
                        Values = new ChartValues<double> { 4, 2, 7, 2, 7 },
                    }
                };

                //Labels = new List<string> { "Jan", "Feb", "Mar", "Apr", "May" };
                //YFormatter = value => value.ToString("C");
            }
            else if (model.InPorts[0].IsConnected && model.InPorts[1].IsConnected && model.InPorts[2].IsConnected)
            {
                if (model.Labels.Count == model.Values.Count && model.Labels.Count > 0)
                {
                    for (var i = 0; i < model.Labels.Count; i++)
                    {
                        var lineValues = new ChartValues<double>();
                        foreach(double value in model.Values[i])
                        {
                            lineValues.Add(value);
                        }

                        SeriesCollection.Add(new LineSeries
                        {
                            Title = model.Labels[i],
                            Values = lineValues,
                            Stroke = model.Colors[i],
                            Fill = Brushes.Transparent

                            //PointGeometry = DefaultGeometries.Square,
                            //PointGeometrySize = 15
                        });
                    }
                }

                DataContext = this;
            }

            /*
            // Modifying the series collection will animate and update the chart
            SeriesCollection.Add(new LineSeries
            {
                Title = "Series 4",
                Values = new ChartValues<double> { 5, 3, 2, 4 },
                LineSmoothness = 0, //0: straight lines, 1: really smooth lines
                PointGeometry = Geometry.Parse("m 25 70.36218 20 -28 -20 22 -8 -6 z"),
                PointGeometrySize = 50,
                PointForeground = Brushes.Gray
            });

            // Modifying any series values will also animate and update the chart
            SeriesCollection[3].Values.Add(5d);
            */
        }

        private void NodeModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "DataUpdated")
            {
                var model = sender as BasicLineChartNodeModel;

                // Invoke on UI thread
                this.Dispatcher.Invoke(() =>
                {
                    BasicLineChart.Series.Clear();

                    for (var i = 0; i < model.Labels.Count; i++)
                    {
                        var lineValues = new ChartValues<double>();
                        foreach (double value in model.Values[i])
                        {
                            lineValues.Add(value);
                        }

                        SeriesCollection.Add(new LineSeries
                        {
                            Title = model.Labels[i],
                            Values = lineValues,
                            Stroke = model.Colors[i],
                            Fill = Brushes.Transparent

                            //PointGeometry = DefaultGeometries.Square,
                            //PointGeometrySize = 15
                        });
                    }
                });
            }
        }

        private void ThumbResizeThumbOnDragDeltaHandler(object sender, DragDeltaEventArgs e)
        {
            var yAdjust = ActualHeight + e.VerticalChange;
            var xAdjust = ActualWidth + e.HorizontalChange;

            if (this.Parent.GetType() == typeof(Grid))
            {
                var inputGrid = this.Parent as Grid;

                if (xAdjust >= inputGrid.MinWidth)
                {
                    Width = xAdjust;
                }

                if (yAdjust >= inputGrid.MinHeight)
                {
                    Height = yAdjust;
                }
            }
        }
    }
}
