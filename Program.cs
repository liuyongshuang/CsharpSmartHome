/****************************************************************************
**
** Copyright 2018 by Emotiv. All rights reserved
** Example - AverageBandPowers
** The average band power for a specific channel from the latest epoch with
** 0.5 seconds step size and 2 seconds window size
** This example is used for single connection.
**
****************************************************************************/

using System;
using System.Collections.Generic;
using Emotiv;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Windows.Forms;

namespace AverageBandPowers
{
    class AverageBandPowers
    {              
        static int[] fig=new int[20];
        static double [] Eng=new double[1280];
        static public string state = "中";
        static double engagement;
        static public double En=0;
        static public string eye_state = "无"; 

        static double sum = 0;
        static double[] score=new double[320];
        static int count = 0;

        static EmoEngine engine;                 
        static int userID = -1;

        /// <summary>
        /// 输出文件变量
        /// </summary>
        static string filename = "AverageBandPowers.csv";
        static TextWriter file = new StreamWriter(filename, false);  
                                                  
        static string filename2 = "EEG_Data_Logger.csv"; 
        static TextWriter file2 = new StreamWriter(filename2, false);
        static int grox = 0;

        /// <summary>
        /// 眨眼相关变量
        /// </summary>
        static double THRESHOLD = 6000;
        static int COUNT_FOR_THRESHOLD = 3;
        static bool is_above = false;
        static public bool is_blink = false;
        static int above_cnt = 0;
        static double last_blink = -10;
        static public bool is_double_blink = false;
        
        static EdkDll.IEE_DataChannel_t[] channelList = new EdkDll.IEE_DataChannel_t[5] { EdkDll.IEE_DataChannel_t.IED_AF3, 
                                                                                       EdkDll.IEE_DataChannel_t.IED_AF4, 
                                                                                       EdkDll.IEE_DataChannel_t.IED_T7, 
                                                                                       EdkDll.IEE_DataChannel_t.IED_T8, 
                                                                                       EdkDll.IEE_DataChannel_t.IED_O1 };

        static void engine_UserAdded_Event(object sender, EmoEngineEventArgs e)
        {
            Console.WriteLine("User Added Event has occured");
            userID = (int)e.userId;

            EmoEngine.Instance.IEE_FFTSetWindowingType((uint)userID, EdkDll.IEE_WindowingTypes.IEE_HAMMING);
            
            // enable data aquisition for this user.
            engine.DataAcquisitionEnable((uint)userID, true);

            // ask for up to 1 second of buffered data
            engine.DataSetBufferSizeInSec(1); 
        }
        /// <summary>
        /// 陀螺仪队列添加
        /// </summary>
        /// <param name="陀螺仪数组"></param>
        /// <param name="数组长度"></param>
        void addToQueue(int[] a,int b)
        {
            for(int i =0;i<19;i++)
            {
                a[i] = a[i + 1];
            }
            a[19] = b;
        }               
        int averageQue(int[] a, int b, int c)
        {
            int sum = 0;
            for(int i =b;i< c;i++)
            {
                sum+=a[i];
            }
            return (sum / (c - b));
        }
        /// <summary>
        /// 专注度队列添加
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        void addTOArry(double[] a, double b)
        {
            for (int i = 0; i < 1279; i++)
            {
                a[i] = a[i + 1];
            }
            a[1279] = b;
        }
        double averageArry(double[] a,int b, int c)
        {
            double sum = 0;
            for (int i = b; i < c; i++)
            {
                sum += a[i];
            }
            return (sum / (c - b));
        }
        void Run()
        {
            // 如果不能连接就无法进行
            if ((int)userID == -1)
                return;
            
            Dictionary<EdkDll.IEE_DataChannel_t, double[]> data = engine.GetData((uint)userID);
           
            if (data == null)
            {
                return;
            }
           
            int _bufferSize = data[EdkDll.IEE_DataChannel_t.IED_TIMESTAMP].Length;            
            //Console.WriteLine("Writing " + _bufferSize.ToString() + " lines of data ");

            //定义各个波段存储的变量
            double[] alpha = new double[1];
            double[] low_beta = new double[1];
            double[] high_beta = new double[1];
            double[] gamma = new double[1];
            double[] theta = new double[1];
            double[] beta = new double[1];
           
            for (int i = 0; i < _bufferSize; i++)
            {
                // 可以读写数据                                          
                foreach (EdkDll.IEE_DataChannel_t channel in data.Keys)
                {
                    file.Write(data[channel][i] + ",");//写数据到文件
                    #region 眨眼函数（单双次眨眼）
                    ///************眨眼检测函数***************////
                    double v = data[EdkDll.IEE_DataChannel_t.IED_AF3][i];
                    //Console.WriteLine(data[EdkDll.IEE_DataChannel_t.IED_TIMESTAMP][i]);
                    if (v > THRESHOLD)
                    {
                        if (is_blink) { }
                        else
                        {
                            if (!is_above)
                            {
                                is_above = true;
                                above_cnt = 1;
                            }
                            else
                            {
                                above_cnt++;
                                if (above_cnt >= COUNT_FOR_THRESHOLD)
                                {                                    
                                    double t = data[EdkDll.IEE_DataChannel_t.IED_TIMESTAMP][i];
                                    if ((t - last_blink) < 0.5)
                                    {
                                        //Console.WriteLine("双次眨眼");
                                        is_double_blink = true;
                                        eye_state = "双次眨眼";
                                    }
                                    else
                                    {
                                        is_blink = true;
                                        //Console.WriteLine("单次眨眼");
                                        eye_state = "单次眨眼";
                                    }
                                    last_blink = t; 
                                }                             
                            }
                        }
                    }
                    else
                    {
                        if (is_above)
                        {
                            is_above = false;
                        }
                        if (is_blink)
                        {
                            is_blink = false;
                        }
                        if(is_double_blink)
                        {
                            is_double_blink = false;
                            eye_state = "无";                              
                        }
                    }
                    #endregion                   
                    grox = Convert.ToInt32(data[EdkDll.IEE_DataChannel_t.IED_GYROX][i])-8165;
                    //Console.WriteLine(grox);                    
                    
                }
                #region 陀螺仪函数
                addToQueue(fig, grox);
                if (System.Math.Abs(averageQue(fig, 0, 5)) < 30)
                {
                    if (averageQue(fig, 5, 20) > 50)
                    {
                        state = "左";
                        Console.WriteLine("当前动作： " + state);
                    }
                    else if (averageQue(fig, 5, 20) < -50)
                    {
                        state = "右";
                        Console.WriteLine("当前动作： " + state);
                    }
                    else
                    {
                        state = "中";
                        Console.WriteLine("当前动作： " + state);
                    }
                }
                #endregion
                #region 专注度检测
                for (int j = 0; j < 5; j++)
                {
                    Int32 result = engine.IEE_GetAverageBandPowers((uint)userID, channelList[j], theta, alpha, low_beta, high_beta, gamma);
                    if (result == EdkDll.EDK_OK)
                    {
                        file.Write(theta[0] + ",");
                        file.Write(alpha[0] + ",");
                        file.Write(low_beta[0] + ",");
                        file.Write(high_beta[0] + ",");
                        file.WriteLine(gamma[0] + ",");

                        beta[0] = low_beta[0] + high_beta[0];                       
                        engagement = beta[0] / (alpha[0] + theta[0]);
                        //addTOArry(Eng, engagement);
                        //En = averageArry(Eng,0,1280);                       
                        //En = System.Math.Sqrt(En);                                              
                        //if(flag == 100)
                        //{
                        //    //Console.WriteLine("Theta: " + theta[0] + "    Beta: " + beta[0] + "  B/T: " + (beta[0] / theta[0])); 
                        //    Console.WriteLine(En);
                        //    flag = 0;
                        //}
                        //flag++;  
 
                        score[count] = beta[0] / theta[0];
                        count++;
                        if (count == 320) 
                        { 
                            count = 0; 
                        }
                        if(score[59] != 0)
                        {
                            sum = 0;
                            for(int t = 0;t < 320; t++)
                            {
                                sum = sum + score[t];
                            }
                        }
                        En = System.Math.Sqrt(sum / 10) * 6;

                        while (En > 100)
                        {
                            En = En - 10;
                        }

                        if (En <= 50)
                        {
                            En = 100 * System.Math.Sqrt(En / 50) * 0.5;
                        }
                        else
                        {
                            En = 100 * 0.5 * (1 + ((En - 50) / 50) * ((En - 50) / 50));
                        }
                        En = System.Math.Abs((En - 20) / 0.8)*2;
                        while (En > 100)
                        {
                            En = En - 10;
                        }
                        //Console.WriteLine(En);
                    }
                }
                #endregion
            }           
        }

