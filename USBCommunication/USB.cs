using CyUSB;
using NovoCommunication.CommonFunction;
using NovoCommunication.FCCommand;
using NovoCommunication.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NovoCommunication.USBCommunication
{
    public enum CommunicationState : ushort
    {
        CommOK,
        CommSendError,
        CommDecodeError,
        CommTimeOut,
        CommReject,
        CommReceivedError,
    }

    public class UsbReaderEventArgs : EventArgs
    {
        private string _Usage;// 这条命令的用途
        private CBase _Command;

        #region properties
        /// <summary>
        /// 这条命令的用途
        /// </summary>
        public string usage
        {
            get
            {
                return _Usage;
            }
        }
        public CBase Comd
        {
            get
            {
                return _Command;
            }
        }
        #endregion


        public UsbReaderEventArgs(string usage, CBase comd)
        {
            _Usage = usage;
            _Command = comd;
        }
    }

    class USB : CommunicateBase
    {
        private Latch latch;
        public Latch Latch { get { return this.latch; } }
        private TxQueue txQueue;
        public TxQueue TxQueue { get { return this.txQueue; } }
        private RxQueue rxQueue;
        public RxQueue RxQueue { get { return this.rxQueue; } }

        private CyUSBDevice loopDevice;
        private USBDeviceList usbDevices;
        private CyBulkEndPoint inEndpoint;
        public CyUSBEndPoint InEndpoint
        {
            get { return this.inEndpoint; }
        }
        private CyBulkEndPoint outEndpoint;
        private string usbName;
        private readonly int _delayms; //延迟时间,单位毫秒
        private bool _isUSBAvailable;

        private static readonly USB instance = new USB();
        public static USB UsbInstance
        {
            get
            {
                return instance;
            }
        }

        log4net.ILog log = log4net.LogManager.GetLogger("MyLogger");

        private USB()
        {
            _isUSBAvailable = false;
            _delayms = 10;
            usbName = "Bulkloop - no device";

            try
            {
                usbDevices = new USBDeviceList(CyConst.DEVICES_CYUSB);
                usbDevices.DeviceAttached += new EventHandler(usbDevices_DeviceAttached);
                usbDevices.DeviceRemoved += new EventHandler(usbDevices_DeviceRemoved);

                intDevice();
                txQueue = new TxQueue();
                rxQueue = new RxQueue();
                latch = new Latch();
            }
            catch
            {
                // the old CyUSB.dll is not compatible with Win10
            }
        }

        public bool intDevice()
        {
            // 可能抛出“无法访问已释放的对象”的异常
            try { loopDevice = usbDevices[0x04b4, 0x3031] as CyUSBDevice; }
            catch { loopDevice = null; }
            //PID 0x1004 is used for cardio
            //if (loopDevice == null) loopDevice = usbDevices[0x04b4, 0x1004] as CyUSBDevice;

            if (loopDevice != null)
            {
                try
                {
                    usbName = loopDevice.FriendlyName;
                    outEndpoint = loopDevice.EndPointOf(0x02) as CyBulkEndPoint;
                    inEndpoint = loopDevice.EndPointOf(0x86) as CyBulkEndPoint;
                    outEndpoint.TimeOut = 500;
                    inEndpoint.TimeOut = 1000;
                    _isUSBAvailable = true;
                    InitializeParams();
                    PreReadAsysnchonous();
                }
                catch (Exception ex)
                {
                    return false;
                }
                return true;
            }
            else
            {
                _isUSBAvailable = false;
                return false;
            }
        }

        public override bool Recover()
        {
            try
            {
                if (loopDevice != null)
                {
                    loopDevice.Dispose();
                    loopDevice = null;
                }

                if (usbDevices != null)
                {
                    usbDevices.DeviceRemoved -= usbDevices_DeviceRemoved;
                    usbDevices.DeviceAttached -= usbDevices_DeviceAttached;
                    usbDevices.Dispose();
                    usbDevices = null;
                }
            }
            catch (System.Exception ex)
            {

            }


            inEndpoint = null;
            outEndpoint = null;

            usbDevices = new USBDeviceList(CyConst.DEVICES_CYUSB);
            usbDevices.DeviceRemoved += new EventHandler(usbDevices_DeviceRemoved);
            usbDevices.DeviceAttached += new EventHandler(usbDevices_DeviceAttached);
            return intDevice();
        }
        /// <summary>
        /// 数据接收到触发事件
        /// </summary>
        public event EventHandler<UsbReaderEventArgs> DataArrived;
        private void OnDataArrived(UsbReaderEventArgs e)
        {
            if (DataArrived != null)
            {
                DataArrived(this, e);
            }
        }

        private void usbDevices_DeviceRemoved(object sender, EventArgs e)
        {
            // release usb device
            _isUSBAvailable = false;
            if (loopDevice != null)
                loopDevice.Dispose();
            loopDevice = null;
            //             intDevice();
            OnDeviceRemoved();
        }

        private void usbDevices_DeviceAttached(object sender, EventArgs e)
        {
            if (_isUSBAvailable) return;

            intDevice();

            if (_isUSBAvailable)
                OnDeviceAttached();
        }

        public string UsbName  //返回USB名称
        {
            get
            {
                return UsbName;
            }
        }
        public override bool isUSBAvailable  //返回USB是否可用
        {
            get
            {
                return _isUSBAvailable;
            }
        }

        public bool readDataSynchronous(out byte[] iniData)
        {
            iniData = new byte[0];
            Array.Clear(_receiveBuf, 0, _receiveBuf.Length);
            int packageLength = _packageLength;
            if (/*inEndpoint != null &&*/ inEndpoint.XferData(ref _receiveBuf, ref packageLength))
            {
                if (packageLength > 10)
                {
                    //CheckAndRemoveByteReceive( _receiveBuf, ref packageLength);
                    iniData = new byte[packageLength];
                    for (int i = 0; i < packageLength; i++)
                    {
                        iniData[i] = _receiveBuf[i];
                    }
                    return true;
                }
                //  ProcessError("USB has receive data ,but the data length has less than 10,the data length error.");
            }
            return false;
        }

        int BufSz;
        int QueueSz;
        int PPX;
        int IsoPktBlockSize;
        byte[] cBufs;
        byte[] xBufs;
        byte[] oLaps;
        ISO_PKT_INFO[] pktsInfo;
        private void InitializeParams()
        {
            BufSz = InEndpoint.MaxPktSize * Convert.ToUInt16(16);
            QueueSz = Convert.ToUInt16(8);
            PPX = Convert.ToUInt16(16);

            InEndpoint.XferSize = BufSz;

            if (InEndpoint is CyIsocEndPoint)
                IsoPktBlockSize = (InEndpoint as CyIsocEndPoint).GetPktBlockSize(BufSz);
            else
                IsoPktBlockSize = 0;
        }

        public unsafe bool PreReadAsysnchonous()
        {
            cBufs = new byte[CyConst.SINGLE_XFER_LEN + IsoPktBlockSize + ((InEndpoint.XferMode == XMODE.BUFFERED) ? BufSz : 0)];
            xBufs = new byte[BufSz];
            oLaps = new byte[20];
            pktsInfo = new ISO_PKT_INFO[PPX];
            fixed (byte* tL0 = oLaps, tc0 = cBufs, tb0 = xBufs)
            {
                OVERLAPPED* ovLapStatus = (OVERLAPPED*)tL0;
                ovLapStatus->hEvent = (IntPtr)PInvoke.CreateEvent(0, 0, 0, 0);
                // Pre-load the queue with a request
                int len = BufSz;
                if (InEndpoint.BeginDataXfer(ref cBufs, ref xBufs, ref len, ref oLaps) == false)
                {
                    LogHelper.GetLogger<USB>().Debug("Begin data xfer failure");
                }
                if (!InEndpoint.WaitForXfer(ovLapStatus->hEvent, 500))
                {
                    InEndpoint.Abort();
                    PInvoke.WaitForSingleObject(ovLapStatus->hEvent, 500);
                }

            }
            return true;
        }

        public void ReadAsysnchonous()
        {
            if (InEndpoint.Attributes == 1)
            {
                CyIsocEndPoint isoc = InEndpoint as CyIsocEndPoint;
                // FinishDataXfer
                if (isoc.FinishDataXfer(ref cBufs, ref xBufs, ref BufSz, ref oLaps, ref pktsInfo))
                {
                    LogHelper.GetLogger<USB>().Debug("isoc finish data xfer: ");
                }
                else
                {
                    if (InEndpoint.FinishDataXfer(ref cBufs, ref xBufs, ref BufSz, ref oLaps))
                    {
                        LogHelper.GetLogger<USB>().Debug("finish data xfer: ");
                    }
                }
            }
        }
        public bool readDataSynchronous2(CBase cBaseCommand, out int nLen)
        {
            nLen = 0;
            int packageLength = _packageLength;
            bool readOK;
            try
            {
                readOK = inEndpoint.XferData(ref _receiveBuf, ref packageLength);
            }
            catch (System.Exception ex)
            {
                return false;
            }
            if (readOK)
            {
                if (packageLength >= 10)
                {
                    nLen = packageLength;
                    return true;
                }
                ProcessError(cBaseCommand, "USB has receive data ,but the data length has less than 10,the data length error.");
            }
            else
            {
                ProcessError(cBaseCommand, "Read data from USB time out error.");
            }

            return false;
        }

        private void Sendasy(object sender, UsbReaderEventArgs e)
        {
            if (!isUSBAvailable) return;
            byte[] bBuf = e.Comd.encode();
            int nTotalDataLength = bBuf.Length;
            lock (this)
            {
                //ClearInPoint();
                if (!outEndpoint.XferData(ref bBuf, ref nTotalDataLength)) return;
                Thread.Sleep(_delayms);
                if (!readDataSynchronous(out bBuf)) return;
            }
            if (!e.Comd.decode(bBuf)) return;
            OnDataArrived(e);
        }
        /// <summary>
        /// 同步发送数据
        /// </summary>
        /// <param name="cBaseCommand">传入通信命令</param>
        /// <returns>成功返回true,否则返回false</returns>

        public override CommunicationState SendSymetric(CBase cSendCommand, CBase cDecodeCommand)
        {
            int nLen = 0;
            if (!isUSBAvailable)
            {
                ProcessError(cSendCommand, "USB is unavailable !");
                return CommunicationState.CommSendError;
            }
            byte[] bBuf = cSendCommand.encode();
            int nTotalDataLength = bBuf.Length;
            if (cDecodeCommand == null)
            {
                cDecodeCommand = cSendCommand;
            }
            lock (this)
            {
                try
                {
                    if (!outEndpoint.XferData(ref bBuf, ref nTotalDataLength))
                    {
                        Thread.Sleep(_delayms * 3);
                        if (inEndpoint.XferData(ref _receiveBufTemp, ref nTotalDataLength))
                        {
                            bBuf = cSendCommand.encode();
                            nTotalDataLength = bBuf.Length;
                            Thread.Sleep(_delayms * 3);
                            if (!outEndpoint.XferData(ref bBuf, ref nTotalDataLength))
                            {
                                ProcessError(cSendCommand, "Re-send data to usb error!" + GetUSBStatus());
                                return CommunicationState.CommSendError; ;
                            }
                        }
                        else
                        {
                            ProcessError(cSendCommand, "Send data to usb error and cannot re-read the data from usb error!" + GetUSBStatus());
                            return CommunicationState.CommSendError; ; //send data error.
                        }
                    }
                    else
                    {
                        if (nTotalDataLength <= 0)
                        {
                            ProcessError(cSendCommand, "Send data to usb error,the send byte is zero!" + GetUSBStatus());
                            return CommunicationState.CommSendError; ; //send data error.
                        }

                        // 在log里记录发送的命令
                        if (IsCommandNeedRecordLog(cDecodeCommand))
                        {
                            RecordCommandLog(bBuf, bBuf.Length, true);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    return CommunicationState.CommSendError;
                }

                Thread.Sleep(_delayms);
                int nTimes = 0;
                ReadAsysnchonous();
                //while (readDataSynchronous2(cSendCommand, out nLen))
                //{
                //    // 在log里记录接收的命令
                //    if (IsCommandNeedRecordLog(cDecodeCommand))
                //    {
                //        RecordCommandLog(_receiveBuf, nLen, false);
                //    }
                //    bBuf = BitConverter.GetBytes(nLen);
                //    Array.Copy(bBuf, 0, _receiveBuf, _receiveBuf.Length - 5, 4);//write data length at end

                //    cDecodeCommand.decode(_receiveBuf);
                //    if (cDecodeCommand.ErrorCode == (ushort)ReturnCode.CommandDecodeError)
                //    {
                //        nTimes++;
                //    }
                //    else if (cDecodeCommand.ErrorCode == (ushort)ReturnCode.CommandReceivedError)
                //    {
                //        return CommunicationState.CommReceivedError;
                //    }
                //    else if (cDecodeCommand.ErrorCode == (ushort)ReturnCode.CommandReject)
                //    {
                //        return CommunicationState.CommReject;
                //    }
                //    else
                //    {
                //        return CommunicationState.CommOK;
                //    }

                //    if (nTimes >= 9)
                //    {
                //        ProcessError(cDecodeCommand, "This command decode error,and it have re-try 9 times read,and it also error !");
                //        return CommunicationState.CommDecodeError;
                //    }
                //    Thread.Sleep(_delayms);
                //}
            }
            return CommunicationState.CommTimeOut;
        }

        private void RecordCommandLog(byte[] bBuf, int length, bool isSend)
        {
            if (bBuf != null && length > 0 && bBuf.Length >= length)
            {
                string log = string.Empty;

                for (int i = 0; i < length; i++)
                {
                    log += bBuf[i].ToString("X2");
                    if (i < length - 1)
                    {
                        log += ", ";
                    }
                }
                if (isSend)
                {
                    //ProcessError(null, string.Format("----------Send Command {0}", log));
                    LogHelper.GetLogger<Form1>().Debug(string.Format("----------Send Command {0}", log));
                }
                else
                {
                    //ProcessError(null, string.Format("++++++++++Receive Command {0}", log));
                    LogHelper.GetLogger<Form1>().Debug(string.Format("++++++++++Receive Command {0}", log));
                }
            }
        }

        private bool IsCommandNeedRecordLog(CBase cBaseCommand)
        {
            return true;
            //return !(cBaseCommand is C11 || cBaseCommand is C58 || cBaseCommand is C18 ||
            //         cBaseCommand is C67 || cBaseCommand is C22 || cBaseCommand is C68 ||
            //         cBaseCommand is CED || cBaseCommand is C58_2 || cBaseCommand is C68_2 ||
            //         cBaseCommand is C05_2);
        }

        public void BeginWrite(CBase cBaseCommand, string usage, AsyncCallback callback)
        {
            EventHandler<UsbReaderEventArgs> eh = new EventHandler<UsbReaderEventArgs>(Sendasy);
            UsbReaderEventArgs e = new UsbReaderEventArgs(usage, cBaseCommand);
            eh.BeginInvoke(this, e, callback, eh);
        }

        public bool sendDataSynchronous(ref byte[] outData, ref int TotalDataLength) //同步发数据
        {
            lock (lockObject)
            {
                if (isUSBAvailable)
                {
                    return outEndpoint.XferData(ref outData, ref TotalDataLength);
                }
            }
            return false;
        }

        public override void ClearInPoint()
        {
            if (!isUSBAvailable) return;
            int len = _packageLength;
            lock (lockObject)
            {
                inEndpoint.XferData(ref _receiveBuf, ref len);
                Array.Clear(_receiveBuf, 0, _receiveBuf.Length);
            }
            return;
        }

        private string GetUSBStatus()
        {
            string status = "  UsbdStatus: " + outEndpoint.UsbdStatus.ToString("X8") + CyUSBDevice.UsbdStatusString(outEndpoint.UsbdStatus) +
                            ";" + "  OutEndpoint last error: " + outEndpoint.LastError.ToString("X8") + ";" + "  InEndpoint last error: " + inEndpoint.LastError.ToString("X8");
            return status;
        }
    }
}
