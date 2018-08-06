using NovoCommunication.FCCommand;
using NovoCommunication.Queue;
using NovoCommunication.USBCommunication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NovoCommunication.Threading
{
    public class ProcThread
    {
        private Latch latch;
        private USB usb;

        public volatile bool NeedRunning = true;
        private TxQueue txQueue;
        private RxQueue rxQueue;
        //private log4net.ILog log = log4net.LogManager.GetLogger("MyLogger");

        public ProcThread()
        {
            UsbTxPrepare();
            txQueue = usb.TxQueue;
            rxQueue = usb.RxQueue;
            latch = usb.Latch;
        }

        public void Release()
        {
            latch.Release();
        }

        private void UsbTxPrepare()
        {
            usb = USB.UsbInstance;
            usb.DeviceRemoved += usb_DeviceRemoved;
            usb.DeviceAttached += usb_DeviceAttached;
        }

        /// <summary>
        /// 发送数据消费者，每隔50ms读取队列数据，通过USB发送数据
        /// </summary>
        public void UsbTxStart()
        {
            latch.Acquire();
            while (NeedRunning)
            {
                var list = txQueue.PopAll();
                foreach (var np in list)
                {
                    try
                    {
                        var data = np.encode();
                        int len = data.Length;
                        usb.sendDataSynchronous(ref data, ref len);
                        LogHelper.GetLogger<ProcThread>().Debug("Send Data: " + ByteHelper.Byte2ReadalbeXstring(data));
                    }
                    catch (Exception ex)
                    {
                        LogHelper.GetLogger<ProcThread>().Error(string.Format("Send data Error. Reason: {0}, Detail: {1}.", ex.ToString(), ex.StackTrace));
                    }
                }
                if (!NeedRunning) break;
                Thread.Sleep(50);
            }
        }

        public void UsbRxStart()
        {
            latch.Acquire();
            while (NeedRunning)
            {
                try
                {
                    byte[] data;
                    if (usb.readDataSynchronous(out data))
                    {
                        //rxQueue.Push(data);
                        LogHelper.GetLogger<ProcThread>().Debug(string.Format("Received  Data: {0}", ByteHelper.Byte2ReadalbeXstring(data)));
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.GetLogger<ProcThread>().Error(string.Format("Received data Error. Reason: {0}, Detail: {1}.", ex.ToString(), ex.StackTrace));
                }
            }
        }

        private void usb_DeviceRemoved(object sender, EventArgs e)
        {
            // raise communication lost event, may be we can retry several time before raise this event
            ProcessError(null, "Device Removed!");
        }

        private void usb_DeviceAttached(object sender, EventArgs e)
        {
            // raise communication establish event
            ProcessError(null, "Device Attached!");
        }

        protected void ProcessError(CBase command, string str)
        {
            if (command != null)
                LogHelper.GetLogger<ProcThread>().Debug("Command: " + command.Message.ToString("X2") + ", " + str);
            else
                LogHelper.GetLogger<ProcThread>().Debug(str);
        }
    }
}
