
# LWM.WakeOnLan

**LWM.WakeOnLan** is a PowerShell module for sending Wake-on-LAN (WOL) requests and monitoring the status of target computers. This module supports both single and multiple target configurations and offers advanced features such as asynchronous execution, proxy-based WOL requests, and result monitoring.


## Features

- **Wake-on-LAN**:
  - Send WOL requests to one or more target computers.
  - Use proxies to send WOL packets for cross-network support.
- **Monitoring**:
  - Monitor the status of target computers after sending WOL requests.
  - Retrieve and manage monitoring logs.
- **Advanced Configuration**:
  - Configure retries, timeouts, and ports for WOL operations.
  - Use secure credentials for proxy connections.

---

## Installation

### From PowerShell Gallery (Recommended)

To install the module directly from the PowerShell Gallery, run:

```powershell
Install-Module -Name LWM.WakeOnLan -AllowPrerelease
```

### Verify Installation

```powershell
# Check the installed module
Get-Module -Name LWM.WakeOnLan -ListAvailable

# Import the module
Import-Module -Name LWM.WakeOnLan

# List available cmdlets
Get-Command -Module LWM.WakeOnLan
```

### Uninstallation

To uninstall the module:

```powershell
Uninstall-Module -Name LWM.WakeOnLan
```

---

## Usage

### Cmdlets

| Cmdlet                   | Description                                      |
|--------------------------|--------------------------------------------------|
| `Invoke-WakeOnLan`       | Sends WOL requests and monitors target status.   |
| `Get-WakeOnLanData`      | Retrieves WOL monitor logs or results.           |

---


### Wake-on-LAN Command

#### Single Target Example

```powershell
Invoke-WakeOnLan -MacAddress "00:1A:2B:3C:4D:5E" -ComputerName "TargetPC" -ProxyComputerName "ProxyPC"
```


#### Multiple Targets Example

```powershell
$targets = @{
    "ProxyPC1" = @(
        @{ MacAddress = "00:1A:2B:3C:4D:5E"; ComputerName = "TargetPC1" },
        @{ MacAddress = "00:1A:2B:3C:4D:5F"; ComputerName = "TargetPC2" }
    )
}

Invoke-WakeOnLan -HashtableTargets $targets
```

### Monitor Logs

Retrieve WOL monitor logs:


```powershell
Get-WakeOnLanData -MonitorLog | Format-Table
```

#### Output

| ComputerName | ProxyComputerName | WolSentTime         | MonitoringEndTime   | WolSuccess | IsMonitoringComplete | PingCount |
|--------------|-------------------|---------------------|---------------------|------------|-----------------------|-----------|
| TargetPC2    | 172.31.115.247    | 1/7/2025 8:44:33 AM | 1/7/2025 8:45:33 AM | False      | False                | 1         |
| TargetPC1    | 172.31.115.247    | 1/7/2025 8:44:33 AM | 1/7/2025 8:45:33 AM | True      | True                | 5         |


- **ComputerName**: The target computer that the WOL request was sent to.
- **ProxyComputerName**: The proxy computer used to send the WOL request.
- **WolSentTime**: The timestamp of when the WOL packet was sent.
- **MonitoringEndTime**: The timestamp of when monitoring for the target ends.
- **WolSuccess**: Indicates whether the WOL request was successful (`True` or `False`).
- **IsMonitoringComplete**: Indicates whether monitoring for the target has been completed.
- **PingCount**: The number of ping attempts made during monitoring.


### Results

Retrieve WOL Results logs:

```powershell
Get-WakeOnLanData -Results | Format-Table
```

#### Output

| TargetComputerName | TargetMacAddress  | ProxyComputerName | Port | Timestamp           | RequestSent | WolSuccess | ErrorMessage                                          |
|--------------------|-------------------|-------------------|------|---------------------|-------------|------------|------------------------------------------------------|
| TargetPC3          | 00:1A:2B:3C:4D:60 | ProxyPC2          | 9    | 1/7/2025 8:44:33 AM | False       | False      | Proxy computer is not reachable.                    |
| TargetPC2          | 00:1A:2B:3C:4D:5F | 172.31.115.247    | 9    | 1/7/2025 8:44:34 AM | True        | False      | Ping response not received within the timeout period.|
| TargetPC4          | 00:1A:2B:3C:4D:61 | ProxyPC2          | 9    | 1/7/2025 8:44:33 AM | False       | False      | Proxy computer is not reachable.                    |
| TargetPC1          | 00:1A:2B:3C:4D:5F | 172.31.115.247    | 9    | 1/7/2025 8:44:34 AM | True        | True       |                                                    |

- **TargetComputerName**: The name of the target computer for which the WOL request was sent.
- **TargetMacAddress**: The MAC address of the target computer.
- **ProxyComputerName**: The proxy computer used to send the WOL request.
- **Port**: The network port used for the WOL request.
- **Timestamp**: The date and time when the request was sent.
- **RequestSent**: Indicates whether the WOL request was sent (`True` or `False`).
- **WolSuccess**: Indicates whether the WOL operation was successful (`True` or `False`).
- **ErrorMessage**: Provides details about any errors encountered during the operation.

### Clearing Logs

You can clear either Result Logs or Monitor Logs by running `Get-WakeOnLanData` with the `-ClearAfterRetrieval` switch

```powershell
#Clear Results:
Get-WakeOnLanData -Results -ClearAfterRetrieval

#Clear Monitor:
Get-WakeOnLanData -Monitor  -ClearAfterRetrieval
```

## Configuration

You can customize the following parameters for WOL operations:

- **Port**: Specify the port used for WOL packets (default: `9`).
- **MaxPingAttempts**: Set the maximum number of ping retries (default: `5`).
- **TimeoutInSeconds**: Configure the timeout for WOL operations (default: `60`).
- **Credentials**: Provide secure credentials for proxy connections.

---

## Release Notes

### Version 0.2.0

- Initial release.
- Added support for WOL requests with single and multiple targets.
- Implemented monitoring and log retrieval features.
