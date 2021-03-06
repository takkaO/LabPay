﻿using LabPay.Common;
using LabPay.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.System;
using Windows.UI.Xaml.Controls;

namespace LabPay.ViewModel
{
    class Configuration : INotifyPropertyChanged
    {

        public ICommand BackToBeforePageClicked { get; set; }
        public ICommand ServerSettingClicked { get; set; }
        public ICommand UserSettingClicked { get; set; }
        public ICommand ProductSettingClicked { get; set; }
        public ICommand PowerButtonClicked { get; set; }

        
        private ConfigurationPage page;
        public Configuration(ConfigurationPage mainPage)
        {
            page = mainPage;
            BackToBeforePageClicked = new RelayCommand(BackToBeforePage);
            ServerSettingClicked = new RelayCommand(MoveServerSettingPage);
            UserSettingClicked = new RelayCommand(MoveUserSettingPage);
            ProductSettingClicked = new RelayCommand(MoveProductSettingPage);
            PowerButtonClicked = new RelayCommand(PowerConfiguration);
        }

        private async void PowerConfiguration()
        {
            var res = await CustomDialog.AskPowerOff();
            if (res == ContentDialogResult.Primary)
            {
                //Debug.WriteLine("Shutdown");
                ShutdownManager.BeginShutdown(ShutdownKind.Shutdown, TimeSpan.Zero);
            }
            else if (res == ContentDialogResult.Secondary)
            {
                //Debug.WriteLine("Reboot");
                ShutdownManager.BeginShutdown(ShutdownKind.Restart, TimeSpan.Zero);
            }
            else
            {
                //Debug.WriteLine("Cancel");
            }
        }

        private void BackToBeforePage()
        {
            var parentPage = PageStack.Pop();
            page.Frame.Navigate(parentPage, PageStack);
        }

        private void MoveProductSettingPage()
        {
            PageStack.Push(page.GetType());
            page.Frame.Navigate(typeof(ProductSettingPage), PageStack);
        }

        private void MoveUserSettingPage()
        {
            PageStack.Push(page.GetType());
            page.Frame.Navigate(typeof(UserSettingPage), PageStack);
        }

        private void MoveServerSettingPage()
        {
            PageStack.Push(page.GetType());
            page.Frame.Navigate(typeof(ServerSettingPage), PageStack);
        }

        public bool BackToBeforePageEnabled
        {
            get
            {
                return PageStack.Count != 0;
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
