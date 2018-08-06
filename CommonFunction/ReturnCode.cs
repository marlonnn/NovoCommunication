using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovoCommunication.CommonFunction
{
    /*  ErrorCode:9999 代表出错命令解码出错.
   *           9998;代表下位机正忙,命令不被接收.
   *           9997;代表应答命令解码出错.
   *           9996;命令发送时出错 */
    public enum ReturnCode : ushort
    {
        //----ErrorCode-目前先定义这几条,它们现在只作用于返回响应命令的命令---     
        CommandOK = 0,//命令成功被执行
        CommandSendError = 9995,
        CommandDecodeError = 9996,
        CommandReject = 9997,  //下位机正忙.
        CommandReceivedError = 9998, //命令发送时出错.     
    }

    public enum LogicGateType : byte
    {
        AND,
        OR,
        NOT,
    }
}
