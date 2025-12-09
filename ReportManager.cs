using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using System;
using System.IO;

namespace ProjectMyntra.Reports
{
    public static class ReportManager
    {
        private static ExtentReports _extent;

        public static ExtentReports GetReporter()
        {
            if (_extent == null)
            {
                var reportPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    $"MyntraReport_{DateTime.Now:yyyyMMdd_HHmmss}.html"
                );

                var reporter = new ExtentHtmlReporter(reportPath);
                _extent = new ExtentReports();
                _extent.AttachReporter(reporter);
            }
            return _extent;
        }
    }
}
