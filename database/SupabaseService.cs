using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Security.Cryptography;
using ELMS_Group1.model;
using System.Globalization;
using System.Text.RegularExpressions;
using MigraDoc.Rendering;
using System.IO;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;

namespace ELMS_Group1.database
{
    internal class SupabaseService
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly string supabaseUrl = "https://dfitlnephdaiappcrsqt.supabase.co";
        private readonly string supabaseAnonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImRmaXRsbmVwaGRhaWFwcGNyc3F0Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDc5NjY3OTksImV4cCI6MjA2MzU0Mjc5OX0.aqWeBMRnZ-HHE2nVZaPvr0PwFn9ncj7B3LdyNF_-6XY";
        public SupabaseService()
        {
            _httpClient = new HttpClient();
            PrepareHeaders();
        }
        private void PrepareHeaders()
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("apikey", supabaseAnonKey);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {supabaseAnonKey}");
            _httpClient.DefaultRequestHeaders.Add("Prefer", "return=representation");
        }
        public async Task<(bool success, string message)> CreateAsync(string table, object payload)
        {
            try
            {
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{supabaseUrl}/rest/v1/{table}", content);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return (true, responseContent);
                }
                else
                {
                    return (false, $"Error: {response.StatusCode} - {responseContent}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Exception: {ex.Message}");
            }
        }
        public async Task<(bool success, string message)> CreateAsync<T>(string table, List<T> payloadList)
        {
            try
            {
                var content = new StringContent(JsonSerializer.Serialize(payloadList), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{supabaseUrl}/rest/v1/{table}", content);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return (true, responseContent);
                }
                else
                {
                    return (false, $"Error: {response.StatusCode} - {responseContent}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Exception: {ex.Message}");
            }
        }
        public async Task<(bool success, string message)> ReadAsync(string table, string? filter = null)
        {
            try
            {
                string url = $"{supabaseUrl}/rest/v1/{table}";
                if (string.IsNullOrWhiteSpace(filter))
                    url += "?select=*";
                else
                    url += $"?{filter}&select=*";

                var response = await _httpClient.GetAsync(url);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return (true, responseContent);
                }
                else
                {
                    return (false, $"Error: {response.StatusCode} - {responseContent}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Exception: {ex.Message}");
            }
        }
        public async Task<(bool success, string message)> UpdateAsync(string table, string filter, object payload)
        {
            try
            {
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await _httpClient.PatchAsync($"{supabaseUrl}/rest/v1/{table}?{filter}", content);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return (true, responseContent);
                }
                else
                {
                    return (false, $"Error: {response.StatusCode} - {responseContent}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Exception: {ex.Message}");
            }
        }
        public async Task<(bool success, string message)> DeleteAsync(string table, string filter)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{supabaseUrl}/rest/v1/{table}?{filter}");
                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return (true, responseContent);
                }
                else
                {
                    return (false, $"Error: {response.StatusCode} - {responseContent}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Exception: {ex.Message}");
            }
        }
        public async Task<(bool success, string message, Admin? admin)> VerifyAdminLoginAsync(string email, string password)
        {
            try
            {
                // Step 1: Get only the password hash
                string hashQuery = $"email=eq.{Uri.EscapeDataString(email)}&select=id,password_hash";
                var hashResult = await ReadAsync("AdminTable", hashQuery);

                if (!hashResult.success)
                    return (false, hashResult.message, null);

                var hashRecords = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(hashResult.message);

                if (hashRecords == null || hashRecords.Count == 0)
                    return (false, "Email not found.", null);

                string storedHash = hashRecords[0]["password_hash"].ToString()!;
                string inputHash = ComputeSha256Hash(password);

                if (inputHash != storedHash)
                    return (false, "Invalid password.", null);

                string id = hashRecords[0]["id"].ToString()!;
                string fullAdminQuery = $"id=eq.{id}";
                var adminResult = await ReadAsync("AdminTable", fullAdminQuery);

                if (!adminResult.success)
                    return (false, "Login succeeded but failed to load admin details.", null);

                var admins = JsonSerializer.Deserialize<List<Admin>>(adminResult.message);

                if (admins != null && admins.Count > 0)
                    return (true, "Login successful.", admins[0]);
                else
                    return (true, "Login successful, but admin details missing.", null);
            }
            catch (Exception ex)
            {
                return (false, $"Exception: {ex.Message}", null);
            }
        }
        public async Task<(bool success, string message)> RegisterAdminAsync(string fullName, string email, string phone, string password)
        {
            try
            {
                string passwordHash = ComputeSha256Hash(password);

                var payload = new
                {
                    full_name = fullName,
                    email = email,
                    phone = phone,
                    password_hash = passwordHash
                };

                return await CreateAsync("AdminTable", payload);
            }
            catch (Exception ex)
            {
                return (false, $"Exception: {ex.Message}");
            }
        }
        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();

                foreach (byte b in bytes)
                    builder.Append(b.ToString("x2"));

                return builder.ToString();
            }
        }
        public async Task<(bool success, Admin? admin, string message)> GetAdminByNameAsync(string fullName)
        {
            try
            {
                string filter = $"full_name=eq.{Uri.EscapeDataString(fullName)}";
                var readResult = await ReadAsync("AdminTable", filter);

                if (!readResult.success)
                    return (false, null, readResult.message);

                var admins = JsonSerializer.Deserialize<List<Admin>>(readResult.message);
                if (admins != null && admins.Count > 0)
                    return (true, admins[0], "Admin found.");
                else
                    return (false, null, "Admin not found.");
            }
            catch (Exception ex)
            {
                return (false, null, $"Exception: {ex.Message}");
            }
        }
        public async Task<(bool success, Admin? admin, string message)> GetAdminByIDAsync(long AdminId)
        {
            try
            {
                var readResult = await ReadAsync("AdminTable");
                if (!readResult.success)
                    return (false, null, readResult.message);

                var admins = JsonSerializer.Deserialize<List<Admin>>(readResult.message);
                if (admins == null)
                    return (false, null, "No inventory data found.");

                var matchedAdmin = admins.FirstOrDefault(admin => admin.Id == AdminId);
                if (matchedAdmin == null)
                    return (false, null, $"Item with ID {AdminId} not found.");

                return (true, matchedAdmin, "Item found successfully.");
            }
            catch (Exception ex)
            {
                return (false, null, $"Exception: {ex.Message}");
            }
        }
        public async Task<(bool success, List<Admin>? admins, string message)> SearchAdminsAsync(string searchText)
        {
            try
            {
                string encodedText = Uri.EscapeDataString($"%{searchText}%");
                string filter = $"or=(full_name.ilike.{encodedText},email.ilike.{encodedText},phone.ilike.{encodedText})";

                var readResult = await ReadAsync("AdminTable", filter);
                if (!readResult.success)
                    return (false, null, readResult.message);

                var admins = JsonSerializer.Deserialize<List<Admin>>(readResult.message);
                return (true, admins, "Search completed successfully.");
            }
            catch (Exception ex)
            {
                return (false, null, $"Exception: {ex.Message}");
            }
        }
        public async Task<(bool success, List<User>? users, string message)> SearchUsersAsync(string searchText)
        {
            try
            {
                string encodedText = Uri.EscapeDataString($"%{searchText}%");

                string filter = $"or=(" +
                                $"full_name.ilike.{encodedText}," +
                                $"id_number.ilike.{encodedText}," +
                                $"course_year.ilike.{encodedText}," +
                                $"email.ilike.{encodedText}," +
                                $"address.ilike.{encodedText}" +
                                $")";

                var readResult = await ReadAsync("User", filter);
                if (!readResult.success)
                    return (false, null, readResult.message);

                var users = JsonSerializer.Deserialize<List<User>>(readResult.message);
                return (true, users, "User search completed successfully.");
            }
            catch (Exception ex)
            {
                return (false, null, $"Exception: {ex.Message}");
            }
        }
        public async Task<(bool success, List<InventoryItem>? items, string message)> SearchInventoryItemsAsync(string searchText)
        {
            return await SearchInventoryItemsAsync(searchText, null);
        }
        public async Task<(bool success, List<InventoryItem>? items, string message)> SearchInventoryItemsAsync(string searchText, string? statusFilter)
        {
            try
            {
                var readResult = await ReadAsync("Inventory");
                if (!readResult.success)
                    return (false, null, readResult.message);

                var items = JsonSerializer.Deserialize<List<InventoryItem>>(readResult.message);
                if (items == null)
                    return (false, null, "No items found.");

                // Apply client-side filtering
                IEnumerable<InventoryItem> filtered = items;

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    string lowerSearch = searchText.ToLower();
                    filtered = filtered.Where(item =>
                        (item.Name?.ToLower().Contains(lowerSearch) ?? false) ||
                        (item.Category?.ToLower().Contains(lowerSearch) ?? false) ||
                        (item.Description?.ToLower().Contains(lowerSearch) ?? false) ||
                        (item.SerialNumber?.ToLower().Contains(lowerSearch) ?? false) ||
                        (item.Location?.ToLower().Contains(lowerSearch) ?? false));
                }

                if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "No Filter")
                {
                    filtered = filtered.Where(item =>
                        string.Equals(item.Status, statusFilter, StringComparison.OrdinalIgnoreCase));
                }

                return (true, filtered.ToList(), "Client-side search completed successfully.");
            }
            catch (Exception ex)
            {
                return (false, null, $"Exception: {ex.Message}");
            }
        }
        public async Task<(bool success, List<BorrowedStatus>? records, string message)> SearchBorrowedStatusAsync(string searchText, string? statusFilter = null, long? adminId = null, long? userId = null)
        {
            try
            {
                var filters = new List<string>();

                if (!string.IsNullOrWhiteSpace(statusFilter))
                    filters.Add($"status=eq.{Uri.EscapeDataString(statusFilter)}");

                if (adminId.HasValue)
                    filters.Add($"approved_by=eq.{adminId.Value}");

                if (userId.HasValue)
                    filters.Add($"user_id=eq.{userId.Value}");

                string filter = filters.Count > 0 ? $"select=*&{string.Join("&", filters)}" : "select=*";

                var readResult = await ReadAsync("BorrowedStatus", filter);
                if (!readResult.success)
                    return (false, null, readResult.message);

                var records = JsonSerializer.Deserialize<List<BorrowedStatus>>(readResult.message);
                if (records == null)
                    return (false, null, "Failed to deserialize results.");

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    var lowerSearch = searchText.ToLower();
                    records = records.Where(r =>
                        (!string.IsNullOrWhiteSpace(r.BorrowerName) && r.BorrowerName.ToLower().Contains(lowerSearch)) ||
                        (!string.IsNullOrWhiteSpace(r.Status) && r.Status.ToLower().Contains(lowerSearch))
                    ).ToList();
                }

                return (true, records, "Borrowed status search completed successfully.");
            }
            catch (Exception ex)
            {
                return (false, null, $"Exception: {ex.Message}");
            }
        }
        public async Task<(bool success, List<PendingUser>? users, string message)> SearchPendingUsersAsync(string searchText)
        {
            try
            {
                string encodedText = Uri.EscapeDataString($"%{searchText}%");
                // Search full_name, id_number, course_year with ilike
                string filter = $"or=(full_name.ilike.{encodedText},id_number.ilike.{encodedText},course_year.ilike.{encodedText})";

                var readResult = await ReadAsync("PendingUser", filter);
                if (!readResult.success)
                    return (false, null, readResult.message);

                var users = JsonSerializer.Deserialize<List<PendingUser>>(readResult.message);
                return (true, users, "Pending user search completed successfully.");
            }
            catch (Exception ex)
            {
                return (false, null, $"Exception: {ex.Message}");
            }
        }
        public async Task<(bool success, List<Report>? reports, string message)> SearchReportsAsync(string searchText, long? adminId = null)
        {
            try
            {
                string encodedText = Uri.EscapeDataString($"%{searchText}%");

                // Base filter: report_type, title, or content ilike
                var filters = new List<string>
                {
                    $"or=(report_type.ilike.{encodedText},title.ilike.{encodedText},content.ilike.{encodedText})"
                };

                // Filter by admin_id if provided
                if (adminId.HasValue)
                {
                    filters.Add($"admin_id.eq.{adminId.Value}");
                }

                // Combine filters with AND if multiple
                string filter = filters.Count > 1
                    ? $"and({string.Join(",", filters)})"
                    : filters[0];

                var readResult = await ReadAsync("Report", filter); // Use quoted "Report" since it has uppercase
                if (!readResult.success)
                    return (false, null, readResult.message);

                var reports = JsonSerializer.Deserialize<List<Report>>(readResult.message);
                return (true, reports, "Report search completed successfully.");
            }
            catch (Exception ex)
            {
                return (false, null, $"Exception: {ex.Message}");
            }
        }
        public async Task<(bool success, string message)> SubmitPendingUserApplicationAsync(string fullName, string idNumber, string courseYear, string email, string address, string password)
        {
            try
            {
                string passwordHash = ComputeSha256Hash(password);

                var newUser = new Dictionary<string, object>
        {
            { "full_name", fullName },
            { "id_number", idNumber },
            { "course_year", courseYear },
            { "password_hash", passwordHash },
            { "registered_at", DateTime.UtcNow },
            { "is_approved", null },
            { "admin_feedback", "" },
            { "reviewed_at", null },
            { "email", email },
            { "address", address }
        };

                var response = await CreateAsync("PendingUser", newUser);

                return response.success
                    ? (true, "Application submitted successfully.")
                    : (false, response.message);
            }
            catch (Exception ex)
            {
                return (false, $"Exception: {ex.Message}");
            }
        }
        public async Task<(bool success, string message)> ApproveAndAddUserAsync(long pendingUserId, Admin admin)
        {
            try
            {
                // Step 1: Retrieve pending user data as raw JSON
                var filter = $"id=eq.{pendingUserId}";
                var pendingResult = await ReadAsync("PendingUser", filter);

                if (!pendingResult.success)
                    return (false, $"Failed to retrieve pending user: {pendingResult.message}");

                using var doc = JsonDocument.Parse(pendingResult.message);
                var root = doc.RootElement;

                if (root.GetArrayLength() == 0)
                    return (false, "Pending user not found.");

                var pendingUserJson = root[0];

                // Step 2: Prepare User payload dictionary directly from JSON properties
                var newUserPayload = new Dictionary<string, object>
                {
                    ["full_name"] = pendingUserJson.GetProperty("full_name").GetString() ?? string.Empty,
                    ["id_number"] = pendingUserJson.GetProperty("id_number").GetString() ?? string.Empty,
                    ["course_year"] = pendingUserJson.GetProperty("course_year").GetString() ?? string.Empty,
                    ["password_hash"] = pendingUserJson.GetProperty("password_hash").GetString() ?? string.Empty,
                    ["email"] = pendingUserJson.GetProperty("email").GetString() ?? string.Empty,
                    ["address"] = pendingUserJson.GetProperty("address").GetString() ?? string.Empty,
                    ["created_at"] = DateTime.UtcNow,
                    ["updated_at"] = DateTime.UtcNow
                };

                // Step 3: Insert into User table
                var createUserResult = await CreateAsync("User", newUserPayload);
                if (!createUserResult.success)
                    return (false, $"Failed to add to User table: {createUserResult.message}");

                // Step 4: Mark PendingUser as approved
                var updatePayload = new
                {
                    is_approved = true,
                    reviewed_at = DateTime.UtcNow,
                    reviewed_by = admin.FullName
                };

                var updatePendingResult = await UpdateAsync("PendingUser", filter, updatePayload);

                if (!updatePendingResult.success)
                    return (false, $"User added but failed to update pending status: {updatePendingResult.message}");

                // Step 5: Add audit logging report
                string title = $"User approved: {newUserPayload["full_name"]}";
                string content = $"Pending user ID {pendingUserId} was approved by admin '{admin.FullName}' (ID: {admin.Id}) and added as a user.";

                var (logSuccess, logMessage) = await CreateReportAsync(
                    reportType: "approve_user",
                    title: title,
                    content: content,
                    format: "text",
                    adminId: admin.Id
                );

                if (!logSuccess)
                {
                    // Return success but notify about logging failure
                    return (true, $"User approved, but logging failed: {logMessage}");
                }

                return (true, "User approved and added to User table.");
            }
            catch (Exception ex)
            {
                return (false, $"Exception: {ex.Message}");
            }
        }
        public async Task<(bool success, string message)> RejectPendingUserAsync(long pendingUserId, string feedbackMessage, Admin admin)
        {
            try
            {
                var payload = new
                {
                    is_approved = false,
                    admin_feedback = feedbackMessage,
                    reviewed_at = DateTime.UtcNow,
                    reviewed_by = admin.FullName
                };

                string filter = $"id=eq.{pendingUserId}";
                var updateResult = await UpdateAsync("PendingUser", filter, payload);

                if (!updateResult.success)
                    return (false, $"Failed to update pending user status: {updateResult.message}");

                // Logging the rejection action
                string title = $"Pending user rejected (ID: {pendingUserId})";
                string content = $"Pending user ID {pendingUserId} was rejected by admin '{admin.FullName}' (ID: {admin.Id}). Feedback: {feedbackMessage}";

                var (logSuccess, logMessage) = await CreateReportAsync(
                    reportType: "reject_user",
                    title: title,
                    content: content,
                    format: "text",
                    adminId: admin.Id
                );

                if (!logSuccess)
                {
                    // Return success but note logging failure
                    return (true, $"Pending user rejected, but logging failed: {logMessage}");
                }

                return (true, "Pending user rejected and updated successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"Exception: {ex.Message}");
            }
        }
        public async Task<(bool success, string message, User? user)> UserLoginEmailorNameAsync(string emailOrName, string password)
        {
            try
            {
                string filter = $"or=(email.eq.{Uri.EscapeDataString(emailOrName)},full_name.eq.{Uri.EscapeDataString(emailOrName)})&select=id,password_hash";
                var hashResult = await ReadAsync("User", filter);

                if (!hashResult.success)
                    return (false, $"Failed to query User table: {hashResult.message}", null);

                var hashRecords = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(hashResult.message);

                if (hashRecords == null || hashRecords.Count == 0)
                    return (false, "User not found.", null);

                string storedHash = hashRecords[0]["password_hash"].ToString()!;
                string inputHash = ComputeSha256Hash(password);

                if (inputHash != storedHash)
                    return (false, "Invalid password.", null);

                string id = hashRecords[0]["id"].ToString()!;
                var fullUserResult = await ReadAsync("User", $"id=eq.{id}");

                if (!fullUserResult.success)
                    return (false, "Login succeeded but failed to load user details.", null);

                var users = JsonSerializer.Deserialize<List<User>>(fullUserResult.message);

                if (users != null && users.Count > 0)
                    return (true, "Login successful.", users[0]);
                else
                    return (true, "Login successful, but user details missing.", null);
            }
            catch (Exception ex)
            {
                return (false, $"Exception: {ex.Message}", null);
            }
        }
        public async Task<(bool success, string message, PendingUser? pendingUser)> PendingUserLoginEmailorNameAsync(string emailOrName, string password)
        {
            try
            {
                string filter = $"or=(email.eq.{Uri.EscapeDataString(emailOrName)},full_name.eq.{Uri.EscapeDataString(emailOrName)})&select=id,password_hash,is_approved,admin_feedback";
                var hashResult = await ReadAsync("PendingUser", filter);

                if (!hashResult.success)
                    return (false, $"Failed to query PendingUser table: {hashResult.message}", null);

                var hashRecords = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(hashResult.message);

                if (hashRecords == null || hashRecords.Count == 0)
                    return (false, "Pending user not found.", null);

                string storedHash = hashRecords[0]["password_hash"].ToString()!;
                string inputHash = ComputeSha256Hash(password);

                if (inputHash != storedHash)
                    return (false, "Invalid password.", null);

                string id = hashRecords[0]["id"].ToString()!;
                var fullPendingResult = await ReadAsync("PendingUser", $"id=eq.{id}");

                if (!fullPendingResult.success)
                    return (false, "Login succeeded but failed to load pending user details.", null);

                var pendingUsers = JsonSerializer.Deserialize<List<PendingUser>>(fullPendingResult.message);

                if (pendingUsers != null && pendingUsers.Count > 0)
                    return (true, "Login successful.", pendingUsers[0]);
                else
                    return (true, "Login successful, but pending user details missing.", null);
            }
            catch (Exception ex)
            {
                return (false, $"Exception: {ex.Message}", null);
            }
        }
        public async Task<(bool success, string message)> DeletePendingUserAsync(long pendingUserId)
        {
            try
            {
                string filter = $"id=eq.{pendingUserId}";
                var deleteResult = await DeleteAsync("PendingUser", filter);
                if (deleteResult.success)
                    return (true, "Pending user application deleted successfully.");
                else
                    return (false, $"Failed to delete pending user application: {deleteResult.message}");
            }
            catch (Exception ex)
            {
                return (false, $"Exception: {ex.Message}");
            }
        }
        public async Task<(bool success, string message)> DeleteUserAsync(long userId, Admin? admin)
        {
            try
            {
                string filter = $"id=eq.{userId}";
                var deleteResult = await DeleteAsync("User", filter);

                if (!deleteResult.success)
                    return (false, $"Failed to delete user: {deleteResult.message}");

                // Add audit logging
                string title = $"User deleted (ID: {userId})";
                string content = $"User ID {userId} was deleted by admin '{admin.FullName}' (ID: {admin.Id}).";

                var (logSuccess, logMessage) = await CreateReportAsync(
                    reportType: "delete_user",
                    title: title,
                    content: content,
                    format: "text",
                    adminId: admin.Id
                );

                if (!logSuccess)
                {
                    return (true, $"User deleted successfully, but logging failed: {logMessage}");
                }

                return (true, "User deleted successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"Exception: {ex.Message}");
            }
        }
        public async Task<(bool success, string message)> UpdateUserData(User user, Admin admin)
        {
            try
            {
                if (user == null)
                    return (false, "User cannot be null.");

                string filter = $"id=eq.{user.Id}";

                var updateResult = await UpdateAsync("User", filter, user);

                if (!updateResult.success)
                    return (false, $"Failed to update user: {updateResult.message}");

                string title = $"User updated (ID: {user.Id})";
                string content = $"User ID {user.Id} was updated by admin '{admin.FullName}' (ID: {admin.Id}).";

                var (logSuccess, logMessage) = await CreateReportAsync(
                    reportType: "update_user",
                    title: title,
                    content: content,
                    format: "text",
                    adminId: admin.Id
                );

                if (!logSuccess)
                {
                    return (true, $"User updated successfully, but logging failed: {logMessage}");
                }

                return (true, "User updated successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"Exception: {ex.Message}");
            }
        }
        public async Task<(bool hasDuplicates, string message, Dictionary<string, List<string>> duplicates)> CheckForDuplicatesAsync(string tableName, Dictionary<string, List<string>> fieldValueMap)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tableName) || fieldValueMap.Count == 0)
                    return (false, "Invalid table name or fields.", new());

                var orFilters = new List<string>();

                foreach (var field in fieldValueMap)
                {
                    string fieldName = field.Key;
                    var values = field.Value.Distinct().ToList();
                    if (values.Count == 0) continue;

                    string joined = string.Join(",", values.Select(v => $"\"{v}\""));
                    orFilters.Add($"{fieldName}.in.({joined})");
                }

                string filter = $"or=({string.Join(",", orFilters)})";

                string selectFields = string.Join(",", fieldValueMap.Keys);

                var result = await ReadAsync(tableName, $"{filter}&select={selectFields}");

                if (!result.success)
                    return (false, $"Query failed: {result.message}", new());

                var records = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(result.message);
                if (records == null || records.Count == 0)
                    return (false, "No duplicates found.", new());

                var duplicates = new Dictionary<string, List<string>>();

                foreach (var record in records)
                {
                    foreach (var field in fieldValueMap.Keys)
                    {
                        if (record.ContainsKey(field))
                        {
                            string val = record[field]?.ToString() ?? "";
                            if (!duplicates.ContainsKey(field))
                                duplicates[field] = new List<string>();
                            if (!duplicates[field].Contains(val))
                                duplicates[field].Add(val);
                        }
                    }
                }

                return (true, "Duplicates found.", duplicates);
            }
            catch (Exception ex)
            {
                return (false, $"Exception: {ex.Message}", new());
            }
        }
        private async Task<(bool success, string message)> AddInventoryItemsAsync(List<InventoryItem> items, long? createdByAdminId)
        {
            if (createdByAdminId == null || createdByAdminId <= 0)
                return (false, "Please log in with an admin account to add inventory items.");

            if (items == null || items.Count == 0)
                return (false, "No items to add.");

            // Assign CreatedBy admin ID to each item
            foreach (var item in items)
            {
                item.CreatedBy = createdByAdminId;
            }

            // Step 1: Validate all items (e.g. value > 0)
            var invalids = items
                .Where(i => i.Value <= 0 || string.IsNullOrWhiteSpace(i.SerialNumber) || string.IsNullOrWhiteSpace(i.Name) || string.IsNullOrWhiteSpace(i.Category))
                .ToList();

            if (invalids.Count > 0)
            {
                string summary = string.Join("\n", invalids.Select(i => $"- Invalid: Serial #{i.SerialNumber}, Value: {i.Value}"));
                return (false, $"Validation failed for the following items:\n{summary}");
            }

            // Step 2: Build map of fields to check for duplicates
            var fieldValueMap = new Dictionary<string, List<string>>
            {
                { "serial_number", items.Select(i => i.SerialNumber).ToList() }
            };

            // Step 3: Query Supabase for any duplicates
            var (hasDuplicates, _, duplicates) = await CheckForDuplicatesAsync("Inventory", fieldValueMap);

            if (hasDuplicates)
            {
                var msg = new StringBuilder("Duplicate values found:\n");
                foreach (var pair in duplicates)
                {
                    msg.AppendLine($"- {pair.Key}: {string.Join(", ", pair.Value)}");
                }
                return (false, msg.ToString());
            }

            // Step 4: Insert items into Inventory table
            var result = await CreateAsync("Inventory", items);

            if (result.success)
            {
                var itemSummaries = string.Join("\n", items.Select(i => $"- {i.Name} (Serial: {i.SerialNumber})"));
                string title = items.Count == 1
                    ? "Added 1 new inventory item"
                    : $"Added {items.Count} new inventory items";

                var (reportSuccess, reportMessage) = await CreateReportAsync(
                    reportType: "inventory_addition",
                    title: title,
                    content: $"The following items were added by admin ID {createdByAdminId}:\n{itemSummaries}",
                    format: "text",
                    adminId: createdByAdminId.Value
                );

                if (!reportSuccess)
                {
                    return (true, $"Items added successfully, but audit logging failed: {reportMessage}");
                }
            }

            return result;
        }
        public async Task<(bool success, string message)> AddSingleInventoryItemAsync(InventoryItem item, long? createdByAdminId)
        {
            if (createdByAdminId == null || createdByAdminId <= 0)
                return (false, "Please log in with an admin account to add inventory items.");

            return await AddInventoryItemsAsync(new List<InventoryItem> { item }, createdByAdminId);
        }
        public async Task<(bool success, string message)> AddMultipleInventoryItemsAsync(List<InventoryItem> items, long? createdByAdminId)
        {
            if (createdByAdminId == null || createdByAdminId <= 0)
                return (false, "Please log in with an admin account to add inventory items.");

            return await AddInventoryItemsAsync(items, createdByAdminId);
        }
        public async Task<(bool success, string message)> AddFromCsvAsync(string csv, long? createdByAdminId)
        {
            if (createdByAdminId == null || createdByAdminId <= 0)
                return (false, "Please log in with an admin account to add inventory items.");

            try
            {
                var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                var items = new List<InventoryItem>();

                string preferredFormat = "MM/dd/yyyy h:mm:ss tt";
                var culture = CultureInfo.InvariantCulture;

                foreach (var line in lines.Skip(1)) // Skip header
                {
                    var fields = ParseCsvLine(line);

                    if (fields.Count < 7)
                        continue;

                    // Parse AcquisitionDate from index 5
                    DateTime? acquisitionDate = null;
                    var dateStr = fields[5].Trim();

                    if (!string.IsNullOrWhiteSpace(dateStr))
                    {
                        if (!DateTime.TryParseExact(dateStr, preferredFormat, culture, DateTimeStyles.None, out var dt))
                        {
                            if (!DateTime.TryParse(dateStr, out dt))
                            {
                                dt = default;
                            }
                        }

                        if (dt != default)
                            acquisitionDate = dt;
                    }

                    items.Add(new InventoryItem
                    {
                        Name = fields[0].Trim(),
                        Category = fields[1].Trim(),
                        Description = fields[2].Trim(),
                        SerialNumber = fields[3].Trim(),
                        Location = fields[4].Trim(),
                        AcquisitionDate = acquisitionDate,
                        Value = decimal.TryParse(fields[6].Trim(), out var val) ? val : 0m,
                        Status = "Available"
                    });
                }

                return await AddInventoryItemsAsync(items, createdByAdminId);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to parse CSV: {ex.Message}");
            }
        }
        private List<string> ParseCsvLine(string line)
        {
            var fields = new List<string>();
            bool inQuotes = false;
            StringBuilder field = new();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"') // Escaped quote
                    {
                        field.Append('"');
                        i++; // skip next quote
                    }
                    else
                    {
                        inQuotes = !inQuotes; // toggle quotes state
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    fields.Add(field.ToString());
                    field.Clear();
                }
                else
                {
                    field.Append(c);
                }
            }

            fields.Add(field.ToString()); // Add last field
            return fields;
        }
        public async Task<(bool success, string message)> DeleteInventoryItemAsync(long? itemId, Admin admin)
        {
            try
            {
                if (itemId == null || itemId <= 0)
                    return (false, "Invalid inventory item ID.");

                if (admin == null || admin.Id <= 0)
                    return (false, "Invalid admin.");

                string filter = $"id=eq.{itemId}";

                // Optional: fetch item info before deletion for better audit details
                var fetchResult = await ReadAsync("Inventory", filter);
                InventoryItem? item = null;
                if (fetchResult.success)
                {
                    var items = JsonSerializer.Deserialize<List<InventoryItem>>(fetchResult.message);
                    if (items != null && items.Count > 0)
                        item = items[0];
                }

                var deleteResult = await DeleteAsync("Inventory", filter);

                if (!deleteResult.success)
                    return (false, $"Failed to delete inventory item: {deleteResult.message}");

                // Create audit log
                var itemSummary = item != null
                    ? $"- Inventory Item ID #{item.Id} ('{item.Name ?? "Unknown"}') deleted by Admin: {admin.FullName} (ID: {admin.Id})"
                    : $"- Inventory Item ID #{itemId} deleted by Admin: {admin.FullName} (ID: {admin.Id})";

                var (auditSuccess, auditMessage) = await CreateReportAsync(
                    reportType: "inventory_delete",
                    title: "Inventory Item Deleted",
                    content: itemSummary,
                    format: "text",
                    adminId: admin.Id
                );

                if (!auditSuccess)
                    return (true, $"Item deleted, but audit log failed: {auditMessage}");

                return (true, "Inventory item deleted successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"Exception: {ex.Message}");
            }
        }
        public async Task<(bool success, string message)> UpdateInventoryItemData(InventoryItem item, Admin admin)
        {
            try
            {
                if (item == null)
                    return (false, "Item cannot be null.");

                if (admin == null || admin.Id <= 0)
                    return (false, "Invalid admin.");

                string filter = $"id=eq.{item.Id}";

                // Optional: fetch old item for diff/audit
                var fetchResult = await ReadAsync("Inventory", filter);
                InventoryItem? oldItem = null;
                if (fetchResult.success)
                {
                    var items = JsonSerializer.Deserialize<List<InventoryItem>>(fetchResult.message);
                    if (items != null && items.Count > 0)
                        oldItem = items[0];
                }

                var updateResult = await UpdateAsync("Inventory", filter, item);

                if (!updateResult.success)
                    return (false, $"Failed to update inventory item: {updateResult.message}");

                // Create audit log with simple before-after summary
                var oldName = oldItem?.Name ?? "(unknown)";
                var newName = item.Name ?? "(unknown)";
                var itemSummary = $"- Inventory Item ID #{item.Id} updated by Admin: {admin.FullName} (ID: {admin.Id})\n" +
                                  $"  Name: '{oldName}' → '{newName}'";

                var (auditSuccess, auditMessage) = await CreateReportAsync(
                    reportType: "inventory_update",
                    title: "Inventory Item Updated",
                    content: itemSummary,
                    format: "text",
                    adminId: admin.Id
                );

                if (!auditSuccess)
                    return (true, $"Item updated, but audit log failed: {auditMessage}");

                return (true, "Inventory item updated successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"Exception: {ex.Message}");
            }
        }
        private async Task<(bool success, string message)> CreateReportAsync(string reportType, string title, string content, string format, long adminId)
        {
            var report = new Report
            {
                AdminId = adminId,
                ReportType = reportType,
                Title = title,
                Content = content,
                Format = format,
                CreatedAt = DateTime.UtcNow
            };

            var result = await CreateAsync("Report", new List<Report> { report });
            return result;
        }
        private async Task<(bool success, string message)> CreateInventoryExportAsync(Admin admin)
        {
            try
            {
                // Step 1: Read all inventory items
                var inventoryResult = await ReadAsync("Inventory");
                if (!inventoryResult.success)
                    return (false, $"Failed to fetch inventory: {inventoryResult.message}");

                using var doc = JsonDocument.Parse(inventoryResult.message);
                var root = doc.RootElement;
                if (root.GetArrayLength() == 0)
                    return (false, "No inventory items to export.");

                // Step 2: Build CSV content
                var sb = new StringBuilder();
                sb.AppendLine("ID,Name,Serial Number,Category,Value,Description,Created By,Created At");

                foreach (var item in root.EnumerateArray())
                {
                    sb.AppendLine(string.Join(",",
                        item.GetProperty("id").GetInt64(),
                        EscapeCsv(item.GetProperty("name").GetString()),
                        EscapeCsv(item.GetProperty("serial_number").GetString()),
                        EscapeCsv(item.GetProperty("category").GetString()),
                        item.GetProperty("value").GetDecimal(),
                        EscapeCsv(item.GetProperty("description").GetString() ?? ""),
                        item.GetProperty("created_by").GetInt64(),
                        item.GetProperty("created_at").GetDateTime().ToString("u")
                    ));
                }

                string csvContent = sb.ToString();

                // Step 3: Save report log
                var (reportSuccess, reportMessage) = await CreateReportAsync(
                    reportType: "inventory_export",
                    title: $"Inventory Export ({DateTime.UtcNow:yyyy-MM-dd})",
                    content: csvContent,
                    format: "csv",
                    adminId: admin.Id
                );

                return reportSuccess
                    ? (true, "Inventory exported and logged successfully.")
                    : (false, $"Export generated but logging failed: {reportMessage}");
            }
            catch (Exception ex)
            {
                return (false, $"Exception during export: {ex.Message}");
            }
        }
        private string EscapeCsv(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "";

            if (input.Contains(',') || input.Contains('"') || input.Contains('\n'))
                return $"\"{input.Replace("\"", "\"\"")}\"";

            return input;
        }
        private async Task<(bool success, long Count, string? ErrorMessage)> GetTableCount(string tableName, string filter = null)
        {
            try
            {
                var readResult = await ReadAsync(tableName, filter);
                if (!readResult.success)
                    return (false, 0, readResult.message);

                var records = JsonSerializer.Deserialize<List<object>>(readResult.message);

                long count = records?.Count ?? 0;
                return (true, count, null);
            }
            catch (Exception ex)
            {
                return (false, 0, $"Exception: {ex.Message}");
            }
        }
        public async Task<(bool success, StatsModel? Stats, string Message)> GetStats()
        {
            try
            {
                var (adminsSuccess, adminsCount, adminsError) = await GetTableCount("AdminTable");
                if (!adminsSuccess) return (false, null, $"Failed to get admins count: {adminsError}");

                var (usersSuccess, usersCount, usersError) = await GetTableCount("User");
                if (!usersSuccess) return (false, null, $"Failed to get users count: {usersError}");

                var (pendingSuccess, pendingUsersCount, pendingError) = await GetTableCount("PendingUser", "is_approved=is.null");
                if (!pendingSuccess) return (false, null, $"Failed to get pending users count: {pendingError}");

                var (inventorySuccess, inventoryCount, inventoryError) = await GetTableCount("Inventory");
                if (!inventorySuccess) return (false, null, $"Failed to get inventory count: {inventoryError}");

                var (borrowedSuccess, borrowedItemsCount, borrowedError) = await GetTableCount("BorrowedStatus", "status=eq.Borrowed");
                if (!borrowedSuccess) return (false, null, $"Failed to get borrowed items count: {borrowedError}");

                var (pendingBorrowSuccess, pendingBorrowsCount, pendingBorrowError) = await GetTableCount("BorrowedStatus", "status=eq.Pending");
                if (!pendingBorrowSuccess) return (false, null, $"Failed to get pending borrows count: {pendingBorrowError}");

                var (overdueSuccess, overdueCount, overdueError) = await GetTableCount("BorrowedStatus", "status=eq.Overdue");
                if (!overdueSuccess) return (false, null, $"Failed to get overdue items count: {overdueError}");

                var (damagedSuccess, damagedCount, damagedError) = await GetTableCount("BorrowedStatus", "status=eq.Damaged");
                if (!damagedSuccess) return (false, null, $"Failed to get damaged items count: {damagedError}");

                var (lostSuccess, lostCount, lostError) = await GetTableCount("BorrowedStatus", "status=eq.Lost");
                if (!lostSuccess) return (false, null, $"Failed to get lost items count: {lostError}");

                var stats = new StatsModel
                {
                    Admins = adminsCount,
                    Users = usersCount,
                    PendingUsers = pendingUsersCount,
                    InventoryItems = inventoryCount,
                    BorrowedItems = borrowedItemsCount,
                    PendingBorrows = pendingBorrowsCount,
                    OverdueItems = overdueCount,
                    DamagedItems = damagedCount,
                    LostItems = lostCount
                };

                return (true, stats, "Statistics loaded successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error in GetStats: {ex.Message}");
                return (false, null, $"Error loading stats: {ex.Message}");
            }
        }
        public async Task<(bool success, string message)> BorrowRequestItemAsync(long? inventoryId, long userId, string borrowerName)
        {
            // ----------------- Step 1: Validate Inputs -----------------
            if (!inventoryId.HasValue)
                return (false, "Inventory ID is required.");

            if (inventoryId.Value <= 0)
                return (false, "Inventory ID must be greater than zero.");

            if (userId <= 0)
                return (false, "Invalid user ID.");

            if (string.IsNullOrWhiteSpace(borrowerName))
                return (false, "Borrower name cannot be empty.");

            // ----------------- Step 2: Check for Existing Request -----------------
            var statusesToCheck = new List<string>
            {
                BorrowStatus.Pending.ToString(),
                BorrowStatus.Borrowed.ToString(),
                BorrowStatus.Overdue.ToString(),
                BorrowStatus.Lost.ToString(),
                BorrowStatus.Damaged.ToString()
            };

            var (hasDuplicate, duplicateMessage, duplicates) = await CheckBorrowRequestDuplicateAsync(inventoryId.Value, userId, statusesToCheck);

            if (hasDuplicate)
            {
                var msg = new StringBuilder("You already have an active or pending request for this item.\n");
                msg.AppendLine(duplicateMessage);

                msg.AppendLine("Duplicate values found:");
                foreach (var pair in duplicates)
                {
                    msg.AppendLine($"- {pair.Key}: {string.Join(", ", pair.Value)}");
                }
                return (false, msg.ToString());
            }

            // ----------------- Step 3: Create Borrow Request -----------------
            var borrowRequest = new BorrowedStatus
            {
                InventoryId = inventoryId.Value,
                UserId = userId,
                BorrowerName = borrowerName,
                BorrowedDate = DateTime.UtcNow,
                Status = BorrowStatus.Pending.ToString(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var (success, message) = await CreateAsync("BorrowedStatus", borrowRequest);

            return success
                ? (true, "Borrow request submitted successfully and is pending approval.")
                : (false, $"Failed to submit borrow request. {message}");
        }
        public async Task<(bool hasDuplicate, string message, Dictionary<string, List<string>> duplicates)> CheckBorrowRequestDuplicateAsync(long inventoryId, long userId, List<string> statuses)
        {
            try
            {
                if (inventoryId <= 0 || userId <= 0 || statuses == null || statuses.Count == 0)
                    return (false, "Invalid parameters.", new());

                string statusList = string.Join(",", statuses.Select(s => $"\"{s}\""));
                string filter = $"inventory_id=eq.{inventoryId}&user_id=eq.{userId}&status=in.({statusList})";

                string selectFields = "inventory_id,user_id,status";

                var result = await ReadAsync("BorrowedStatus", $"{filter}&select={selectFields}");

                if (!result.success)
                    return (false, $"Query failed: {result.message}", new());

                var records = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(result.message);
                if (records == null || records.Count == 0)
                    return (false, "No duplicates found.", new());

                var duplicates = new Dictionary<string, List<string>>();

                foreach (var record in records)
                {
                    foreach (var field in new[] { "inventory_id", "user_id", "status" })
                    {
                        if (record.ContainsKey(field))
                        {
                            string val = record[field]?.ToString() ?? "";
                            if (!duplicates.ContainsKey(field))
                                duplicates[field] = new List<string>();
                            if (!duplicates[field].Contains(val))
                                duplicates[field].Add(val);
                        }
                    }
                }

                return (true, "Existing borrow request found with matching user, inventory, and status.", duplicates);
            }
            catch (Exception ex)
            {
                return (false, $"Exception: {ex.Message}", new());
            }
        }
        public async Task<(bool success, string message)> ApproveBorrowRequestAsync(long? borrowRequestId, Admin admin, DateTime dueDate)
        {
            // Step 1: Validate inputs individually
            if (borrowRequestId == null || borrowRequestId <= 0)
                return (false, "Invalid borrow request ID.");

            if (admin == null)
                return (false, "Admin cannot be null.");

            if (admin.Id <= 0)
                return (false, "Invalid admin ID.");

            if (dueDate <= DateTime.UtcNow)
                return (false, "Due date must be in the future.");

            // Step 2: Fetch the borrow request
            var result = await ReadAsync("BorrowedStatus", $"id=eq.{borrowRequestId}&select=*");

            if (!result.success)
                return (false, $"Failed to fetch borrow request: {result.message}");

            var requests = JsonSerializer.Deserialize<List<BorrowedStatus>>(result.message);
            if (requests == null || requests.Count == 0)
                return (false, "Borrow request not found.");

            var request = requests[0];

            if (request.Status != BorrowStatus.Pending.ToString())
                return (false, "Only pending requests can be approved.");

            // Step 2.5: Fetch inventory item status
            var inventoryResult = await ReadAsync("Inventory", $"id=eq.{request.InventoryId}&select=status");

            if (!inventoryResult.success)
                return (false, $"Failed to fetch inventory item status: {inventoryResult.message}");

            var inventoryItems = JsonSerializer.Deserialize<List<InventoryItem>>(inventoryResult.message);
            if (inventoryItems == null || inventoryItems.Count == 0)
                return (false, "Inventory item not found.");

            var inventoryItem = inventoryItems[0];

            // Step 2.6: Check if inventory is already borrowed or unavailable
            if (inventoryItem.Status == "Borrowed" || inventoryItem.Status == "Lost" || inventoryItem.Status == "Damaged" || inventoryItem.Status == "Unavailable")
            {
                return (false, $"Cannot approve borrow request: Inventory item is already {inventoryItem.Status.ToLower()}.");
            }

            // Step 3: Prepare update payload for borrow request approval
            var updatePayload = new
            {
                status = BorrowStatus.Borrowed.ToString(),
                approved_by = admin.Id,
                due_date = dueDate,
                updated_at = DateTime.UtcNow
            };

            // Step 4: Perform update on borrow request
            var (success, message) = await UpdateAsync("BorrowedStatus", $"id=eq.{borrowRequestId}", updatePayload);

            if (!success)
                return (false, $"Failed to approve borrow request: {message}");

            // Step 5: Update inventory item status to "Borrowed"
            var inventoryUpdatePayload = new
            {
                status = BorrowStatus.Borrowed.ToString(),
                updated_at = DateTime.UtcNow
            };

            var (invSuccess, invMessage) = await UpdateAsync("Inventory", $"id=eq.{request.InventoryId}", inventoryUpdatePayload);

            if (!invSuccess)
                return (false, $"Failed to update inventory item status: {invMessage}");

            // Step 6: Optional audit log
            var itemSummary = $"- Borrow Request ID #{borrowRequestId} approved by Admin: {admin.FullName} (ID: {admin.Id}), Due: {dueDate:yyyy-MM-dd}";

            var (reportSuccess, reportMessage) = await CreateReportAsync(
                reportType: "borrow_approval",
                title: "Borrow Request Approved",
                content: itemSummary,
                format: "text",
                adminId: admin.Id
            );

            if (!reportSuccess)
                return (true, $"Request approved, but audit log failed: {reportMessage}");

            return (true, "Borrow request approved successfully.");
        }
        public async Task<(bool success, string message)> RejectBorrowRequestAsync(long? borrowRequestId, Admin admin)
        {
            // Validate inputs individually
            if (borrowRequestId == null || borrowRequestId <= 0)
                return (false, "Invalid borrow request ID.");

            if (admin == null)
                return (false, "Admin cannot be null.");

            if (admin.Id <= 0)
                return (false, "Invalid admin ID.");

            // Fetch the borrow request
            var result = await ReadAsync("BorrowedStatus", $"id=eq.{borrowRequestId}&select=*");

            if (!result.success)
                return (false, $"Failed to fetch borrow request: {result.message}");

            var requests = JsonSerializer.Deserialize<List<BorrowedStatus>>(result.message);
            if (requests == null || requests.Count == 0)
                return (false, "Borrow request not found.");

            var request = requests[0];

            if (request.Status != BorrowStatus.Pending.ToString())
                return (false, "Only pending requests can be rejected.");

            // Prepare update payload for rejection (no rejected_date column)
            var updatePayload = new
            {
                status = BorrowStatus.Rejected.ToString(),
                approved_by = (long?)null, // clear approved_by
                updated_at = DateTime.UtcNow
            };

            // Update borrow request status to Rejected
            var (success, message) = await UpdateAsync("BorrowedStatus", $"id=eq.{borrowRequestId}", updatePayload);

            if (!success)
                return (false, $"Failed to reject borrow request: {message}");

            // Optional audit log
            var itemSummary = $"- Borrow Request ID #{borrowRequestId} rejected by Admin: {admin.FullName} (ID: {admin.Id})";

            var (reportSuccess, reportMessage) = await CreateReportAsync(
                reportType: "borrow_rejection",
                title: "Borrow Request Rejected",
                content: itemSummary,
                format: "text",
                adminId: admin.Id
            );

            if (!reportSuccess)
                return (true, $"Request rejected, but audit log failed: {reportMessage}");

            return (true, "Borrow request rejected successfully.");
        }
        public async Task<(bool success, string message)> CheckOverdueAsync()
        {
            try
            {
                string currentDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
                string filter = $"due_date=lt.{currentDate}&status=eq.Borrowed";


                var payload = new { status = "Overdue" };

                var (success, message) = await UpdateAsync("BorrowedStatus", filter, payload);

                if (success)
                {
                    return (true, "Successfully updated overdue borrow records.");
                }
                else
                {
                    return (false, $"Failed to update overdue records: {message}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Exception during overdue check: {ex.Message}");
            }
        }
        public async Task<(bool success, InventoryItem? item, string message)> GetInventoryItemByIdAsync(long inventoryId)
        {
            try
            {
                var readResult = await ReadAsync("Inventory");
                if (!readResult.success)
                    return (false, null, readResult.message);

                var items = JsonSerializer.Deserialize<List<InventoryItem>>(readResult.message);
                if (items == null)
                    return (false, null, "No inventory data found.");

                var matchedItem = items.FirstOrDefault(item => item.Id == inventoryId);
                if (matchedItem == null)
                    return (false, null, $"Item with ID {inventoryId} not found.");

                return (true, matchedItem, "Item found successfully.");
            }
            catch (Exception ex)
            {
                return (false, null, $"Exception: {ex.Message}");
            }
        }
        public async Task<(bool success, string message)> UpdateBorrowHistoryAsync(BorrowedStatus borrowedStatus, Admin admin)
        {
            try
            {
                if (borrowedStatus == null)
                    return (false, "Borrowed status cannot be null.");
                if (admin == null || admin.Id <= 0)
                    return (false, "Invalid admin.");

                // --- Update BorrowedStatus record ---
                string filter = $"id=eq.{borrowedStatus.Id}";
                borrowedStatus.UpdatedAt = DateTime.UtcNow;

                var updateBorrowResult = await UpdateAsync("BorrowedStatus", filter, borrowedStatus);
                if (!updateBorrowResult.success)
                    return (false, $"Failed to update borrow history: {updateBorrowResult.message}");

                // Create audit log for borrow status update
                string borrowAuditContent = $"- BorrowedStatus ID #{borrowedStatus.Id} updated by Admin: {admin.FullName} (ID: {admin.Id})\n" +
                                            $"  Status: '{borrowedStatus.Status}'\n" +
                                            $"  ReturnedDate: '{borrowedStatus.ReturnedDate?.ToString("u") ?? "null"}'\n" +
                                            $"  UpdatedAt: '{borrowedStatus.UpdatedAt:u}'";

                var (borrowAuditSuccess, borrowAuditMessage) = await CreateReportAsync(
                    reportType: "borrow_status_update",
                    title: "Borrow Status Updated",
                    content: borrowAuditContent,
                    format: "text",
                    adminId: admin.Id
                );

                if (!borrowAuditSuccess)
                    return (true, $"Borrow history updated, but audit log failed: {borrowAuditMessage}");

                // --- Fetch related inventory item ---
                var fetchResult = await ReadAsync("Inventory", $"id=eq.{borrowedStatus.InventoryId}");
                if (!fetchResult.success)
                    return (false, $"Failed to fetch inventory item: {fetchResult.message}");

                var items = JsonSerializer.Deserialize<List<InventoryItem>>(fetchResult.message);
                if (items == null || items.Count == 0)
                    return (false, "Inventory item not found.");

                var inventoryItem = items[0];

                // Update inventory status based on borrowed status
                switch (borrowedStatus.StatusEnum)
                {
                    case BorrowStatus.Returned:
                    case BorrowStatus.Resolved:
                    case BorrowStatus.ReturnedLate:
                        inventoryItem.StatusEnum = InventoryItemStatus.Available;
                        break;

                    case BorrowStatus.Borrowed:
                        inventoryItem.StatusEnum = InventoryItemStatus.Borrowed;
                        break;

                    case BorrowStatus.Lost:
                        inventoryItem.StatusEnum = InventoryItemStatus.Lost;
                        break;

                    case BorrowStatus.Damaged:
                        inventoryItem.StatusEnum = InventoryItemStatus.Damaged;
                        break;

                    case BorrowStatus.Overdue:
                        inventoryItem.StatusEnum = InventoryItemStatus.Borrowed;
                        break;

                    default:
                        // Leave inventory status as is for other statuses
                        break;
                }

                inventoryItem.UpdatedAt = DateTime.UtcNow;

                // --- Update inventory item with audit logging handled inside UpdateInventoryItemData ---
                var (inventoryUpdateSuccess, inventoryUpdateMsg) = await UpdateInventoryItemData(inventoryItem, admin);
                if (!inventoryUpdateSuccess)
                    return (false, $"Borrow updated but failed to update inventory: {inventoryUpdateMsg}");

                return (true, "Borrow history and inventory item updated successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"Exception: {ex.Message}");
            }
        }
        public async Task<(bool success, string message)> CreateBorrowReportAsync(BorrowRecordDetail record, string reportType = ReportTypes.ReturnLate, string format = "pdf")
        {
            string title = $"Borrow_Report_{record.BorrowStatus.Id}";
            long adminId = record.Admin?.Id ?? 0;

            string content = format.ToLower() switch
            {
                "pdf" => GeneratePdfBase64(record, reportType),
                "text" => GenerateReportText(record, reportType),
                _ => throw new NotSupportedException($"Format '{format}' is not supported")
            };

            return await CreateReportAsync(reportType, title, content, format, adminId);
        }
        private string GenerateReportText(BorrowRecordDetail record, string reportType)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Borrow Report");
            sb.AppendLine($"Borrow ID: {record.BorrowStatus.Id}");
            sb.AppendLine($"Borrower: {record.BorrowStatus.BorrowerName}");
            sb.AppendLine($"Status: {record.BorrowStatus.Status}");
            sb.AppendLine($"Borrowed Date: {record.BorrowStatus.BorrowedDate:yyyy-MM-dd}");
            sb.AppendLine($"Due Date: {record.BorrowStatus.DueDate:yyyy-MM-dd}");
            sb.AppendLine($"Returned Date: {(record.BorrowStatus.ReturnedDate.HasValue ? record.BorrowStatus.ReturnedDate.Value.ToString("yyyy-MM-dd") : "Not Returned")}");

            if (record.Inventory != null)
            {
                sb.AppendLine("Inventory Info:");
                sb.AppendLine($"- Name: {record.Inventory.Name}");
                sb.AppendLine($"- Serial Number: {record.Inventory.SerialNumber}");
                sb.AppendLine($"- Category: {record.Inventory.Category}");
                sb.AppendLine($"- Status: {record.Inventory.Status}");
            }

            if (record.Admin != null)
            {
                sb.AppendLine("Handled by Admin:");
                sb.AppendLine($"- Name: {record.Admin.FullName}");
                sb.AppendLine($"- Email: {record.Admin.Email}");
            }

            DateTime now = DateTime.Now.Date;
            DateTime? returnedDate = record.BorrowStatus.ReturnedDate?.Date;
            DateTime? dueDate = record.BorrowStatus.DueDate?.Date;

            if (reportType == ReportTypes.ReturnLate && dueDate.HasValue)
            {
                int daysLate = 0;

                if (returnedDate.HasValue)
                {
                    if (returnedDate > dueDate)
                        daysLate = (returnedDate.Value - dueDate.Value).Days;
                }
                else
                {
                    if (now > dueDate)
                        daysLate = (now - dueDate.Value).Days;
                }

                if (daysLate > 0)
                {
                    decimal fee = daysLate * 2;
                    sb.AppendLine($"\nLate Fee: {daysLate} day(s) × ₱2 = ₱{fee}");
                }
            }
            else if ((reportType == ReportTypes.Lost || reportType == ReportTypes.Damaged) && record.Inventory != null)
            {
                decimal value = record.Inventory.Value;
                sb.AppendLine($"\nPayment Due: ₱{value} for {reportType.ToLower()} item.");
            }

            return sb.ToString();
        }
        private string GeneratePdfBase64(BorrowRecordDetail record, string reportType)
        {
            var doc = new Document();
            var section = doc.AddSection();

            // Set page margins
            section.PageSetup.TopMargin = "2cm";
            section.PageSetup.BottomMargin = "2cm";
            section.PageSetup.LeftMargin = "2.5cm";
            section.PageSetup.RightMargin = "2.5cm";

            // Define styles for reuse
            var style = doc.Styles["Normal"];
            style.Font.Name = "Segoe UI";
            style.Font.Size = 11;

            var headingStyle = doc.Styles.AddStyle("Heading1", "Normal");
            headingStyle.Font.Size = 22;
            headingStyle.Font.Bold = true;
            headingStyle.ParagraphFormat.Alignment = ParagraphAlignment.Center;
            headingStyle.ParagraphFormat.SpaceAfter = "0.5cm";

            var subHeadingStyle = doc.Styles.AddStyle("SubHeading", "Normal");
            subHeadingStyle.Font.Size = 12;
            subHeadingStyle.ParagraphFormat.Alignment = ParagraphAlignment.Center;
            subHeadingStyle.ParagraphFormat.SpaceAfter = "1cm";
            subHeadingStyle.Font.Color = Colors.DarkSlateGray;

            var sectionTitleStyle = doc.Styles.AddStyle("SectionTitle", "Normal");
            sectionTitleStyle.Font.Size = 14;
            sectionTitleStyle.Font.Bold = true;
            sectionTitleStyle.Font.Color = Colors.DarkBlue;
            sectionTitleStyle.ParagraphFormat.SpaceBefore = "0.8cm";
            sectionTitleStyle.ParagraphFormat.SpaceAfter = "0.3cm";

            // Header - Title and subtitle
            var heading = section.AddParagraph("OFFICIAL BORROW RECEIPT");
            heading.Style = "Heading1";

            var subHeading = section.AddParagraph("Electronic Library Management System");
            subHeading.Style = "SubHeading";

            // Add a line below header
            var line = section.AddParagraph();
            line.Format.Borders.Bottom.Width = 1.5;
            line.Format.Borders.Bottom.Color = Colors.DarkSlateGray;
            line.Format.SpaceAfter = "0.8cm";

            // Helper to add label-value in table cell
            void AddCell(Row row, int index, string text, bool isHeader = false)
            {
                var cell = row.Cells[index];
                cell.AddParagraph(text);
                if (isHeader)
                {
                    cell.Format.Font.Bold = true;
                    cell.Shading.Color = Colors.LightSteelBlue;
                }
                cell.Format.Alignment = ParagraphAlignment.Left;
                cell.VerticalAlignment = VerticalAlignment.Center;
                cell.Format.SpaceAfter = "0.1cm";
                cell.Format.SpaceBefore = "0.1cm";
            }

            // Borrow Details Table
            var borrowTable = section.AddTable();
            borrowTable.Borders.Width = 0.75;
            borrowTable.Borders.Color = Colors.Gray;
            borrowTable.Rows.LeftIndent = 0;

            borrowTable.AddColumn("4cm");
            borrowTable.AddColumn("10cm");

            var headerRow = borrowTable.AddRow();
            AddCell(headerRow, 0, "Borrow Details", true);
            AddCell(headerRow, 1, "", true);

            var row1 = borrowTable.AddRow();
            AddCell(row1, 0, "Borrow ID:");
            AddCell(row1, 1, record.BorrowStatus.Id.ToString());

            var row2 = borrowTable.AddRow();
            AddCell(row2, 0, "Borrower:");
            AddCell(row2, 1, record.BorrowStatus.BorrowerName);

            var row3 = borrowTable.AddRow();
            AddCell(row3, 0, "Status:");
            AddCell(row3, 1, record.BorrowStatus.Status.ToString());

            var row4 = borrowTable.AddRow();
            AddCell(row4, 0, "Borrowed Date:");
            AddCell(row4, 1, record.BorrowStatus.BorrowedDate.ToString("yyyy-MM-dd"));

            var row5 = borrowTable.AddRow();
            AddCell(row5, 0, "Due Date:");
            AddCell(row5, 1, record.BorrowStatus.DueDate?.ToString("yyyy-MM-dd") ?? "N/A");

            var row6 = borrowTable.AddRow();
            AddCell(row6, 0, "Returned Date:");
            AddCell(row6, 1, record.BorrowStatus.ReturnedDate?.ToString("yyyy-MM-dd") ?? "Not Returned");

            // Inventory Information Section
            if (record.Inventory != null)
            {
                var invTitle = section.AddParagraph("Inventory Information");
                invTitle.Style = "SectionTitle";

                var invTable = section.AddTable();
                invTable.Borders.Width = 0.75;
                invTable.Borders.Color = Colors.Gray;
                invTable.AddColumn("4cm");
                invTable.AddColumn("10cm");

                var invHeader = invTable.AddRow();
                AddCell(invHeader, 0, "Field", true);
                AddCell(invHeader, 1, "Details", true);

                var invRows = new[]
                {
                    ("Name:", record.Inventory.Name),
                    ("Serial Number:", record.Inventory.SerialNumber),
                    ("Category:", record.Inventory.Category),
                    ("Status:", record.Inventory.Status)
                };

                foreach (var (label, value) in invRows)
                {
                    var r = invTable.AddRow();
                    AddCell(r, 0, label);
                    AddCell(r, 1, value);
                }
            }

            // Admin Information Section
            if (record.Admin != null)
            {
                var adminTitle = section.AddParagraph("Handled By");
                adminTitle.Style = "SectionTitle";

                var adminTable = section.AddTable();
                adminTable.Borders.Width = 0.75;
                adminTable.Borders.Color = Colors.Gray;
                adminTable.AddColumn("4cm");
                adminTable.AddColumn("10cm");

                var adminHeader = adminTable.AddRow();
                AddCell(adminHeader, 0, "Field", true);
                AddCell(adminHeader, 1, "Details", true);

                var adminRows = new[]
                {
            ("Name:", record.Admin.FullName),
            ("Email:", record.Admin.Email)
        };

                foreach (var (label, value) in adminRows)
                {
                    var r = adminTable.AddRow();
                    AddCell(r, 0, label);
                    AddCell(r, 1, value);
                }
            }

            // Payment Summary Box with background shading
            section.AddParagraph().AddLineBreak();

            // Payment Summary Table to simulate a box with shading and border
            var paymentTable = section.AddTable();
            paymentTable.Borders.Width = 1;
            paymentTable.Borders.Color = Colors.Goldenrod;
            paymentTable.Shading.Color = Colors.LightYellow;
            paymentTable.Format.SpaceBefore = "0.5cm";
            paymentTable.Format.SpaceAfter = "0.5cm";

            paymentTable.AddColumn("12cm");

            var paymentRow = paymentTable.AddRow();
            var paymentCell = paymentRow.Cells[0];

            // Add title paragraph inside cell
            var payTitle = paymentCell.AddParagraph("Payment Summary");
            payTitle.Format.Font.Size = 14;
            payTitle.Format.Font.Bold = true;
            payTitle.Format.SpaceAfter = "0.3cm";
            payTitle.Format.Alignment = ParagraphAlignment.Center;

            bool paymentRequired = false;

            DateTime now = DateTime.Now.Date;
            DateTime? returnedDate = record.BorrowStatus.ReturnedDate?.Date;
            DateTime? dueDate = record.BorrowStatus.DueDate?.Date;

            if (reportType == ReportTypes.ReturnLate && dueDate.HasValue)
            {
                int daysLate = 0;

                if (returnedDate.HasValue)
                {
                    if (returnedDate > dueDate)
                        daysLate = (returnedDate.Value - dueDate.Value).Days;
                }
                else
                {
                    if (now > dueDate)
                        daysLate = (now - dueDate.Value).Days;
                }

                if (daysLate > 0)
                {
                    decimal fee = daysLate * 2;
                    paymentRequired = true;

                    var p = paymentCell.AddParagraph();
                    p.Format.Font.Bold = true;
                    p.AddText($"Late Fee: {daysLate} day(s) × ₱2 = ₱{fee:F2}");
                    p.Format.Alignment = ParagraphAlignment.Center;
                }
            }
            else if ((reportType == ReportTypes.Lost || reportType == ReportTypes.Damaged) && record.Inventory != null)
            {
                decimal value = record.Inventory.Value;
                paymentRequired = true;

                var p = paymentCell.AddParagraph();
                p.Format.Font.Bold = true;
                p.AddText($"Item Fee: ₱{value:F2} for {reportType.ToLower()} item");
                p.Format.Alignment = ParagraphAlignment.Center;
            }

            if (!paymentRequired)
            {
                var p = paymentCell.AddParagraph();
                p.Format.Font.Italic = true;
                p.AddText("No payment is required for this report.");
                p.Format.Alignment = ParagraphAlignment.Center;
            }

            // Footer with line and generated timestamp
            var footerLine = section.AddParagraph();
            footerLine.Format.Borders.Top.Width = 1;
            footerLine.Format.Borders.Top.Color = Colors.Gray;
            footerLine.Format.SpaceBefore = "1cm";

            var footer = section.AddParagraph($"Generated On: {DateTime.Now:MMMM dd, yyyy hh:mm tt}");
            footer.Format.Font.Size = 9;
            footer.Format.Alignment = ParagraphAlignment.Right;
            footer.Format.SpaceBefore = "0.2cm";

            // Render PDF to base64
            var renderer = new PdfDocumentRenderer { Document = doc };
            renderer.RenderDocument();

            using var stream = new MemoryStream();
            renderer.PdfDocument.Save(stream, false);
            return Convert.ToBase64String(stream.ToArray());
        }
    }
}