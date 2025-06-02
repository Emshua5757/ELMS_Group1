# Electronic Library Management System (ELMS)

## Project Overview
The **Electronic Library Management System (ELMS)** is a Windows desktop application designed to streamline library operations in academic and institutional settings. It manages multiple inventory categories including books, electronics, and other resources.

Key features:
- Borrowing and return process with physical return confirmations by admins
- Real-time monitoring of item availability and status
- PDF receipt generation for transactions
- Penalty calculation for overdue, lost, or damaged items
- Detailed audit trails of admin activities for accountability
- Secure, role-based access with admin-only management capabilities

---

## Features

### Admin Features
- Inventory management (single, multiple, and CSV uploads)
- User management (pending and approved users)
- Application approval workflow
- Borrow request processing with due date assignment
- Overdue and problematic item tracking
- Borrow history filtering and statistics
- Reports and audit logs with export options
- Comprehensive admin dashboard

### User Features
- View and filter library inventory
- Track current and past borrowed items
- View penalties and amounts owed
- Manage personal account information
- Access quick stats dashboard
- About Us dialog with team info

---

## Technology Stack

- **Frontend:** WPF (Windows Presentation Foundation) using C#
- **Backend:** Supabase cloud service with PostgreSQL database and REST API
- **Other tools:** Visual Studio for development, PdfSharp/MigraDoc for PDF generation, GitHub for version control

---

## System Architecture
ELMS uses the MVVM (Model-View-ViewModel) design pattern to ensure maintainable and testable code by separating UI, business logic, and data models.

- **Models:** Represent core entities like Admin, User, InventoryItem, BorrowedStatus, and Reports.
- **Views:** WPF UI components handling user interactions.
- **ViewModels:** Bridge Views and Models; manage commands and data binding.
- **Database Service:** Interfaces with Supabase backend via REST API calls.

Data flows from UI through ViewModels to Models and backend services, ensuring smooth synchronization and real-time updates.

---

## Folder Structure Overview

- **database:** Contains `SupabaseService.cs` handling backend communication  
- **model:** Contains data classes: `Admin`, `BorrowedStatus`, `BorrowRecordDetail`, `InventoryItem`, `PendingUser`, `Report`, `StatsModel`, `User`  
- **window:** UI components including dialogs, main screens, custom controls, and user/admin dashboards such as:
  - `AboutUs`
  - `AddInventoryOptionDialog`
  - `AddMultipleInventoryWindow`
  - `AddSingleInventoryWindow`
  - `AdminCard` (custom control for admin display)
  - `AdminDashboard`
  - `AdminLogin`
  - `AdminRegister`
  - `EditInventoryWindow`
  - `EditUserWindow`
  - `InputDialog`
  - `OpeningScreen` (main entry point)
  - `ProblematicBorrowControl` (handles expandable item details with action buttons)
  - `ReportCard` (report preview with download button)
  - `ReportPreviewWindow`
  - `ReturnStatusDialog`
  - `UserDashboard`
  - `UserLogin`
  - `UserRegister`

---

## Development Team and Contributions

- **Dia Monique Baylon** — UI/UX and frontend design  
  *“If you keep going, anything is possible.”*

- **Clark Kent Georpe** — UI/UX and frontend design  
  *“To Infinity and Beyond.”*

- **Shaine Michael Carreon** — Testing and Quality Assurance  
  *“Commitment is a big part of what I am and what I believe.”*

- **Glegel Novo** — Testing and Quality Assurance  
  *“To understand the future, one must master the present.”*

- **Joshua Ygot** — Backend development, database communication, models, and majority of backend logic  
  *“Learn to respect to be respected.”*

---

Thank you for reviewing our project!
