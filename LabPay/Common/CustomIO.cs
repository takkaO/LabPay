using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace LabPay.Common
{
    static class CustomIO
    {
        private static StorageFolder localFolder = ApplicationData.Current.LocalFolder;
        public static string BrokerSettingFileName { get; set; } = "Broker.txt";

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
            return await ExistFile(BrokerSettingFileName);
        }

        public static async Task<string> ReadFile()
        {
            return await ReadFile(BrokerSettingFileName);
        }

        public static async Task WriteFile(string message, CreationCollisionOption option = CreationCollisionOption.ReplaceExisting)
        {
            await WriteFile(BrokerSettingFileName, message, option);
        }
    }
}
