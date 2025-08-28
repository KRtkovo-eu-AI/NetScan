# NetScan

Simple Windows network scanner built with C# and WinForms. The application scans a specified subnet for active hosts and checks a list of ports on those hosts.

## Building

Requires the .NET SDK (8.0 or later). On Windows with the SDK installed:

```
dotnet build
```

## Usage

1. Enter the subnet prefix (e.g. `192.168.1`).
2. Choose the range of host numbers to scan.
3. Provide a comma-separated list of ports to check.
4. Click **Scan** to list responding devices and their open ports.