        public static void FormStart()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        public static void EEGStart()
        {
            Console.WriteLine("===================================================================================");
            Console.WriteLine("Example to get the average band power for a specific channel from the latest epoch.");
            Console.WriteLine("===================================================================================");

            AverageBandPowers p = new AverageBandPowers();

            // create the engine
            engine = EmoEngine.Instance;
            engine.UserAdded += new EmoEngine.UserAddedEventHandler(engine_UserAdded_Event);
            engine.Connect();

            string header = "Theta, Alpha, Low_beta, High_beta, Gamma"; ;            
            string header2 = "COUNTER, INTERPOLATED, RAW_CQ, AF3, F7, F3, FC5, T7, P7, O1, O2, P8," +
                "T8, FC6, F4, F8, AF4, GYROX, GYROY, TIMESTAMP, MARKER_HARDWARE, ES_TIMESTAMP, FUNC_ID, FUNC_VALUE, MARKER, SYNC_SIGNAL";

            file.WriteLine(header);
            file.WriteLine("");
           
            file2.WriteLine(header2);
            file2.WriteLine("");
            //minuteSecond = minute * 60 + second;
            //time = minuteSecond;

            for (int i = 0; i < fig.Length; i++)
            {
                fig[i] = 0;//初始化惯性转量数组
            }
            for (int i = 0; i < Eng.Length;i++ )
            {
                Eng[i] = 0;
            }
            for (int i = 0; i < score.Length;i++)
            {
                score[i]=0;
            }
            while (true)
            {
                engine.ProcessEvents(10);
                if (userID < 0) continue;

                if (Console.KeyAvailable)
                    break;
                p.Run();
                Thread.Sleep(10);
            }

            file.Close();           
            file2.Close();
            engine.Disconnect();
        }
        static void Main(string[] args)
        {
            ThreadStart ts = new ThreadStart(FormStart);
            Thread t = new Thread(ts);
            t.Start();

            ThreadStart ts2 = new ThreadStart(EEGStart);
            Thread t2 = new Thread(ts2);
            t2.Start();            
        }
    }
}
