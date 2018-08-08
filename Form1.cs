using CyUSB;
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
        public enum WorkState
        {
            UnKnown = 0,        // initial value, not defined in protocol
            Initializing = 1,
            StandBy = 2,
            Collecting = 3,
            FluidMaintaining = 4,
            Debug = 5,
            ErrorHandling = 6,
            SamplerMoving = 7,
            Sleeping = 8,
            ShuttingDown = 9,
            FirstPriming = 10,
            Drain = 11,
            EnterSleeping = 12,
            ExitSleeping = 13,
            Disinfection = 14,
            ErrorDiagnosis = 15,
        }
        private USB usb;
        public Form1()
        {
            InitializeComponent();
            usb = USB.UsbInstance;
            usb.intDevice();
            InitializeValue();
        }

        private void InitializeValue()
        {
            PpxBox.Text = "16"; //Set default value to 8 Packets
            QueueBox.Text = "8";
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            CloseProcess();
        }

        private void CloseProcess()
        {
        }

        private void MonitorTimer_Tick(object sender, EventArgs e)
        {
            WorkState workState = WorkState.UnKnown;
            C11 c11 = new C11();
            CommunicationState ComState = usb.SendSymetric(c11, null);
            workState = (WorkState)c11.M1;
            //Console.WriteLine("work state: " + workState.ToString());
        }

        private void PpxBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        static byte DefaultBufInitValue = 0xA5;

        int BufSz;
        int QueueSz;
        int PPX;
        int IsoPktBlockSize;
        private void CalculateParams()
        {
            BufSz = usb.InEndpoint.MaxPktSize * Convert.ToUInt16(PpxBox.Text);
            QueueSz = Convert.ToUInt16(QueueBox.Text);
            PPX = Convert.ToUInt16(PpxBox.Text);

            usb.InEndpoint.XferSize = BufSz;

            if (usb.InEndpoint is CyIsocEndPoint)
                IsoPktBlockSize = (usb.InEndpoint as CyIsocEndPoint).GetPktBlockSize(BufSz);
            else
                IsoPktBlockSize = 0;
        }

        private void AsysnchonousXferData()
        {
            // Setup the queue buffers
            byte[][] cmdBufs = new byte[QueueSz][];
            byte[][] xferBufs = new byte[QueueSz][];
            byte[][] ovLaps = new byte[QueueSz][];

            ISO_PKT_INFO[][] pktsInfo = new ISO_PKT_INFO[QueueSz][];


        }
    }
}
