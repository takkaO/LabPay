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
    class UserSetting : INotifyPropertyChanged
    {
        public ICommand BackToBeforePageClicked { get; set; }
        public ICommand RegisterClicked { get; set; }

        private UserSettingPage page;
        public UserSetting(UserSettingPage mainPage)
        {
            page = mainPage;
            BackToBeforePageClicked = new RelayCommand(BackToBeforePage, CanGoBack);
            RegisterClicked = new RelayCommandWithParameter<object>(Register, CanGoBack);
            ResultVisibility = Visibility.Collapsed;
            Registering = false;
            CheckServerSettingFile();
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

        private enum CheckInputDataResult
        {
            PasswordTooShort,
            EmailTooShort,
            EmailTooLong,
            NoError
        }

        private CheckInputDataResult CheckInputData(string email, string pass)
        {
            const int MIN_PASSWORD_LENGTH = 6;
            const int MIN_EMAIL_LENGTH = 10;
            const int MAX_EMAIL_LENGTH = 254;   // ヌル文字分-1
            if (pass.Length < MIN_PASSWORD_LENGTH)
            {
                return CheckInputDataResult.PasswordTooShort;
            }
            if (email.Length < MIN_EMAIL_LENGTH)
            {
                return CheckInputDataResult.EmailTooShort;
            }
            if (email.Length > MAX_EMAIL_LENGTH)
            {
                return CheckInputDataResult.EmailTooLong;
            }
            return CheckInputDataResult.NoError;
        }

        private async void Register(object parameter)
        {
            Registering = true;
            ResultVisibility = Visibility.Collapsed;
            var passwordBox = parameter as PasswordBox;
            switch (CheckInputData(Email, passwordBox.Password))
            {
                case CheckInputDataResult.PasswordTooShort:
                    await CustomDialog.UserRegisterPasswordLengthError();
                    Registering = false;
                    return;
                case CheckInputDataResult.EmailTooShort:
                case CheckInputDataResult.EmailTooLong:
                    await CustomDialog.UserRegisterEmailLengthError();
                    Registering = false;
                    return;
            }

            (bool res, string ip, string port) = await CustomIO.GetIpAndPort();
            if (res == false)
            {
                // 一応ここでもチェックする
                await CustomDialog.ServerSettingLoadError();
                PageStack.Push(page.GetType());
                page.Frame.Navigate(typeof(ServerSettingPage), PageStack);
                Registering = false;
                return;
            }

            Communication tcp = new Communication();
            var stat = await tcp.ConnectAndTest(ip, port);
            if (stat != Communication.TcpError.NoError)
            {
                await CustomDialog.ServerConnectError();
                tcp.Disconnect();
                Registering = false;
                return;
            }

            var hash = CalcHash.Sha256(passwordBox.Password);
            await tcp.SendMessageAsync(tcp.GetTcpCommand(Communication.TcpCommandNumber.CmdAddUser));
            Communication.TcpStatus comStat;
            do
            {
                var resMsg = await tcp.ReceiveMessageAsync();
                comStat = tcp.GetStatus(resMsg);
                switch (comStat){
                    case Communication.TcpStatus.StatRequestHash:
                        await tcp.SendMessageAsync(hash);
                        break;
                    case Communication.TcpStatus.StatRequestEmail:
                        await tcp.SendMessageAsync(Email);
                        break;
                    case Communication.TcpStatus.StatFIN:
                        break;
                    case Communication.TcpStatus.StatHashConflict:
                        await CustomDialog.UserRegisterHashConflictError();
                        tcp.Disconnect();
                        Registering = false;
                        return;
                    default:
                        await CustomDialog.UnknownError();
                        tcp.Disconnect();
                        Registering = false;
                        return;
                }
            } while (comStat != Communication.TcpStatus.StatFIN);

            ResultVisibility = Visibility.Visible;
            passwordBox.Password = "";
            Email = "";
            await CustomDialog.UserRegisterComplete();
            tcp.Disconnect();
            Registering = false;
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

        private bool CanGoBack()
        {
            //Debug.WriteLine(BackToBeforePageEnabled);
            return !Registering;
        }

        private bool _registering;
        public bool Registering
        {
            get
            {
                return _registering;
            }
            set
            {
                _registering = value;
                NotifyPropertyChanged("BackToBeforePageEnabled");
                NotifyPropertyChanged("RegisterEnabled");
            }
        }

        public bool RegisterEnabled
        {
            get
            {
                return !Registering;
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

        private string _Email;
        public string Email
        {
            get
            {
                return _Email;
            }
            set
            {
                _Email = value.ToLower();
                NotifyPropertyChanged("Email");
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
