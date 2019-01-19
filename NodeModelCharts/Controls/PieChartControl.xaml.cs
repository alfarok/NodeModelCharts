using System;
using System.Windows;
using System.Windows.Controls;
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
        public PieChartNodeModel NodeModel { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;


        private string sampleName;

        public string SampleName
        {
            get { return sampleName; }
            set
            {
                sampleName = value;
                OnPropertyChanged("SampleName");
            }
        }

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

            NodeModel = model;

            NodeModel.PropertyChanged += NodeModel_PropertyChanged;

            SampleName = NodeModel.InputValue;

            PointLabel = chartPoint =>
                string.Format("{0} ({1:P})", chartPoint.Y, chartPoint.Participation);

            DataContext = this;
        }

        private void NodeModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var thing = e.PropertyName;
            if(e.PropertyName == "InputValue")
            {
                SampleName = (sender as PieChartNodeModel).InputValue;
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
