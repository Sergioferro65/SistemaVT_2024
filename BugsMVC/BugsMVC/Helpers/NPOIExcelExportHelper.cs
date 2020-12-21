using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;

namespace BugsMVC.Helpers
{
    /// <summary>
    /// Helper para pruebas de performance en generacion de reportes excel
    /// con NPOI
    /// </summary>
    public class NPOIExcelExportHelper
    {
        private XSSFCellStyle defaultCellStyle;
        private short doubleFormat;

        public NPOIExcelExportHelper(XSSFCellStyle defaultCellStyle, short doubleFormat)
        {
            this.defaultCellStyle = defaultCellStyle;
            this.doubleFormat = doubleFormat;
        }

        public void CreateTextCell(IRow row, int colIdx, string value)
        {
            var cell = row.CreateCell(colIdx);
            cell.SetCellValue(value);
            cell.CellStyle = defaultCellStyle;
        }

        public ICell CreateNumericCell(IRow row, int colIdx, decimal value)
        {
            var cell = row.CreateCell(colIdx);
            cell.SetCellValue(Convert.ToDouble(value));
            cell.CellStyle = defaultCellStyle;
            cell.CellStyle.DataFormat = doubleFormat;

            return cell;
        }
    }
}