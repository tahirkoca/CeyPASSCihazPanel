using System;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;

namespace CeyPASSCihazPanel.Business.Helpers
{
    public static class MiniExcel
    {
        public static void CreateXlsx(string filePath, DataTable dt)
        {
            if (File.Exists(filePath)) File.Delete(filePath);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, true))
            {
                // [Content_Types].xml
                CreateEntry(archive, "[Content_Types].xml", GetContentTypesXml());

                // _rels/.rels
                CreateEntry(archive, "_rels/.rels", GetRelsXml());

                // xl/_rels/workbook.xml.rels
                CreateEntry(archive, "xl/_rels/workbook.xml.rels", GetWorkbookRelsXml());

                // xl/workbook.xml
                CreateEntry(archive, "xl/workbook.xml", GetWorkbookXml());

                // xl/styles.xml
                CreateEntry(archive, "xl/styles.xml", GetStylesXml());

                // xl/worksheets/sheet1.xml
                CreateEntry(archive, "xl/worksheets/sheet1.xml", GetSheetXml(dt));
            }
        }

        private static void CreateEntry(ZipArchive archive, string entryName, string content)
        {
            var entry = archive.CreateEntry(entryName);
            using (var entryStream = entry.Open())
            using (var streamWriter = new StreamWriter(entryStream, Encoding.UTF8))
            {
                streamWriter.Write(content);
            }
        }

        private static string GetContentTypesXml()
        {
            return @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Types xmlns=""http://schemas.openxmlformats.org/package/2006/content-types"">
  <Default Extension=""rels"" ContentType=""application/vnd.openxmlformats-package.relationships+xml""/>
  <Default Extension=""xml"" ContentType=""application/xml""/>
  <Override PartName=""/xl/workbook.xml"" ContentType=""application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml""/>
  <Override PartName=""/xl/worksheets/sheet1.xml"" ContentType=""application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml""/>
  <Override PartName=""/xl/styles.xml"" ContentType=""application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml""/>
</Types>";
        }

        private static string GetRelsXml()
        {
            return @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
  <Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"" Target=""xl/workbook.xml""/>
</Relationships>";
        }

        private static string GetWorkbookRelsXml()
        {
            return @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
  <Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet"" Target=""worksheets/sheet1.xml""/>
  <Relationship Id=""rId2"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles"" Target=""styles.xml""/>
</Relationships>";
        }

        private static string GetWorkbookXml()
        {
            return @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<workbook xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main"" xmlns:r=""http://schemas.openxmlformats.org/officeDocument/2006/relationships"">
  <sheets>
    <sheet name=""Sheet1"" sheetId=""1"" r:id=""rId1""/>
  </sheets>
</workbook>";
        }

        private static string GetStylesXml()
        {
            return @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<styleSheet xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main"">
  <fonts count=""1"">
    <font>
      <sz val=""11""/>
      <name val=""Calibri""/>
    </font>
  </fonts>
  <fills count=""2"">
    <fill><patternFill patternType=""none""/></fill>
    <fill><patternFill patternType=""gray125""/></fill>
  </fills>
  <borders count=""1"">
    <border><left/><right/><top/><bottom/><diagonal/></border>
  </borders>
  <cellStyleXfs count=""1"">
    <xf numFmtId=""0"" fontId=""0"" fillId=""0"" borderId=""0""/>
  </cellStyleXfs>
  <cellXfs count=""1"">
    <xf numFmtId=""0"" fontId=""0"" fillId=""0"" borderId=""0"" xfId=""0""/>
  </cellXfs>
</styleSheet>";
        }

        private static string GetSheetXml(DataTable dt)
        {
            var sb = new StringBuilder();
            sb.Append(@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>");
            sb.Append(@"<worksheet xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main"">");
            sb.Append(@"<sheetData>");

            // Header Row
            sb.Append(@"<row r=""1"">");
            int colIndex = 1;
            foreach (DataColumn col in dt.Columns)
            {
                string val = EscapeXml(col.ColumnName);
                sb.Append($"<c r=\"{GetColumnName(colIndex)}1\" t=\"inlineStr\"><is><t>{val}</t></is></c>");
                colIndex++;
            }
            sb.Append(@"</row>");

            // Data Rows
            int rowIndex = 2;
            foreach (DataRow row in dt.Rows)
            {
                sb.Append($"<row r=\"{rowIndex}\">");
                colIndex = 1;
                foreach (var item in row.ItemArray)
                {
                    string val = EscapeXml(item?.ToString() ?? "");
                    sb.Append($"<c r=\"{GetColumnName(colIndex)}{rowIndex}\" t=\"inlineStr\"><is><t>{val}</t></is></c>");
                    colIndex++;
                }
                sb.Append(@"</row>");
                rowIndex++;
            }

            sb.Append(@"</sheetData>");
            sb.Append(@"</worksheet>");
            return sb.ToString();
        }

        public static string GetColumnName(int index)
        {
            string columnName = "";
            int modulo;

            while (index > 0)
            {
                modulo = (index - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                index = (int)((index - modulo) / 26);
            }

            return columnName;
        }

        public static string EscapeXml(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("&", "&amp;")
                    .Replace("\"", "&quot;")
                    .Replace("'", "&apos;")
                    .Replace("<", "&lt;")
                    .Replace(">", "&gt;");
        }
    }
}
