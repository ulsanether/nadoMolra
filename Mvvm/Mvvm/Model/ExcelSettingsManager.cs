using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;

using OfficeOpenXml;

namespace Mvvm.Model {
   public class ExcelSettingsManager {
      /// <summary>
      /// </summary>
      public void SaveExcelDataToSettings(string filePath) {
         StringCollection modbusNameList = new StringCollection();
         StringCollection modbusUnitList = new StringCollection();

         ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
         using(var package = new ExcelPackage(new FileInfo(filePath))) {
            var worksheet = package.Workbook.Worksheets[0];
            int row = 2; 

            while(worksheet.Cells[row,1].Value != null) { 
               modbusNameList.Add(worksheet.Cells[row,1].Value?.ToString() ?? string.Empty);
               modbusUnitList.Add(worksheet.Cells[row,3].Value?.ToString() ?? string.Empty);

               row++;
               }

            Properties.Settings.Default.ModbusName = modbusNameList;
            Properties.Settings.Default.ModbusUnit = modbusUnitList;
            Properties.Settings.Default.Save();
            }
         }

      /// <summary>
      /// </summary>
      public (List<string> ModbusName, List<string> ModbusUnit) LoadDataFromSettings() {
         var savedModbusName = Properties.Settings.Default.ModbusName;
         var modbusNameList = savedModbusName != null ? new List<string>(savedModbusName.Cast<string>()) : new List<string>();

         var savedModbusUnit = Properties.Settings.Default.ModbusUnit;
         var modbusUnitList = savedModbusUnit != null ? new List<string>(savedModbusUnit.Cast<string>()) : new List<string>();

         return (modbusNameList, modbusUnitList);
         }

      /// <summary>
      /// </summary>
      public void PrintDataToConsole() {
         var (modbusNameList, modbusUnitList) = LoadDataFromSettings();

         Console.WriteLine("Settings 데이터 출력:");
         for(int i = 0;i < Math.Max(modbusNameList.Count,modbusUnitList.Count);i++) {
            var name = i < modbusNameList.Count ? modbusNameList[i] : "(빈 데이터)";
            var unit = i < modbusUnitList.Count ? modbusUnitList[i] : "(빈 데이터)";
            Console.WriteLine($"[{i}] ModbusName: {name}, ModbusUnit: {unit}");
            }
         }
      }
   }
