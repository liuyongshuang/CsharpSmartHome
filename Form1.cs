using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CCWin;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;
using System.IO;


namespace AverageBandPowers
{
    public partial class Form1 : CCSkinMain
    {
        static int select_mod1 = 0;
        static int select_mod2 = 0;
        
        #region 设备的初始状态
        static public string fan = "关";
        static public string led = "关";
        static public string jiashi = "关";
        static public string hand = "无";
        #endregion

        static public string state_homecom;        
        static public string state_handcom;
        static int p_x, p_y;        
        
        #region 菜单按钮刷新界面变量
        UserControl_fan u1 = new UserControl_fan();
        UserControl_led u2 = new UserControl_led();
        UserControl_jiashi u3 = new UserControl_jiashi();
        UserControl_hand u4 = new UserControl_hand();
        UserControl_switch u5 = new UserControl_switch();
        #endregion
       
        public Form1()
        {
            InitializeComponent();
            state_homecom = TextBox_homecom.Text;
            state_handcom = TextBox_handcom.Text;            
        }
  
        /// <summary>
        /// 开始按钮，移动到风扇位置作为初始状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
    
        private void button_start_Click(object sender, EventArgs e)
        {                      
            p_x = this.Location.X+44;
            p_y = this.Location.Y+77;
            SetCursorPos(p_x, p_y);//移动到风扇位置
                                    
            timer_select.Enabled = true;//启动选择设备时间控件
            timer1.Enabled = true;//启动显示用户状态数据
        }                                   
        enum MouseEventFlag : uint
        {
            Move = 0x0001,
            LeftDown = 0x0002,
            LeftUp = 0x0004,
            RightDown = 0x0008,
            RightUp = 0x0010,
            MiddleDown = 0x0020,
            MiddleUp = 0x0040,
            XDown = 0x0080,
            XUp = 0x0100,
            Wheel = 0x0800,
            VirtualDesk = 0x4000,
            Absolute = 0x8000
        }
        [DllImport("user32.dll")]
        public extern static void SetCursorPos(int x, int y);//声明外部函数，移动到x，y位置

        [DllImport("User32")]
        public extern static bool GetCursorPos(ref Point lpPoint);
        Point p = new Point(100, 100);

        [DllImport("user32.dll")]  
        static extern void mouse_event(MouseEventFlag flags, int dx, int dy, uint data, UIntPtr extraInfo);

        /// <summary>
        /// 左单击鼠标事件
        /// </summary>
        public void Left_Click()
        {
            mouse_event(MouseEventFlag.Move, 0, 0, 0, UIntPtr.Zero);
            //执行左键按下 
            mouse_event(MouseEventFlag.LeftDown, 0, 0, 0, UIntPtr.Zero);
            //执行左键松开 
            mouse_event(MouseEventFlag.LeftUp, 0, 0, 0, UIntPtr.Zero);
            //完成一个单击动作 
            //之后执行以上代码便可模似鼠标单击一次 
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            state_homecom = TextBox_homecom.Text;
            state_handcom = TextBox_handcom.Text;   
        }
        /// <summary>
        /// 显示用户状态数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {                        
            Label_userState.Text = AverageBandPowers.state;          
            state_homecom = TextBox_homecom.Text;
            state_handcom = TextBox_handcom.Text;
            Label_fan.Text = fan;
            Label_led.Text = led;
            Label_jiashi.Text = jiashi;
            Label_hand.Text = hand;                      
            Label_eye.Text = AverageBandPowers.eye_state;

            GetCursorPos(ref p);
            this.Text = "脑电智能家居控制终端    (X:" + p.X + "  Y:" + p.Y + ")";
        }
        /// <summary>
        /// 专注度判断并启动家居界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_leftclick_Tick(object sender, EventArgs e)
        {
            //Thread.Sleep(3000);                                
            if (AverageBandPowers.En > 65)
            {
                Left_Click();                  
                timer_leftclick.Enabled = false;
            }            
        }
        /// <summary>
        /// 风扇菜单按钮刷新界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_fan_Click(object sender, EventArgs e)
        {
            panel1.Controls.Remove(u2);
            panel1.Controls.Remove(u3);
            panel1.Controls.Remove(u4);
            panel1.Controls.Remove(u5);
            panel1.Controls.Add(u1);
        }

        /// <summary>
        /// 台灯菜单按钮刷新界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_led_Click(object sender, EventArgs e)
        {
            panel1.Controls.Remove(u1);
            panel1.Controls.Remove(u3);
            panel1.Controls.Remove(u4);
            panel1.Controls.Remove(u5);
            panel1.Controls.Add(u2);
        }

        /// <summary>
        /// 加湿器菜单按钮刷新界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_jiashi_Click(object sender, EventArgs e)
        {
            panel1.Controls.Remove(u1);
            panel1.Controls.Remove(u2);
            panel1.Controls.Remove(u4);
            panel1.Controls.Remove(u5);
            panel1.Controls.Add(u3);
        }

        /// <summary>
        /// 机械手菜单按钮刷新界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_hand_Click(object sender, EventArgs e)
        {
            panel1.Controls.Remove(u1);
            panel1.Controls.Remove(u2);
            panel1.Controls.Remove(u3);
            panel1.Controls.Remove(u5);
            panel1.Controls.Add(u4);
        }

