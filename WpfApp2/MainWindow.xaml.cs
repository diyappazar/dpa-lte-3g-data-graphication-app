using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
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
using System.Threading;

namespace WpfApp2
{

    public partial class MainWindow : Window
    {
        long FileSize = 0;
        DispatcherTimer Timer = new DispatcherTimer(); // zamanlayıcı objesini oluşturduk

        public MainWindow() { 
            InitializeComponent(); // Tüm componentleri başlatıyor
            Timer.Interval = TimeSpan.FromMilliseconds(10); // zamanlayıcının her 10 ms'de artmasını sağladık
            Timer.Tick += GetData; // Her zaman artışında GetData() adlı fonksiyondan return alınıp veri çekiliyor.
            this.chart(); // chart başlatıldı.
            this.datosbacis(); // datosbacis başlatıldı
        }
        void getFile() //Bu fonksiyon SFTP/FTP server'dan belirtilen dosyayı indirecek.
        {
            Timer.Start(); // zamanlayıcının tanımını yaptık ve başlattık. 
            String Host = "68.183.74.196";
            int Port = 22;
            String RemoteFileName = "/var/test.csv";
            String LocalDestinationFilename = @"in_process/test.csv";
            String Username = "root";
            String Password = "Diyap11Pazar";
            //Yukarıda server'a bağlanmak için gerekli olan bilgileri ssh.net kütüphanesi formatında ki variablellara attık. 
            using (var sftp = new SftpClient(Host, Port, Username, Password)) // sftp objesini oluşturduk bu kütüphanede ki StfpClient fonksiyonunu alacak.
            {
                sftp.Connect();   //bağlantı başladı
                using (var file = File.OpenWrite(LocalDestinationFilename)) //file değişkenini oluşturduk ve sistemde var olan File classının openwrite özelliğini kullanarak dosyayı oluşturduk.
                {
                    FileSize = sftp.Get(RemoteFileName).Attributes.Size;//sftp bağlanıtsını kullanarak get fonksiyonu ile dosya boyutunu öğrendik.
                    sftp.DownloadFile(RemoteFileName, file); //Dosyayı belirtilen lokasyona indirdik..
                }
                sftp.Disconnect();// bağlantıyı kestik.

               
            }
        }
        void GetData(object sender, EventArgs e) // GetData fonksiyonun oluşturduk bu fonksiyon bilgi gönderecek, gönderilen bilginin alıcısı progress bar ve altında ki label.
        {
            if (File.Exists(@"in_process/test.csv") & FileSize > 0 && new FileInfo(@"in_process/test.csv").Length > 0)//File.Exists dosyanın inip inmediğini kontrol ediyor daha sonra dosya boyutunun >0 ve byte cinsinden değerini>0 kontrol ediyor
            {
                var downloaded = new FileInfo(@"in_process/test.csv").Length;//indirilen dosya byte biçiminden alınıyor.
                Pb1.Value = downloaded * 100 / FileSize;//alınan bu değer küçük bir formül ile progress barda gösteriliyor.

                if (Pb1.Value == 100) // eğer progressbar >100den mesaj bu:
                {
                    this.ShowMsg("Download Finished! 100%");
                }
                else// eğer küçük ise bu şekilde bir yazı çıkacak.
                {
                    this.ShowMsg("Downloading... " + Pb1.Value + "%");
                }

            }
        }
        void DownloadFile(object sender, RoutedEventArgs e)// Bilgisayarın dosya inene kadar threadlerini dondurmasını istiyoruz bu fonksiyonun görevi bu ve xaml ile doğrudan ilişkili butona basınca bu çalışıyor.
        {
            Thread Proc = new Thread(new ThreadStart(this.getFile));//bu kısım ile butona basınca getFile çalışıyor...
            Proc.SetApartmentState(ApartmentState.STA);//ve sonuç alınıncaya kadar başka işlem yapılmıyor.
            Proc.Start();
        }
        void ShowMsg(String Msg)// progressbar altında ki mesaj bu kısım ile aktarılıyor.
        {
            label_Copy2.Content = Msg; 
        }
        public void PieChart_Loaded(object sender, RoutedEventArgs e)
        {

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
        private void LogOut(object sender, RoutedEventArgs e)
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


    }
}
