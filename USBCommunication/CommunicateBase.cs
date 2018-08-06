using NovoCommunication.FCCommand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovoCommunication.USBCommunication
{
    public abstract class CommunicateBase
    {
        protected enum ErrorTypes : int	//defined by PC software
        {
            None = 0x00,
            USBAvailable_Error = 0x01,
            USBSend_Error = 0x02,
            USBReceive_Error = 0x03,
            ReceiveDataFormat_Error = 0x04,
        };

        protected const byte FRAME_HEAD = 0x7E;
        protected const byte FRAME_END = 0x0D;
        protected const byte INSERT_BYTE = 0x81;

        protected static object lockObject = new object();
        protected static readonly int _packageLength = 20000 * 268 + 30;//20000个细胞,32FL
        protected byte[] _receiveBuf = new byte[_packageLength];
        protected byte[] _receiveBufBack = new byte[_packageLength];
        protected byte[] _receiveBufTemp = new byte[_packageLength];

        public event EventHandler DeviceRemoved;

        protected void OnDeviceRemoved()
        {
            if (DeviceRemoved != null)
            {
                DeviceRemoved(this, new EventArgs());
            }
        }

        public event EventHandler DeviceAttached;

        protected void OnDeviceAttached()
        {
            if (DeviceAttached != null)
            {
                DeviceAttached(this, new EventArgs());
            }
        }

        public abstract bool isUSBAvailable
        {
            get;
        }
        public abstract void ClearInPoint();

        public abstract CommunicationState SendSymetric(CBase cSendCommand, CBase cDecodeCommand);
        public abstract bool Recover();

        protected void CheckAndRemoveByteReceive(byte[] buf, ref int len)
        {
            int nResLen = len;
            Array.Copy(buf, _receiveBufBack, len);
            int nRemoveCount = 0;

            for (int i = 0; i < len; i++)
            {
                if (buf[i] == INSERT_BYTE && buf[i + 1] == FRAME_HEAD)
                {
                    buf[i - nRemoveCount] = _receiveBufBack[i + 1];
                    nRemoveCount++;
                }
                else
                {
                    buf[i - nRemoveCount] = _receiveBufBack[i];
                }
            }
            len -= nRemoveCount;
        }

        //  [Conditional("DEBUG")]
        protected void ProcessError(CBase command, ErrorTypes nErrorType)
        {
            string str = "Error has occurred in USB operation duration !Command: " + command.Message.ToString("X2") + " [Error Code]:" + nErrorType.ToString();
            //  System.Windows.Forms.MessageBox.Show(str);
            log4net.ILog log = log4net.LogManager.GetLogger("MyLogger");
            log.Debug(str);
        }
        //  [Conditional("DEBUG")]

        protected void ProcessError(CBase command, string str)
        {
            log4net.ILog log = log4net.LogManager.GetLogger("MyLogger");
            if (command != null)
                log.Debug("Command: " + command.Message.ToString("X2") + ", " + str);
            else
                log.Debug(str);
        }

    }
}
