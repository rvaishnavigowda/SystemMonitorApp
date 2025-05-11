# System Monitor Application

This is a cross-platform system monitor application built on .Net 8. It tracks system resources like CPU, memory, and disk usage and provides plugin support along with optional API integration.

## Design Decisions and Challenges

The System Monitor application is built using the Clean Architecture pattern to promote separation of concerns, scalability, and maintainability. The solution is structured into distinct layers—Domain, Application, Infrastructure, and Console with clear responsibilities. This modular design ensures that core business logic remains independent of external dependencies like the operating system APIs or the console UI. A key decision was to introduce a plugin system, allowing future enhancements or monitoring modules (e.g., GPU usage, network stats) to be added dynamically without changing the core codebase. 

One of the main challenges was accessing accurate and real-time system metrics in a cross-platform manner. While Windows offers robust performance counters via WMI and System.Diagnostics, Linux and macOS require alternative approaches, and some metrics aren't universally available. Managing platform differences while keeping the core logic clean required careful abstraction.

## How to Build and Run the Solution

### Prerequisites

- **.NET 8.0 SDK** installed on your machine. 

### Running the Published Application

 Download the SystemMonitorEsecFile. Once you have extracted the files from the zip archive, follow the steps below to run the application:

1. **Navigate to the folder** containing the `SystemMonitorApp.Console.exe` file. The path should look like this:
    ```
    D:\SystemMonitorExecFile\
    ```

2. **Run the Application**:
    - Open a **Command Prompt** window (press `Windows + R`, type `cmd`, and hit Enter).
    - Change to the directory where the `.exe` file is located:
      ```bash
      cd D:\SystemMonitorExecFile
      ```

    - Run the application using the following command:
      ```bash
      SystemMonitorApp.Console.exe
      ```

3. **Verify the Application**:
    - The console application should start, and you'll begin seeing logs and data related to CPU, memory, and disk usage.
    - The application will continue running and monitoring your system resources.

### Configuration

- Configuration files are located in the **`Configuration/`** directory within the zip file, with the main configuration file being `config.json`.
- You can modify `config.json` to adjust the application's behavior (e.g., set thresholds for system resource monitoring).

## Design and Architecture

- This application follows the **Clean Architecture** pattern to ensure maintainability and scalability.
- **Plugins** are dynamically loaded to allow users to extend the application with additional features.



## Future Enhancements

- Adding a GUI for easier monitoring.
- Enhanced API integrations for extended monitoring features.
