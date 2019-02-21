using LabPay.Common;
using LabPay.View;
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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LabPay.ViewModel
{
    class ServerSetting : INotifyPropertyChanged
    {

        public ICommand BackToBeforePageClicked { get; set; }
        public ICommand ConnectTestClicked { get; set; }
        public ICommand RemoveServerSettingClicked { get; set; }

        private ServerSettingPage page;
        public ServerSetting(ServerSettingPage mainPage)
        {
            page = mainPage;
            BackToBeforePageClicked = new RelayCommand(BackToBeforePage, CanGoBack);
            ConnectTestClicked = new RelayCommand(ConnectTest, CanGoBack);
            RemoveServerSettingClicked = new RelayCommand(RemoveServerSetting, CanGoBack);
            ConnectTesting = false;
            ResultVisibility = Visibility.Collapsed;
            LoadSetting();
        }

        private void BackToBeforePage()
        {
            var parentPage = PageStack.Pop();
            page.Frame.Navigate(parentPage, PageStack);
        }

        private async void RemoveServerSetting()
        {
            var result = await CustomDialog.AskRemoveFile("server setting");
            if(result == ContentDialogResult.Primary)
            {
                await CustomIO.RemoveFile();
                IpAddress = "";
                Port = "";
            }
        }

        private async void LoadSetting()
        {
            (bool res, string ip, string port) = await CustomIO.GetIpAndPort();
            if (res == true)
            {
                IpAddress = ip;
                Port = port;
            }
        }

        private async void ConnectTest()
        {
            ConnectTesting = true;
            ResultVisibility = Visibility.Collapsed;
            Communication tcp = new Communication();
            var stat = await tcp.ConnectAndTest(IpAddress, Port);
            if (stat != Communication.TcpError.NoError)
            {
                await CustomDialog.ServerConnectError();
                tcp.Disconnect();
                ConnectTesting = false;
                return;
            }
            await CustomIO.WriteFile(IpAddress + "," + Port);   // 設定ファイルを読み込み
            ResultVisibility = Visibility.Visible;
            tcp.Disconnect();
            ConnectTesting = false;
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
            //Debug.WriteLine(BackToBeforePageEnabled);
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

        public bool RemoveServerSettingEnabled
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

        private Visibility _resultVisibility;
        public Visibility ResultVisibility
        {
            get
            {
                return _resultVisibility;
            }
            set
            {
                _resultVisibility = value;
                NotifyPropertyChanged("ResultVisibility");
            }
        }

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
