using AsanSimulatorGUI.FCU_ORM;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AsanSimulatorGUI.Modules
{
    public class TestControl
    {
        public delegate void reset_graph();
        public event reset_graph reset_graph_event;

        public delegate void draw_time_graph(SMALL_TEST smalltest);
        public event draw_time_graph draw_time_graph_event;

        public delegate void update_graph(int time, int resistor);
        public event update_graph update_graph_event;

        public delegate void need_lamp(FCUTest fcutest);
        public event need_lamp need_lamp_event;

        public delegate void order_screenshot();
        public event order_screenshot order_screenshot_event;

        public delegate void test_done();
        public event test_done test_done_event;

        Thread test_thread;
        volatile bool flag_test_thread = false;
        volatile bool flag_test_resume = false;

        volatile bool flag_get_lamp_data = false;

        public ConcurrentQueue<bool> new_fcudata_signal;
        public ConcurrentQueue<FCUTest> test_queue;
        public ConcurrentQueue<FCUData> write_queue;

        public FCUData fcudata;

        int test_time = 2500;//2.5s

        /// <summary>
        /// 
        /// </summary>
        /// <param name="test_queue">시험항목 리스트</param>
        /// <param name="new_fcudata_signal">modbus로 업데이트 되었는지 여부</param>
        /// <param name="fcudata">실시간 데이터</param>
        /// <param name="write_queue">modbus로 시뮬레이터에 write할 데이터</param>
        public TestControl(ConcurrentQueue<bool> new_fcudata_signal, FCUData fcudata, ConcurrentQueue<FCUData> write_queue)
        {
            this.new_fcudata_signal = new_fcudata_signal;
            this.fcudata = fcudata;
            this.write_queue = write_queue;
        }

        public void set_test_queue(ConcurrentQueue<FCUTest> test_queue)
        {
            this.test_queue = test_queue;
        }

        public void set_flag_lamp()
        {
            flag_get_lamp_data = true;
        }

        public void start_test()
        {
            flag_test_thread = true;
            flag_test_resume = false;
            if (test_thread == null || test_thread.ThreadState == System.Threading.ThreadState.Stopped)
            {
                test_thread = new Thread(run_test);
                test_thread.IsBackground = true;
                test_thread.Start();
            }
        }

        public void stop_test()
        {
            flag_test_resume = false;
            flag_test_thread = false;
        }

        public void pause_test()
        {
            flag_test_resume = true;
        }

        void enqueue_write_queue(FCUTest fcutest)
        {
            FCUData write_data = new FCUData();
            write_data.set_relay(fcutest.resistor_set);
            write_data.voltage_fcu_adj = fcutest.voltage_set;
            write_queue.Enqueue(write_data);
            while (write_queue.Count > 0)
            {
                Thread.Sleep(10);
                bool temp;
                while (new_fcudata_signal.Count() > 0)
                {
                    new_fcudata_signal.TryDequeue(out temp);
                }
            }
        }

        void enqueue_write_queue()
        {
            FCUData write_data = new FCUData();
            write_data.set_relay(0);
            write_queue.Enqueue(write_data);
            while (write_queue.Count > 0)
            {
                Thread.Sleep(10);
                bool temp;
                while (new_fcudata_signal.Count() > 0)
                {
                    new_fcudata_signal.TryDequeue(out temp);
                }
            }
        }

      #region 추가 코드 
      public delegate void update_grid_status(FCUTest test,string status);
      public event update_grid_status update_grid_status_event;


      #endregion


      public async void run_test() {
         // 시험시작시 해당 함수가 스레드로 동작함
         // 각 시험은 2.5초의 시험시간을 가지고 동작하며 일부(반응시간 혹은 램프 점멸체크)의 경우 추가시간이 필요하다.
         Stopwatch stopwatch = new Stopwatch();

         while(flag_test_thread) {
            while(test_queue.Count() > 0) {
               FCUTest now_test;
               test_queue.TryDequeue(out now_test);

               // update_grid_status_event?.Invoke(now_test, "테스트");

               if(now_test.need_graph) reset_graph_event();

               if(now_test.test_index >= 5 && now_test.test_index <= 7) {
                  enqueue_write_queue(now_test);
                  }

               stopwatch.Restart();
               while(stopwatch.ElapsedMilliseconds < test_time) {
                  if(new_fcudata_signal.Count() > 0) {
                     bool temp;
                     new_fcudata_signal.TryDequeue(out temp);

                     // TODO now_test에 해당하는 테스트에 맞춰서 시나리오 작성하기
                     if(now_test.test_index == 3 || now_test.test_index == 9) {
                        now_test.SMALL_TESTs[0].measure_value = fcudata.time_fire;
                        break;
                        } else if(now_test.test_index == 4) {
                        now_test.SMALL_TESTs[0].measure_value = fcudata.current;
                        now_test.SMALL_TESTs[1].measure_value = fcudata.power;
                        } else if(now_test.test_index >= 5 && now_test.test_index <= 7) {
                        now_test.SMALL_TESTs[0].measure_value = fcudata.voltage_fire;
                        if(stopwatch.ElapsedMilliseconds < 1200) {
                           update_graph_event((int)stopwatch.ElapsedMilliseconds,0);
                           } else if(stopwatch.ElapsedMilliseconds > 1400) {
                           update_graph_event((int)stopwatch.ElapsedMilliseconds,now_test.resistor_set);
                           } else {
                           enqueue_write_queue(now_test);
                           }
                        } else {
                        now_test.SMALL_TESTs[0].measure_value = fcudata.voltage_fault;
                        now_test.SMALL_TESTs[1].measure_value = fcudata.voltage_fire;
                        }

                     foreach(SMALL_TEST small_test in now_test.SMALL_TESTs) {
                        if(small_test.small_tag == "미실시") continue;
                        if(small_test.value_min > small_test.measure_value)
                           small_test.value_min = small_test.measure_value;
                        if(small_test.value_max < small_test.measure_value)
                           small_test.value_max = small_test.measure_value;
                        }
                     }

                  if(!flag_test_thread) {
                     break;
                     }
                  while(flag_test_resume) {
                     Thread.Sleep(30);
                     }
                  Thread.Sleep(30);
                  }

               if(now_test.test_index != 3 && now_test.test_index != 4 && now_test.test_index != 9) {


                  need_lamp_event(now_test);
               
                  
                  while(!flag_get_lamp_data) {
                     Thread.Sleep(250);
                     }

           
                     

                  foreach(SMALL_TEST small_test in now_test.SMALL_TESTs) {
                     if(small_test.small_tag == "미실시") continue;
                     if(small_test.value_min > small_test.measure_value)
                        small_test.value_min = small_test.measure_value;
                     if(small_test.value_max < small_test.measure_value)
                        small_test.value_max = small_test.measure_value;
                     }
                  }

               try {
                  var tasks = new List<Task>();
                  for(int i = 0;i < 4;i++) {
                     if(now_test.SMALL_TESTs[i].small_tag.Contains("미실시")) continue;
                     string result = "진행중";
                     if(now_test.SMALL_TESTs[i].small_tag.Contains("시간")) {
                        draw_time_graph_event(now_test.SMALL_TESTs[i]);
                        }
                     if(now_test.SMALL_TESTs[i].ref_L <= now_test.SMALL_TESTs[i].value_min
                         && now_test.SMALL_TESTs[i].ref_R >= now_test.SMALL_TESTs[i].value_max)
                        result = "Pass";
                     else
                        result = "Fail";

                     tasks.Add(Task.Run(() =>
                     {
                        lock(now_test) {
                           switch(i) {
                              case 0:
                                 now_test.test_result1 = result;
                                 break;
                              case 1:
                                 now_test.test_result2 = result;
                                 break;
                              case 2:
                                 now_test.test_result3 = result;
                                 break;
                              case 3:
                                 now_test.test_result4 = result;
                                 break;
                              }
                           }
                     }));
                     }
                  await Task.WhenAll(tasks);
                  }
               catch(Exception ex) {
                  MessageBox.Show($"Error: {ex.Message}\n{ex.InnerException?.Message}","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
                  }

               if(now_test.need_graph) order_screenshot_event();

               enqueue_write_queue();

               if(!flag_test_thread) {
                  break;
                  }
               }
            stop_test();
            test_done_event();
            }
         }

      }
   }
