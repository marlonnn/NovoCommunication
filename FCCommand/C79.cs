using NovoCommunication.CommonFunction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovoCommunication.FCCommand
{
    /// <summary>
    /// 报错命令
    /// </summary>
    class C79 : CBase
    {
        public C79()
        {
            message = 0X79;
            //parameter = new byte[2]; //这里只须考虑new发送命令的parameter.
            ErrorCode = (ushort)ReturnCode.CommandSendError;
        }
        public override bool decode(byte[] buf) //只对parameter decode
        {
            if (!this.Decode(message, buf, out parameter)) return false;
            //具体decode添加在这里
            ErrorCode = BitConverter.ToUInt16(Parameter, 0);

            return true;
        }
        public override byte[] encode()//只对parameter encode
        {
            parameter = new byte[2]; //这里只须考虑new发送命令的parameter.
            //这里添加具体encode信息

            return this.Eecode(message, parameter);
        }
    }
}
