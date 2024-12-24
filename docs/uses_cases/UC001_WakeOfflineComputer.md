# Use Case: Wake Offline Computer on Any Subnet

## **Use Case ID**: UC001

## **Primary Actor**:
- Admin

## **Secondary Actors**:
- WoL Service
- Proxy Computer
- Monitor Service

### **Trigger**:
- The admin submits a wake-up request for a target computer via the system interface.


## **Preconditions**:
- MAC address is valid.
- Target computer is offline.
- Proxy computer is online.
- The WoL Service is operational and configured.

## **Postconditions**:
- WoL command is sent, and the record is captured.
- Target computer is woken up.
- Success/Failure record is captured.
- Admin is notified of the final status of the wake-up request.

---

## **Main Flow**:
### **1. Description**:
The admin wants to wake up a computer that is residing on a different subnet than the current host.

### **2. Actor's Actions**:
- Admin provides the target computer name/IP, target MAC address, and proxy computer name to the system.

### **3. System Actions**:
1. The system validates the provided information (computer name/IP, MAC address, proxy computer name).
2. The system ensures the target computer is offline.
3. The system ensures the proxy computer is online and reachable.
4. The system generates the magic packet using the target computer's MAC address.
5. The system creates an event record for the wake-up request.
6. The system periodically checks the target computer's status (e.g., ping or network response).
7. The system updates the event record when the target computer is successfully woken up.
8. The system notifies the admin of the success or failure of the operation.

---

## **Alternate Flows**:
### **Invalid Target Computer Name or MAC Address**:
- The system validates the provided target computer name and MAC address.
- If the input is invalid:
  - The system logs the error.
  - The system notifies the admin of the invalid input via an alert.
  - The use case terminates without further action.

### **Target Already Online or Proxy Computer Offline**:
- The system checks if the target computer is online.
- The system verifies that the proxy computer is reachable and operational.
- If the target is online or the proxy is offline:
  - The system logs the issue for troubleshooting.
  - The system notifies the admin with details about the issue.
  - The use case terminates.

### **Target Fails to Wake Up**:
- The system monitors the wake-up process.
- If the target does not respond after three retries:
  - The system logs the failure and updates the event record.
  - The system notifies the admin of the failed wake-up attempt.
  - The use case terminates after recording the failure.

---

## **Extensions**:
- **Asynchronous Processing**:
  - The system must handle wake-up requests asynchronously to ensure responsiveness and scalability.
  - Tasks should be queued to prevent delays when processing multiple requests.

- **Throttling**:
  - The system should limit the number of wake-up verifications to a configurable threshold, such as 10 requests per minute, to avoid overloading the network.

- **Failure Handling**:
  - If a target computer fails to wake up, the system should record the failure and continue processing subsequent requests.
  - Failure details should be logged for troubleshooting purposes.

- **Scalability**:
  - The system must be designed to handle hundreds or thousands of concurrent wake-up requests without significant performance degradation.
  - The architecture should support dynamic scaling to handle peak loads when necessary.

---

## **Special Requirements**:
- **Security**:
  - The system must authenticate the admin's credentials before processing any wake-up requests.
  - All communication between system components must use secure protocols (e.g., HTTPS or encrypted communication) to prevent unauthorized access.

- **Logging and Auditing**:
  - The system must maintain detailed logs of all wake-up requests and their results.
  - Logs should include:
    - Timestamps
    - Input parameters (e.g., target computer name, MAC address)
    - Outcome (success or failure)
  - Audit logs must be accessible for troubleshooting and compliance purposes.

- **Configurable Parameters**:
  - The system should allow configuration of retry intervals, monitoring intervals, and throttling limits via a settings file or user interface.

- **Performance**:
  - The system should ensure that wake-up requests are processed within an acceptable time frame (e.g., less than 10 seconds per request under normal load conditions).
