using ELMS_Group1.model;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ELMS_Group1.window
{
    /// <summary>
    /// Interaction logic for ReportCard.xaml
    /// </summary>
    public partial class ReportCard : UserControl
    {
        public Report ReportData { get; set; }

        public ReportCard(Report report)
        {
            InitializeComponent();
            ReportData = report;
            DataContext = ReportData;

            // Hide content preview if it's a PDF
            ContentPreview.Visibility = report.Format?.ToLower() == "pdf" ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Download_Click(object sender, RoutedEventArgs e)
        {
            string extension = ReportData.Format?.ToLower() switch
            {
                "csv" => ".csv",
                "json" => ".json",
                "markdown" => ".md",
                "pdf" => ".pdf",
                _ => ".txt"
            };

            string suggestedFileName = $"{ReportData.Title ?? "Report"}_{ReportData.Id}{extension}";

            var dialog = new SaveFileDialog
            {
                FileName = suggestedFileName,
                Filter = "All files (*.*)|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    if (ReportData.Format?.ToLower() == "pdf")
                    {
                        // Decode base64 and save binary for PDF
                        var bytes = Convert.FromBase64String(ReportData.Content);
                        File.WriteAllBytes(dialog.FileName, bytes);
                    }
                    else
                    {
                        File.WriteAllText(dialog.FileName, ReportData.Content);
                    }

                    MessageBox.Show("Report downloaded successfully.", "Download Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error writing file:\n{ex.Message}", "Download Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void ReportCard_Click(object sender, MouseButtonEventArgs e)
        {
            var format = ReportData.Format?.ToLower();
            if (format == "pdf")
            {
                MessageBox.Show("PDF preview is not supported. Please download the file to view it.", "Preview Unavailable", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var preview = new ReportPreviewWindow(ReportData.Title ?? "Preview", ReportData.Content);
            preview.ShowDialog();
        }

    }

}
