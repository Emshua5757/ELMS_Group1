# University Library Management System (ELMS_Group1)

A modern, user-friendly WPF application for managing university library inventory, users, and borrowing activities.

## Features

- **Admin Dashboard:**  
  Centralized interface for managing inventory, users, applications, borrow requests, and reports.

- **Inventory Management:**  
  Add, edit, and remove inventory items.  
  Search and filter inventory by title, author, and other fields.

- **User Management:**  
  Register, edit, and remove users.  
  Search users by name or ID.

- **Application Processing:**  
  Review, approve, or reject pending user applications with feedback.

- **Borrowing System:**  
  Manage borrow requests, set due dates, and track borrowed items.

- **Reports & Analytics:**  
  View library usage statistics and generate reports.

- **Authentication:**  
  Secure admin login and registration.

## Project Structure

- `window/`  
  Contains all WPF windows (UI) such as `AdminDashboard`, `EditInventoryWindow`, `UserRegister`, etc.

- `model/`  
  Data models for entities like `Admin`, `User`, `InventoryItem`, `PendingUser`, `BorrowedStatus`.

- `database/`  
  Data access and service classes, e.g., `SupabaseService` for backend communication.

- `App.xaml`  
  Application entry point and global resources.

## Technologies

- **.NET 8**
- **C# 12**
- **WPF (Windows Presentation Foundation)**
- **Supabase/PostgREST** (for backend data storage and retrieval)

## Getting Started

1. **Clone the repository**
2. **Open the solution in Visual Studio 2022 or later**
3. **Restore NuGet packages** (if any)
4. **Build and run the solution**

## Usage

- Launch the application.
- Log in as an admin or register a new admin account.
- Use the navigation buttons to access different management sections.
- Perform CRUD operations on inventory and users, process applications, and manage borrow requests.

## Customization

- Update Supabase credentials in `database/SupabaseService.cs` as needed.
- Modify UI in the `window/` XAML files to match your institution’s branding.

## Contributing

Pull requests and suggestions are welcome! Please open an issue first to discuss changes.

## License

This project is for educational purposes. See [LICENSE](LICENSE) for more information.

---
