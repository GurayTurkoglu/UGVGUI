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
using GMap.NET;
using GMap.NET.WindowsPresentation;
using GMap.NET.MapProviders;
using LiveCharts;
using LiveCharts.Wpf;
namespace UGVGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class AnaEkran
    {
        bool manuelkontrol = false;
        private void manuel_kontrol(object sender, RoutedEventArgs e)
        {
            manuelkontrol = !manuelkontrol;
            if (telemetriportu.IsOpen == true && manuelkontrol == true)
            {     
                telemetriportu.WriteLine("!CX#xxxxxxxxxxxxxxxxxxxxxxx>");
            }
            if (telemetriportu.IsOpen == true && manuelkontrol == false)
            {
                telemetriportu.WriteLine("#");
            }
        }
        private void gorev_durdur(object sender, RoutedEventArgs e)
        {
            if (telemetriportu.IsOpen == true)
            {
                telemetriportu.WriteLine("!RM#xxxxxxxxxxxxxxxxxxxxxxx>");
            }
            else
            {
                MessageBox.Show("Görev Durdurma İşlemi İçin Araç Bağlantısı Sağlanmalıdır");
            }
        }
        private void gorev_baslat(object sender, RoutedEventArgs e)
        {
            if (telemetriportu.IsOpen==true && pointlatlang.Count>1 && manuelkontrol==false)
            {
                int anlikcoord = 0;
                foreach (PointLatLng nokta in pointlatlang)
                {
                    if (anlikcoord<10)
                    {
                        telemetriportu.WriteLine("!KT" + "0"+anlikcoord.ToString() + "#" + nokta.Lat.ToString().Substring(0, 9).Replace(",", ".") + "#" + nokta.Lng.ToString().Substring(0, 9).Replace(",", ".")+ ">");
                    }
                    else
                    {
                        telemetriportu.WriteLine("!KT" + anlikcoord.ToString() + "#" + nokta.Lat.ToString().Substring(0, 9).Replace(",", ".") + "#" + nokta.Lng.ToString().Substring(0, 9).Replace(",", ".") + ">");
                    }

                    System.Threading.Thread.Sleep(100);
                    anlikcoord++;
                }
                telemetriportu.WriteLine("!RM#xxxxxxxxxxxxxxx");
            }
            else if(telemetriportu.IsOpen != true)
            {
                MessageBox.Show("Görev Başlatma İşlemi İçin Araç Bağlantısı Sağlanmalıdır");
            }
            else if (pointlatlang.Count <= 1)
            {
                MessageBox.Show("Görev Başlatma İşlemi İçin Minimum 2 Koordinat Girilmelidir");
            }
        }
        private void port_yenile(object sender, RoutedEventArgs e)
        {
            comport_secim.Items.Clear();
            string[] seri_portlar = System.IO.Ports.SerialPort.GetPortNames();
            foreach (string seriport in seri_portlar)
            {
                comport_secim.Items.Add(seriport);
            }
        }
        bool birkere = false;
        int markerno=100;
        void port_oku()
        {
            string telemetri_veri;
            while (telemetriportu.IsOpen)
            {
                    try
                    {
                        telemetri_veri = telemetriportu.ReadLine();
                 
                        if (telemetri_veri != null && telemetri_veri.StartsWith("<GPS"))
                        {
                            double araclat = double.Parse(telemetri_veri.Replace("<GPS", "").Replace('.',',').Split('/')[0]);
                            double araclng = double.Parse(telemetri_veri.Replace("<GPS", "").Replace('.', ',').Split('/')[1]);
                            PointLatLng arackonum = new PointLatLng(araclat, araclng);
                            Application.Current.Dispatcher.Invoke(new Action(() => {
                            var m1 = new GMapMarker(arackonum);
                            m1.Shape = new Rectangle
                            {
                                Width = 10,
                                Height = 10,
                                Stroke = Brushes.LightGreen,
                                StrokeThickness = 10,
                                Visibility = Visibility.Visible,
                                Fill = Brushes.Green,
                            };
                             if(birkere == false)
                             {
                                   HaritaObj.Markers.Add(m1);
                                   birkere = true;
                                   markerno = HaritaObj.Markers.Count;
                             }
                             if (markerno != 100 && birkere == true)
                             {
                                    HaritaObj.Markers[markerno-1].Position = arackonum;
                             }
                            }));

                            

                    }
                    if (telemetri_veri != null)
                    {

                        if (telemetri_veri.StartsWith("<YON"))
                        {

                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                yonyazi.Content = (telemetri_veri.Replace("<YON", "").Replace('.', ','));
                            }));
                        }
                        if (telemetri_veri.StartsWith("<HDOP"))
                        {

                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                hdop.Value = double.Parse(telemetri_veri.Replace("<HDOP", "").Replace('.', ','));
                            }));
                        }
                        if (telemetri_veri.StartsWith("<SAT"))
                        {
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                sat.Value = double.Parse(telemetri_veri.Replace("<SAT", "").Replace('.', ','));
                            }));
                        }
                        if (telemetri_veri.StartsWith("<HIZ"))
                        {

                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                gaugehiz.Value = double.Parse(telemetri_veri.Replace("<HIZ", "").Replace('.', ','));
                            }));
                        }
                        if (telemetri_veri.StartsWith("<VIB"))
                        {
                            if (chrt.Series[0].Values.Count > 14)
                            {
                                chrt.Series[0].Values.RemoveAt(0);
                                chrt2.Series[0].Values.RemoveAt(0);
                                chrt3.Series[0].Values.RemoveAt(0);
                            }

                            chrt.Series[0].Values.Add(double.Parse(telemetri_veri.Replace("<VIB", "")));
                            chrt2.Series[0].Values.Add(double.Parse(telemetri_veri.Replace("<VIB", "")));
                            chrt3.Series[0].Values.Add(double.Parse(telemetri_veri.Replace("<VIB", "")));
                        }

                    }
                }
                    catch
                    {

                    }
                    telemetri_veri = null;
          }
        }

        public System.Threading.Thread portokuma;
        public System.IO.Ports.SerialPort telemetriportu = new System.IO.Ports.SerialPort();
        private void combaglan(object sender, RoutedEventArgs e)
        {
            int paritesecim = parite_secim.SelectedIndex;
            int bitoranisecim = bitorani_secim.SelectedIndex;
            if (paritesecim != -1 && bitoranisecim != -1 && comport_secim.SelectedIndex!=-1 && telemetriportu.IsOpen != true)
            {
                string portsecim = comport_secim.SelectedItem.ToString();
                telemetriportu.PortName = portsecim;

                switch (bitoranisecim)
                {
                    case 0:
                        telemetriportu.BaudRate = 4800;
                        break;
                    case 1:
                        telemetriportu.BaudRate = 9600;
                        break;
                    case 2:
                        telemetriportu.BaudRate = 115200;
                        break;
                }


                switch (paritesecim)
                {
                    case 0:
                        telemetriportu.Parity = System.IO.Ports.Parity.None;
                        break;
                    case 1:
                        telemetriportu.Parity = System.IO.Ports.Parity.Even;
                        break;
                    case 2:
                        telemetriportu.Parity = System.IO.Ports.Parity.Odd;
                        break;
                }
                telemetriportu.DataBits = 8;
                telemetriportu.StopBits = System.IO.Ports.StopBits.One;
                telemetriportu.Handshake = System.IO.Ports.Handshake.None;
                telemetriportu.ReadTimeout = 500;
                try
                {
                        telemetriportu.Open();
                        if (telemetriportu.IsOpen == true)
                        {
                            comport_baslat.Icon = "comportkapa.ico";
                            comport_baslat.Header = "COM Bağlantısını Kapa";

                            portokuma = new System.Threading.Thread(() => port_oku());
                            portokuma.Priority = System.Threading.ThreadPriority.Highest;
                            portokuma.IsBackground = true;
                            portokuma.Start();
                        }
                        else
                        {
                            comport_baslat.Icon = "comportbaglan.ico";
                            comport_baslat.Header = "COM Bağlantısını Başlat";
                        }
                }
                catch(Exception ex)
                {
                    comport_baslat.Icon = "comportbaglan.ico";
                    comport_baslat.Header = "COM Bağlantısını Başlat";
                    MessageBox.Show("Telemetri Portuna Bağlanılamadı: " + ex.Message, "UGVGUI-ExceptionHandlingAracı");
                }
            }
            else
            {
                telemetriportu.Close();
                comport_baslat.Icon = "comportbaglan.ico";
                comport_baslat.Header = "COM Bağlantısını Başlat";
            }

        }
        bool rota_tasarim_modu=false;
        private void rotatemizle(object sender, RoutedEventArgs e)
        {
            pointlatlang.Clear();
            for (int i=1;i<HaritaObj.Markers.Count;i++)
            {
                HaritaObj.Markers[i].Clear();
            }
            listBox.Items.Clear();
        }
        private void sadece_server(object sender, RoutedEventArgs e)
        {
            if (GMap.NET.GMaps.Instance.Mode != GMap.NET.AccessMode.ServerOnly)
            {
                GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerOnly;
                HaritaObj.ReloadMap();
            }
        }
        private void sadece_cache(object sender, RoutedEventArgs e)
        {
            if (GMap.NET.GMaps.Instance.Mode != GMap.NET.AccessMode.CacheOnly)
            {
                GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.CacheOnly;
                HaritaObj.ReloadMap();
            }
        }
        private void server_ve_cache(object sender, RoutedEventArgs e)
        {
            if (GMap.NET.GMaps.Instance.Mode != GMap.NET.AccessMode.ServerAndCache)
            {
                GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerAndCache;
                HaritaObj.ReloadMap();
            }
        }
        private void rotatasarimiacik(object sender, RoutedEventArgs e)
        {
         
                if (rota_tasarim_modu != true)
                {
                    rota_tasarim_modu = true;
                    rota_tasarim_buton.Header = "Rota Tasarımını Kapat";
                    HaritaObj.CanDragMap = false;
                    rota_tasarim_buton.Icon = "rotakapa.ico";
                }
                else
                {
                    rota_tasarim_modu = false;
                    rota_tasarim_buton.Header = "Rota Tasarımı";
                    HaritaObj.CanDragMap = true;
                    rota_tasarim_buton.Icon = "rotatasarimi.ico";
                }
          
          
   
        }
        private void yakinlastirma(object sender, RoutedEventArgs e)
        {
            HaritaObj.Zoom += 1;
        }
        private void uzaklastirma(object sender, RoutedEventArgs e)
        {
            HaritaObj.Zoom -= 1;
        }
        private void kaydirma_aktif(object sender, RoutedEventArgs e)
        {
            if (HaritaObj.CanDragMap != true && rota_tasarim_modu == false)
            {
                HaritaObj.CanDragMap = true;
                kaydirmabuton.Header = "Kaydırma Aktif";
                kaydirmabuton.Icon = "kaydir.ico";
            }
            else if (rota_tasarim_modu == false && HaritaObj.CanDragMap != false)
            {
                HaritaObj.CanDragMap = false;
                kaydirmabuton.Header = "Kaydırma Kapalı";
                kaydirmabuton.Icon = "kaydirkapali.ico";
            }
        }
        private void kaydirma_kapali(object sender, RoutedEventArgs e)
        {
           
        }
        private void harita_arazi(object sender, RoutedEventArgs e)
        {
            if (HaritaObj.MapProvider != GMap.NET.MapProviders.GoogleTerrainMapProvider.Instance)
            {

                HaritaObj.MapProvider = GMap.NET.MapProviders.GoogleTerrainMapProvider.Instance;
                HaritaObj.MinZoom = 5;
                HaritaObj.MaxZoom = 25;

                HaritaObj.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter;
                HaritaObj.CanDragMap = true;
                HaritaObj.DragButton = MouseButton.Left;
                GMap.NET.GMaps.Instance.OptimizeMapDb(null);
            }
        }
        private void harita_hibrit(object sender, RoutedEventArgs e)
        {
            if (HaritaObj.MapProvider != GMap.NET.MapProviders.GoogleHybridMapProvider.Instance)
            {
                HaritaObj.MapProvider = GMap.NET.MapProviders.GoogleHybridMapProvider.Instance;
                HaritaObj.MinZoom = 5;
                HaritaObj.MaxZoom = 25;

                HaritaObj.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter;
                HaritaObj.CanDragMap = true;
                HaritaObj.DragButton = MouseButton.Left;
                GMap.NET.GMaps.Instance.OptimizeMapDb(null);
            }
        }
        private void harita_sokak(object sender, RoutedEventArgs e)
        {
            if (HaritaObj.MapProvider != GMap.NET.MapProviders.GoogleMapProvider.Instance)
            {
                HaritaObj.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
                HaritaObj.MinZoom = 5;
                HaritaObj.MaxZoom = 25;

                HaritaObj.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter;
                HaritaObj.CanDragMap = true;
                HaritaObj.DragButton = MouseButton.Left;
                GMap.NET.GMaps.Instance.OptimizeMapDb(null);
            }
        }
        private void harita_uydu(object sender, RoutedEventArgs e)
        {
            if (HaritaObj.MapProvider != GMap.NET.MapProviders.GoogleSatelliteMapProvider.Instance)
            {
                HaritaObj.MapProvider = GMap.NET.MapProviders.GoogleSatelliteMapProvider.Instance;
                HaritaObj.MinZoom = 5;
                HaritaObj.MaxZoom = 25;

                HaritaObj.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter;
                HaritaObj.CanDragMap = true;
                HaritaObj.DragButton = MouseButton.Left;
                GMap.NET.GMaps.Instance.OptimizeMapDb(null);
            }
        }

        private void harita_yukle(object sender, RoutedEventArgs e)
        {
            chrt.Zoom = LiveCharts.ZoomingOptions.Xy;
            chrt.DisableAnimations = true;
            chrt2.DisableAnimations = true;
            chrt3.DisableAnimations = true;
            chrt.Series.Add(new LineSeries
            {
                Title = "X",
                Values = new ChartValues<double> {}
            });
            chrt2.Series.Add(new LineSeries
            {
                Title = "X",
                Values = new ChartValues<double> {}
            });
            chrt3.Series.Add(new LineSeries
            {
                Title = "X",
                Values = new ChartValues<double> {}
            });
            chrt.Series.Add(new LineSeries
            {
                Title = "Y",
                Values = new ChartValues<double> { }
            });
            chrt2.Series.Add(new LineSeries
            {
                Title = "Y",
                Values = new ChartValues<double> { }
            });
            chrt3.Series.Add(new LineSeries
            {
                Title = "Y",
                Values = new ChartValues<double> { }
            });
            chrt.Series.Add(new LineSeries
            {
                Title = "Z",
                Values = new ChartValues<double> { }
            });
            chrt2.Series.Add(new LineSeries
            {
                Title = "Z",
                Values = new ChartValues<double> { }
            });
            chrt3.Series.Add(new LineSeries
            {
                Title = "Z",
                Values = new ChartValues<double> { }
            });
            string[] seri_portlar = System.IO.Ports.SerialPort.GetPortNames();
            foreach (string seriport in seri_portlar)
            {
                comport_secim.Items.Add(seriport);
            }
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerAndCache;
            HaritaObj.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
            HaritaObj.MinZoom = 5;
            HaritaObj.MaxZoom = 25;
            HaritaObj.Zoom = 17;
            HaritaObj.Position = new GMap.NET.PointLatLng(41.45431, 31.76363);
            HaritaObj.ShowCenter = true;
            HaritaObj.ZoomAndCenterMarkers(null);

            HaritaObj.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter;
            HaritaObj.CanDragMap = true;
            HaritaObj.DragButton = MouseButton.Left;
            GMap.NET.GMaps.Instance.OptimizeMapDb(null);
        }

        public AnaEkran()
        {
            InitializeComponent();
        }


        private void RibbonWindow_MouseDown(object sender, MouseEventArgs e)
        {
            
        }
        List<PointLatLng> pointlatlang = new List<PointLatLng>();
        PointLatLng baslangic_noktasi;
        private void RibbonWindow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (rota_tasarim_modu  && e.LeftButton == MouseButtonState.Pressed)
            {

                Point mousekoordinat = e.GetPosition(HaritaObj);
                PointLatLng mouse_enlemboylam = HaritaObj.FromLocalToLatLng((int)mousekoordinat.X, (int)mousekoordinat.Y);

                pointlatlang.Add(mouse_enlemboylam);
                if (pointlatlang.Count > 1)
                {
                    for (int k=1; k<HaritaObj.Markers.Count;k++)
                    {
                        HaritaObj.Markers[k].Clear();
                    }
                    listBox.Items.Clear();
                    int i = 1;
                    foreach (PointLatLng Pointx in pointlatlang)
                    {
                        var m1 = new GMapMarker(Pointx);
                        m1.Shape = new Ellipse
                        {
                            Width = 5,
                            Height = 5,
                            Stroke = Brushes.Blue,
                            StrokeThickness = 10,
                            Visibility = Visibility.Visible,
                            Fill = Brushes.Green,

                        };

                        var textBlock = new TextBlock(new Run("#" + Convert.ToString(i)));
                        textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                        textBlock.Arrange(new Rect(textBlock.DesiredSize));
                        textBlock.FontSize = 12;
                        textBlock.Foreground = Brushes.Red;
                        var m2 = new GMapMarker(Pointx);
                        m2.Shape = textBlock;
                        HaritaObj.Markers.Add(m1);
                        HaritaObj.Markers.Add(m2);
                        listBox.Items.Add("#" + i + " " + Pointx.Lat.ToString() + " " + Pointx.Lng.ToString());
                        i++;
                    }

                    GMapRoute polygon = new GMapRoute(pointlatlang);
                    HaritaObj.RegenerateShape(polygon);
                    (polygon.Shape as Path).Stroke = Brushes.Black;
                    (polygon.Shape as Path).StrokeThickness = 2;

                    HaritaObj.Markers.Add(polygon);

                }
                else
                {
                    baslangic_noktasi = mouse_enlemboylam;
                   
                }
            }
        }

        private void RibbonWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (manuelkontrol == true)
            {
                switch (e.Key)
                {
                    case Key.W:
                        telemetriportu.WriteLine("!NP1#xxxxxxxxxxxxx>");
                        break;
                    case Key.A:
                        telemetriportu.WriteLine("!NP2#xxxxxxxxxxxxx>");
                        break;
                    case Key.S:
                        telemetriportu.WriteLine("!NP3#xxxxxxxxxxxxx>");
                        break;
                    case Key.D:
                        telemetriportu.WriteLine("!NP4#xxxxxxxxxxxxx>");
                        break;
                }
            }
        }

        private void RibbonWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (manuelkontrol == true)
            {
                switch (e.Key)
                {
                    case Key.W:
                        telemetriportu.WriteLine("!NP3#xxxxxxxxxxxxx>");
                        break;
                    case Key.A:
                        telemetriportu.WriteLine("!NP5#xxxxxxxxxxxxx>");
                        break;
                    case Key.D:
                        telemetriportu.WriteLine("!NP5#xxxxxxxxxxxxx>");
                        break;
                }
            }
        }
    }
}
