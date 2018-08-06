using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovoCommunication.Queue
{
    /// <summary>
    /// 接受数据队列
    /// </summary>
    public class RxQueue : ConcurrentQueue<byte[]>
    {
    }
}
