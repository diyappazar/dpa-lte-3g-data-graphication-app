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
using System.Windows.Controls;

namespace WpfApp2
{
    public partial class MainWindow : Window
    {
        System.Windows.Threading.DispatcherTimer Timer = new System.Windows.Threading.DispatcherTimer();
        System.Windows.Threading.DispatcherTimer CheckFile = new System.Windows.Threading.DispatcherTimer();
        System.Windows.Threading.DispatcherTimer CheckUI = new System.Windows.Threading.DispatcherTimer();
        float FileSize = 0;
        Boolean Downloading = false;
        DateTime LastTime = DateTime.Now;

        [Obsolete]
        public MainWindow() {
            InitializeComponent();

            ClearGrid();
            RenderGrid(150, 215);

            Timer.Tick += GetData;
            Timer.Interval = new TimeSpan(0, 0, 0, 0, 10);

            CheckFile.Tick += UpdateChart;
            CheckFile.Interval = new TimeSpan(0, 0, 0, 2);
            CheckFile.Start();

            //Reset Form
            Pb1.Value = 0;

            CheckUI.Tick += CheckInterface;
            CheckUI.Interval = new TimeSpan(0, 0, 0, 0, 1);
            CheckUI.Start();
        }

        void ClearGrid()
        {
            DashboardContent.RowDefinitions.Clear();
            DashboardContent.ColumnDefinitions.Clear();
            DashboardContent.Children.Clear();

            DashboardContent.ColumnDefinitions.Add(new ColumnDefinition());
            DashboardContent.ColumnDefinitions.Add(new ColumnDefinition());
            DashboardContent.ColumnDefinitions.Add(new ColumnDefinition());
            DashboardContent.ColumnDefinitions.Add(new ColumnDefinition());

            DashboardContent.RowDefinitions.Add(new RowDefinition());
            DashboardContent.RowDefinitions.Add(new RowDefinition());
            DashboardContent.RowDefinitions.Add(new RowDefinition());
        }

        [Obsolete]
        void RenderGrid(int height, int width)
        {
            int startrow = 0;
            int startcol = 0;
            foreach (String dataFile in Directory.GetFiles("files/", "*.csv"))
            {
                if (startcol >= 4)
                {
                    startcol = 0;
                    startrow++;
                }
                Canvas mycanvas = new Canvas();
                Canvas mytop = new Canvas();
                TextBlock mytitle = new TextBlock();
                Button mybtn = new Button();

                mycanvas.Background = Brushes.White;
                mycanvas.Height = height;
                mycanvas.Width = width;
                Grid.SetRow(mycanvas, startrow);
                Grid.SetColumn(mycanvas, startcol);

                mytop.Width = mycanvas.Width;
                mytop.Height = 20;
                mytop.Background = Brushes.Azure;

                mytitle.Text = dataFile.Replace("files/", "").Replace(".csv","").Replace("_", " ");
                mytitle.ToolTip = dataFile.Replace("files/", "").Replace(".csv", "").Replace("_", " ");
                mytitle.Width = mytop.Width - 50;
                Canvas.SetLeft(mytitle, 5);

                mybtn.Content = "↕";
                mybtn.HorizontalAlignment = HorizontalAlignment.Right;
                mybtn.Height = 20;
                mybtn.FontSize = 10;
                mybtn.Tag = dataFile;
                Canvas.SetRight(mybtn, 0);

                mybtn.Click += BeMax;

                mytop.Children.Add(mytitle);
                mytop.Children.Add(mybtn);

                mycanvas.Children.Add(mytop);

                CartesianChart deneme = new CartesianChart();

                SeriesCollection MySeriesCollection;
                List<string> Labels;
                Func<double, string> MyFormatter;
                Func<double, string> MyFormatter2;

                ChartValues<double> Values1  = new ChartValues<double> { 3, 4, 6, 3, 2, 6 };
                ChartValues<double> Values2 = new ChartValues<double> { 5, 3, 5, 7, 3, 9 };

                MySeriesCollection = new LiveCharts.SeriesCollection();
                Labels = new List<string>();
                Values1.Clear();
                Values2.Clear();

                TextFieldParser tfp = new TextFieldParser(dataFile);
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
                        Values1.Add(Convert.ToDouble(kpi.Replace("%", "")));
                        Values2.Add(Convert.ToDouble(data));
                        Labels.Add(time.ToString());
                        //Values1.Add(Convert.ToDouble(kpi) * 100);
                        //Values2.Add(Convert.ToDouble(data));
                        //Labels.Add(time.ToString());
                    }
                }

                MyFormatter = value => value.ToString("N") + "%";
                MyFormatter2 = value => value.ToString("N");

                Axis myAX = new Axis();

                myAX.Position = AxisPosition.LeftBottom;
                myAX.LabelsRotation = 20;
                myAX.Labels = Labels;

