using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows;
using OfficeOpenXml;

namespace Mvvm.Model.IniFileRead
{
    public class ExcelSettingsManager
    {
        static ExcelSettingsManager()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        #region Public Methods
        public void SaveExcelDataToSettings(string filePath)
        {
            try
            {
                var modbusNameList = new StringCollection();
                var modbusUnitList = new StringCollection();

                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    var row = 2;

                    while (worksheet.Cells[row, 1].Value != null)
                    {
                        modbusNameList.Add(worksheet.Cells[row, 1].Value?.ToString() ?? string.Empty);
                        modbusUnitList.Add(worksheet.Cells[row, 3].Value?.ToString() ?? string.Empty);
                        row++;
                    }

                    Properties.Settings.Default.ModbusName = modbusNameList;
                    Properties.Settings.Default.ModbusUnit = modbusUnitList;
                    Properties.Settings.Default.Save();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"설정 저장 중 오류 발생: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static Dictionary<int, (string Description, string Unit, double DefaultValue, string Note)> LoadModbusParameters(string filePath)
        {
            var modbusData = new Dictionary<int, (string Description, string Unit, double DefaultValue, string Note)>();

            try
            {
                if (!File.Exists(filePath))
                    throw new FileNotFoundException($"엑셀 파일을 찾을 수 없습니다: {filePath}");

                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    var rowCount = worksheet.Dimension?.Rows ?? 0;

                    // 헤더를 제외하고 2번째 행부터 데이터 읽기
                    for (var row = 2; row <= rowCount; row++)
                    {
                        var indexValue = worksheet.Cells[row, 1].Value;
                        if (indexValue == null) continue;

                        if (!int.TryParse(indexValue.ToString(), out var index))
                            continue;

                        var description = GetCellValue(worksheet, row, 3);
                        var unit = GetCellValue(worksheet, row, 4);
                        var defaultValue = ParseDefaultValue(GetCellValue(worksheet, row, 5));
                        var note = GetCellValue(worksheet, row, 7);

                        if (!string.IsNullOrWhiteSpace(description))
                            modbusData.Add(index, (description, unit, defaultValue, note));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"엑셀 파일 로드 중 오류 발생: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return new Dictionary<int, (string Description, string Unit, double DefaultValue, string Note)>();
            }

            return modbusData;
        }

        public Dictionary<int, (string Description, string Unit, double DefaultValue, string Note)> LoadModbusParameters()
        {
            var defaultPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Data",
                "ModbusParameters.xlsx");

            return LoadModbusParameters(defaultPath);
        }

        public (List<string> ModbusName, List<string> ModbusUnit) LoadDataFromSettings()
        {
            var savedModbusName = Properties.Settings.Default.ModbusName;
            var modbusNameList = savedModbusName != null
                ? new List<string>(savedModbusName.Cast<string>())
                : new List<string>();

            var savedModbusUnit = Properties.Settings.Default.ModbusUnit;
            var modbusUnitList = savedModbusUnit != null
                ? new List<string>(savedModbusUnit.Cast<string>())
                : new List<string>();

            return (modbusNameList, modbusUnitList);
        }

        public void PrintDataToConsole()
        {
            var (modbusNameList, modbusUnitList) = LoadDataFromSettings();

            Console.WriteLine("Settings 데이터 출력:");
            for (var i = 0; i < Math.Max(modbusNameList.Count, modbusUnitList.Count); i++)
            {
                var name = i < modbusNameList.Count ? modbusNameList[i] : "(빈 데이터)";
                var unit = i < modbusUnitList.Count ? modbusUnitList[i] : "(빈 데이터)";
                Console.WriteLine($"[{i}] ModbusName: {name}, ModbusUnit: {unit}");
            }
        }
        #endregion

        #region Private Helper Methods
        private static string GetCellValue(ExcelWorksheet worksheet, int row, int col)
        {
            return worksheet.Cells[row, col].Value?.ToString()?.Trim() ?? string.Empty;
        }

        private static double ParseDefaultValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return 0.0;

            // 숫자 형식이 아닌 문자 제거
            value = new string(value.Where(c => char.IsDigit(c) || c == '.' || c == '-').ToArray());

            if (double.TryParse(value, out var result))
                return result;

            return 0.0;
        }
        #endregion
    }
}
