using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Security.Cryptography;
using ELMS_Group1.model;

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
                // Search relevant fields with ilike, you can add more fields if needed
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
        public async Task<(bool success, string message)> ApproveAndAddUserAsync(long pendingUserId, string adminReviewer)
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
                    reviewed_by = adminReviewer
                };

                var updatePendingResult = await UpdateAsync("PendingUser", filter, updatePayload);

                return updatePendingResult.success
                    ? (true, "User approved and added to User table.")
                    : (false, $"User added but failed to update pending status: {updatePendingResult.message}");
            }
            catch (Exception ex)
            {
                return (false, $"Exception: {ex.Message}");
            }
        }
        public async Task<(bool success, string message)> RejectPendingUserAsync(long pendingUserId, string feedbackMessage, string adminReviewer)
        {
            try
            {
                var payload = new
                {
                    is_approved = false,
                    admin_feedback = feedbackMessage,
                    reviewed_at = DateTime.UtcNow,
                    reviewed_by = adminReviewer
                };

                string filter = $"id=eq.{pendingUserId}";
                return await UpdateAsync("PendingUser", filter, payload);
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
        public async Task<(bool success, string message)> DeleteUserAsync(long userId)
        {
            try
            {
                string filter = $"id=eq.{userId}";
                var deleteResult = await DeleteAsync("User", filter);
                if (deleteResult.success)
                    return (true, "User deleted successfully.");
                else
                    return (false, $"Failed to delete user: {deleteResult.message}");
            }
            catch (Exception ex)
            {
                return (false, $"Exception: {ex.Message}");
            }
        }
        public async Task<(bool success, string message)> UpdateUserData(User user)
        {
            try
            {
                if (user == null)
                    return (false, "User cannot be null.");

                string filter = $"id=eq.{user.Id}";

                var updateResult = await UpdateAsync("User", filter, user);

                if (updateResult.success)
                    return (true, "User updated successfully.");
                else
                    return (false, $"Failed to update user: {updateResult.message}");
            }
            catch (Exception ex)
            {
                return (false, $"Exception: {ex.Message}");
            }
        }

    }
}
