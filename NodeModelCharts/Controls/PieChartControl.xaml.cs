using System;
using System.Windows;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Wpf;
using NodeModelCharts.Nodes;
using System.ComponentModel;

using System.Windows.Media;

namespace NodeModelCharts.Controls
{
    /// <summary>
    /// Interaction logic for PieChartControl.xaml
    /// </summary>
    public partial class PieChartControl : UserControl, INotifyPropertyChanged
    {
        private Func<ChartPoint, string> PointLabel { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        private Random rnd = new Random();

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public PieChartControl(PieChartNodeModel model)
        {
            InitializeComponent();

            model.PropertyChanged += NodeModel_PropertyChanged;

            PointLabel = chartPoint => string.Format("{0} ({1:P})", chartPoint.Y, chartPoint.Participation);

            // TODO - Make a function that sets Default data and make sure it triggers when a port is disconnected.
            PieChart.Series.Add(new PieSeries { Title = "BAD", Fill = Brushes.Red, StrokeThickness = 0, Values = new ChartValues<double> { 50.0 }, DataLabels = true, LabelPoint = PointLabel });
            PieChart.Series.Add(new PieSeries { Title = "GOOD", Fill = Brushes.Green, StrokeThickness = 0, Values = new ChartValues<double> { 100.0 }, DataLabels = true, LabelPoint = PointLabel });

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
                        Color randomColor = Color.FromRgb((byte)rnd.Next(256), (byte)rnd.Next(256), (byte)rnd.Next(256));
                        SolidColorBrush brush = new SolidColorBrush(randomColor);
                        PieChart.Series.Add(new PieSeries
                        {
                            Title = nodeModel.Labels[i],
                            Fill = brush, StrokeThickness = 0,
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
