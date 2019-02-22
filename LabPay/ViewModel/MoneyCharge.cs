﻿using LabPay.Common;
using LabPay.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LabPay.ViewModel
{
    class MoneyCharge : INotifyPropertyChanged
    {
        public ICommand BackToBeforePageClicked { get; set; }
        public ICommand Charge500YenClicked { get; set; }
        public ICommand Charge1000YenClicked { get; set; }

        private MoneyChargePage page;
        public MoneyCharge(MoneyChargePage mainPage)
        {
            page = mainPage;
            BackToBeforePageClicked = new RelayCommand(BackToBeforePage);
            Charge500YenClicked = new RelayCommand(Charge500Yen);
            Charge1000YenClicked = new RelayCommand(Charge1000Yen);
            CheckServerSettingFile();
        }

        private void Charge500Yen()
        {

        }

        private void Charge1000Yen()
        {

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