
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.ApplicationModel;
using Windows.System;
using Windows.System.Threading;

namespace AuraEditor
{
    [Windows.UI.Xaml.Data.Bindable]
    public class ServiceViewModel
    {
        // RpcClient API's implemented in C++ dll
        [DllImport("RpcClient.dll")]
        static extern long RpcClientInitialize(out IntPtr rpcClient);

        [DllImport("RpcClient.dll")]
        static extern long ACTrigger(IntPtr rpcClient, [MarshalAs(UnmanagedType.LPStr)] string xmlString, out int _p_val);

        [DllImport("RpcClient.dll")]
        static extern long SendEffectNumber(IntPtr rpcClient, int effect_id, out int _p_val);

        [DllImport("RpcClient.dll")]
        static extern long SendEffectXml(IntPtr rpcClient, [MarshalAs(UnmanagedType.LPStr)] string xmlString);

        [DllImport("RpcClient.dll")]
        static extern long CreatorTrigger(IntPtr rpcClient, [MarshalAs(UnmanagedType.LPStr)] string TriggerString);

        [DllImport("RpcClient.dll")]
        static extern long CreatorTriggerTime(IntPtr rpcClient, [MarshalAs(UnmanagedType.LPStr)] string TriggerString, long time);

        [DllImport("RpcClient.dll")]
        static extern long CreatorSetEngine(IntPtr rpcClient, [MarshalAs(UnmanagedType.LPStr)] string TriggerString);

        [DllImport("RpcClient.dll")]
        static extern long CreatorLoadFile(IntPtr rpcClient, [MarshalAs(UnmanagedType.LPStr)] string File, [MarshalAs(UnmanagedType.LPWStr)] out string Content);

        [DllImport("RpcClient.dll")]
        static extern long CreatorSaveFile(IntPtr rpcClient, [MarshalAs(UnmanagedType.LPStr)] string File, [MarshalAs(UnmanagedType.LPStr)] string Content);

        IntPtr rpcClient;
        long retCode = 0L;
        long error = 0L;
        //private bool DEBUG = true;
        string cmd = @"C:\ProgramData\Asus\AURA Creator\lastscript.xml";
        int num = 0, returnnum = 0;

        public async Task Sendupdatestatus(string updatestring)
        {
            await Task.Run(() =>
            {
                rpcClient = IntPtr.Zero;
                if (NotifyIfAnyError(RpcClientInitialize(out rpcClient))) return;
                retCode = ACTrigger(rpcClient, updatestring.ToUpper(), out returnnum);
            });
        }

        public async Task EffectNumber(int value)
        {
            await Task.Run(() =>
            {
                Debug.WriteLine("SendEffectNumber");
                rpcClient = IntPtr.Zero;
                if (NotifyIfAnyError(RpcClientInitialize(out rpcClient))) return;
                error = SendEffectNumber(rpcClient, value, out num);
                returnnum = num;
            });
        }


        public async Task SaveFile(string FileName, string Content)
        {
            await Task.Run(() =>
            {
                Debug.WriteLine("SaveFile by LiveService");
                rpcClient = IntPtr.Zero;

                if (NotifyIfAnyError(RpcClientInitialize(out rpcClient)))
                    return;

                error = CreatorSaveFile(rpcClient, FileName, Content);


            });
        }

        public string LoadFile(string FileName)
        {

            string Content = "";

            Debug.WriteLine("SaveFile by LiveService");
            rpcClient = IntPtr.Zero;

            if (NotifyIfAnyError(RpcClientInitialize(out rpcClient)))
                    return Content;

            error = CreatorLoadFile(rpcClient, FileName, out Content);

            return Content;

        }



        public async Task AuraEditorTrigger(long startTime)
        {
            await Task.Run(() =>
            {
                Debug.WriteLine("SendTTriggerString");
                rpcClient = IntPtr.Zero;

                if (NotifyIfAnyError(RpcClientInitialize(out rpcClient)))
                    return;

                //error = CreatorTrigger(rpcClient, cmd);
                error = CreatorTriggerTime(rpcClient, cmd, startTime);

            });
        }

        public async Task AuraEditorStopEngine()
        {
            await Task.Run(() =>
            {
                Debug.WriteLine("SendTTriggerString");
                rpcClient = IntPtr.Zero;

                if (NotifyIfAnyError(RpcClientInitialize(out rpcClient)))
                    return;

                error = CreatorSetEngine(rpcClient, "stop");

            });
        }

        public bool NotifyIfAnyError(long errCode)
        {
            if (errCode != 0)
            {
                return true;
            }
            return false;
        }
    }
}
