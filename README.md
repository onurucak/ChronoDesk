# ChronoDesk

ChronoDesk is a modern Windows desktop time-tracking application built with .NET 9 and WPF. It helps you track time spent on projects, view daily/weekly/monthly summaries, and export data to CSV.

## Features

*   **Project Management**: Create, view, and list projects.
*   **Time Tracking**: Start/stop timers for selected projects. Only one active timer allows tracking at a time.
*   **Tray Icon**: Minimize to tray, quick access to restore or exit.
*   **Auto-Save**: Active timer state is persisted; recovers on restart.
*   **Reporting**: View summaries by project for a selected date range.
*   **Export**: Export time entries to CSV format.
*   **Modern Architecture**: Clean Architecture, MVVM, Dependency Injection, EF Core (SQLite).

## Requirements

*   .NET 9 SDK
*   Windows OS

## How to Run

1.  Open a terminal in the solution directory.
2.  Run the application:
    ```bash
    dotnet run --project ChronoDesk.UI
    ```
3.  The database `chronodesk.db` will be automatically created in the output directory on first run.

## Architecture

The solution follows a Clean Architecture approach:

*   **ChronoDesk.Domain**: Core entities (`Project`, `TimeEntry`) and logic.
*   **ChronoDesk.Application**: Service interfaces and business logic (`TimerService`, `ProjectService`).
*   **ChronoDesk.Infrastructure**: EF Core implementation (`ChronoDbContext`), Repositories, and File Services.
*   **ChronoDesk.UI**: WPF Presentation layer using MVVM, `Microsoft.Extensions.DependencyInjection`, and `System.Windows.Forms` for Tray Icon.

## Next Improvements (Backlog)

*   [ ] **Visual Charts**: Implement graphical charts (Pie/Bar) for summaries using a library like LiveCharts.
*   [ ] **Dark/Light Theme Toggle**: Add user preference for UI theme.
*   [ ] **Detailed Editing**: Allow editing past time entries (start/end times) not just notes.
*   [ ] **Idle Detection**: Auto-pause timer if user is inactive for X minutes.
*   [ ] **Cloud Sync**: Sync database to cloud storage or API.
