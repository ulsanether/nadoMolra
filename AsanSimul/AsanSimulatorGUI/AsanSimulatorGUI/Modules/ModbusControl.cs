using FluentModbus;
using multimediatimer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO.Ports;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace AsanSimulatorGUI.Modules
{
    public class ModbusControl
    {
        public delegate void firmware_ver_event();
        public event firmware_ver_event firmware_ver_get;
        public delegate void update_real_event();
        public event update_real_event update_real;

        public delegate void update_comport_event(string[] comports);
        public event update_comport_event update_comport;
        string[] beforecomport;

        ConcurrentQueue<FCUData> write_queue;

        FCUData fcudata;

        Thread modbus_thread;
        long interval = 100;
        volatile bool flag_modbus_control_thread = false;
        volatile bool flag_close_thread = false;

        ModbusRtuClient modbusRtuClient = new ModbusRtuClient();
        MultiMediaTimer timer = new MultiMediaTimer(); // comport check timer이다.
        byte slave_id = 0x01;


        public ModbusControl(FCUData fcudata, ConcurrentQueue<FCUData> write_queue)
        {
            this.write_queue = write_queue;
            this.fcudata = fcudata;
            init_timer();
            init_modbus();
        }

        void init_modbus()
        {
            modbusRtuClient.BaudRate = 9600;
            modbusRtuClient.Parity = System.IO.Ports.Parity.None;
            modbusRtuClient.StopBits = System.IO.Ports.StopBits.One;
            modbusRtuClient.WriteTimeout = 1000;
            modbusRtuClient.ReadTimeout = 1000;
        }

        public bool modbus_isconnection()
        {
            return modbusRtuClient.IsConnected;
        }

        public bool connect_modbus(string comport)
        {
            if (!modbusRtuClient.IsConnected)
            {
                flag_modbus_control_thread = true;
                //TODO 이부분은 통신해보고 엔디안 체크 필요함
                modbusRtuClient.Connect(comport, ModbusEndianness.BigEndian);
                modbus_thread = new Thread(task_modbus);
                modbus_thread.IsBackground = true;
                modbus_thread.Start();
                return true;
            }
            return false;
        }

        public bool disconnect_modbus()
        {
            if (modbusRtuClient.IsConnected)
            {
                flag_modbus_control_thread = false;
                while (flag_close_thread) ;// thread 종료 대기
                flag_close_thread = false;
                modbusRtuClient.Close();
                //thread 제거처리
                return true;
            }
            return false;
        }

        void init_timer()
        {
            timer.Interval = 500;
            timer.Resolution = 100;
            timer.Elapsed += Timer_Elapsed;
            if (!timer.IsRunning)
            {
                timer.Start();
            }
        }

        private void Timer_Elapsed(object sender, EventArgs e)
        {
            //string[] comports = SerialPort.GetPortNames();

            //if ((beforecomport == null) || !Enumerable.SequenceEqual(comports, beforecomport))
            //{
            //    update_comport(comports);
                
            //    beforecomport = new string[comports.Length];
            //    for (int i = 0; i < comports.Length - 1; i++)
            //    {
            //        beforecomport[i] = comports[i];
            //    }
            //}
        }

        void task_modbus()
        {
            long delay = 0;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Span<byte> readdata = modbusRtuClient.ReadHoldingRegisters(slave_id, (UInt16)fcu_modbus.version, 1);
            fcudata.get_fcu_version(readdata);
            firmware_ver_get();
            stopwatch.Stop();

            delay = interval - stopwatch.ElapsedMilliseconds;
            if (delay >= 0)
            {
                Thread.Sleep((int)delay);
            }

            while (flag_modbus_control_thread)
            {
                stopwatch.Restart();

                try
                {
                    if (write_queue.Count() > 0)
                    {
                        FCUData write_fcudata;
                        write_queue.TryDequeue(out write_fcudata);
                        modbusRtuClient.WriteMultipleRegisters(slave_id, (UInt16)fcu_modbus.voltage_adj, write_fcudata.set_fcu_data());
                    }
                    else
                    {
                        readdata = modbusRtuClient.ReadHoldingRegisters(slave_id, (UInt16)fcu_modbus.ctrl_code, 10);
                        fcudata.get_fcu_data(readdata);
                        update_real();
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message == "The operation has timed out.")
                    {
                        disconnect_modbus();
                    }
                }
                
                if (!flag_modbus_control_thread)
                {
                    break;
                }

                stopwatch.Stop();
                delay = interval - stopwatch.ElapsedMilliseconds;
                if (delay >= 0)
                {
                    Thread.Sleep((int)delay);
                }
            }
            flag_close_thread = true;
        }
    }
}
