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
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public PieChartControl(PieChartNodeModel model)
        {
            InitializeComponent();

            model.PropertyChanged += NodeModel_PropertyChanged;

            // Default data
            PieChart.Series.Add(new PieSeries { Title = "BAD", Fill = Brushes.Red, StrokeThickness = 0, Values = new ChartValues<double> { 50.0 } });
            PieChart.Series.Add(new PieSeries { Title = "GOOD", Fill = Brushes.Green, StrokeThickness = 0, Values = new ChartValues<double> { 100.0 } });

            DataContext = this;
        }

        private Random rnd = new Random();

        private void NodeModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var thing = e.PropertyName;
            if(e.PropertyName == "DataUpdated")
            {
                var nodeModel = sender as PieChartNodeModel;

                this.Dispatcher.Invoke(() =>
                {
                    PieChart.Series.Clear();

                    for (var i = 0; i < nodeModel.Labels.Count; i++)
                    {
                        Color randomColor = Color.FromRgb((byte)rnd.Next(256), (byte)rnd.Next(256), (byte)rnd.Next(256));
                        SolidColorBrush brush = new SolidColorBrush(randomColor);
                        PieChart.Series.Add(new PieSeries { Title = nodeModel.Labels[i], Fill = brush, StrokeThickness = 0, Values = new ChartValues<double> { nodeModel.Values[i] } });
                    }
                });
            }
        }

        public Func<ChartPoint, string> PointLabel { get; set; }

        private void Chart_OnDataClick(object sender, ChartPoint chartpoint)
        {
            var chart = (LiveCharts.Wpf.PieChart)chartpoint.ChartView;

            //clear selected slice.
            foreach (PieSeries series in chart.Series)
                series.PushOut = 0;

            var selectedSeries = (PieSeries)chartpoint.SeriesView;
            selectedSeries.PushOut = 8;
        }
    }
}
