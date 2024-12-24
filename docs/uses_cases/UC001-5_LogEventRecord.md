# Use Case: Log Event Record

## **Use Case ID**: UC001-5

---

## **Short Description**
The system creates and updates an event record to track the status and results of the Wake-on-LAN process. This ensures a complete log of actions, outcomes, and errors for auditing and troubleshooting purposes.

---

## **Primary Actor**
- **WoL Service**: Responsible for initiating the creation of event records and updating them as the process progresses.

---

## **Secondary Actors**
- **Monitor Service**: Updates the event record with the target computer's status during monitoring.
- **Admin**: Reviews the event record for status and troubleshooting information.

---

## **Preconditions**
- The WoL Service has initiated a wake-up request for the target computer.
- Logging functionality is operational and accessible.

---

## **Postconditions**
- **On Success**:
  - A new event record is created for the wake-up process.
  - The record is updated with all relevant statuses and outcomes (e.g., magic packet sent, monitoring results, success/failure).
- **On Failure**:
  - Any errors encountered during the logging process are recorded in the system logs.
  - The admin is notified if logging fails.

---

## **Main Flow**
1. The WoL Service initiates the creation of a new event record.
2. The event record is populated with initial details, including:
   - Target computer name/IP.
   - MAC address.
   - Timestamp of the wake-up request.
   - Proxy computer name/IP.
3. After the magic packet is sent, the record is updated with the transmission status.
4. During monitoring, the Monitor Service periodically updates the record with the target computer’s status (e.g., online, retries remaining).
5. When the target computer is successfully woken up or the process fails:
   - The event record is finalized with the outcome (success/failure) and a timestamp.
6. The system ensures the event record is stored securely and is accessible for future review.

---

## **Alternate Flows**

### **Failure to Create Event Record**
- If the system cannot create an event record due to an error:
  - The system logs the error.
  - The admin is notified of the logging issue.
  - The wake-up process continues without an event record.

---

## **Extensions**
- **Audit Trail**:
  - The system includes additional fields in the event record for audit purposes, such as:
    - User who initiated the request.
    - Duration of the wake-up process.
    - Number of retries attempted.
- **Integration with External Systems**:
  - The event record can be exported or synchronized with external monitoring systems or logging frameworks.

---

## **Special Requirements**
1. **Persistence**:
   - Event records must be stored in a persistent database or logging system for long-term access.
2. **Performance**:
   - Logging operations must complete within 100 milliseconds to avoid impacting the overall process.
3. **Security**:
   - Event records must be protected against unauthorized access and tampering.
4. **Searchability**:
   - Event records must be searchable by fields such as target computer name, timestamp, and outcome.

---

## **Trigger**
- The use case starts when the WoL Service begins the wake-up process by sending a magic packet to the Proxy Computer.

---
