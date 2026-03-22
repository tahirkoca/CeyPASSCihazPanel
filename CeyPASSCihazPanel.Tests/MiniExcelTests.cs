using System;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using CeyPASSCihazPanel.Business.Helpers;
using FluentAssertions;
using Xunit;

namespace CeyPASSCihazPanel.Tests
{
    public class MiniExcelTests : IDisposable
    {
        private readonly string _tempPath;

        public MiniExcelTests()
        {
            _tempPath = Path.Combine(Path.GetTempPath(), $"miniexcel_test_{Guid.NewGuid()}.xlsx");
        }

        public void Dispose()
        {
            if (File.Exists(_tempPath))
                File.Delete(_tempPath);
        }

        private static DataTable BuildTable(string[] columns, object[][] rows = null)
        {
            var dt = new DataTable();
            foreach (var col in columns)
                dt.Columns.Add(col);

            if (rows != null)
                foreach (var row in rows)
                    dt.Rows.Add(row);

            return dt;
        }

        private static string ReadEntry(ZipArchive zip, string entryName)
        {
            var entry = zip.GetEntry(entryName);
            if (entry == null) return null;
            using (var reader = new StreamReader(entry.Open()))
                return reader.ReadToEnd();
        }

        [Fact]
        public void CreateXlsx_ProducesValidZipFile()
        {
            var dt = BuildTable(new[] { "Ad", "Soyad" }, new object[][] { new object[] { "Ali", "Veli" } });

            MiniExcel.CreateXlsx(_tempPath, dt);

            File.Exists(_tempPath).Should().BeTrue();
            Action open = () =>
            {
                using (var zip = ZipFile.OpenRead(_tempPath)) { }
            };
            open.Should().NotThrow();
        }

        [Fact]
        public void CreateXlsx_ContainsRequiredEntries()
        {
            var dt = BuildTable(new[] { "Col1" });

            MiniExcel.CreateXlsx(_tempPath, dt);

            using (var zip = ZipFile.OpenRead(_tempPath))
            {
                var names = zip.Entries.Select(e => e.FullName).ToList();
                names.Should().Contain("[Content_Types].xml");
                names.Should().Contain("xl/workbook.xml");
                names.Should().Contain("xl/worksheets/sheet1.xml");
            }
        }

        [Fact]
        public void CreateXlsx_HeaderRow_ContainsColumnNames()
        {
            var dt = BuildTable(new[] { "PersonelId", "Ad" });

            MiniExcel.CreateXlsx(_tempPath, dt);

            using (var zip = ZipFile.OpenRead(_tempPath))
            {
                var sheet = ReadEntry(zip, "xl/worksheets/sheet1.xml");
                sheet.Should().Contain("PersonelId");
                sheet.Should().Contain("Ad");
            }
        }

        [Fact]
        public void CreateXlsx_DataRows_WrittenCorrectly()
        {
            var dt = BuildTable(
                new[] { "Ad", "Soyad" },
                new object[][] {
                    new object[] { "Ahmet", "Yilmaz" },
                    new object[] { "Mehmet", "Kaya" }
                });

            MiniExcel.CreateXlsx(_tempPath, dt);

            using (var zip = ZipFile.OpenRead(_tempPath))
            {
                var sheet = ReadEntry(zip, "xl/worksheets/sheet1.xml");
                sheet.Should().Contain("Ahmet");
                sheet.Should().Contain("Yilmaz");
                sheet.Should().Contain("Mehmet");
                sheet.Should().Contain("Kaya");
            }
        }

        [Fact]
        public void CreateXlsx_EmptyDataTable_OnlyHeaderWritten()
        {
            var dt = BuildTable(new[] { "Sütun1", "Sütun2" });

            MiniExcel.CreateXlsx(_tempPath, dt);

            using (var zip = ZipFile.OpenRead(_tempPath))
            {
                var sheet = ReadEntry(zip, "xl/worksheets/sheet1.xml");
                sheet.Should().Contain(@"<row r=""1"">");
                sheet.Should().NotContain(@"<row r=""2"">");
            }
        }

        [Fact]
        public void CreateXlsx_XmlSpecialChars_AreEscaped()
        {
            var dt = BuildTable(
                new[] { "Deger" },
                new object[][] { new object[] { "A & B < C > D" } });

            MiniExcel.CreateXlsx(_tempPath, dt);

            using (var zip = ZipFile.OpenRead(_tempPath))
            {
                var sheet = ReadEntry(zip, "xl/worksheets/sheet1.xml");
                sheet.Should().Contain("&amp;");
                sheet.Should().Contain("&lt;");
                sheet.Should().Contain("&gt;");
                sheet.Should().NotContain("A & B");
            }
        }

        [Fact]
        public void CreateXlsx_OverwritesExistingFile()
        {
            var dt = BuildTable(new[] { "Col" }, new object[][] { new object[] { "ilk" } });
            MiniExcel.CreateXlsx(_tempPath, dt);

            var dt2 = BuildTable(new[] { "Col" }, new object[][] { new object[] { "ikinci" } });
            Action act = () => MiniExcel.CreateXlsx(_tempPath, dt2);

            act.Should().NotThrow();
            File.Exists(_tempPath).Should().BeTrue();
            using (var zip = ZipFile.OpenRead(_tempPath))
            {
                var sheet = ReadEntry(zip, "xl/worksheets/sheet1.xml");
                sheet.Should().Contain("ikinci");
            }
        }

        [Theory]
        [InlineData(1, "A")]
        [InlineData(2, "B")]
        [InlineData(26, "Z")]
        [InlineData(27, "AA")]
        public void GetColumnName_MappedCorrectly(int index, string expected)
        {
            MiniExcel.GetColumnName(index).Should().Be(expected);
        }
    }
}
