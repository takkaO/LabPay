
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LabPay.Common
{
    class Communication
    {
        private TcpClient tcp;
        private NetworkStream ns;
        public Communication()
        {
            tcp = new TcpClient(AddressFamily.InterNetwork);
        }

        public enum TcpCommandNumber
        {
            CmdTest,            // 接続テストコマンド番号
            CmdAddUser,         // ユーザ追加コマンド番号
            CmdAddProduct,      // 商品追加コマンド番号
            CmdBuyProduct,      // 購入コマンド番号
            CmdSendMail,        // メール送信コマンド番号
            CmdChargeMoney,
            CmdRequestHash,
            CmdRequestBuyProduct,
            CmdClientFIN,       // クライアント側の送信終了メッセージ
            CmdUnknown
        }

        public enum TcpStatus
        {
            StatOK,             // ステータスOK
            StatRequestHash,
            StatRequestEmail,
            StatRequestAmountOfMoney,
            StatNoUser,
            StatFIN,            // ステータス通信終了
            StatHashConflict,
            StatRequestBuyProductName,
            StatRequestBuyProductAmount,
            StatNoEnoughMoney,
            StatError,
            StatUnknown
        }

        public enum TcpError
        {
            NoError,
            ConnectError,
            TestCommandError
        }

        public string GetTcpCommand(TcpCommandNumber num)
        {
            return Enum.GetName(typeof(TcpCommandNumber), num);
        }

        public TcpStatus GetStatus(string receiveStr)
        {
            switch (receiveStr)
            {
                case "FIN":
                    return TcpStatus.StatFIN;
                case "HASH":
                    return TcpStatus.StatRequestHash;
                case "EMAIL":
                    return TcpStatus.StatRequestEmail;
                case "HASH_CONFLICT":
                    return TcpStatus.StatHashConflict;
                case "AMOUNT_MONEY":
                    return TcpStatus.StatRequestAmountOfMoney;
                case "NO_USER":
                    return TcpStatus.StatNoUser;
                case "BUY_PRODUCT_NAME":
                    return TcpStatus.StatRequestBuyProductName;
                case "BUY_PRODUCT_AMOUNT":
                    return TcpStatus.StatRequestBuyProductAmount;
                case "NO_ENOUGH_MONEY":
                    return TcpStatus.StatNoEnoughMoney;
                case "ERROR":
                    return TcpStatus.StatError;
                default:
                    return TcpStatus.StatUnknown;
            }
        }

        public async Task<TcpError> ConnectAndTest(string ipAddress, string port, int timeout = 5000)
        {
            try
            {
                return await ConnectAndTest(ipAddress, int.Parse(port), timeout);
            }
            catch
            {
                return TcpError.ConnectError;
            }
        }

        public async Task<TcpError> ConnectAndTest(string ipAddress, int port, int timeout = 5000)
        {
            if (await Connect(ipAddress, port, timeout) == false)
            {
                return TcpError.ConnectError;
            }
            await SendMessageAsync(GetTcpCommand(TcpCommandNumber.CmdTest));
            string s = await ReceiveMessageAsync();
            if (GetStatus(s) != TcpStatus.StatFIN)
            {
                return TcpError.TestCommandError;
            }

            return TcpError.NoError;
        }

        public async Task<bool> Connect(string ipAddress, int port, int timeout = 5000)
        {
            bool result = true;
            try
            {
                await tcp.ConnectAsync(ipAddress, port);
                BeginIO(timeout);
            }
            catch
            {
                result = false;
            }
            return result;
        }

        public async Task<bool> Connect(string ipAddress, string port, int timeout = 5000)
        {
            try
            {
                return await Connect(ipAddress, int.Parse(port), timeout);
            }
            catch
            {
                return false;
            }
        }

        public void Disconnect()
        {
            EndIO();
            tcp.Close();
        }

        public bool SendMessage(string message, string end = "\n")
        {
            bool result = true;
            ns.WriteTimeout = 5000;
            try
            {
                Encoding enc = Encoding.UTF8;
                byte[] sendBytes = enc.GetBytes(message + end);
                ns.Write(sendBytes, 0, sendBytes.Length);
            }
            catch
            {
                result = false;
            }
            return result;
        }

        public async Task<bool> SendMessageAsync(string message, string end = "\n")
        {
            bool result = true;

            try
            {
                Encoding enc = Encoding.UTF8;
                byte[] sendBytes = enc.GetBytes(message + end);
                await ns.WriteAsync(sendBytes, 0, sendBytes.Length);
            }
            catch
            {
                result = false;
            }
            return result;
        }

        public string ReceiveMessage()
        {
            string resMsg = "";
            ns.ReadTimeout = 5000;
            try
            {
                Encoding enc = Encoding.UTF8;
                MemoryStream ms = new MemoryStream();
                byte[] resBytes = new byte[256];
                int resSize = 0;
                do
                {
                    resSize = ns.Read(resBytes, 0, resBytes.Length);
                    if (resSize == 0)
                    {
                        break;
                    }
                    ms.Write(resBytes, 0, resSize);
                } while (ns.DataAvailable || resBytes[resSize - 1] != '\n');
                resMsg = enc.GetString(ms.GetBuffer(), 0, (int)ms.Length);
                resMsg = resMsg.TrimEnd('\n');
                ms.Close();
            }
            catch
            {
                resMsg = "";
            }
            return resMsg;
        }

        public async Task<string> ReceiveMessageAsync()
        {
            string resMsg = "";

            try
            {
                using (var cancellationTokenSource = new CancellationTokenSource(5000))
                {
                    using (cancellationTokenSource.Token.Register(() => ns.Close()))
                    {
                        Encoding enc = Encoding.UTF8;
                        MemoryStream ms = new MemoryStream();
                        byte[] resBytes = new byte[256];
                        int resSize = 0;
                        do
                        {
                            resSize = await ns.ReadAsync(resBytes, 0, resBytes.Length);
                            if (resSize == 0)
                            {
                                break;
                            }
                            ms.Write(resBytes, 0, resSize);
                        } while (ns.DataAvailable || resBytes[resSize - 1] != '\n');
                        resMsg = enc.GetString(ms.GetBuffer(), 0, (int)ms.Length);
                        resMsg = resMsg.TrimEnd('\n');
                        ms.Close();
                    }
                }
            }
            catch
            {
                resMsg = "";
            }
            return resMsg;
        }

        public void BeginIO(int timeout = 5000)
        {
            ns = tcp.GetStream();
            ns.ReadTimeout = timeout;
            ns.WriteTimeout = timeout;
        }

        public void EndIO()
        {
            if (ns == null)
            {
                return;
            }
            ns.Close();
        }
    }
}
