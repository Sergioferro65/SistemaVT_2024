using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugsMVC.Helpers
{
    public class ExcelHelper
    {
        public static XSSFCellStyle GetDefaultCellStyle(IWorkbook workbook, bool isForDate = false)
        {
            XSSFCellStyle newCellStyle = (XSSFCellStyle)workbook.CreateCellStyle();
            newCellStyle.Alignment = HorizontalAlignment.Right;
            newCellStyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
            newCellStyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
            newCellStyle.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
            newCellStyle.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;

            if (isForDate)
            {
                newCellStyle.DataFormat = workbook.CreateDataFormat().GetFormat("dd/MM/yyyy");
            }

            return newCellStyle;
        }

        public static XSSFCellStyle GetHeaderCellStyle(IWorkbook workbook)
        {
            IFont font = workbook.CreateFont();
            font.Color = IndexedColors.Black.Index;
            font.IsBold = true;

            XSSFCellStyle newCellStyle = (XSSFCellStyle)workbook.CreateCellStyle();
            newCellStyle.Alignment = HorizontalAlignment.Center;
            newCellStyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
            newCellStyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
            newCellStyle.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
            newCellStyle.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
            newCellStyle.SetFont(font);

            return newCellStyle;
        }
    }
}