                myAX.Separator.Step = 1;

                Axis myAY = new Axis();

                myAY.ShowLabels = false;

                AxisSection section1 = new AxisSection();
                section1.Value = 98;
                section1.SectionWidth = 2;
                section1.Label = "Best";
                section1.Fill = Brushes.Lime;
                section1.Opacity = .8;

                AxisSection section2 = new AxisSection();
                section2.Value = 97;
                section2.SectionWidth = 1;
                section2.Label = "Good";
                section2.Fill = Brushes.Green;
                section2.Opacity = .4;

                AxisSection section3 = new AxisSection();
                section3.Value = 85;
                section3.SectionWidth = 12;
                section3.Label = "Bad";
                section3.Fill = Brushes.PaleVioletRed;
                section3.Opacity = .4;

                AxisSection section4 = new AxisSection();
                section4.Value = 0;
                section4.SectionWidth = 85;
                section4.Label = "Very Bad";
                section4.Fill = Brushes.Red;
                section4.Opacity = .8;

                myAY.Sections.Add(section1);
                myAY.Sections.Add(section2);
                myAY.Sections.Add(section3);
                myAY.Sections.Add(section4);

                Axis myKpi = new Axis();

                myKpi.Title = "Kpi";
                myKpi.LabelFormatter = MyFormatter;
                myKpi.Position = AxisPosition.LeftBottom;

                Axis myData = new Axis();

                myData.Title = "Data";
                myData.LabelFormatter = MyFormatter2;
                myData.Position = AxisPosition.RightTop;

                deneme.AxisX.Add(myAX);
                deneme.AxisY.Add(myAY);
                deneme.AxisY.Add(myKpi);
                deneme.AxisY.Add(myData);

                MySeriesCollection.Add(new LiveCharts.Wpf.LineSeries
                {
                    Values = Values1,
                    ScalesYAt = 0,
                    StrokeThickness = 0,
                    Fill = Brushes.Transparent,
                    Width = 0,
                    PointGeometrySize = 0
                });

                MySeriesCollection.Add(new LiveCharts.Wpf.ColumnSeries
                {
                    Title = "Kpi",
                    Values = Values1,
                    ScalesYAt = 1,
                    Fill = Brushes.DeepSkyBlue
                });

                MySeriesCollection.Add(new LiveCharts.Wpf.LineSeries
                {
                    Title = "Data",
                    Values = Values2,
                    ScalesYAt = 2,
                    Stroke = Brushes.Red
                });


                DataContext = this;

                deneme.Series = MySeriesCollection;

                Canvas.SetTop(deneme, 20);
                deneme.Height = height-20;
                deneme.Width = width;

                mycanvas.Children.Add(deneme);

                DashboardContent.Children.Add(mycanvas);

