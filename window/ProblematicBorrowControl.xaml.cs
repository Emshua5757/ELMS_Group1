using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ELMS_Group1.database;
using ELMS_Group1.model;
using MigraDoc.Rendering;

namespace ELMS_Group1.window
{
    public partial class ProblematicBorrowControl : UserControl
    {
        private bool _isExpanded = false;
        private readonly SupabaseService _supabaseService = new SupabaseService();

        public ProblematicBorrowControl()
        {
            InitializeComponent();
        }

        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isExpanded = !_isExpanded;
            DetailsPanel.Visibility = _isExpanded ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void ResolveButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is BorrowRecordDetail record)
            {
                // Update status to Resolved and set returned date
                record.BorrowStatus.StatusEnum = BorrowStatus.Resolved;
                record.BorrowStatus.Status = BorrowStatus.Resolved.ToString(); // Fix: Convert enum to string
                record.BorrowStatus.ReturnedDate = DateTime.UtcNow;

                var (success, message) = await _supabaseService.UpdateBorrowHistoryAsync(record.BorrowStatus, record.Admin);

                if (success)
                {
                    MessageBox.Show("Item resolved and inventory updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Failed to resolve item: " + message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private async void GenerateReportButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is BorrowRecordDetail record)
            {
                if (record.Admin == null)
                {
                    MessageBox.Show("Cannot generate report: No admin is signed in.",
                                    "Missing Admin", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string reportType;

                switch (record.BorrowStatus.StatusEnum)
                {
                    case BorrowStatus.ReturnedLate:
                    case BorrowStatus.Overdue:
                        reportType = ReportTypes.ReturnLate;
                        break;

                    case BorrowStatus.Lost:
                        reportType = ReportTypes.Lost;
                        break;

                    case BorrowStatus.Damaged:
                        reportType = ReportTypes.Damaged;
                        break;

                    default:
                        reportType = ReportTypes.ReturnLate;
                        break;
                }

                var (success, message) = await _supabaseService.CreateBorrowReportAsync(record, reportType);

                if (success)
                {
                    MessageBox.Show("Report generated and saved to database.",
                                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Failed to create report:\n" + message,
                                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        public void ExpandDetails()
        {
            DetailsPanel.Visibility = Visibility.Visible;
            _isExpanded = true;
        }

        public void CollapseDetails()
        {
            DetailsPanel.Visibility = Visibility.Collapsed;
            _isExpanded = false;
        }

        public bool IsExpanded => DetailsPanel.Visibility == Visibility.Visible;

        public string GetSearchText()
        {
            if (DataContext is BorrowRecordDetail record)
            {
                var parts = new List<string>
                {
                    record.BorrowStatus.BorrowerName ?? "",
                    record.BorrowStatus.Status ?? "",
                    record.BorrowStatus.Id.ToString(),
                    record.BorrowStatus.InventoryId.ToString(),
                    record.BorrowStatus.BorrowedDate.ToString("yyyy-MM-dd"),
                    record.BorrowStatus.DueDate?.ToString("yyyy-MM-dd") ?? ""
                };

                if (record.Inventory != null)
                {
                    parts.Add(record.Inventory.Name ?? "");
                    parts.Add(record.Inventory.Description ?? "");
                    parts.Add(record.Inventory.SerialNumber ?? "");
                    parts.Add(record.Inventory.Category ?? "");
                    parts.Add(record.Inventory.Status ?? "");
                }

                if (record.Admin != null)
                {
                    parts.Add(record.Admin.FullName ?? "");
                    parts.Add(record.Admin.Email ?? "");
                    parts.Add(record.Admin.Phone ?? "");
                }

                return string.Join(" ", parts).ToLowerInvariant();
            }

            return "";
        }
    }
}