        /// <summary>
        /// 总开关菜单按钮刷新界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_switch_Click(object sender, EventArgs e)
        {
            panel1.Controls.Remove(u1);
            panel1.Controls.Remove(u2);
            panel1.Controls.Remove(u3);
            panel1.Controls.Remove(u4);
            panel1.Controls.Add(u5);
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_close_Click(object sender, EventArgs e)
        {                       
            this.Close();
        }

        /// <summary>
        /// 选择设备菜单按钮判断函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_select_Tick(object sender, EventArgs e)
        {            
            if(AverageBandPowers.state=="左")
            {
                if (select_mod1 == 4)
                    select_mod1 = 0;
                else 
                    select_mod1++;
            }
            if(AverageBandPowers.state=="右")
            {
                if (select_mod1 == 0)
                    select_mod1 = 4;
                else 
                    select_mod1--;
            }
            switch(select_mod1)
            {
                case 0:
                    p_x = this.Location.X+44;
                    p_y = this.Location.Y+77;
                    SetCursorPos(p_x, p_y);
                    Thread.Sleep(370);
                    break;
                case 1:
                    p_x = this.Location.X+44;
                    p_y = this.Location.Y+77+25;
                    SetCursorPos(p_x, p_y);
                    Thread.Sleep(370);
                    break;
                case 2:
                    p_x = this.Location.X+44;
                    p_y = this.Location.Y+77+25*2;
                    SetCursorPos(p_x, p_y);
                    Thread.Sleep(370);
                    break;
                case 3:
                    p_x = this.Location.X+44;
                    p_y = this.Location.Y+77+25*3;
                    SetCursorPos(p_x, p_y);
                    Thread.Sleep(370);
                    break;
                case 4:                    
                    p_x = this.Location.X+44;
                    p_y = this.Location.Y+77+25*4;
                    SetCursorPos(p_x, p_y);
                    Thread.Sleep(370);
                    break;
                default:
                    p_x = this.Location.X+44;
                    p_y = this.Location.Y+77;
                    SetCursorPos(p_x, p_y);
                    Thread.Sleep(370);
                    break;
            }
            if(AverageBandPowers.is_blink)
            {
                Left_Click();
                p_x = this.Location.X + 253;
                p_y = this.Location.Y + 83;
                Thread.Sleep(10);
                ///移到模式2开启位置///
                p_x = this.Location.X + 184;
                p_y = this.Location.Y + 153;
                SetCursorPos(p_x, p_y);
                timer_select.Enabled = false;
                timer_selectmod2.Enabled = true;
                Thread.Sleep(50);
            }
            if(AverageBandPowers.is_double_blink)
            {
                p_x = this.Location.X + 708;
                p_y = this.Location.Y + 309;
                SetCursorPos(p_x, p_y);

                Thread.Sleep(10);
                Left_Click();//关闭窗口
            }
        }

        /// <summary>
        /// 端口确认按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_comfin_Click(object sender, EventArgs e)
        {
            if (TextBox_homecom.Text == "" && TextBox_handcom.Text == "")
            {
                MessageBox.Show("请输入端口，格式：COM+数字", "提示");
            }
            else
            {
                p_x = this.Location.X + 708;                
                p_y = this.Location.Y + 77;
                //p_x = button_start.Location.X;
                //p_y = button_start.Location.Y;

                SetCursorPos(p_x, p_y);
                timer_leftclick.Enabled = true;
            }                
        }

        private void timer_selectmod2_Tick(object sender, EventArgs e)
        {           
            
            if (AverageBandPowers.state == "左")
            {
                if(select_mod2==0)
                {
                    select_mod2 = 2;
                }
                else
                {
                    select_mod2--;
                }                               
            }
            if (AverageBandPowers.state == "右")
            {
                if(select_mod2==2)
                {
                    select_mod2 = 0;
                }
                else
                {
                    select_mod2++;
                }                
            }   
            switch(select_mod2)
            {
                case 0:
                    p_x = this.Location.X + 184;
                    p_y = this.Location.Y + 153;//开                    
                    SetCursorPos(p_x, p_y);
                    Thread.Sleep(370);
                    break;
                case 1:
                    p_x = this.Location.X + 263;
                    p_y = this.Location.Y + 153;//退出
                    SetCursorPos(p_x, p_y);
                    Thread.Sleep(370);
                    break;
                case 2:
                    p_x = this.Location.X + 344;
                    p_y = this.Location.Y + 153;//关
                    SetCursorPos(p_x, p_y);
                    Thread.Sleep(370);
                    break;
            }
          
            if(AverageBandPowers.is_blink)
            {
                Left_Click();               
            }
            
           
                if (AverageBandPowers.is_double_blink)
                {
                    p_x = this.Location.X + 44;
                    p_y = this.Location.Y + 77;
                    SetCursorPos(p_x, p_y);
                    Label_eye.Text = "双次眨眼";
                    timer_selectmod2.Enabled = false;
                    timer_select.Enabled = true;
                }         
                      
        }        

        /// <summary>
        /// 显示专注度数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_displayen_Tick(object sender, EventArgs e)
        {
            Label_eng.Text = Convert.ToSingle(AverageBandPowers.En).ToString();
        }

        /// <summary>
        /// 强制转换double数据
        /// </summary>
        /// <param name="InStr"></param>
        /// <returns></returns>
        private double ConvertToDouble(string InStr)
        {
            try
            {
                return Convert.ToDouble(InStr);
            }
            catch
            {
                return 0; //转换错误返回 0
            }
        }              
    }
}
