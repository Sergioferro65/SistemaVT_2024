using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.Style.XmlAccess;
using System;

namespace BugsMVC.Helpers
{
    public class EPPlusExcelExportHelper
    {
        private ExcelWorkbook workbook;
        private ExcelNamedStyleXml defaultCellStyle;
        private ExcelNamedStyleXml numberCellStyle;
        private string numberFormat;

        public EPPlusExcelExportHelper(ExcelWorkbook workbook, string numberFormat)
        {
            this.workbook = workbook;
            this.numberFormat = numberFormat;
            this.defaultCellStyle = GetDefaultCellStyle(workbook);
            this.numberCellStyle = GetNumberCellStyle();
        }

        public void CreateTextCell(ExcelWorksheet sheet, int rowIdx, int colIdx, string value)
        {
            var cell = sheet.Cells[rowIdx, colIdx];
            cell.Value = value;
            cell.StyleName = this.defaultCellStyle.Name;
        }

        public void CreateNumericCell(ExcelWorksheet sheet, int rowIdx, int colIdx, decimal value)
        {
            var cell = sheet.Cells[rowIdx, colIdx];
            cell.Value = Convert.ToDouble(value);
            cell.StyleName = this.numberCellStyle.Name;
        }

        private ExcelNamedStyleXml GetDefaultCellStyle(ExcelWorkbook workbook)
        {
            var cellStyle = workbook.Styles.CreateNamedStyle("default_cell");
            cellStyle.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            cellStyle.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            cellStyle.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            cellStyle.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            cellStyle.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            return cellStyle;
        }

        private ExcelNamedStyleXml GetNumberCellStyle()
        {
            var cellStyle = workbook.Styles.CreateNamedStyle("number_cell");
            cellStyle.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            cellStyle.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            cellStyle.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            cellStyle.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            cellStyle.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            cellStyle.Style.Numberformat.Format = this.numberFormat;

            return cellStyle;
        }

        public ExcelNamedStyleXml GetHeaderCellStyle()
        {
            var headerCellStyle = this.workbook.Styles.CreateNamedStyle("header");
            headerCellStyle.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            headerCellStyle.Style.Font.Color.SetColor(System.Drawing.Color.Black);
            headerCellStyle.Style.Font.Bold = true;
            headerCellStyle.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            headerCellStyle.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            headerCellStyle.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            headerCellStyle.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

            return headerCellStyle;
        }
    }
}