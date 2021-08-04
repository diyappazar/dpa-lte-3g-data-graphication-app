using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;
using Renci.SshNet;
using System.Threading;
using Microsoft.VisualBasic.FileIO;

namespace WpfApp2
{
    public partial class MainWindow : Window
    {
        System.Windows.Threading.DispatcherTimer Timer = new System.Windows.Threading.DispatcherTimer();
        System.Windows.Threading.DispatcherTimer CheckFile = new System.Windows.Threading.DispatcherTimer();
        float FileSize = 0;
        Boolean Downloading = false;
        DateTime LastTime = DateTime.Now;


        public SeriesCollection MySeriesCollection { get; set; }
        public List<string> Labels { get; set; }
        public Func<double, string> MyFormatter { get; set; }
        public Func<double, string> MyFormatter2 { get; set; }

        public ChartValues<double> Values1 { get; set; } = new ChartValues<double> { 3, 4, 6, 3, 2, 6 };
        public ChartValues<double> Values2 { get; set; } = new ChartValues<double> { 5, 3, 5, 7, 3, 9 };

        public MainWindow() { 
            InitializeComponent();

            ButtonsDemoChip.Content = "Turkcell App";

            Timer.Tick += GetData;
            Timer.Interval = new TimeSpan(0, 0, 0, 0, 10);

            CheckFile.Tick += UpdateChart;
            CheckFile.Interval = new TimeSpan(0, 0, 0,2);
            CheckFile.Start();

            //Reset Form
            Pb1.Value = 0;
        }

        private void UpdateChart(object sender, EventArgs e)
        {

            if (this.Downloading == false && File.Exists("files/test.csv") && new FileInfo("files/test.csv").LastWriteTime != this.LastTime)
            {
                this.LastTime = new FileInfo("files/test.csv").LastWriteTime;

                this.MySeriesCollection = new LiveCharts.SeriesCollection();
                this.Labels = new List<string>();
                this.Values1.Clear();
                this.Values2.Clear();

                TextFieldParser tfp = new TextFieldParser("files/test.csv");
                tfp.Delimiters = new string[] { ";" };
                tfp.TextFieldType = FieldType.Delimited;

                tfp.ReadLine();

                while (tfp.EndOfData == false)
                {
                    string[] fields = tfp.ReadFields();
                    string name = fields[0];
                    string time = fields[1];
                    string kpi = fields[2];
                    string data = fields[3];

                    if (kpi != "")
                    {
                        Values1.Add(Convert.ToDouble(kpi) * 100);
                        Values2.Add(Convert.ToDouble(data));
                        Labels.Add(time.ToString());
                    }
                }

                this.MySeriesCollection.Add(new LiveCharts.Wpf.LineSeries {
                    Values = this.Values1,
                    ScalesYAt = 0,
                    StrokeThickness = 0,
                    Fill = Brushes.Transparent,
                    Width = 0,
                    PointGeometrySize = 0
                });

                this.MySeriesCollection.Add(new LiveCharts.Wpf.ColumnSeries {
                    Title = "Kpi",
                    Values = this.Values1,
                    ScalesYAt = 1,
                    Fill = Brushes.DeepSkyBlue
                });

                this.MySeriesCollection.Add(new LiveCharts.Wpf.LineSeries {
                    Title = "Data",
                    Values = this.Values2,
                    ScalesYAt = 2,
                    Stroke = Brushes.Red
                });

                this.MyFormatter = value => value.ToString("N") + "%";
                this.MyFormatter2 = value => value.ToString("N");
                DataContext = this;
            }
        }

        private void GetData(object sender, EventArgs e)
        {
            if (File.Exists("files/test.csv") && this.FileSize > 0 && new FileInfo("files/test.csv").Length> 0)
            {
                var downloaded = new FileInfo("files/test.csv").Length;
                Pb1.Value = downloaded * 100 / this.FileSize;
                label_Copy2.Content = Pb1.Value + "%";
            }
        }

        public void LogOut(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void DownloadFile(object sender, RoutedEventArgs e)
        {
            if (this.Downloading == false)
            {
                Thread Proc = new Thread(new ThreadStart(this.RealProgress));
                Proc.SetApartmentState(ApartmentState.STA);
                Proc.Start();
                this.Downloading = true;
            }
        }

        private void RealProgress()
        {           
            this.Timer.Start();

            if (Directory.Exists("files"))
            {
                if (File.Exists("files/test.csv"))
                {
                    if (!Directory.Exists("files/backup"))
                    {
                        Directory.CreateDirectory("files/backup"); 
                    }

                    File.Move("files/test.csv", "files/backup/test - " + DateTime.Now.ToString().Replace("/", ".").Replace(":", ".") + ".csv");
                }

                String Host = "68.183.74.196";
                int Port = 22;
                String RemoteFileName = "/var/test.csv";
                String LocalDestinationFilename = "files/test.csv";
                String Username = "root";
                String Password = "Diyap11Pazar";

                using (SftpClient Sftp = new SftpClient(Host, Port, Username, Password))
                {
                    Sftp.Connect();

                    using (var myFile = File.OpenWrite(LocalDestinationFilename))
                    {
                        this.FileSize = Sftp.Get(RemoteFileName).Attributes.Size;
                        Sftp.DownloadFile(RemoteFileName, myFile);
                    }

                    Sftp.Disconnect();
                    this.Downloading = false;
                }
            }
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
