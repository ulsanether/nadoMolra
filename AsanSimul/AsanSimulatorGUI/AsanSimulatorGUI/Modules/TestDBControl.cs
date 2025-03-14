using AsanSimulatorGUI.FCU_ORM;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentModbus;
using System.Collections;
using System.Diagnostics;
using multimediatimer;
using DevExpress.Xpo.DB;
using DevExpress.Utils;
using NLog;
using System.Windows.Forms;


namespace AsanSimulatorGUI.Modules  //pase false기준값이 바뀌지 않거나, 항목이 추가되지 않는다면 건들이지 말것 
{


   /// <summary>
   /// 테스트 시나리오 초기화하는 클래스
   /// </summary>
   public class TestDBControl {

      private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
      //DB/ORM
      IDataLayer DB_FCU;
      string connectionString = @"XpoProvider=SQLite;Data Source=C:\asanSNT\FCU\db\fcu_simul.db";
      UnitOfWork UOW;
      XPCollection test_collection;


      public TestDBControl(IDataLayer DB,UnitOfWork UOW,XPCollection collection) {
         this.DB_FCU = DB;
         this.UOW = UOW;
         this.test_collection = collection;
         }

      public void connect_DB() {
         DB_FCU = XpoDefault.GetDataLayer(connectionString,AutoCreateOption.DatabaseAndSchema);
         UOW = new UnitOfWork(DB_FCU);

         test_collection = new XPCollection(typeof(FCUTest));
         test_collection.Session = UOW;
         test_collection.DisplayableProperties = "test_status;test_class;result1;result2;result3";
         }

