using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using LabPay.ModelView;
using System.Diagnostics;

// 空白ページの項目テンプレートについては、https://go.microsoft.com/fwlink/?LinkId=234238 を参照してください

namespace LabPay.View
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class ServerSettingPage : Page
    {
        private ServerSetting p;
        public ServerSettingPage()
        {
            this.InitializeComponent();

            // x86/x64でもリモート環境と同じWindowサイズで起動する
            ApplicationView.PreferredLaunchViewSize = new Size { Width = 800, Height = 480 };
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;

            p = new ServerSetting(this);
            DataContext = p;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Debug.WriteLine("Load OK");
            //Debug.WriteLine(!p.ConnectTesting && p.ParentPage != null);
            var pages = (Stack<Type>)e.Parameter;
            p.PageStack = pages;
            //Debug.WriteLine(!p.ConnectTesting && p.ParentPage != null);
            base.OnNavigatedTo(e);
        }
    }
}
