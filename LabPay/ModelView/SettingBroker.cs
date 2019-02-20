using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using LabPay.Common;
using LabPay.View;
using Windows.UI.Xaml.Controls;

namespace LabPay.ModelView
{
    class SettingBroker : INotifyPropertyChanged
    {

        public ICommand BackToBeforePageClicked { get; set; }
        public ICommand ConnectTestClicked { get; set; }


        public SettingBroker(SettingBrokerPage mainPage)
        {
            page = mainPage;
            BackToBeforePageClicked = new RelayCommand(BackToBeforePage, CanGoBack);
            ConnectTestClicked = new RelayCommand(ConnectTest, CanGoBack);
            ConnectTesting = false;
            IpAddress = "localhost";
            Port = "65500";
        }

        private void BackToBeforePage()
        {

            var parentPage = PageStack.Pop();
            page.Frame.Navigate(parentPage, PageStack);
        }

        private async void ConnectTest()
        {
            ConnectTesting = true;
            TcpClient tcp = new TcpClient(AddressFamily.InterNetwork);
            try
            {
                await tcp.ConnectAsync(IpAddress, int.Parse(Port));
            }
            catch
            {
                var invalidIpAddressDialog = new ContentDialog
                {
                    Title = "Failed to connect server.",
                    Content = "Check your host ip address and port and try again.",
                    CloseButtonText = "OK"
                };
                ContentDialogResult result = await invalidIpAddressDialog.ShowAsync();
                ConnectTesting = false;
                return;
            }


            NetworkStream ns = tcp.GetStream();
            ns.ReadTimeout = 5000;
            ns.WriteTimeout = 5000;
            Encoding enc = Encoding.UTF8;
            byte[] sendBytes = enc.GetBytes("Test Command OK");
            await ns.WriteAsync(sendBytes, 0, sendBytes.Length);
            Debug.WriteLine(sendBytes);

            MemoryStream ms = new MemoryStream();
            byte[] resBytes = new byte[256];
            int resSize = 0;
            do
            {
                resSize = ns.Read(resBytes, 0, resBytes.Length);
                if (resSize == 0)
                {
                    break;
                }
                ms.Write(resBytes, 0, resSize);
            } while (ns.DataAvailable || resBytes[resSize - 1] != '\n');
            var resMsg = enc.GetString(ms.GetBuffer(), 0, (int)ms.Length);
            ms.Close();

            Debug.WriteLine(resMsg);

            ns.Close();
            tcp.Close();
            ConnectTesting = false;

            return;
            if (CheckIpAddress(IpAddress))
            {
                //check start
            }
            else
            {
                var invalidIpAddressDialog = new ContentDialog
                {
                    Title = "Invalid IP Address",
                    Content = "Check your host ip address and try again.",
                    CloseButtonText = "OK"
                };
                ContentDialogResult result = await invalidIpAddressDialog.ShowAsync();
            }
        }

        private bool CheckIpAddress(string address)
        {
            if (address == null)
            {
                // 何も入力されていない場合
                return false;
            }
            address = address.Replace(" ", "");
            if (!System.Text.RegularExpressions.Regex.IsMatch(address, @"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$"))
            {
                // IPアドレスの型になっていない場合
                return false;
            }
            var arr = address.Split('.');
            foreach (var n in arr)
            {
                if (int.Parse(n) > 254)
                {
                    // 数値が範囲外の場合（負値は前段階で除去済み）
                    return false;
                }
            }
            return true;
        }

        private Stack<Type> pageStack = new Stack<Type>();
        public Stack<Type> PageStack
        {
            get
            {
                return pageStack;
            }
            set
            {
                pageStack = value;
                NotifyPropertyChanged("BackToBeforePageEnabled");
            }
        }

        private bool _connectTesting;
        public bool ConnectTesting
        {
            get
            {
                return _connectTesting;
            }
            set
            {
                _connectTesting = value;
                NotifyPropertyChanged("BackToBeforePageEnabled");
                NotifyPropertyChanged("ConnectTestEnabled");
            }
        }


        private bool CanGoBack()
        {
            Debug.WriteLine(BackToBeforePageEnabled);
            return !ConnectTesting;
        } 

        
        public bool BackToBeforePageEnabled
        {
            get
            {
                return (!ConnectTesting && PageStack.Count != 0);
            }
        }

        public bool ConnectTestEnabled
        {
            get
            {
                return !ConnectTesting;
            }
        }



        private string _ipAddress;
        public string IpAddress
        {
            get
            {
                return _ipAddress;
            }
            set
            {
                _ipAddress = value;
                NotifyPropertyChanged("IpAddress");
            }
        }

        private string _port;
        public string Port
        {
            get
            {
                return _port;
            }
            set
            {
                _port = value;
                NotifyPropertyChanged("Port");
            }
        }

        private SettingBrokerPage page;

        #region イベント設定
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        #endregion
    }
}