      public void initital_testclass() {
         List<FCUTest> testlist = UOW.Query<FCUTest>().ToList();
         if(testlist.Count() < 11) {
            for(int i = 0;i < 11;i++) {
               FCUTest fcu_test = new FCUTest(UOW);
               fcu_test.test_index = (UInt16)i;
               fcu_test.test_status = false;
               fcu_test.test_result1 = "미실시";
               fcu_test.test_result2 = "미실시";
               fcu_test.test_result3 = "미실시";
               fcu_test.test_result4 = "미실시";
               fcu_test.voltage_set = 2800;
               fcu_test.resistor_set = 2200;
               fcu_test.need_graph = false;

               for(int j = 0;j < 4;j++) {
                  fcu_test.SMALL_TESTs.Add(new SMALL_TEST(UOW) {
                     small_tag = "미실시",
                     ref_L = 2550,
                     ref_R = 2850,
                     value_max = 0,
                     value_min = 30000,
                     });
                  }

               switch(i) {
                  case 0: //전원 핀 정상 확인
                     fcu_test.large_categ = "전기 인터페이스";
                     fcu_test.small_categ = "PIN 정상확인";
                     fcu_test.resistor_set = 0;

                     fcu_test.SMALL_TESTs[0].small_tag = "Fault 전압";
                     fcu_test.test_name1 = fcu_test.SMALL_TESTs[0].small_tag;
                     fcu_test.SMALL_TESTs[0].ref_L = 0;
                     fcu_test.SMALL_TESTs[0].ref_R = 10;

                     fcu_test.SMALL_TESTs[1].small_tag = "Fire 전압";
                     fcu_test.test_name2 = fcu_test.SMALL_TESTs[1].small_tag;
                     fcu_test.SMALL_TESTs[1].ref_L = 0;
                     fcu_test.SMALL_TESTs[1].ref_R = 10;

                     fcu_test.SMALL_TESTs[2].small_tag = "알람 점등";
                     fcu_test.test_name3 = fcu_test.SMALL_TESTs[2].small_tag;
                     fcu_test.SMALL_TESTs[2].ref_L = 0;
                     fcu_test.SMALL_TESTs[2].ref_R = 0;

                     fcu_test.SMALL_TESTs[3].small_tag = "화재 점등";
                     fcu_test.test_name4 = fcu_test.SMALL_TESTs[3].small_tag;
                     fcu_test.SMALL_TESTs[3].ref_L = 0;
                     fcu_test.SMALL_TESTs[3].ref_R = 0;
                     break;

                  case 1: //Fault 정상확인
                     fcu_test.large_categ = "전기 인터페이스";
                     fcu_test.small_categ = "Fault 정상확인";
                     fcu_test.resistor_set = 10000;

                     fcu_test.SMALL_TESTs[0].small_tag = "Fault 전압";
                     fcu_test.test_name1 = fcu_test.SMALL_TESTs[0].small_tag;

                     fcu_test.SMALL_TESTs[1].small_tag = "Fire 전압";
                     fcu_test.test_name2 = fcu_test.SMALL_TESTs[1].small_tag;
                     fcu_test.SMALL_TESTs[1].ref_L = 0;
                     fcu_test.SMALL_TESTs[1].ref_R = 10;

                     fcu_test.SMALL_TESTs[2].small_tag = "알람 점등";
                     fcu_test.test_name3 = fcu_test.SMALL_TESTs[2].small_tag;
                     fcu_test.SMALL_TESTs[2].ref_L = 0;
                     fcu_test.SMALL_TESTs[2].ref_R = 0;

                     fcu_test.SMALL_TESTs[3].small_tag = "화재 점등";
                     fcu_test.test_name4 = fcu_test.SMALL_TESTs[3].small_tag;
                     fcu_test.SMALL_TESTs[3].ref_L = 0;
                     fcu_test.SMALL_TESTs[3].ref_R = 0;
                     break;
                  case 2: //Fire 정상확인
                     fcu_test.large_categ = "전기 인터페이스";
                     fcu_test.small_categ = "Fire 정상확인";
                     fcu_test.resistor_set = 2200;

                     fcu_test.SMALL_TESTs[0].small_tag = "Fault 전압";
                     fcu_test.test_name1 = fcu_test.SMALL_TESTs[0].small_tag;
                     fcu_test.SMALL_TESTs[0].ref_L = 0;
                     fcu_test.SMALL_TESTs[0].ref_R = 10;

                     fcu_test.SMALL_TESTs[1].small_tag = "Fire 전압";
                     fcu_test.test_name2 = fcu_test.SMALL_TESTs[1].small_tag;

                     fcu_test.SMALL_TESTs[2].small_tag = "알람 점등";
                     fcu_test.test_name3 = fcu_test.SMALL_TESTs[2].small_tag;
                     fcu_test.SMALL_TESTs[2].ref_L = 1;
                     fcu_test.SMALL_TESTs[2].ref_R = 1;

                     fcu_test.SMALL_TESTs[3].small_tag = "화재 점등";
                     fcu_test.test_name4 = fcu_test.SMALL_TESTs[3].small_tag;
                     fcu_test.SMALL_TESTs[3].ref_L = 1;
                     fcu_test.SMALL_TESTs[3].ref_R = 1;
                     break;
                  case 3: // 반응시간
                     fcu_test.need_graph = true;
                     fcu_test.large_categ = "성능";
                     fcu_test.small_categ = "반응시간";
                     fcu_test.resistor_set = 2200;

                     fcu_test.SMALL_TESTs[0].small_tag = "반응시간";
                     fcu_test.test_name1 = fcu_test.SMALL_TESTs[0].small_tag;
                     fcu_test.SMALL_TESTs[0].ref_L = 0;
                     fcu_test.SMALL_TESTs[0].ref_R = 500;
                     break;
                  case 4: // 전력
                     fcu_test.large_categ = "전력";
                     fcu_test.small_categ = "소비전류&전력";
                     fcu_test.resistor_set = 0;

                     fcu_test.SMALL_TESTs[0].small_tag = "소비전류";
                     fcu_test.test_name1 = fcu_test.SMALL_TESTs[0].small_tag;
                     fcu_test.SMALL_TESTs[0].ref_L = 0;
                     fcu_test.SMALL_TESTs[0].ref_R = 135;

                     fcu_test.SMALL_TESTs[1].small_tag = "소비전력";
                     fcu_test.test_name2 = fcu_test.SMALL_TESTs[1].small_tag;
                     fcu_test.SMALL_TESTs[1].ref_L = 0;
                     fcu_test.SMALL_TESTs[1].ref_R = 378;
                     break;
                  case 5: //화재 시험 저항
                     fcu_test.need_graph = true;
                     fcu_test.large_categ = "기능";
                     fcu_test.small_categ = "화재 저항 미달";
                     fcu_test.resistor_set = 2000;

                     //fcu_test.SMALL_TESTs[0].small_tag = "Fault 전압";
                     //fcu_test.SMALL_TESTs[0].ref_L = 0;
                     //fcu_test.SMALL_TESTs[0].ref_R = 10;

                     fcu_test.SMALL_TESTs[0].small_tag = "Fire 전압";
                     fcu_test.test_name1 = fcu_test.SMALL_TESTs[0].small_tag;
                     fcu_test.SMALL_TESTs[0].ref_L = 0;
                     fcu_test.SMALL_TESTs[0].ref_R = 10;

                     fcu_test.SMALL_TESTs[1].small_tag = "알람 점등";
                     fcu_test.test_name2 = fcu_test.SMALL_TESTs[1].small_tag;
                     fcu_test.SMALL_TESTs[1].ref_L = 0;
                     fcu_test.SMALL_TESTs[1].ref_R = 0;

                     fcu_test.SMALL_TESTs[2].small_tag = "화재 점등";
                     fcu_test.test_name3 = fcu_test.SMALL_TESTs[2].small_tag;
                     fcu_test.SMALL_TESTs[2].ref_L = 0;
                     fcu_test.SMALL_TESTs[2].ref_R = 0;
                     break;
                  case 6: //화재 시험 저항
                     fcu_test.need_graph = true;
                     fcu_test.large_categ = "기능";
                     fcu_test.small_categ = "화재 저항 정상";
                     fcu_test.resistor_set = 2200;

                     //fcu_test.SMALL_TESTs[0].small_tag = "Fault 전압";
                     //fcu_test.SMALL_TESTs[0].ref_L = 0;
                     //fcu_test.SMALL_TESTs[0].ref_R = 10;

                     fcu_test.SMALL_TESTs[0].small_tag = "Fire 전압";
                     fcu_test.test_name1 = fcu_test.SMALL_TESTs[0].small_tag;

                     fcu_test.SMALL_TESTs[1].small_tag = "알람 점등";
                     fcu_test.test_name2 = fcu_test.SMALL_TESTs[1].small_tag;
                     fcu_test.SMALL_TESTs[1].ref_L = 1;
                     fcu_test.SMALL_TESTs[1].ref_R = 1;

                     fcu_test.SMALL_TESTs[2].small_tag = "화재 점등";
                     fcu_test.test_name3 = fcu_test.SMALL_TESTs[2].small_tag;
                     fcu_test.SMALL_TESTs[2].ref_L = 1;
                     fcu_test.SMALL_TESTs[2].ref_R = 1;
                     break;
                  case 7: //화재 시험 저항
                     fcu_test.need_graph = true;
                     fcu_test.large_categ = "기능";
                     fcu_test.small_categ = "화재 저항 초과";
                     fcu_test.resistor_set = 2400;

                     //fcu_test.SMALL_TESTs[0].small_tag = "Fault 전압";
                     //fcu_test.SMALL_TESTs[0].ref_L = 0;
                     //fcu_test.SMALL_TESTs[0].ref_R = 10;

                     fcu_test.SMALL_TESTs[0].small_tag = "Fire 전압";
                     fcu_test.test_name1 = fcu_test.SMALL_TESTs[0].small_tag;

                     fcu_test.SMALL_TESTs[1].small_tag = "알람 점등";
                     fcu_test.test_name2 = fcu_test.SMALL_TESTs[1].small_tag;
                     fcu_test.SMALL_TESTs[1].ref_L = 1;
                     fcu_test.SMALL_TESTs[1].ref_R = 1;

                     fcu_test.SMALL_TESTs[2].small_tag = "화재 점등";
                     fcu_test.test_name3 = fcu_test.SMALL_TESTs[2].small_tag;
                     fcu_test.SMALL_TESTs[2].ref_L = 1;
                     fcu_test.SMALL_TESTs[2].ref_R = 1;
                     break;
                  case 8: //Fire 정상확인
                     fcu_test.large_categ = "전기 인터페이스";
                     fcu_test.small_categ = "Fire 정상확인";
                     fcu_test.resistor_set = 2200;

                     fcu_test.SMALL_TESTs[0].small_tag = "Fault 전압";
                     fcu_test.test_name1 = fcu_test.SMALL_TESTs[0].small_tag;
                     fcu_test.SMALL_TESTs[0].ref_L = 0;
                     fcu_test.SMALL_TESTs[0].ref_R = 10;

                     fcu_test.SMALL_TESTs[1].small_tag = "Fire 전압";
                     fcu_test.test_name2 = fcu_test.SMALL_TESTs[1].small_tag;

                     fcu_test.SMALL_TESTs[2].small_tag = "알람 점등";
                     fcu_test.test_name3 = fcu_test.SMALL_TESTs[2].small_tag;
                     fcu_test.SMALL_TESTs[2].ref_L = 1;
                     fcu_test.SMALL_TESTs[2].ref_R = 1;

                     fcu_test.SMALL_TESTs[3].small_tag = "화재 점등";
                     fcu_test.test_name4 = fcu_test.SMALL_TESTs[3].small_tag;
                     fcu_test.SMALL_TESTs[3].ref_L = 1;
                     fcu_test.SMALL_TESTs[3].ref_R = 1;
                     break;
                  case 9: // 반응시간
                     fcu_test.need_graph = true;
                     fcu_test.large_categ = "성능";
                     fcu_test.small_categ = "반응시간";
                     fcu_test.resistor_set = 2200;

                     fcu_test.SMALL_TESTs[0].small_tag = "반응시간";
                     fcu_test.test_name1 = fcu_test.SMALL_TESTs[0].small_tag;
                     fcu_test.SMALL_TESTs[0].ref_L = 0;
                     fcu_test.SMALL_TESTs[0].ref_R = 500;
                     break;
                  case 10: //Fault 정상확인
                     fcu_test.large_categ = "전기 인터페이스";
                     fcu_test.small_categ = "Fault 정상확인";
                     fcu_test.resistor_set = 10000;

                     fcu_test.SMALL_TESTs[0].small_tag = "Fault 전압";
                     fcu_test.test_name1 = fcu_test.SMALL_TESTs[0].small_tag;

                     fcu_test.SMALL_TESTs[1].small_tag = "Fire 전압";
                     fcu_test.test_name2 = fcu_test.SMALL_TESTs[1].small_tag;
                     fcu_test.SMALL_TESTs[1].ref_L = 0;
                     fcu_test.SMALL_TESTs[1].ref_R = 10;

                     fcu_test.SMALL_TESTs[2].small_tag = "알람 점등";
                     fcu_test.test_name3 = fcu_test.SMALL_TESTs[2].small_tag;
                     fcu_test.SMALL_TESTs[2].ref_L = 0;
                     fcu_test.SMALL_TESTs[2].ref_R = 0;

                     fcu_test.SMALL_TESTs[3].small_tag = "화재 점등";
                     fcu_test.test_name4 = fcu_test.SMALL_TESTs[3].small_tag;
                     fcu_test.SMALL_TESTs[3].ref_L = 0;
                     fcu_test.SMALL_TESTs[3].ref_R = 0;
                     break;
                  }
               testlist.Add(fcu_test);
               }
            } else {
            reset_testdb();
            }
         UOW.CommitChanges();
         }

