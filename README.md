# 🚀 TaskManager - Modern ASP.NET Core 8 MVC Productivity Platform

![.NET 8](https://img.shields.io/badge/.NET-8.0-512bd4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-12.0-239120?style=for-the-badge&logo=csharp&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-MVC-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Entity Framework Core](https://img.shields.io/badge/EF%20Core-8.0-purple?style=for-the-badge)
![SQL Server](https://img.shields.io/badge/Database-SQL%20Server-CC292B?style=for-the-badge&logo=microsoftsqlserver&logoColor=white)
![Bootstrap 5](https://img.shields.io/badge/Bootstrap-5.3-7952B3?style=for-the-badge&logo=bootstrap&logoColor=white)

A feature-rich, high-performance web application designed for personal and team productivity. Built with **ASP.NET Core 8 MVC**, **Entity Framework Core**, and **ASP.NET Core Identity**, styled with an ultra-modern **Glassmorphism Design System**, and powered by automated **Gmail SMTP** email workflows.

---

## ✨ Key Features

### 1. 📋 Task Workflow & Interactive Kanban Board
- **Three Workflow States**: `To Do`, `In Progress`, and `Completed`.
- **Drag-and-Drop Kanban Board**: Real-time HTML5 drag-and-drop allows you to move cards between columns and automatically sync their status via AJAX without page reloads.
- **1-Click Completion**: Checkbox on task cards and tables to immediately toggle completion status and record the completion timestamp.
- **Multiple Views**: Seamlessly switch between **Kanban Board**, **Grid Cards**, and **Compact Table** views.

### 2. ⚡ Priority Levels & Glowing Badges
- **Four Tiers**: `Urgent` (rose flame badge with subtle pulse), `High` (amber warning), `Medium` (indigo badge), and `Low` (slate badge).
- **Fast Filtering**: Dedicated priority dropdown to instantly filter and address high-urgency items.

### 3. ⏰ Due Dates & Overdue Detection
- **Smart Deadline Tracking**:
  - **Overdue**: High-contrast rose highlights with alert indicators.
  - **Due Today**: Distinct amber warning styling.
  - **Upcoming**: Clean calendar date display.
- **Header Overdue Alert**: An eye-catching badge in the top header indicates overdue tasks at a glance.

### 4. 🏷️ Categories & Tagging System
- Organize tasks into **6 predefined categories**: `General`, `Work`, `Personal`, `Study`, `Finance`, and `Health`.
- Quick-filter category pills in the dashboard header for 1-click filtering.

### 5. 📬 Automated Gmail SMTP Email System
- Integrated with Gmail SMTP (`smtp.gmail.com:587`, TLS) via `IEmailSender`.
- **Forgot Password & Account Recovery**: Secure password reset flow with branded, responsive HTML emails.
- **One-Click Email Digest**: Dispatches a comprehensive summary of pending, due today, and overdue tasks directly to your inbox.

### 6. ✅ Step-by-Step Subtask Checklist Builder
- Break complex tasks into bite-sized actionable checklist items.
- Inline creation, completion toggle, and deletion powered by asynchronous AJAX endpoints.
- Dynamic visual progress bar that updates completion percentage in real time.

### 7. 📊 Productivity Analytics & Insights Dashboard
- Real-time productivity metrics:
  - Total Tasks Counter
  - Completed Tasks with % Completion Rate
  - Tasks In Progress
  - Overdue Tasks Alert
- Multi-parameter live search filtering by title, description, category, priority, and status simultaneously.

### 8. 📤 Data Portability & Export
- **CSV Export**: Single-click export (`/UserTasks/ExportCsv`) yielding Excel-friendly files with full task metadata and checklist counts.
- **Print-Ready / PDF Report**: Clean, printer-optimized layout (`/UserTasks/PrintReport`) with automated browser print invocation.

### 9. 👤 User Profiles & Personalization
- Dedicated user profile management (`/Profile`).
- Custom **Display Name** and personalized **Avatar Color Theme** (6 modern gradient palettes).
- Dynamic navbar badge showing custom avatar initials and user name.
- Built-in secure password change interface.

### 10. 🎨 Modern Glassmorphism UI
- Handcrafted with modern Vanilla CSS, frosted glass cards, glowing borders, and smooth micro-animations.
- Premium typography using **Plus Jakarta Sans**.
- Fully responsive across desktop, tablet, and mobile screens with dark and light theme support.

---

## 🛠️ Technology Stack

- **Backend**: C# 12, ASP.NET Core 8.0 MVC
- **Authentication**: ASP.NET Core Identity with Entity Framework Core
- **Database & ORM**: Microsoft SQL Server / LocalDB, Entity Framework Core 8 (Code-First Migrations)
- **Mailing**: `System.Net.Mail` via `Microsoft.AspNetCore.Identity.UI.Services.IEmailSender`
- **Frontend**: Razor Pages & Views, Bootstrap 5.3, Bootstrap Icons, Vanilla JavaScript (Fetch API / Drag-and-Drop)
- **Styling**: Modern Glassmorphism CSS with CSS Custom Properties and responsive design

---

## 📂 Project Structure

```text
TaskManager/
├── Areas/
│   └── Identity/              # Customized Identity pages (Login, Register, Forgot Password)
├── Controllers/
│   ├── HomeController.cs       # Landing page controller
│   ├── ProfileController.cs    # User profile & account customization
│   └── UserTaskController.cs   # Core task CRUD, Kanban, subtasks, export, email digest
├── Data/
│   ├── ApplicationDbContext.cs # EF Core DbContext with UserTasks & SubTasks
│   └── Migrations/             # Database migration snapshots
├── Models/
│   ├── ApplicationUser.cs      # Identity user with DisplayName & AvatarColor
│   ├── EmailSettings.cs        # Strongly typed SMTP settings model
│   ├── SubTask.cs              # Subtask entity model
│   └── UserTask.cs             # Core task model with enums (Status, Priority, Category)
├── Services/
│   └── EmailSender.cs          # SMTP email delivery service implementation
├── Views/
│   ├── Profile/                # Profile editing & password management
│   ├── Shared/                 # Layout, navigation, and partials
│   └── UserTasks/              # Dashboard (Index), Create, Edit, Details, PrintReport
├── wwwroot/
│   ├── css/site.css            # Glassmorphism design system & micro-animations
│   └── js/site.js             # Client-side helpers
└── appsettings.json            # Configuration template
```

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or higher
- [SQL Server](https://www.microsoft.com/sql-server) (Express, Developer, or LocalDB)
- Optional: Gmail account with an [App Password](https://myaccount.google.com/apppasswords) for SMTP email delivery

### Installation & Setup

1. **Clone the Repository**
   ```bash
   git clone https://github.com/Abivarsan/TaskManager.git
   cd TaskManager/TaskManager
   ```

2. **Configure Settings**
   Open `appsettings.json` (or create `appsettings.Development.json`) and configure your database connection string and email settings:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=TaskManagerDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;"
     },
     "EmailSettings": {
       "SmtpServer": "smtp.gmail.com",
       "SmtpPort": 587,
       "SenderEmail": "your-email@gmail.com",
       "SenderName": "TaskManager",
       "SenderPassword": "your-gmail-app-password",
       "EnableSsl": true
     }
   }
   ```

3. **Apply Database Migrations**
   ```bash
   dotnet ef database update
   ```

4. **Run the Application**
   ```bash
   dotnet run
   ```

5. **Open in Browser**
   - HTTPS: `https://localhost:7290`
   - HTTP: `http://localhost:5028`

---

## 🔒 Security Best Practices

- **Secrets Management**: Do not commit real passwords or sensitive credentials to source control. Use `appsettings.Development.json` (ignored in `.gitignore`) or `dotnet user-secrets` for local development.
- **Data Isolation**: All tasks and subtasks are strictly isolated and queried by the authenticated user's ID (`UserId`), preventing unauthorized access across accounts.
- **Anti-Forgery Tokens**: All mutating POST/AJAX endpoints require valid ASP.NET Core Anti-Forgery Tokens.

---

## 📄 License

This project is licensed under the [MIT License](LICENSE).
