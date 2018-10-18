
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
        static extern long SendEffectNumber(IntPtr rpcClient, int effect_id, out int _p_val);

        [DllImport("RpcClient.dll")]
        static extern long SendEffectXml(IntPtr rpcClient, [MarshalAs(UnmanagedType.LPStr)] string xmlString);

        [DllImport("RpcClient.dll")]
        static extern long CreatorTrigger(IntPtr rpcClient, [MarshalAs(UnmanagedType.LPStr)] string TriggerString);

        IntPtr rpcClient;
        long error = 0L;
        //private bool DEBUG = true;
        string cmd = @"C:\ProgramData\Asus\AURA Creator\script\lastscript.xml";
        int num = 0, returnnum = 0;

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

        public async Task AuraEditorTrigger()
        {
            await Task.Run(() =>
            {
                Debug.WriteLine("SendTTriggerString");
                rpcClient = IntPtr.Zero;
                if (NotifyIfAnyError(RpcClientInitialize(out rpcClient))) return;
                error = CreatorTrigger(rpcClient, cmd);
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
