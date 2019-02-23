using LabPay.Common;
using LabPay.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LabPay.ViewModel
{
    class MoneyCharge : INotifyPropertyChanged
    {
        public ICommand BackToBeforePageClicked { get; set; }
        public ICommand Charge500YenClicked { get; set; }
        public ICommand Charge1000YenClicked { get; set; }
        public ICommand PasswordSubmitClicked { get; set; }

        private MoneyChargePage page;
        public MoneyCharge(MoneyChargePage mainPage)
        {
            page = mainPage;
            BackToBeforePageClicked = new RelayCommand(BackToBeforePage);
            Charge500YenClicked = new RelayCommand(Charge500Yen);
            Charge1000YenClicked = new RelayCommand(Charge1000Yen);
            PasswordSubmitClicked = new RelayCommandWithParameter<object>(PasswordSubmit);
            PasswordInputPanelVisibility = Visibility.Collapsed;
            amountOfCharge = 0;
            Connecting = false;
            CheckServerSettingFile();
        }

        private async void PasswordSubmit(object parameter)
        {
            Connecting = true;
            var passwordBox = parameter as PasswordBox;
            (bool res, string ip, string port) = await CustomIO.GetIpAndPort();
            if (res == false)
            {
                // 一応ここでもチェックする
                await CustomDialog.ServerSettingLoadError();
                PageStack.Push(page.GetType());
                page.Frame.Navigate(typeof(ServerSettingPage), PageStack);
                passwordBox.Password = "";
                amountOfCharge = 0;
                Connecting = false;
                PasswordInputPanelVisibility = Visibility.Collapsed;
                return;
            }

            Communication tcp = new Communication();
            var stat = await tcp.ConnectAndTest(ip, port);
            if (stat != Communication.TcpError.NoError)
            {
                await CustomDialog.ServerConnectError();
                tcp.Disconnect();
                passwordBox.Password = "";
                amountOfCharge = 0;
                Connecting = false;
                PasswordInputPanelVisibility = Visibility.Collapsed;
                return;
            }

            var hash = CalcHash.Sha256(passwordBox.Password);
            await tcp.SendMessageAsync(tcp.GetTcpCommand(Communication.TcpCommandNumber.CmdChargeMoney));
            Communication.TcpStatus comStat;
            do
            {
                var resMsg = await tcp.ReceiveMessageAsync();
                comStat = tcp.GetStatus(resMsg);
                switch (comStat)
                {
                    case Communication.TcpStatus.StatRequestHash:
                        await tcp.SendMessageAsync(hash);
                        break;
                    case Communication.TcpStatus.StatRequestAmountOfMoney:
                        await tcp.SendMessageAsync(amountOfCharge.ToString());
                        break;
                    case Communication.TcpStatus.StatFIN:
                        break;
                    case Communication.TcpStatus.StatNoUser:
                        await CustomDialog.NoUserError();
                        tcp.Disconnect();
                        passwordBox.Password = "";
                        amountOfCharge = 0;
                        Connecting = false;
                        PasswordInputPanelVisibility = Visibility.Collapsed;
                        return;
                    default:
                        await CustomDialog.UnknownError();
                        tcp.Disconnect();
                        passwordBox.Password = "";
                        amountOfCharge = 0;
                        Connecting = false;
                        PasswordInputPanelVisibility = Visibility.Collapsed;
                        return;
                }
            } while (comStat != Communication.TcpStatus.StatFIN);

            passwordBox.Password = "";
            amountOfCharge = 0;
            Connecting = false;
            PasswordInputPanelVisibility = Visibility.Collapsed;
            await CustomDialog.ChargeComplete();
        }

        private void Charge500Yen()
        {
            amountOfCharge = 500;
            PasswordInputPanelVisibility = Visibility.Visible;
        }

        private void Charge1000Yen()
        {
            amountOfCharge = 1000;
            PasswordInputPanelVisibility = Visibility.Visible;
        }

        private async void CheckServerSettingFile()
        {
            (bool res, string ip, string port) = await CustomIO.GetIpAndPort();
            if (res == false)
            {
                await CustomDialog.ServerSettingLoadError();
                PageStack.Push(page.GetType());
                page.Frame.Navigate(typeof(ServerSettingPage), PageStack);
                return;
            }
        }

        private void BackToBeforePage()
        {
            var parentPage = PageStack.Pop();
            page.Frame.Navigate(parentPage, PageStack);
        }

        public bool BackToBeforePageEnabled
        {
            get
            {
                return PageStack.Count != 0;
            }
        }

        private int amountOfCharge;
        private bool connecting;
        public bool Connecting
        {
            get
            {
                return connecting;
            }
            set
            {
                connecting = value;
                NotifyPropertyChanged("PasswordSubmitEnabled");
            }
        }

        public bool PasswordSubmitEnabled
        {
            get
            {
                return !Connecting;
            }
        }

        private Visibility passwordInputPanelVisibility;
        public Visibility PasswordInputPanelVisibility
        {
            get
            {
                return passwordInputPanelVisibility;
            }
            set
            {
                passwordInputPanelVisibility = value;
                NotifyPropertyChanged("PasswordInputPanelVisibility");
            }
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
