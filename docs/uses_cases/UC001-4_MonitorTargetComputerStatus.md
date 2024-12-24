# Use Case: Monitor Target Computer Status

## **Use Case ID**: UC001-4

---

## **Short Description**
The system periodically monitors the status of the target computer to determine whether the Wake-on-LAN process successfully brought the computer online. If the target remains offline after a specified number of retries, the system logs the failure and notifies the admin.

---

## **Primary Actor**
- **Monitor Service**: Responsible for checking the target computer's status and updating the event log.

---

## **Secondary Actors**
- **Admin**: Receives notifications about the success or failure of the monitoring process.

---

## **Preconditions**
- The magic packet has been successfully transmitted to the Proxy Computer.
- The target computer’s name/IP is available for status checks.
- Monitor Service is operational and configured for retries and intervals.

---

## **Postconditions**
- **On Success**:
  - The system detects that the target computer is online.
  - The event record is updated to reflect a successful wake-up.
  - The admin is notified of the success.
- **On Failure**:
  - The system detects that the target computer remains offline after the maximum number of retries.
  - The failure is logged in the event record.
  - The admin is notified of the failure.

---

## **Main Flow**
1. The Monitor Service begins status checks for the target computer.
2. The system pings the target computer’s IP or attempts a network connection to verify it is online.
3. If the target computer responds:
   - The system logs the successful wake-up event.
   - The admin is notified of the success.
4. If the target computer does not respond:
   - The system waits for the configured interval before retrying.
   - The system retries up to the maximum number of attempts.
5. If the target computer is detected online within the retry limit:
   - The process ends successfully.

---

## **Alternate Flows**

### **Target Fails to Wake Up**
- If the target computer remains offline after the maximum number of retries:
  - The system logs the failure.
  - The admin is notified of the failed wake-up attempt.
  - The use case terminates.

---

## **Extensions**
- **Custom Retry Intervals**:
  - Admins can configure the interval between retries and the maximum number of retries.
- **Alternative Detection Methods**:
  - The system can use alternative methods (e.g., querying a directory service) to verify the target’s status if ping is blocked.

---

## **Special Requirements**
1. **Performance**:
   - Status checks must complete within 2 seconds per attempt to ensure responsiveness.
2. **Scalability**:
   - The system must handle simultaneous monitoring of multiple target computers without significant performance degradation.
3. **Asynchronous Processing**:
   - The monitoring process should run asynchronously to avoid blocking other operations.

---

## **Trigger**
- The use case starts when the magic packet has been sent to the Proxy Computer in the **"Send Magic Packet"** sub-use case.

---
