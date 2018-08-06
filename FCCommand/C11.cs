using NovoCommunication.CommonFunction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovoCommunication.FCCommand
{
    //读取仪器工作状态 
    public class C11 : CBase
    {
        /// <summary>
        /// —表示仪器处在的模式 
        /// </summary>
        public uint M1 { set; get; }

        /// <summary>
        /// 预留为 0
        /// </summary>
        public uint M2 { set; get; }

        /// <summary>
        /// count of warning
        /// </summary>
        public ushort W { set; get; }

        /// <summary>
        /// count of error
        /// </summary>
        public ushort E { set; get; }

        /// <summary>
        /// duration of test
        /// </summary>
        public uint T { set; get; }

        /// <summary>
        /// volume of test
        /// </summary>
        public double V { get; set; }

        /// <summary>
        /// 重力传感器是否使能
        /// </summary>
        public bool C { get; set; }

        /// <summary>
        /// 流程执行的节拍数
        /// </summary>
        public uint t1 { get; set; }

        /// <summary>
        /// 流程总节拍数
        /// </summary>
        public uint t2 { get; set; }

        /// <summary>
        /// 自动上样器连机状态
        /// </summary>
        public byte S1 { get; set; }

        /// <summary>
        /// 表示boosting子状态
        /// </summary>
        public uint M3 { set; get; }


        public C11()
        {
            message = 0X11;
            parameter = null;
            ErrorCode = (ushort)ReturnCode.CommandSendError;//命令发送时出错
        }

        public override bool decode(byte[] buf) //只对parameter decode
        {
            if (!this.Decode(message, buf, out parameter)) return false;
            //具体decode添加在这里
            M1 = BitConverter.ToUInt32(parameter, 0);
            M2 = BitConverter.ToUInt32(parameter, 4);
            if (parameter.Length >= 12)
            {
                W = BitConverter.ToUInt16(parameter, 8);
                E = BitConverter.ToUInt16(parameter, 10);
            }
            if (parameter.Length >= 20)
            {
                T = BitConverter.ToUInt32(parameter, 12);
                V = Math.Round(BitConverter.ToSingle(parameter, 16), 2);
            }
            if (parameter.Length >= 29)
            {
                t1 = BitConverter.ToUInt32(parameter, 21);
                t2 = BitConverter.ToUInt32(parameter, 25);
            }
            if (parameter.Length >= 30)
            {
                S1 = parameter[29];
            }
            else
            {
                S1 = 0x1f;
            }
            if (parameter.Length >= 35)
            {
                M3 = BitConverter.ToUInt32(parameter, 31);
            }
            else
            {
                M3 = 0;
            }
            return true;
        }

        public override byte[] encode()//只对parameter encode 
        {
            //这里添加具体encode信息
            parameter = null;
            return this.Eecode(message, parameter);
        }
    }
}
