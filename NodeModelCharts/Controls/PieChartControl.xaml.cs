using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;
using NodeModelCharts.Nodes;
using System.ComponentModel;

namespace NodeModelCharts.Controls
{
    /// <summary>
    /// Interaction logic for PieChartControl.xaml
    /// </summary>
    public partial class PieChartControl : UserControl, INotifyPropertyChanged
    {
        private Func<ChartPoint, string> PointLabel { get; set; }
        private Random rnd = new Random();

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public PieChartControl(PieChartNodeModel model)
        {
            InitializeComponent();

            model.PropertyChanged += NodeModel_PropertyChanged;

            PointLabel = chartPoint => string.Format("{0} ({1:P})", chartPoint.Y, chartPoint.Participation);

            if(model.InPorts[0].IsConnected == false && model.InPorts[1].IsConnected == false && model.InPorts[2].IsConnected == false)
            {
                PieChart.Series.Add(new PieSeries { Title = "Item1", Values = new ChartValues<double> { 100.0 }, DataLabels = true, LabelPoint = PointLabel });
                PieChart.Series.Add(new PieSeries { Title = "Item2", Values = new ChartValues<double> { 100.0 }, DataLabels = true, LabelPoint = PointLabel });
                PieChart.Series.Add(new PieSeries { Title = "Item3", Values = new ChartValues<double> { 100.0 }, DataLabels = true, LabelPoint = PointLabel });
            }

            else if(model.InPorts[0].IsConnected == true && model.InPorts[1].IsConnected == true && model.InPorts[2].IsConnected == true)
            {
                if (model.Labels.Count == model.Values.Count && model.Labels.Count > 0)
                {
                    for (var i = 0; i < model.Labels.Count; i++)
                    {
                        PieChart.Series.Add(new PieSeries
                        {
                            Title = model.Labels[i],
                            Values = new ChartValues<double> { model.Values[i] },
                            DataLabels = true,
                            LabelPoint = PointLabel
                        });
                    }
                }
            }

            DataContext = this;
        }

        private void NodeModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "DataUpdated")
            {
                var nodeModel = sender as PieChartNodeModel;

                // Invoke on UI thread
                this.Dispatcher.Invoke(() =>
                {
                    PieChart.Series.Clear();

                    for (var i = 0; i < nodeModel.Labels.Count; i++)
                    {
                        PieChart.Series.Add(new PieSeries
                        {
                            Title = nodeModel.Labels[i],
                            Fill = nodeModel.Colors[i],
                            //StrokeThickness = 0,
                            Values = new ChartValues<double> { nodeModel.Values[i] },
                            DataLabels = true,
                            LabelPoint = PointLabel
                        });
                    }
                });
            }
        }

        private void Chart_OnDataClick(object sender, ChartPoint chartpoint)
        {
            var chart = (PieChart)chartpoint.ChartView;

            // Clear selected slice
            foreach (PieSeries series in chart.Series)
            {
                series.PushOut = 0;
            }

            var selectedSeries = (PieSeries)chartpoint.SeriesView;
            selectedSeries.PushOut = 8;
        }
    }
}
