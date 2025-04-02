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
        public static Dictionary<int, (string Size, string Description, string Unit, double DefaultValue, string NormalRange, string Func, string Note, string Endian, string Symbols, string Style)> LoadModbusParameters(string filePath)
        {
            var modbusData = new Dictionary<int, (string Size, string Description, string Unit, double DefaultValue, string NormalRange, string Func, string Note, string Endian, string Symbols, string Style)>();

            try
            {
                if (!File.Exists(filePath))
                    throw new FileNotFoundException($"엑셀 파일을 찾을 수 없습니다: {filePath}");

                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    var rowCount = worksheet.Dimension?.Rows ?? 0;


                    for (var row = 6; row <= rowCount; row++)
                    {
                        var indexValue = worksheet.Cells[row, 1].Value;
                        if (indexValue == null) continue;

                        if (!int.TryParse(indexValue.ToString(), out var index))
                            continue;

                        var size = GetCellValue(worksheet, row, 2);
                        var description = GetCellValue(worksheet, row, 3);
                        var unit = GetCellValue(worksheet, row, 4);
                        var defaultValue = ParseDefaultValue(GetCellValue(worksheet, row, 5));
                        var normalRange = GetCellValue(worksheet, row, 6);
                        var func = GetCellValue(worksheet, row, 7);
                        var note = GetCellValue(worksheet, row, 8);
                        var endian = GetCellValue(worksheet, row, 9);
                        var symbols = GetCellValue(worksheet, row, 10); // 추가된 부분
                        var style = GetCellValue(worksheet, row, 11); // 추가된 부분

                        if (!string.IsNullOrWhiteSpace(description))
                            modbusData.Add(index, (size, description, unit, defaultValue, normalRange, func, note, endian, symbols, style));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"엑셀 파일 로드 중 오류 발생: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return new Dictionary<int, (string Size, string Description, string Unit, double DefaultValue, string NormalRange,string Func, string Note, string Endian, string Symbols, string Style)>();
            }

            return modbusData;
        }

        public static void SaveModbusParametersToSettings(Dictionary<int, (string Size, string Description, string Unit, double DefaultValue, string NormalRange,string Func, string Note, string Endian, string Symbols, string Style)> modbusData)
        {
            try
            {
                var indexList = new StringCollection();
                var sizeList = new StringCollection();
                var descriptionList = new StringCollection();
                var unitList = new StringCollection();
                var defaultValueList = new StringCollection();
                var normalRangeList = new StringCollection();
                var noteList = new StringCollection();
                var funcList = new StringCollection();
                var endianList = new StringCollection();
                var symbolsList = new StringCollection();
                var styleList = new StringCollection();


                foreach (var kvp in modbusData)
                {
                    indexList.Add(kvp.Key.ToString());
                    sizeList.Add(kvp.Value.Size);
                    descriptionList.Add(kvp.Value.Description);
                    unitList.Add(kvp.Value.Unit);
                    defaultValueList.Add(kvp.Value.DefaultValue.ToString());
                    normalRangeList.Add(kvp.Value.NormalRange);
                    funcList.Add(kvp.Value.Func);
                    noteList.Add(kvp.Value.Note);
                    endianList.Add(kvp.Value.Endian);
                    symbolsList.Add(kvp.Value.Symbols);
                    styleList.Add(kvp.Value.Style);
                }

                Properties.Settings.Default.Index = indexList;
                Properties.Settings.Default.Size = sizeList;
                Properties.Settings.Default.Description = descriptionList;
                Properties.Settings.Default.Unit = unitList;
                Properties.Settings.Default.DefaultValue = defaultValueList;
                Properties.Settings.Default.NormalRange = normalRangeList;
                Properties.Settings.Default.Note = noteList;
                Properties.Settings.Default.Func = funcList;
                Properties.Settings.Default.Endian = endianList;
                Properties.Settings.Default.Symbols = symbolsList;
                Properties.Settings.Default.Style = styleList;
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"설정 저장 중 오류 발생: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
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
