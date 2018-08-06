using NovoCommunication.CommonFunction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovoCommunication.FCCommand
{
    public abstract class CBase
    {
        enum ErrorTypes : int   //defined by PC software
        {
            Head_Error = 0x01,
            EndByte_Error = 0x02,
            CheckSum_Error = 0x03,
            Command_Error = 0x04,
            Length_Error = 0x06,
            Reveived79_Error = 0x07,
        };

        protected byte message;
        protected byte[] parameter;

        #region 属性
        /// <summary>
        /// message
        /// </summary>
        public byte Message
        {
            get
            {
                return message;
            }
            set
            {
                message = value;
            }
        }
        /// <summary>
        /// parameter
        /// </summary>
        public byte[] Parameter
        {
            get
            {
                return parameter;
            }
            set
            {
                parameter = value;
            }
        }
        public ushort ErrorCode { set; get; }
        #endregion
        public abstract bool decode(byte[] buf);
        public abstract byte[] encode();

        /// <summary>
        /// 包装协议头尾部
        /// </summary>
        /// <param name="mess">Message</param>
        /// <param name="para">Parameter,可以为null</param>
        /// <returns></returns>
        protected byte[] Eecode(byte mess, byte[] para) //message,parameter
        {
            int paralength = para == null ? 0 : para.Length;
            byte[] tempData = new byte[paralength + 10];

            tempData[0] = 0X7E; tempData[1] = 0X7E; //head

            int lgh = paralength + 4;
            tempData[2] = (byte)(lgh); //(length / 256 / 256);
            tempData[3] = (byte)(lgh >> 8);  //(length / 256);
            tempData[4] = (byte)(lgh >> 16);
            tempData[5] = (byte)(lgh >> 24);
            tempData[6] = mess;
            if (para != null && para.Length != 0)
            {
                para.CopyTo(tempData, 7);  //parameter
            }
            short tempCheckSum = 0;
            int tempDatalength = tempData.Length;
            for (int i = 0; i < tempDatalength - 3; i++)
            {
                tempCheckSum += tempData[i];
            }
            tempData[tempDatalength - 3] = (byte)(tempCheckSum);
            tempData[tempDatalength - 2] = (byte)(tempCheckSum >> 8);
            tempData[tempDatalength - 1] = 0X0D;

            //return insert81ToArray(tempData);
            return tempData;
        }

        //解析报文头尾,并反回parameter字段
        protected bool Decode(byte comd, byte[] data, out byte[] paradata)
        {
            string strTest;
            int length = BitConverter.ToInt32(data, data.Length - 5);
            if (length <= 16)
            {
                byte[] test = new byte[length];
                Array.Copy(data, 0, test, 0, length);
                strTest = BitConverter.ToString(test);
            }
            else
            {
                byte[] test = new byte[16];
                Array.Copy(data, 0, test, 0, 13);
                Array.Copy(data, length - 3, test, 13, 3);
                strTest = BitConverter.ToString(test);
            }
            strTest += "(" + length.ToString() + ")";


            ErrorCode = (ushort)ReturnCode.CommandDecodeError;
            if (data[6] == 0X79 && comd != 0X79)
            {
                C79 c79 = new C79();
                if (c79.decode(data))
                {
                    paradata = null;
                    ErrorCode = (ushort)ReturnCode.CommandReceivedError;
                    ProcessError(ErrorTypes.Reveived79_Error, strTest);
                    return false;
                }
                else
                {
                    paradata = null;
                    ErrorCode = (ushort)ReturnCode.CommandDecodeError; //上位机定义的错误,代表出错命令解码出错.
                    ProcessError(ErrorTypes.Reveived79_Error, strTest);
                    return false;
                }
            }

            int len = BitConverter.ToInt32(data, data.Length - 5);
            if (len <= 10)
            {
                paradata = null;
                ProcessError(ErrorTypes.Length_Error, strTest);
                return false;
            }

            paradata = new byte[len - 10];
            short header = BitConverter.ToInt16(data, 0);
            if (header != 32382)
            {
                ProcessError(ErrorTypes.Head_Error, strTest);
                return false;
            }
            int lth = BitConverter.ToInt32(data, 2); //(int)(256 * 256 * data[2] + 256 * data[3] + data[4]);
            if (lth != (len - 6))
            {
                ProcessError(ErrorTypes.Length_Error, strTest);
                return false;
            }
            if (comd != data[6])
            {
                ProcessError(ErrorTypes.Command_Error, strTest);
                return false;
            }
            if (paradata != null || paradata.Length != 0)
            {
                Array.Copy(data, 7, paradata, 0, len - 10);
            }
            ushort chekSum = BitConverter.ToUInt16(data, len - 3);
            uint tempCheckSum = 0;
            for (int i = 0; i < len - 3; i++)
            {
                tempCheckSum += data[i];
            }
            ushort temCheckSum = (ushort)tempCheckSum;
            if (temCheckSum != chekSum)
            {
                ProcessError(ErrorTypes.CheckSum_Error, strTest);
                return false;
            }
            byte ed = data[len - 1];
            if (0X0D != ed)
            {
                ProcessError(ErrorTypes.EndByte_Error, strTest);
                return false;
            }
            ErrorCode = (ushort)ReturnCode.CommandOK;
            return true;
        }
        //insert81ToArray 用于Eecode中
        private static byte[] insert81ToArray(byte[] buf)
        {
            List<byte> list = new List<byte>();
            list.Add(buf[0]);
            list.Add(buf[1]);
            for (int i = 2; i < buf.Length; i++)
            {
                if (buf[i] == 0X7E)
                {
                    list.Add(0X81);
                }
                list.Add(buf[i]);
            }
            return list.ToArray();
        }
        //   [Conditional("DEBUG")]
        private void ProcessError(ErrorTypes nErrorType, string strCmd)
        {
            string str = "Command decode error !Command: " + Message.ToString("X2") + " [Error Code]:" + nErrorType.ToString() + " [Error Cmd]:" + strCmd;
            //  System.Windows.Forms.MessageBox.Show(str);
            log4net.ILog log = log4net.LogManager.GetLogger("MyLogger");
            log.Debug(str);
        }
    }
}
