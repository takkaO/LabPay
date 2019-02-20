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
        public ICommand SettingClicked { get; set; }
        public Stack<Type> PageStack { get; set; } = new Stack<Type>();

        public MainMenu(LabPay.MainPage mainPage)
        {
            page = mainPage;
            //Debug.WriteLine("{0}, {1}", typeof(MainPage), page.GetType());
            SelectProductsClicked = new RelayCommand(MoveSelectProductsPage);
            SettingClicked = new RelayCommand(MoveSettingPage);
        }

        private void MoveSettingPage()
        {
            PageStack.Push(page.GetType());
            Debug.WriteLine(PageStack.Count);
            page.Frame.Navigate(typeof(ConfigurationPage), PageStack);
        }

        private async void MoveSelectProductsPage()
        {
            if(await CustomIO.ExistFile() == false)
            {
                PageStack.Push(page.GetType());
                page.Frame.Navigate(typeof(SettingServerPage), PageStack);
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
