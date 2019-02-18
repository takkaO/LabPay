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


namespace LabPay.ModelView
{
    class MainMenu : INotifyPropertyChanged
    {
        
        public ICommand SelectProductsClicked { get; set; }

        public MainMenu(LabPay.MainPage mainPage)
        {
            page = mainPage;
            SelectProductsClicked = new RelayCommand(MoveSelectProductsPage);
        }

        private async void MoveSelectProductsPage()
        {
            if(await CustomIO.ExistFile() == false)
            {
                page.Frame.Navigate(typeof(SettingBrokerPage));
            }
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
