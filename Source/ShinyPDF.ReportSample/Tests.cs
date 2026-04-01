using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ShinyPDF.Drawing;
using ShinyPDF.Fluent;
using ShinyPDF.Infrastructure;
using ShinyPDF.ReportSample.Layouts;

namespace ShinyPDF.ReportSample
{
    public class ReportGeneration
    {
        private StandardReport Report { get; set; }
        
        [SetUp]
        public void SetUp()
        {
            var model = DataSource.GetReport();
            Report = new StandardReport(model);
        }
        
        [Test] 
        public void GenerateAndShowPdf()
        {
            //ImagePlaceholder.Solid = true;
        
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"test_result.pdf");
            Report.GeneratePdf(path);
            Process.Start("explorer.exe", path);
        }
        
        [Test] 
        public void GenerateAndShowXps()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"test_result.xps");
            Report.GenerateXps(path);
            Process.Start("explorer.exe", path);
        }
    }
}