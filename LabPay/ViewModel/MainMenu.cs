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
using LabPay.ViewModel;


namespace LabPay.ViewModel
{
    class MainMenu : INotifyPropertyChanged
    {
        
        public ICommand SelectProductsClicked { get; set; }
        public ICommand SettingClicked { get; set; }
        public ICommand MoneyChargeClicked { get; set; }
        public Stack<Type> PageStack { get; set; } = new Stack<Type>();

        public MainMenu(LabPay.MainPage mainPage)
        {
            page = mainPage;
            //Debug.WriteLine("{0}, {1}", typeof(MainPage), page.GetType());
            SelectProductsClicked = new RelayCommand(MoveSelectProductsPage);
            SettingClicked = new RelayCommand(MoveSettingPage);
            MoneyChargeClicked = new RelayCommand(MoveMoneyChargePage);
        }

        private void MoveSettingPage()
        {
            PageStack.Push(page.GetType());
            page.Frame.Navigate(typeof(ConfigurationPage), PageStack);
        }

        private async void MoveMoneyChargePage()
        {
            (bool res, string ip, string port) = await CustomIO.GetIpAndPort();
            if (res == false)
            {
                await CustomDialog.ServerSettingLoadError();
                PageStack.Push(page.GetType());
                page.Frame.Navigate(typeof(ServerSettingPage), PageStack);
                return;
            }
            PageStack.Push(page.GetType());
            page.Frame.Navigate(typeof(MoneyChargePage), PageStack);
        }

        private async void MoveSelectProductsPage()
        {
            (bool res, string ip, string port) = await CustomIO.GetIpAndPort();
            if (res == false)
            {
                await CustomDialog.ServerSettingLoadError();
                PageStack.Push(page.GetType());
                page.Frame.Navigate(typeof(ServerSettingPage), PageStack);
                return;
            }
            PageStack.Push(page.GetType());
            page.Frame.Navigate(typeof(SelectProductsPage), PageStack);
        }        

        private LabPay.MainPage page;

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
