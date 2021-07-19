using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LiveCharts;
using LiveCharts.Wpf;
using System.IO;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;

namespace WpfApp2
{

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.chart();
            this.datosbacis();
            this.getFile();
    
        }
        public void getFile() //Bu fonksiyon SFTP/FTP server'dan belirtilen dosyayı indirecek.
        {
            String Host = "68.183.74.196";
            int Port = 22;
            String RemoteFileName = "/var/test.csv";
            String LocalDestinationFilename = @"in_process/new.csv";
            String Username = "root";
            String Password = "Diyap11Pazar";

            using (var sftp = new SftpClient(Host, Port, Username, Password))
            {
                sftp.Connect();   
                using (var file = File.OpenWrite(LocalDestinationFilename))
                {
                    sftp.DownloadFile(RemoteFileName, file);
                }
                sftp.Disconnect();
            }
        }
        public void chart()
        {
            PointLabel = chartPoint =>
             string.Format("{0} ({1:P})", chartPoint.Y, chartPoint.Participation);

            DataContext = this;
        }


        public void datosbacis()
        {

            SeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Ventas",
                    Values = new ChartValues<double> { 400, 600, 500,500  }
                },
                new LineSeries
                {
                    Title = "Compras",
                    Values = new ChartValues<double> { 600, 700, 300, 40 },
                    PointGeometry = null
                },
                new LineSeries
                {
                    Title = "Producción",
                    Values = new ChartValues<double> { 400,200,300,600 },
                    PointGeometry = DefaultGeometries.Square,
                    PointGeometrySize = 15
                }
            };

            Labels = new[] { "Mayo", "Junio", "Julio", "Agosto" };
            YFormatter = value => value.ToString("C");

            //modifying the series collection will animate and update the chart
            SeriesCollection.Add(new LineSeries
            {
                Title = "Comprobación",
                Values = new ChartValues<double> { 5, 3, 2 },
                LineSmoothness = 0, //0: straight lines, 1: really smooth lines
                PointGeometry = Geometry.Parse("m 25 70.36218 20 -28 -20 22 -8 -6 z"),
                PointGeometrySize = 50,
                PointForeground = Brushes.Gray
            });

         
            SeriesCollection[3].Values.Add(5d);

            DataContext = this;
        }


        public Func<ChartPoint, string> PointLabel { get; set; }
        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> YFormatter { get; set; }


        private void ButtonsDemoChip_DeleteClick_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    

        private void Chart_OnDataClick(object sender, ChartPoint chartPoint)
        {
            var chart = (LiveCharts.Wpf.PieChart)chartPoint.ChartView;

            //clear selected slice.
            foreach (PieSeries series in chart.Series)
                series.PushOut = 0;

            var selectedSeries = (PieSeries)chartPoint.SeriesView;
            selectedSeries.PushOut = 8;
        }

        private void ButtonsDemoChip_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonsDemoChip_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }
}
