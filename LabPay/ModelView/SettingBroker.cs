using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
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

        public ICommand BackToHomeClicked { get; set; }
        public ICommand ConnectClicked { get; set; }

        public SettingBroker(SettingBrokerPage mainPage)
        {
            page = mainPage;
            BackToHomeClicked = new RelayCommand(MoveMainMenuPage);
            ConnectClicked = new RelayCommand(ConnectBroker);
        }

        private void MoveMainMenuPage()
        {
            page.Frame.Navigate(typeof(MainPage));
        }

        private async void ConnectBroker()
        {
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
