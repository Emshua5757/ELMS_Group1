using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Security.Cryptography;
using ELMS_Group1.model;
using System.Globalization;

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
            try
            {
                string encodedText = Uri.EscapeDataString($"%{searchText}%");
                string filter = $"or=(name.ilike.{encodedText},category.ilike.{encodedText},description.ilike.{encodedText},serial_number.ilike.{encodedText},location.ilike.{encodedText},status.ilike.{encodedText})";

                var readResult = await ReadAsync("Inventory", filter);
                if (!readResult.success)
                    return (false, null, readResult.message);

                var items = JsonSerializer.Deserialize<List<InventoryItem>>(readResult.message);
                return (true, items, "Inventory search completed successfully.");
            }
            catch (Exception ex)
            {
                return (false, null, $"Exception: {ex.Message}");
            }
        }
        public async Task<(bool success, List<BorrowedStatus>? records, string message)> SearchBorrowedStatusAsync(string searchText, long? adminId = null, long? userId = null)
        {
            try
            {
                string encodedText = Uri.EscapeDataString($"%{searchText}%");

                // Base filter: borrower_name or status ilike
                string baseFilter = $"or=(borrower_name.ilike.{encodedText},status.ilike.{encodedText})";

                // Append admin_id filter if provided
                if (adminId.HasValue)
                {
                    baseFilter += $",admin_id.eq.{adminId.Value}";
                }

                // Append user_id filter if provided
                if (userId.HasValue)
                {
                    baseFilter += $",user_id.eq.{userId.Value}";
                }

                // Wrap whole filter with 'and' if multiple conditions
                // PostgREST expects filters combined like: and(condition1,condition2,...)
                var filters = new List<string> { $"or=(borrower_name.ilike.{encodedText},status.ilike.{encodedText})" };
                if (adminId.HasValue) filters.Add($"admin_id.eq.{adminId.Value}");
                if (userId.HasValue) filters.Add($"user_id.eq.{userId.Value}");
                string filter = filters.Count > 1 ? $"and({string.Join(",", filters)})" : filters[0];

                var readResult = await ReadAsync("BorrowedStatus", filter);
                if (!readResult.success)
                    return (false, null, readResult.message);

                var records = JsonSerializer.Deserialize<List<BorrowedStatus>>(readResult.message);
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
        public async Task<(bool success, string message)> DeleteInventoryItemAsync(long? itemId)
        {
            try
            {
                string filter = $"id=eq.{itemId}";
                var deleteResult = await DeleteAsync("Inventory", filter);
                if (deleteResult.success)
                    return (true, "Inventory item deleted successfully.");
                else
                    return (false, $"Failed to delete inventory item: {deleteResult.message}");
            }
            catch (Exception ex)
            {
                return (false, $"Exception: {ex.Message}");
            }
        }
        public async Task<(bool success, string message)> UpdateInventoryItemData(InventoryItem item)
        {
            try
            {
                if (item == null)
                    return (false, "Item cannot be null.");
                string filter = $"id=eq.{item.Id}";
                var updateResult = await UpdateAsync("Inventory", filter, item);
                if (updateResult.success)
                    return (true, "Inventory item updated successfully.");
                else
                    return (false, $"Failed to update inventory item: {updateResult.message}");
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

    }
}