                startcol++;
            }
        }

        void goBack(object sender, EventArgs e)
        {
            BigPage.Visibility = Visibility.Collapsed;
            DashboardContent.Visibility = Visibility.Visible;
        }

        [Obsolete]
        private void BeMax(object sender, EventArgs e)
        {
            Button dataFile = (Button)sender;
            BigPage.Children.Clear();
            BigPage.Visibility = Visibility.Visible;
            DashboardContent.Visibility = Visibility.Collapsed;

            CartesianChart deneme = new CartesianChart();

            SeriesCollection MySeriesCollection;
            List<string> Labels;
            Func<double, string> MyFormatter;
            Func<double, string> MyFormatter2;

            ChartValues<double> Values1 = new ChartValues<double> { 3, 4, 6, 3, 2, 6 };
            ChartValues<double> Values2 = new ChartValues<double> { 5, 3, 5, 7, 3, 9 };

            MySeriesCollection = new LiveCharts.SeriesCollection();
            Labels = new List<string>();
            Values1.Clear();
            Values2.Clear();

            TextFieldParser tfp = new TextFieldParser(dataFile.Tag.ToString());
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
                    Values1.Add(Convert.ToDouble(kpi.Replace("%", "")));
                    Values2.Add(Convert.ToDouble(data));
                    Labels.Add(time.ToString());
                }
            }

            MyFormatter = value => value.ToString("N") + "%";
            MyFormatter2 = value => value.ToString("N");

            Axis myAX = new Axis();

            myAX.Position = AxisPosition.LeftBottom;
            myAX.LabelsRotation = 20;
            myAX.Labels = Labels;

            myAX.Separator.Step = 1;

            Axis myAY = new Axis();

            myAY.ShowLabels = false;

            AxisSection section1 = new AxisSection();
            section1.Value = 98;
            section1.SectionWidth = 2;
            section1.Label = "Best";
            section1.Fill = Brushes.Lime;
            section1.Opacity = .8;

            AxisSection section2 = new AxisSection();
            section2.Value = 97;
            section2.SectionWidth = 1;
            section2.Label = "Good";
            section2.Fill = Brushes.Green;
            section2.Opacity = .4;

            AxisSection section3 = new AxisSection();
            section3.Value = 85;
            section3.SectionWidth = 12;
            section3.Label = "Bad";
            section3.Fill = Brushes.PaleVioletRed;
            section3.Opacity = .4;

            AxisSection section4 = new AxisSection();
            section4.Value = 0;
            section4.SectionWidth = 85;
            section4.Label = "Very Bad";
            section4.Fill = Brushes.Red;
            section4.Opacity = .8;

            myAY.Sections.Add(section1);
            myAY.Sections.Add(section2);
            myAY.Sections.Add(section3);
            myAY.Sections.Add(section4);

            Axis myKpi = new Axis();

            myKpi.Title = "Kpi";
            myKpi.LabelFormatter = MyFormatter;
            myKpi.Position = AxisPosition.LeftBottom;

            Axis myData = new Axis();

            myData.Title = "Data";
            myData.LabelFormatter = MyFormatter2;
            myData.Position = AxisPosition.RightTop;

            deneme.AxisX.Add(myAX);
            deneme.AxisY.Add(myAY);
            deneme.AxisY.Add(myKpi);
            deneme.AxisY.Add(myData);

            MySeriesCollection.Add(new LiveCharts.Wpf.LineSeries
            {
                Values = Values1,
                ScalesYAt = 0,
                StrokeThickness = 0,
                Fill = Brushes.Transparent,
                Width = 0,
                PointGeometrySize = 0
            });

            MySeriesCollection.Add(new LiveCharts.Wpf.ColumnSeries
            {
                Title = "Kpi",
                Values = Values1,
                ScalesYAt = 1,
                Fill = Brushes.DeepSkyBlue
            });

            MySeriesCollection.Add(new LiveCharts.Wpf.LineSeries
            {
                Title = "Data",
                Values = Values2,
                ScalesYAt = 2,
                Stroke = Brushes.Red
            });


            DataContext = this;

            deneme.Series = MySeriesCollection;

            Canvas.SetTop(deneme, 20);
            deneme.Height = Dashboard.ActualHeight;
            deneme.Width = Dashboard.ActualWidth;

            BigPage.Children.Add(deneme);
        }

        public WindowState mystate;

        private void CheckInterface(object sender, EventArgs e)
        {
            if (mystate != MyForm.WindowState)
            {
                mystate = MyForm.WindowState;
                if (mystate == WindowState.Maximized)
                {
                    ClearGrid();
                    RenderGrid(320, 450);
                }
                else
                {
                    ClearGrid();
                    RenderGrid(150, 215);
                }
            }

        }

        private void UpdateChart(object sender, EventArgs e)
        {

            //if (this.Downloading == false && File.Exists("files/test.csv") && new FileInfo("files/test.csv").LastWriteTime != this.LastTime)
            //{
            //    this.LastTime = new FileInfo("files/test.csv").LastWriteTime;

            //    this.MySeriesCollection = new LiveCharts.SeriesCollection();
            //    this.Labels = new List<string>();
            //    this.Values1.Clear();
            //    this.Values2.Clear();

            //    TextFieldParser tfp = new TextFieldParser("files/test.csv");
            //    tfp.Delimiters = new string[] { ";" };
            //    tfp.TextFieldType = FieldType.Delimited;

            //    tfp.ReadLine();

            //    while (tfp.EndOfData == false)
            //    {
            //        string[] fields = tfp.ReadFields();
            //        string name = fields[0];
            //        string time = fields[1];
            //        string kpi = fields[2];
            //        string data = fields[3];

            //        if (kpi != "")
            //        {
            //            Values1.Add(Convert.ToDouble(kpi) * 100);
            //            Values2.Add(Convert.ToDouble(data));
            //            Labels.Add(time.ToString());
            //        }
            //    }

            //    this.MySeriesCollection.Add(new LiveCharts.Wpf.LineSeries {
            //        Values = this.Values1,
            //        ScalesYAt = 0,
            //        StrokeThickness = 0,
            //        Fill = Brushes.Transparent,
            //        Width = 0,
            //        PointGeometrySize = 0
            //    });

            //    this.MySeriesCollection.Add(new LiveCharts.Wpf.ColumnSeries {
            //        Title = "Kpi",
            //        Values = this.Values1,
            //        ScalesYAt = 1,
            //        Fill = Brushes.DeepSkyBlue
            //    });

            //    this.MySeriesCollection.Add(new LiveCharts.Wpf.LineSeries {
            //        Title = "Data",
            //        Values = this.Values2,
            //        ScalesYAt = 2,
            //        Stroke = Brushes.Red
            //    });

            //    this.MyFormatter = value => value.ToString("N") + "%";
            //    this.MyFormatter2 = value => value.ToString("N");
            //    DataContext = this;
            //}
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
