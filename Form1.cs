using NovoCommunication.FCCommand;
using NovoCommunication.Threading;
using NovoCommunication.USBCommunication;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NovoCommunication
{
    public partial class Form1 : Form
    {
        private ProcThread usbThreads;
        private Thread usbTxThread;
        private Thread usbRxThread;
        private USB usb;
        public Form1()
        {
            InitializeComponent();
            RunProcess();
        }

        private void RunProcess()
        {
            usb = USB.UsbInstance;
            usbThreads = new ProcThread();
            usbTxThread = new Thread(new ThreadStart(usbThreads.UsbTxStart));
            usbTxThread.Start();

            usbRxThread = new Thread(new ThreadStart(usbThreads.UsbRxStart));
            usbRxThread.Start();
            usbThreads.Release();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            CloseProcess();
        }

        private void CloseProcess()
        {
            usbThreads.NeedRunning = false;
        }

        private void MonitorTimer_Tick(object sender, EventArgs e)
        {
            if (usb != null)
            {
                usb.TxQueue.Push(new C11());
            }
        }
    }
}
