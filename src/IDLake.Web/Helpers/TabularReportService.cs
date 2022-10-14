using GemBox.Spreadsheet;
using IDLake.Web.Data;
using MySqlConnector;
using System.Data;
using System.Drawing;
using System.Linq;
namespace IDLake.Web.Helpers
{
    public enum ExportFileTypes
    {
        Excel, Csv, Pdf, Html
    }
    public enum OperatorReport
    {
        NoFilter, SamaDengan, LebihBesarDari, LebihKecilDari, Berisi
    }
    public class TabularReportFilter
    {
        public bool IsInclude { get; set; } = true;
        public string ColumnName { get; set; }
        public OperatorReport Operator { get; set; } = OperatorReport.NoFilter;
        public string ValueFilter { get; set; }

        public string GenerateSql()
        {
            var filter = string.Empty;
            switch (Operator)
            {
                case OperatorReport.SamaDengan:
                    filter = $"{ColumnName} = '{ValueFilter}'";
                    break;
                case OperatorReport.LebihBesarDari:
                    filter = $"{ColumnName} >= '{ValueFilter}'";
                    break;
                case OperatorReport.LebihKecilDari:
                    filter = $"{ColumnName} <= '{ValueFilter}'";
                    break;
                case OperatorReport.Berisi:
                    filter = $"{ColumnName} like '%{ValueFilter}%'";
                    break;
                default:
                    break;
            }
            return filter;
        }
    }
    public class TabularReportService
    {
        InfoDatasetService svc { set; get; }
        public TabularReportService(InfoDatasetService svc)
        {
            this.svc = svc;
        }

        public List<TabularReportFilter> GetFilters(string uid)
        {
            var filter = new List<TabularReportFilter>();
            var cols = GetColumns(uid);
            foreach (var col in cols)
            {
                filter.Add(new TabularReportFilter() { ColumnName = col, ValueFilter = "" });
            }
            return filter;
        }


        public byte[] ExportData(DataTable datas, ExportFileTypes SelType = ExportFileTypes.Excel)
        {
            // If using Professional version, put your serial key below.
            //SpreadsheetInfo.SetLicense(AppConstants.GemLic);

            var workbook = new ExcelFile();
            var worksheet = workbook.Worksheets.Add("Data");
            int row = 1;

            var styleHeader = new CellStyle();
            styleHeader.HorizontalAlignment = HorizontalAlignmentStyle.Center;
            styleHeader.VerticalAlignment = VerticalAlignmentStyle.Center;
            styleHeader.Font.Weight = ExcelFont.BoldWeight;
            styleHeader.Font.Color = Color.Black;
            styleHeader.WrapText = true;
            styleHeader.Borders.SetBorders(MultipleBorders.Left | MultipleBorders.Right | MultipleBorders.Top | MultipleBorders.Bottom, Color.Black, LineStyle.Thin);

            var colCount = 0;
            foreach (DataColumn dc in datas.Columns)
            {
                worksheet.Cells[0, colCount].Value = dc.ColumnName;
                worksheet.Cells[0, colCount].Style = styleHeader;
                colCount++;
            }


            var style = new CellStyle();
            style.HorizontalAlignment = HorizontalAlignmentStyle.Center;
            style.VerticalAlignment = VerticalAlignmentStyle.Center;
            style.Font.Weight = ExcelFont.NormalWeight;
            style.Font.Color = Color.Black;
            style.WrapText = true;
            style.Borders.SetBorders(MultipleBorders.Left | MultipleBorders.Right | MultipleBorders.Top | MultipleBorders.Bottom, Color.Black, LineStyle.Thin);

            foreach (DataRow item in datas.Rows)
            {
                colCount = 0;
                foreach (DataColumn dc in datas.Columns)
                {
                    worksheet.Cells[row, colCount].Value = item[dc.ColumnName].ToString();
                    worksheet.Cells[row, colCount].Style = style;
                    colCount++;
                }


                row++;
            }

            var ext = string.Empty;
            switch (SelType)
            {
                case ExportFileTypes.Html:
                    ext = ".html";
                    break;
                case ExportFileTypes.Pdf:
                    ext = ".pdf";
                    break;
                case ExportFileTypes.Excel:
                    ext = ".xlsx";
                    break;
                case ExportFileTypes.Csv:
                    ext = ".csv";
                    break;
            }
            var tmpfile = Path.GetTempFileName() + ext;

            workbook.Save(tmpfile);
            return File.ReadAllBytes(tmpfile);
        }
        public DataTable GetPreview(string uid, List<TabularReportFilter> filter, int Limit = 25)
        {
            try
            {
                var columns = string.Empty;


                //generate filter
                var SqlWhere = string.Empty;
                var count = 0;
                var colcount = 0;
                foreach (var item in filter)
                {
                    if (item.IsInclude)
                    {
                        if (colcount > 0) columns += ",";
                        columns += $"{item.ColumnName}";
                        colcount++;
                    }
                    if (item.Operator != OperatorReport.NoFilter && item.IsInclude)
                    {
                        if (count > 0) SqlWhere += "AND";
                        SqlWhere += $" ({item.GenerateSql()})";
                        count++;
                    }
                }
                var qry = SqlWhere.Trim().Length > 0 ? $" {SqlWhere} " : $"";
                var dt = GetData(uid,qry,Limit);
                return dt;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

        }

        public string[] GetColumns(string uid)
        {
            try
            {
                var cols = new List<string>();
                var item = svc.GetDataByUid(uid);
                if (item != null && !string.IsNullOrEmpty(item.DatasetPath))
                {
                    var TableDataSet = DataConverter.ConvertCSVtoDataTable(item.DatasetPath);
                    foreach(DataColumn dc in TableDataSet.Columns)
                    {
                        cols.Add(dc.ColumnName);
                    }
                }
                return cols.ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }


        }
        public DataTable GetData(string uid,string Query="", int Limit = -1)
        {
           
            try
            {
                var item = svc.GetDataByUid(uid);
                if (item != null && !string.IsNullOrEmpty(item.DatasetPath))
                {
                    var TableDataSet = DataConverter.ConvertCSVtoDataTable(item.DatasetPath);
                    //filter
                    if (!string.IsNullOrEmpty(Query))
                    {
                        var filtered = TableDataSet.Select(Query);
                        var cloned = filtered.CopyToDataTable();
                        /*
                        var cloned = TableDataSet.Clone();
                        cloned.Rows.Clear();
                        foreach (DataRow dr in filtered) {
                            cloned.Rows.Add(dr);
                        }*/
                        TableDataSet = cloned;
                    }
                    //limit
                    if (Limit > 0)
                    {
                        var newTable= TableDataSet.AsEnumerable().Skip(0).Take(Limit).CopyToDataTable();
                        TableDataSet = newTable;
                    }
                    return TableDataSet;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e.ToString());
               
            }
            return default;

        }
    }
}