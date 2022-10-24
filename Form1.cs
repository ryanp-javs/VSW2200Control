using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VSW2200Control
{
    public partial class Form1 : Form
    {
        private SerialPort port = null;
        private Timer cycleTimer = new Timer();
        private string portName = "COM1";
        private int currentInput = 0;
        private int lowInput = 0;
        private int highInput = 3;

        public Form1()
        {
            InitializeComponent();
            // Attach a method to be called when there
            // is data waiting in the port's buffer
            nudLowInput.Value = int.Parse(System.Configuration.ConfigurationManager.AppSettings["LowInput"]);
            nudHighInput.Value = int.Parse(System.Configuration.ConfigurationManager.AppSettings["HighInput"]);
            portName = System.Configuration.ConfigurationManager.AppSettings["COMPORT"];
            numericUpDown1.Value = int.Parse(System.Configuration.ConfigurationManager.AppSettings["Interval"]);
            currentInput = highInput;

            cycleTimer.Interval = (int)numericUpDown1.Value * 1000;
            cycleTimer.Tick += CycleTimer_Tick;
            cycleTimer.Start();
            CycleTimer_Tick(this, EventArgs.Empty);

            timer1.Interval = 1000;
            timer1.Start();
        }

        private void CycleTimer_Tick(object sender, EventArgs e)
        {
            if (currentInput < highInput)
                SetInput(currentInput + 1);
            else
                SetInput(lowInput);
        }

        void OpenPort()
        {
            if (port == null)
            {
                port = new SerialPort(portName, 115200, Parity.None, 8, StopBits.One);
                port.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
            }
            if (!port.IsOpen)
                port.Open();
        }

        void ClosePort()
        {
            if (port.IsOpen)
                port.Close();
        }
        string rcv = "";
        private void port_DataReceived(object sender,
  SerialDataReceivedEventArgs e)
        {
            // Show all the incoming data in the port's buffer
            rcv = port.ReadExisting();
        }


        void SetInput(int input)
        {
            if (input > 3 || input < 0)
                return;
            List<byte> cmd = new List<byte>();
            cmd.Add(0x05);
            cmd.Add(0x90);
            cmd.Add(0x00);
            cmd.Add((byte)input);
            int chksum = 0;
            for (int i = 0; i < cmd.Count; i++)
            {
                chksum = chksum + cmd[i];
            }
            cmd.Add((byte)chksum);
            OpenPort();
            port.Write(cmd.ToArray<byte>(), 0, cmd.Count);
            currentInput = input;
            lblCurrentInput.Text = currentInput.ToString();
            //Command Acknowledgment
            //0 ACK Length 0x03
            //1 ACK 0x81
            //2 ACK Check sum 0x84
            //3 ACK Length 0x03
            //4 ACK 0x82
            //5 ACK Check sum 0x85
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            cycleTimer.Stop();
            cycleTimer.Interval = (int)numericUpDown1.Value * 1000;
            cycleTimer.Start();
        }

        private void nudLowInput_ValueChanged(object sender, EventArgs e)
        {
            if (nudLowInput.Value > nudHighInput.Value)
                nudLowInput.Value = nudHighInput.Value;
            lowInput = (int)nudLowInput.Value;
        }

        private void nudHighInput_ValueChanged(object sender, EventArgs e)
        {
            if (nudHighInput.Value < nudLowInput.Value)
                nudHighInput.Value = nudLowInput.Value;
            highInput = (int)nudHighInput.Value;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                char[] charValues = rcv.ToCharArray();
                string hexOutput = "";
                foreach (char _eachChar in charValues)
                {
                    // Get the integral value of the character.
                    int value = Convert.ToInt32(_eachChar);
                    // Convert the decimal value to a hexadecimal value in string form.
                    hexOutput += "0x" + String.Format("{0:X}", value) + " ";
                    // to make output as your eg 
                    //  hexOutput +=" "+ String.Format("{0:X}", value);

                }

                lblrcv.Text = hexOutput;
            }
            catch { }
        }
    }
}
