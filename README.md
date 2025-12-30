# ChronoDesk ⏱️

ChronoDesk is a premium, modern Windows desktop time-tracking application built with **.NET 10** and **WPF**. Designed with a focus on aesthetics and user experience, it combines powerful time-tracking capabilities with a sleek, glassmorphism-inspired dark interface.

![ChronoDesk UI](https://via.placeholder.com/800x450.png?text=ChronoDesk+Interface+Preview) <!-- Replace with actual screenshot later -->

## ✨ Features

*   **Modern Aesthetics**: A high-end dark theme featuring glassmorphism effects, smooth animations (fade/slide), and premium typography.
*   **Intuitive Timer**: Start, pause, and stop timers with a single click. Pulse animations provide visual feedback for active sessions.
*   **Project Management**: Organize your work into distinct projects with easy-to-use CRUD operations.
*   **Intelligent Persistence**: 
    *   **Auto-Save**: Active timer states are automatically saved and recovered on application restart.
    *   **Single Instance**: Prevents multiple copies of ChronoDesk from running simultaneously.
*   **System Integration**: 
    *   **Minimize to Tray**: Keep ChronoDesk running in the background via the system tray icon.
    *   **Modern Taskbar**: High-quality icon integration for Windows Search and Taskbar.
*   **Reporting & Analytics**: View detailed summaries by project for any selected date range.
*   **Data Export**: Export your time entries to CSV for external reporting and invoicing.

## 🏗️ Architecture

ChronoDesk is built using **Clean Architecture** principles, ensuring high maintainability and testability:

*   **ChronoDesk.Domain**: Core business entities (`Project`, `TimeEntry`) and domain logic.
*   **ChronoDesk.Application**: Application-specific business rules, interfaces, and services (`ITimerService`, `IProjectService`).
*   **ChronoDesk.Infrastructure**: Implementation of external concerns like EF Core (`SQLite`), File I/O, and specialized services.
*   **ChronoDesk.UI**: WPF presentation layer following the **MVVM** pattern, leveraging modern Dependency Injection and rich XAML styling.

## 🛠️ Technology Stack

*   **Framework**: .NET 10 (WPF)
*   **Database**: SQLite with Entity Framework Core
*   **DI**: Microsoft.Extensions.DependencyInjection
*   **Icons**: Segoe Fluent Icons / MDL2 Assets
*   **Styling**: Pure XAML with Storyboard Animations

## 🚀 Getting Started

### Prerequisites
*   [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
*   Windows 10/11

### Running the Project
1. Clone the repository:
   ```bash
   git clone https://github.com/onurucak/ChronoDesk.git
   ```
2. Navigate to the project root:
   ```bash
   cd ChronoDesk
   ```
3. Run the application:
   ```bash
   dotnet run --project ChronoDesk.UI
   ```
The database (`chronodesk.db`) will be automatically initialized on the first run.

## 📅 Roadmap (Upcoming)

*   [ ] **Visual Charts**: Graphical data visualization (Pie/Bar charts) using LiveCharts.
*   [ ] **Dark/Light Theme**: Support for system-synced theme switching.
*   [ ] **Idle Detection**: Automatic pause when user activity is not detected.
*   [ ] **Cloud Integration**: Optional sync with personal cloud storage or REST APIs.

## 📄 License

This project is licensed under the MIT License.

