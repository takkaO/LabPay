using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace LabPay.Common
{
    static class CustomDialog
    {
        public static async Task<ContentDialogResult> ServerConnectError()
        {
            var dialog = new ContentDialog
            {
                Title = "Failed to connect server.",
                Content = "Check your host ip address and port and try again.",
                CloseButtonText = "OK"
            };
            return await dialog.ShowAsync();
        }

        public static async Task<ContentDialogResult> ServerSettingLoadError()
        {
            var dialog = new ContentDialog
            {
                Title = "Can't find server setting file.",
                Content = "Please server setting first.",
                //CloseButtonText = "OK"
                PrimaryButtonText = "OK"
            };
            return await dialog.ShowAsync();
        }

        public static async Task<ContentDialogResult> UnknownError()
        {
            var dialog = new ContentDialog
            {
                Title = "Unknown error occured.",
                Content = "Unknown error occured.\nSorry ;(",
                CloseButtonText = "OK"
            };
            return await dialog.ShowAsync();
        }

        public static async Task<ContentDialogResult> AskRemoveFile(string fileName)
        {
            var dialog = new ContentDialog
            {
                Title = "Confirmation.",
                Content = "Would you delete " + fileName + " file?",
                PrimaryButtonText = "Yse",
                SecondaryButtonText = "No"
            };
            return await dialog.ShowAsync();
        }

        public static async Task<ContentDialogResult> RemoveServerSettingComplete()
        {
            var dialog = new ContentDialog
            {
                Title = "Remove server setting complete.",
                Content = "Please set up the server again",
                CloseButtonText = "OK"
            };
            return await dialog.ShowAsync();
        }

        public static async Task<ContentDialogResult> UserRegisterComplete()
        {
            var dialog = new ContentDialog
            {
                Title = "User register complete.",
                Content = "Enjoy shopping! :)",
                CloseButtonText = "OK"
            };
            return await dialog.ShowAsync();
        }
    }

    static class CustomIO
    {
        private static StorageFolder localFolder = ApplicationData.Current.LocalFolder;
        public static string ServerSettingFileName { get; set; } = "Server.txt";

        public static async Task<(bool res, string ip, string port)> GetIpAndPort()
        {
            if(await ExistFile() == false)
            {
                return (false, "", "");
            }
            var s = await ReadFile();
            char[] sp = new char[] { ',' };
            string[] ss = s.Split(sp);

            string ip = ss[0];
            string port = ss[1];
            return (true, ip, port);
        }

        public static async Task<string> ReadFile(string fileName)
        {
            string txt = null;
            if (await ExistFile(fileName))
            {
                StorageFile appFile = await localFolder.GetFileAsync(fileName);
                txt = await FileIO.ReadTextAsync(appFile);
            }
            return txt;
        }

        public static async Task WriteFile(string fileName, string message, CreationCollisionOption option = CreationCollisionOption.ReplaceExisting)
        {
            StorageFile appFile = await localFolder.CreateFileAsync(fileName, option);
            await FileIO.WriteTextAsync(appFile, message);
        }

        public static async Task RemoveFile(string fileName)
        {
            if (await ExistFile(fileName) == false)
            {
                return;
            }
            StorageFile appFile = await localFolder.GetFileAsync(fileName);
            await appFile.DeleteAsync();
        }

        public static async Task<bool> ExistFile(string fileName)
        {
            var obj = await localFolder.TryGetItemAsync(fileName);
            if (obj == null)
            {
                return false;
            }
            return true;
        }

        public static async Task<bool> ExistFile()
        {
            return await ExistFile(ServerSettingFileName);
        }

        public static async Task<string> ReadFile()
        {
            return await ReadFile(ServerSettingFileName);
        }

        public static async Task RemoveFile()
        {
            await RemoveFile(ServerSettingFileName);
        }

        public static async Task WriteFile(string message, CreationCollisionOption option = CreationCollisionOption.ReplaceExisting)
        {
            await WriteFile(ServerSettingFileName, message, option);
        }
    }
}
