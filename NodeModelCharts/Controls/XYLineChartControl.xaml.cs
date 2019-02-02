using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using NodeModelCharts.Nodes;
using System.ComponentModel;

namespace NodeModelCharts.Controls
{
    /// <summary>
    /// Interaction logic for XYLineChartControl.xaml
    /// </summary>
    public partial class XYLineChartControl : UserControl, INotifyPropertyChanged
    {
        private Random rnd = new Random();

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public XYLineChartControl(XYLineChartNodeModel model)
        {
            InitializeComponent();

            model.PropertyChanged += NodeModel_PropertyChanged;

            BuildUI(model);

            DataContext = this;
        }

        private void BuildUI(XYLineChartNodeModel model)
        {
            // Load sample data if any ports are not connected
            if (!model.InPorts[0].IsConnected && !model.InPorts[1].IsConnected && !model.InPorts[2].IsConnected && !model.InPorts[3].IsConnected)
            {
                var defaultXValues = new List<List<double>>()
                {
                    new List<double>(){ 0, 1, 2, 3 },
                    new List<double>(){ 0, 1, 2, 3 },
                    new List<double>(){ 0, 1, 2, 3 }
                };

                var defaultYValues = new List<List<double>>()
                {
                    new List<double>(){ 0, 1, 2, 3 },
                    new List<double>(){ 1, 2, 3, 4 },
                    new List<double>(){ 2, 3, 4, 5 }
                };

                for (var i = 0; i < defaultXValues.Count; i++)
                {
                    ChartValues<ObservablePoint> points = new ChartValues<ObservablePoint>();

                    for (int j = 0; j < defaultXValues[i].Count; j++)
                    {
                        points.Add(new ObservablePoint
                        {
                            X = defaultXValues[i][j],
                            Y = defaultYValues[i][j]
                        });
                    }

                    XYLineChart.Series.Add(new LineSeries
                    {
                        Values = points,
                        Fill = Brushes.Transparent
                    });
                }
            }
            // Else load input data
            else if (model.InPorts[0].IsConnected && model.InPorts[1].IsConnected && model.InPorts[2].IsConnected && model.InPorts[3].IsConnected)
            {
                if (model.Labels.Count == model.XValues.Count && model.XValues.Count == model.YValues.Count && model.Labels.Count > 0)
                {
                    for (var i = 0; i < model.Labels.Count; i++)
                    {
                        ChartValues<ObservablePoint> points = new ChartValues<ObservablePoint>();

                        for (int j = 0; j < model.XValues[i].Count; j++)
                        {
                            points.Add(new ObservablePoint
                            {
                                X = model.XValues[i][j],
                                Y = model.YValues[i][j]
                            });
                        }

                        XYLineChart.Series.Add(new LineSeries
                        {
                            Title = model.Labels[i],
                            Values = points,
                            Stroke = model.Colors[i],
                            Fill = Brushes.Transparent
                        });
                    }
                }
            }
        }

        private void NodeModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "DataUpdated")
            {
                var model = sender as XYLineChartNodeModel;

                // Invoke on UI thread
                this.Dispatcher.Invoke(() =>
                {
                    XYLineChart.Series.Clear();

                    for (var i = 0; i < model.Labels.Count; i++)
                    {
                        ChartValues<ObservablePoint> points = new ChartValues<ObservablePoint>();

                        for (int j = 0; j < model.XValues[i].Count; j++)
                        {
                            points.Add(new ObservablePoint
                            {
                                X = model.XValues[i][j],
                                Y = model.YValues[i][j]
                            });
                        }

                        XYLineChart.Series.Add(new LineSeries
                        {
                            Title = model.Labels[i],
                            Values = points,
                            Stroke = model.Colors[i],
                            Fill = Brushes.Transparent
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
