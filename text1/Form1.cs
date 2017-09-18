using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;
using System.Data.SqlClient;
using text1.Properties;

namespace text1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        #region 
        SerialPort port1;//串口
        List<int> receive;//接收到的数据
        byte[] sendData;//发送的数据
        string constr;
        SqlConnection con;//数据库连接
        string userName = "";
        Boxs[] boxs;
        #endregion
        
        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.Text != "" && comboBox2.Text != "")
            {
                byte[] a = new byte[5];
                a[0] = 0x80;
                a[1] = byte.Parse(comboBox1.Text);
                a[2] = byte.Parse(comboBox2.Text);
                a[3] = 0x33;
                int b = a[0] ^ a[1] ^ a[2] ^ a[3];
                string c = b.ToString("x2");
                a[4] = (byte)b;
                sendData = a;
                string send = a[0].ToString("x2") + a[1].ToString("x2") + a[2].ToString("x2") + a[3].ToString("x2") + c;
                string m = HexStringToString(send, System.Text.Encoding.UTF8);
            }
        }

        public void cacu_sendopen(int cardNo,int boxNo)
        {
            byte[] a = new byte[5];
            a[0] = 0x8A;
            a[1] = (byte)cardNo;
            a[2] = (byte)boxNo;
            a[3] = 0x11;
            int b = a[0] ^ a[1] ^ a[2] ^ a[3];
            string c = b.ToString("x2");
            a[4] = (byte)b;
            sendData = a;
            string send = a[0].ToString("x2") + a[1].ToString("x2") + a[2].ToString("x2") + a[3].ToString("x2") + c;
            string m = HexStringToString(send, System.Text.Encoding.UTF8);
        }

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


        //定义 SerialPort对象
         
          //初始化SerialPort对象方法.PortName为COM口名称,例如"COM1","COM2"等,注意是string类型
          public void InitCOM(string PortName)
          {
              port1 = new SerialPort(PortName);
              port1.BaudRate = 9600;//波特率
              port1.Parity  = Parity.None;//无奇偶校验位
              port1.StopBits = StopBits.One;//两个停止位
              //port1.Handshake = Handshake.RequestToSend;//控制协议
              //port1.ReceivedBytesThreshold = 4;//设置 DataReceived 事件发生前内部输入缓冲区中的字节数
              port1.DataReceived += new SerialDataReceivedEventHandler(port1_DataReceived);//DataReceived事件委托
          }

          //DataReceived事件委托方法
          private void port1_DataReceived(object sender, SerialDataReceivedEventArgs e)
          {
              try
              {
                  StringBuilder currentline = new StringBuilder();
                  //循环接收数据
                  while (port1.BytesToRead > 0)
                  {
                      //char ch = (char)port1.ReadByte();
                      //currentline.Append(ch);
                      receive.Add(port1.ReadByte());
                  }
                  //在这里对接收到的数据进行处理
                  //receiveData = currentline.ToString();
                  //
                  //currentline = new StringBuilder();
                  
              }
              catch(Exception ex)
              {
                  Console.WriteLine(ex.Message.ToString());
              }
  
          }
  
          //打开串口的方法
          public void OpenPort()
          {
              if (!port1.IsOpen)
              {
                  try
                  {
                      port1.Open();
                  }
                  catch { }
              }
              if (port1.IsOpen)
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
              port1.Close();
              if (!port1.IsOpen)
              {
                  MessageBox.Show("the port is already closed!");
              }
          }
  
          //向串口发送数据
          public void SendCommand(string CommandString)
          {
              byte[] WriteBuffer = Encoding.ASCII.GetBytes(CommandString);
              port1.Write(WriteBuffer, 0, WriteBuffer.Length);
              //port1.Write(
          }
          //向串口发送数据
          public void SendCommand2(byte[] se)
          {
              port1.Write(se, 0, 5); 
          }
         
         //调用实例
         private void btOpen_Click(object sender, EventArgs e)
         {
             //我现在用的COM5端口,按需要可改成COM2,COM3
             if (cbComm.Text != "")
             {
                 try {
                     InitCOM(cbComm.Text);
                     OpenPort();
                     Settings.Default.lastComm = cbComm.Text;
                     Settings.Default.Save();
                 }
                 catch { MessageBox.Show("打开串口失败"); }
             }
             
         }

         private void button2_Click(object sender, EventArgs e)
         {
             SendCommand2(sendData);
             Thread.Sleep(500);
             if (receive.Count > 4)
             {
                 if (receive[0] == sendData[0] && receive[1] == sendData[1] && receive[2] == sendData[2])
                 {
                     if (receive[3] == 00)
                     {
                         richTextBox1.AppendText(sendData[1].ToString() + "号柜子的" + sendData[2].ToString() + "号锁是关着的" + "\r\n");
                     }
                     else
                         richTextBox1.AppendText(sendData[1].ToString() + "号柜子的" + sendData[2].ToString() + "号锁是开着的" + "\r\n");
                 }
             }
             //if (receive.Count > 4)
             //{
             //    richTextBox1.AppendText(receive[0].ToString() + receive[1].ToString() + receive[2].ToString() + receive[3].ToString() + receive[4].ToString());
             //}
             receive.Clear();
         }

         private void button3_Click(object sender, EventArgs e)
         {
             ClosePort();
         }

         private void Form1_Load(object sender, EventArgs e)
         {
             try
             {
                 InitCOM(Settings.Default.lastComm);
                 OpenPort();
             }
             catch { MessageBox.Show("打开串口失败"); }
             tbUser.Text = Settings.Default.lastUser;
             tabPage1.Parent = null;
             tabPage2.Parent = null;
             tabPage4.Parent = null;
             receive = new List<int>();
             sendData = new byte[5];
             constr = "server=127.0.0.1;database=feipingui;uid=sa1;pwd=sa";
             con = new SqlConnection(constr);
             try
             {
                 con.Open();
             }
             catch
             { MessageBox.Show("连接数据库失败"); }
             #region
             resetButtons();
             boxs = new Boxs[30];
             for (int k = 0; k < 30; k++)//开始遍历100个按钮
             {
                 if (this.tabPage2.Controls.ContainsKey(//窗体中是否有此按钮
                     (k).ToString()))
                 {
                     for (int L = 0; L < this.tabPage2.Controls.Count; L++)//遍历窗体控件集合
                     {
                         if (this.tabPage2.Controls[L].Name == //是否查找到按钮
                             (k).ToString())
                         {
                             boxs[k] = new Boxs();
                             boxs[k].buttonNo = L;
                             break;
                         }
                     }
                 }
             }
             #endregion
         }

         public void resetButtons()
         {
             RemoveControl();//清空所有无用对象
             int p_int_x = 10;//X坐标初始值为10
             int p_int_y = 60;//Y坐标初始值为60
             for (int i = 0; i < 18; i++)//添加100个按钮
             {
                 Button bt = new Button();//创建button按钮
                 //bt.Text = (i + 1).ToString();//设置button按钮的文本值
                 bt.Name = (i + 1).ToString();//设置button按钮的Name属性
                 bt.Width = 35;//设置button按钮的宽
                 bt.Height = 35;//设置button按钮的高
                 bt.Location = new Point(p_int_x, p_int_y);//设置button按钮的位置
                 bt.Click += new EventHandler(bt_Click);//定义button按钮的事件
                 //bt.MouseDown += new MouseEventHandler(bt_MouseDown);//定义button按钮的事件
                 p_int_x += 36;//设置下一个按钮的位置
                 bt.Text = (i + 1).ToString();
                 if ((i + 1) % 6 == 0)//设置换行
                 {
                     p_int_x = 10;//换行后重新设置X坐标
                     p_int_y += 36;//换行后重新设置Y坐标
                 }
                 this.tabPage2.Controls.Add(bt);//将button按钮放入窗体控件集合中
             }
         }
         private void button4_Click(object sender, EventArgs e)
         {
             RemoveControl();//清空所有无用对象
             int p_int_x = 10;//X坐标初始值为10
             int p_int_y = 60;//Y坐标初始值为60
             for (int i = 0; i < 18; i++)//添加100个按钮
             {
                 Button bt = new Button();//创建button按钮
                 //bt.Text = (i + 1).ToString();//设置button按钮的文本值
                 bt.Name = (i + 1).ToString();//设置button按钮的Name属性
                 bt.Width = 35;//设置button按钮的宽
                 bt.Height = 35;//设置button按钮的高
                 bt.Location = new Point(p_int_x, p_int_y);//设置button按钮的位置
                 bt.Click += new EventHandler(bt_Click);//定义button按钮的事件
                 //bt.MouseDown += new MouseEventHandler(bt_MouseDown);//定义button按钮的事件
                 p_int_x += 36;//设置下一个按钮的位置
                 bt.Text = (i + 1).ToString();
                 if ((i + 1) % 3 == 0)//设置换行
                 {
                     p_int_x = 10;//换行后重新设置X坐标
                     p_int_y += 36;//换行后重新设置Y坐标
                 }
                 this.tabPage2.Controls.Add(bt);//将button按钮放入窗体控件集合中
             }
         }


         void bt_Click(object sender, EventArgs e)
         {
             if (tbBoxCode.Text != "")
             {
                 string sql = "select count(*) from store where BoxCode='" + tbBoxCode.Text + "'";
                 SqlCommand com = new SqlCommand(sql, con);
                 try
                 {
                     int x = (int)com.ExecuteScalar();
                     if (x == 0)
                     {

                         MessageBox.Show("该瓶子不存在");
                     }
                     else
                     {
                         MessageBox.Show("该瓶子已存在");
                         return;
                     }
                 }
                 catch (Exception)
                 {
                     throw;
                 }
                 Control P_control = sender as Control;//将sender转换为control类型对象
                 int num = int.Parse(P_control.Name) - 1;
                 int CNO = num / 18 + 1;
                 int BNO = num % 18 + 1;
                 int line = (BNO - 1) / 6 + 1;
                 int cell = (BNO - 1) % 6 + 1;
                 sql = "select count(*) from store where Number='" + CNO.ToString() + "' and line='" + line.ToString() + "' and cell='" + cell.ToString() + "' AND box_status=100 and information_status = 0";
                 com = new SqlCommand(sql, con);
                 try
                 {
                     int x = (int)com.ExecuteScalar();
                     if (x == 0)
                     {

                         MessageBox.Show("该柜子是空的");
                     }
                     else
                     {
                         MessageBox.Show("该柜子不是空的");
                         return;
                     }
                 }
                 catch (Exception)
                 {
                     throw;
                 }
                 sql = "INSERT INTO store (BoxCode,Number,line,cell,storetime,operate_store,box_status,information_status,duedate) VALUES"
                     + "('" + tbBoxCode.Text + "','" + CNO.ToString() + "','" + line.ToString() + "','" + cell.ToString() + "','" + System.DateTime.Now.ToString() + "','" + userName + "',100,0,'" + DateTime.Now.AddDays(7).ToString() +"')";
                 com = new SqlCommand(sql, con);
                 com.ExecuteNonQuery();
                 com.Dispose();
                 cacu_sendopen(CNO, BNO);
                 receive.Clear();
                 SendCommand2(sendData);
                 Thread.Sleep(1000);
                 if (receive.Count > 4)
                 {
                     if (receive[0] == sendData[0] && receive[1] == sendData[1] && receive[2] == sendData[2])
                     {
                         if (receive[3] == 00)
                         {
                             richTextBox1.AppendText(sendData[1].ToString() + "号柜子的" + sendData[2].ToString() + "号锁打开失败" + "\r\n");
                             this.tabPage2.Controls[boxs[num+1].buttonNo].BackColor = Color.Transparent;
                         }
                         else
                         {
                             richTextBox1.AppendText(sendData[1].ToString() + "号柜子的" + sendData[2].ToString() + "号锁打开成功" + "\r\n");
                             this.tabPage2.Controls[boxs[num + 1].buttonNo].BackColor = Color.Red;
                         }
                     }
                 }

                 receive.Clear();
             }
             else
             { MessageBox.Show("请先扫码"); }
         }

         /// <summary>
         /// 用于清空窗体中动态生成的按钮
         /// </summary>
         void RemoveControl()
         {
             for (int i = 0; i < 100; i++)//开始遍历100个按钮
             {
                 if (this.tabPage2.Controls.ContainsKey(//窗体中是否有此按钮
                     (i + 1).ToString()))
                 {
                     for (int j = 0; j < this.tabPage2.Controls.Count; j++)//遍历窗体控件集合
                     {
                         if (this.tabPage2.Controls[j].Name == //是否查找到按钮
                             (i + 1).ToString())
                         {
                             this.tabPage2.Controls.RemoveAt(j);//删除指定按钮
                             break;
                         }
                     }
                 }
             }
         }

         private void button5_Click(object sender, EventArgs e)
         {

             sendData[0] = 0x80;
             sendData[1] = 0x01;
             sendData[2] = 0x00;
             sendData[3] = 0x33;
             sendData[4] = 0xb2;
            receive.Clear();
            SendCommand2(sendData);
            Thread.Sleep(500);
            if (receive.Count > 6)
            {
                if (receive[0] == sendData[0] && receive[1] == sendData[1])
                {
                    for (int i = 0; i < 3; i++)
                    {
                        int getData = receive[4 - i];
                        for (int j = 1; j < 9; j++)
                        {
                            if (i * 8 + j < 19)
                            {
                                int Box_status = getData % 2;
                                getData = getData / 2;
                                if (Box_status == 1)
                                {
                                    //for (int k = 0; k < 18; k++)//开始遍历100个按钮
                                    //{
                                    //    if (this.tabPage2.Controls.ContainsKey(//窗体中是否有此按钮
                                    //        (i * 8 + j + 1).ToString()))
                                    //    {
                                    this.tabPage2.Controls[boxs[i * 8 + j].buttonNo].BackColor = Color.Red;
                                    //for (int L = 0; L < this.tabPage2.Controls.Count; L++)//遍历窗体控件集合
                                    //{
                                    //    if (this.tabPage2.Controls[L].Name == //是否查找到按钮
                                    //        (i * 8 + j + 1).ToString())
                                    //    {
                                    //        this.tabPage2.Controls[L].BackColor = Color.Red;
                                    //        this.tabPage2.Controls[L].Enabled = false;
                                    //        break;
                                    //    }
                                    //}
                                    //    }
                                    //}
                                }
                                else
                                {
                                    this.tabPage2.Controls[boxs[i * 8 + j].buttonNo].BackColor = Color.Transparent;
                                }
                            }
                        }
                    }
                }   
             }
             receive.Clear();

            
         }
       
         private void button6_Click(object sender, EventArgs e)
         {
            
             //constr = "server=127.0.0.1;database=feipingui;uid=sa1;pwd=sa";
             //SqlConnection con = new SqlConnection(constr);
             string sql = "select count(*) from zhanghu where [user]='"+tbUser.Text+"' and password='"+tbPassword.Text+"'";
             SqlCommand com = new SqlCommand(sql, con);
             try
             {
                 //con.Open();
                 //MessageBox.Show("成功连接数据库");
                 int x = (int)com.ExecuteScalar();
                 if (x == 1)
                 {
                     userName = tbUser.Text;
                     tabPage4.Parent = tabControl1;
                     tabPage2.Parent = tabControl1;
                     tabPage1.Parent = tabControl1;
                     label4.Text = userName;
                     Settings.Default.lastUser = userName;
                     Settings.Default.Save();
                     tbPassword.Clear();
                     tabPage3.Parent = null;
                     MessageBox.Show("欢迎:"+tbUser.Text);
                 }
                 else
                     MessageBox.Show("登入失败。");
                 //MessageBox.Show(string.Format("成功读取{0},条记录", x));
             }
             catch (Exception)
             {
                 throw;
             }
             finally
             {
                 //con.Close();
                 //MessageBox.Show("成功关闭数据库连接", "提示信息", MessageBoxButtons.YesNoCancel);
             }
         }

         private void Form1_FormClosing(object sender, FormClosingEventArgs e)
         {
             con.Close();
         }

         private void label4_Click(object sender, EventArgs e)
         {
             if (userName != "")
             {
                 if (MessageBox.Show("是否退出登入", "Confirm Message", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.OK)
                 {
                     userName = "";
                     label4.Text = "未登入";
                     tabPage3.Parent = tabControl1;
                     tabPage1.Parent = null;
                     tabPage2.Parent = null;
                     tabPage4.Parent = null;
                 }
             }
         }

         private void button7_Click(object sender, EventArgs e)
         {
             //SELECT BoxCode as 瓶子码, Number AS 柜号, line AS 行号, cell AS 列号, storetime AS 保存时间, duedate AS 到期日期 FROM store WHERE box_status=100 and information_status=0
             string sql = "SELECT BoxCode as 瓶子码, Number AS 柜号, line AS 行号, cell AS 列号, storetime AS 保存时间, duedate AS 到期日期 FROM store WHERE box_status=100 and information_status=0";
             SqlCommand scmd = new SqlCommand(sql, con);
             SqlDataAdapter sda = new SqlDataAdapter(scmd);
             DataSet ds = new DataSet();
             sda.Fill(ds, "Table");
             dataGridView1.DataSource = ds.Tables["Table"];
         }

         private void button8_Click(object sender, EventArgs e)
         {
             for (int i = 0; i < dataGridView1.RowCount; i++)
             {
                 if (dataGridView1.Rows[i].Cells[0].Value!=null)
                 {
                     if (dataGridView1.Rows[i].Cells[0].Value.ToString() == "True")
                     {
                         int totalNo = int.Parse(dataGridView1.Rows[i].Cells["柜号"].Value.ToString()) * 18 + int.Parse(dataGridView1.Rows[i].Cells["行号"].Value.ToString()) * 6 + int.Parse(dataGridView1.Rows[i].Cells["列号"].Value.ToString()) - 24;
                         int num = totalNo - 1;
                         int CNO = num / 18 + 1;
                         int BNO = num % 18 + 1;
                         cacu_sendopen(CNO, BNO);
                         receive.Clear();
                         SendCommand2(sendData);
                         Thread.Sleep(1000);
                         string sql = "UPDATE store SET box_status = -100 , information_status=-100 , scraptime='" + System.DateTime.Now.ToString() + "' , operate_scrap='" + userName + "'"
                             + "WHERE box_status=100 and information_status=0 and BoxCode='"+dataGridView1.Rows[i].Cells["瓶子码"].Value.ToString()+"'";
                         SqlCommand com = new SqlCommand(sql, con);
                         com.ExecuteNonQuery();
                         com.Dispose();
                         if (receive.Count > 4)
                         {
                             if (receive[0] == sendData[0] && receive[1] == sendData[1] && receive[2] == sendData[2])
                             {
                                 if (receive[3] == 00)
                                 {
                                     richTextBox1.AppendText(sendData[1].ToString() + "号柜子的" + sendData[2].ToString() + "号锁打开失败" + "\r\n");
                                     this.tabPage2.Controls[boxs[num + 1].buttonNo].BackColor = Color.Transparent;
                                 }
                                 else
                                 {
                                     richTextBox1.AppendText(sendData[1].ToString() + "号柜子的" + sendData[2].ToString() + "号锁打开成功" + "\r\n");
                                     this.tabPage2.Controls[boxs[num + 1].buttonNo].BackColor = Color.Red;
                                 }
                             }
                         }

                         receive.Clear();
                     }
                 }
             }
         }


    
    }
}
