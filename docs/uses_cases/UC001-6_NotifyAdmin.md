# Use Case: Notify Admin of Success/Failure

## **Use Case ID**: UC001-6

---

## **Short Description**
The system sends a notification to the admin with the final status of the Wake-on-LAN process, including details about whether the target computer was successfully woken up or if the process failed.

---

## **Primary Actor**
- **WoL Service**: Responsible for initiating and managing the notification process.

---

## **Secondary Actors**
- **Admin**: Receives the notification.

---

## **Preconditions**
- The system has completed the Wake-on-LAN process.
- The event record is finalized with the outcome of the process.

---

## **Postconditions**
- **On Success**:
  - The admin is notified of the outcome (success or failure).
  - The notification includes relevant details such as the target computer name, timestamp, and status.
- **On Failure**:
  - The system logs any errors encountered while sending the notification.
  - The admin is alerted about the notification failure.

---

## **Main Flow**
1. The WoL Service retrieves the outcome of the wake-up process from the finalized event record.
2. The system formats the notification message with details such as:
   - Target computer name/IP.
   - Status of the wake-up process (e.g., success, failure).
   - Timestamp of the outcome.
   - Any relevant error messages (in case of failure).
3. The system sends the notification to the admin via the configured communication channel (e.g., email, dashboard alert).
4. The system logs the successful delivery of the notification.

---

## **Alternate Flows**

### **Notification Delivery Failure**
- If the notification cannot be delivered:
  - The system retries the notification up to three times.
  - After three failed attempts:
    - The system logs the failure.
    - The admin is notified of the notification issue through an alternate channel (e.g., system logs or dashboard).

---

## **Extensions**
- **Custom Notification Channels**:
  - Admins can configure preferred notification methods, such as:
    - Email.
    - SMS.
    - Dashboard alerts.
    - Push notifications.
- **Batch Notifications**:
  - For simultaneous wake-up requests, the system groups notifications into a summary report instead of individual alerts.

---

## **Special Requirements**
1. **Delivery Confirmation**:
   - The system must confirm successful delivery of the notification and log the confirmation.
2. **Timeliness**:
   - Notifications must be sent within 1 second of the process outcome being finalized.
3. **Customizability**:
   - Admins should be able to configure notification templates and thresholds (e.g., notify only on failures).

---

## **Trigger**
- The use case starts when the event record is finalized, indicating the outcome of the Wake-on-LAN process.

---
