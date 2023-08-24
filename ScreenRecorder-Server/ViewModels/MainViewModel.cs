using ScreenRecorder_Server.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.IO;

namespace ScreenRecorder_Server.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public RelayCommand CreateServerCommand { get; set; }

        private BitmapImage screenImage;
        public BitmapImage ScreenImage
        {
            get { return screenImage; }
            set { screenImage = value; OnPropertyChanged(); }
        }

        public bool IsCreated { get; set; } = false;

        [Obsolete]
        public MainViewModel()
        {
            string hostName = Dns.GetHostName();
            string myIP = Dns.GetHostByName(hostName).AddressList[0].ToString();

            CreateServerCommand = new RelayCommand((obj) =>
            {
                if (!IsCreated)
                {
                    Task.Run(() =>
                    {
                        var ipAddress = IPAddress.Parse(myIP);
                        var port = 22003;
                        using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                        {
                            EndPoint endPoint = new IPEndPoint(ipAddress, port);
                            socket.Bind(endPoint);
                            socket.Listen(10);
                            IsCreated = true;
                            MessageBox.Show("The server has been successfully created!", "Successfully!", MessageBoxButton.OK, MessageBoxImage.Information);
                            var client = socket.Accept();

                            while (true)
                            {
                                var bytes = new byte[socket.ReceiveBufferSize];
                                var length = socket.ReceiveFrom(bytes, ref endPoint);

                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    BitmapImage image = CreateBitmapImageFromBytes(bytes);
                                    ScreenImage = image;
                                });
                            }
                        }
                    });
                }

                else
                {
                    MessageBox.Show("You are already created to the server.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }

        public BitmapImage ToImage(byte[] array)
        {
            try
            {
                using (var ms = new System.IO.MemoryStream(array))
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = ms;
                    image.EndInit();
                    return image;
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }

        private BitmapImage CreateBitmapImageFromBytes(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = ms;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
        }
    }
}
