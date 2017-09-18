using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Windows.Forms;

namespace text1
{
    class @lock
    {
        SerialPort port_lock;
        private string HexStringToString(string hs, Encoding encode)
        {
            string strTemp = "";
            byte[] b = new byte[hs.Length / 2];
            for (int i = 0; i < hs.Length / 2; i++)
            {
                strTemp = hs.Substring(i * 2, 2);
                b[i] = Convert.ToByte(strTemp, 16);
            }
            //按照指定编码将字节数组变为字符串
            return encode.GetString(b);
        }
        //初始化SerialPort对象方法.PortName为COM口名称,例如"COM1","COM2"等,注意是string类型
        public void InitCOM(string PortName)
        {
            port_lock = new SerialPort(PortName);
            port_lock.BaudRate = 9600;//波特率
            port_lock.Parity = Parity.None;//无奇偶校验位
            port_lock.StopBits = StopBits.One;//两个停止位
            //port1.Handshake = Handshake.RequestToSend;//控制协议
            //port1.ReceivedBytesThreshold = 4;//设置 DataReceived 事件发生前内部输入缓冲区中的字节数
            port_lock.DataReceived += new SerialDataReceivedEventHandler(port1_DataReceived);//DataReceived事件委托
        }

        //DataReceived事件委托方法
        private void port1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                StringBuilder currentline = new StringBuilder();
                //循环接收数据
                while (port_lock.BytesToRead > 0)
                {
                    //char ch = (char)port1.ReadByte();
                    //currentline.Append(ch);
                    receive.Add(port_lock.ReadByte());
                }
                //在这里对接收到的数据进行处理
                //receiveData = currentline.ToString();
                //
                //currentline = new StringBuilder();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }

        }

        //打开串口的方法
        public void OpenPort()
        {
            if (!port_lock.IsOpen)
            {
                try
                {
                    port_lock.Open();
                }
                catch { }
            }
            if (port_lock.IsOpen)
            {
                //MessageBox.Show("the port is opened!");
            }
            else
            {
                MessageBox.Show("failure to open the port!");
            }
        }

        //关闭串口的方法
        public void ClosePort()
        {
            port_lock.Close();
            if (!port_lock.IsOpen)
            {
                MessageBox.Show("the port is already closed!");
            }
        }


        //向串口发送数据
        public void SendCommand(string CommandString)
        {
            byte[] WriteBuffer = Encoding.ASCII.GetBytes(CommandString);
            port_lock.Write(WriteBuffer, 0, WriteBuffer.Length);
            //port1.Write(
        }
        //向串口发送数据
        public void SendCommand2(byte[] se)
        {
            port_lock.Write(se, 0, 5);
        }
    }
}
