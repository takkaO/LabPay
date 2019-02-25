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
    class SelectProducts : INotifyPropertyChanged
    {
        public ICommand BackToBeforePageClicked { get; set; }
        public ICommand BuyProductsClicked { get; set; }
        public ICommand PasswordSubmitClicked { get; set; }
        public ICommand IncleaseCupNoodleClicked { get; set; }
        public ICommand DecleaseCupNoodleClicked { get; set; }
        public ICommand IncleasePastaClicked { get; set; }
        public ICommand DecleasePastaClicked { get; set; }
        public ICommand IncleaseRiceClicked { get; set; }
        public ICommand DecleaseRiceClicked { get; set; }
        public ICommand IncleaseCurryClicked { get; set; }
        public ICommand DecleaseCurryClicked { get; set; }

        private SelectProductsPage page;
        public SelectProducts(SelectProductsPage mainPage)
        {
            page = mainPage;
            BackToBeforePageClicked = new RelayCommand(BackToBeforePage);
            BuyProductsClicked = new RelayCommand(BuyProducts);
            IncleaseCupNoodleClicked = new RelayCommand(IncleaseCupNoodle);
            DecleaseCupNoodleClicked = new RelayCommand(DecleaseCupNoodle);
            IncleasePastaClicked = new RelayCommand(IncleasePasta);
            DecleasePastaClicked = new RelayCommand(DecleasePasta);
            IncleaseRiceClicked = new RelayCommand(IncleaseRice);
            DecleaseRiceClicked = new RelayCommand(DecleaseRice);
            IncleaseCurryClicked = new RelayCommand(IncleaseCurry);
            DecleaseCurryClicked = new RelayCommand(DecleaseCurry);
            PasswordSubmitClicked = new RelayCommandWithParameter<object>(PasswordSubmit);
            PasswordInputPanelVisibility = Visibility.Collapsed;
            Connecting = false;

            CupNoodleNumber = 0.ToString();
            PastaNumber = 0.ToString();
            RiceNumber = 0.ToString();
            CurryNumber = 0.ToString();

            UpdateBuyButtonState();
        }

        private async Task BeginBuyProducts(string hash)
        {
            Connecting = true;
            (bool res, string ip, string port) = await CustomIO.GetIpAndPort();
            if (res == false)
            {
                // 一応ここでもチェックする
                await CustomDialog.ServerSettingLoadError();
                PageStack.Push(page.GetType());
                page.Frame.Navigate(typeof(ServerSettingPage), PageStack);
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
                Connecting = false;
                PasswordInputPanelVisibility = Visibility.Collapsed;
                return;
            }

            
            await tcp.SendMessageAsync(tcp.GetTcpCommand(Communication.TcpCommandNumber.CmdBuyProduct));
            Communication.TcpStatus comStat;
            var products = CreateStack();
            ProductInfo p = new ProductInfo("", 0);
            do
            {
                var resMsg = await tcp.ReceiveMessageAsync();
                comStat = tcp.GetStatus(resMsg);
                switch (comStat)
                {
                    case Communication.TcpStatus.StatRequestHash:
                        await tcp.SendMessageAsync(hash);
                        break;
                    case Communication.TcpStatus.StatRequestBuyProductName:
                        if (products.Count == 0)
                        {
                            await tcp.SendMessageAsync(tcp.GetTcpCommand(Communication.TcpCommandNumber.CmdClientFIN));
                            break;
                        }
                        p = products.Pop();
                        await tcp.SendMessageAsync(p.Name);
                        break;
                    case Communication.TcpStatus.StatRequestBuyProductAmount:
                        if (p.Name == "")
                        {
                            await CustomDialog.UnknownError();
                            tcp.Disconnect();
                            Connecting = false;
                            PasswordInputPanelVisibility = Visibility.Collapsed;
                        }
                        await tcp.SendMessageAsync(p.Amount.ToString());

                        if (products.Count == 0)
                        {
                            await tcp.SendMessageAsync(tcp.GetTcpCommand(Communication.TcpCommandNumber.CmdClientFIN));
                        }
                        break;
                    case Communication.TcpStatus.StatFIN:
                        break;
                    case Communication.TcpStatus.StatNoEnoughMoney:
                        await CustomDialog.NoEnoughMoney();
                        tcp.Disconnect();
                        Connecting = false;
                        PasswordInputPanelVisibility = Visibility.Collapsed;
                        PageStack.Push(page.GetType());
                        page.Frame.Navigate(typeof(MoneyChargePage), PageStack);
                        return;
                    case Communication.TcpStatus.StatNoUser:
                        await CustomDialog.NoUserError();
                        tcp.Disconnect();
                        Connecting = false;
                        PasswordInputPanelVisibility = Visibility.Collapsed;
                        return;
                    default:
                        await CustomDialog.UnknownError();
                        tcp.Disconnect();
                        Connecting = false;
                        PasswordInputPanelVisibility = Visibility.Collapsed;
                        return;
                }
            } while (comStat != Communication.TcpStatus.StatFIN);

            Connecting = false;
            PasswordInputPanelVisibility = Visibility.Collapsed;
            await CustomDialog.PurchaseComplete();
            page.Frame.Navigate(typeof(MainPage));
        }

        private async void PasswordSubmit(object parameter)
        {
            var passwordBox = parameter as PasswordBox;
            var hash = CalcHash.Sha256(passwordBox.Password);
            passwordBox.Password = "";
            await BeginBuyProducts(hash);
        }

        class ProductInfo
        {
            public string Name;
            public int Amount;
            public ProductInfo(string name, int amount)
            {
                Name = name;
                Amount = amount;
            }
        }

        private Stack<ProductInfo> CreateStack()
        {
            Stack<ProductInfo> p = new Stack<ProductInfo>();
            if (CupNoodleNumber != "0")
            {
                p.Push(new ProductInfo("CupNoodle", int.Parse(CupNoodleNumber)));
            }
            if (PastaNumber != "0")
            {
                p.Push(new ProductInfo("Pasta", int.Parse(PastaNumber)));
            }
            if (RiceNumber != "0")
            {
                p.Push(new ProductInfo("Rice", int.Parse(RiceNumber)));
            }
            if (CurryNumber != "0")
            {
                p.Push(new ProductInfo("Curry", int.Parse(CurryNumber)));
            }
            return p;
        }

        private async void BuyProducts()
        {
            
            ContentDialogResult result = await CustomDialog.AskBuyProducts(GetTotalFee().ToString());
            if (result == ContentDialogResult.Primary)
            {
                PasswordInputPanelVisibility = Visibility.Visible;
            }
        }

        private int GetTotalFee()
        {
            // TODO: もう少し頭のいい実装にする
            int total = 0;
            total = total + int.Parse(CupNoodleNumber) * 100;
            total = total + int.Parse(PastaNumber) * 200;
            total = total + int.Parse(RiceNumber) * 100;
            total = total + int.Parse(CurryNumber) * 100;
            return total;
        }

        private int IncleaseValue(string nowValue)
        {
            int num = 0;
            const int MAX_BUY_LIMIT = 10;
            try
            {
                num = int.Parse(nowValue);
                num++;
                if (num > MAX_BUY_LIMIT)
                {
                    num = MAX_BUY_LIMIT;
                }
            }
            catch
            {
                num = 0;
            }
            return num;
        }

        private int DecleaseValue(string nowValue)
        {
            int num = 0;
            const int MIN_BUY_LIMIT = 0;
            try
            {
                num = int.Parse(nowValue);
                num--;
                if (num < MIN_BUY_LIMIT)
                {
                    num = MIN_BUY_LIMIT;
                }
            }
            catch
            {
                num = 0;
            }
            return num;
        }

        private void UpdateBuyButtonState()
        {
            if (CupNoodleNumber == "0" && PastaNumber == "0" && RiceNumber == "0" && CurryNumber == "0")
            {
                BuyProductsEnabled = false;
            }
            else
            {
                BuyProductsEnabled = true;
            }
        }

        private void IncleaseCupNoodle()
        {
            CupNoodleNumber = IncleaseValue(CupNoodleNumber).ToString();
            UpdateBuyButtonState();
        }
        private void DecleaseCupNoodle()
        {
            CupNoodleNumber = DecleaseValue(CupNoodleNumber).ToString();
            UpdateBuyButtonState();
        }

        private void IncleasePasta()
        {
            PastaNumber = IncleaseValue(PastaNumber).ToString();
            UpdateBuyButtonState();
        }
        private void DecleasePasta()
        {
            PastaNumber = DecleaseValue(PastaNumber).ToString();
            UpdateBuyButtonState();
        }

        private void IncleaseRice()
        {
            RiceNumber = IncleaseValue(RiceNumber).ToString();
            UpdateBuyButtonState();
        }
        private void DecleaseRice()
        {
            RiceNumber = DecleaseValue(RiceNumber).ToString();
            UpdateBuyButtonState();
        }

        private void IncleaseCurry()
        {
            CurryNumber = IncleaseValue(CurryNumber).ToString();
            UpdateBuyButtonState();
        }
        private void DecleaseCurry()
        {
            CurryNumber = DecleaseValue(CurryNumber).ToString();
            UpdateBuyButtonState();
        }

        private string cupNoodleNumber;
        public string CupNoodleNumber
        {
            get
            {
                return cupNoodleNumber;
            }
            set
            {
                cupNoodleNumber = value;
                NotifyPropertyChanged("CupNoodleNumber");
            }
        }

        private string pastaNumber;
        public string PastaNumber
        {
            get
            {
                return pastaNumber;
            }
            set
            {
                pastaNumber = value;
                NotifyPropertyChanged("PastaNumber");
            }
        }

        private string riceNumber;
        public string RiceNumber
        {
            get
            {
                return riceNumber;
            }
            set
            {
                riceNumber = value;
                NotifyPropertyChanged("RiceNumber");
            }
        }

        private string curryNumber;
        public string CurryNumber
        {
            get
            {
                return curryNumber;
            }
            set
            {
                curryNumber = value;
                NotifyPropertyChanged("CurryNumber");
            }
        }

        private bool buyProductsEnabled;
        public bool BuyProductsEnabled
        {
            get
            {
                return buyProductsEnabled;
            }
            set
            {
                buyProductsEnabled = value;
                NotifyPropertyChanged("BuyProductsEnabled");
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