      public void reset_testdb() {
         Logger.Info("reset_testdb 시작");

         List<FCUTest> testlist = UOW.Query<FCUTest>().ToList();
         for(int i = testlist.Count() - 1;i >= 0;i--) {
            testlist[i].test_status = false;
            testlist[i].test_result1 = "미실시";
            testlist[i].test_result2 = "미실시";
            testlist[i].test_result3 = "미실시";
            testlist[i].test_result4 = "미실시";
            for(int j = 0;j < 4;j++) {
               testlist[i].SMALL_TESTs[j].value_min = 30000;
               testlist[i].SMALL_TESTs[j].value_max = 0;
               }

       
            }

         UOW.CommitChanges();
         }



      public void test_pdf_db() {
         List<FCUTest> testlist = UOW.Query<FCUTest>().ToList();

         for(int i = testlist.Count() - 1;i >= 0;i--) {
            testlist[i].test_status = false;
            testlist[i].test_result1 = "미실시";
            testlist[i].test_result2 = "미실시";
            testlist[i].test_result3 = "미실시";
            testlist[i].test_result4 = "미실시";
            for(int j = 0;j < 4;j++) {
               if(testlist[i].SMALL_TESTs[j].small_tag != "미실시") {
                  switch(j) {
                     case 0:
                        testlist[i].test_result1 = "Pass";
                        break;
                     case 1:
                        testlist[i].test_result2 = "Pass";
                        break;
                     case 2:
                        testlist[i].test_result3 = "Pass";
                        break;
                     case 3:
                        testlist[i].test_result4 = "Pass";
                        //testlist[i].test_result4 = "Fail";
                        break;
                     }
                  } else {

    

                  }
               }
            }
         }
      }
   }
