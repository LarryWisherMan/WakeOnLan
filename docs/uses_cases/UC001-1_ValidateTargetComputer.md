# Use Case: Validate Target Computer Information

## **Use Case ID**: UC001-1

---

## **Short Description**
The system validates the target computer name/IP, MAC address, and proxy computer information provided by the admin to ensure they are correctly formatted, adhere to network conventions, and are operational. If any validation fails, the system logs the error and notifies the admin, preventing further actions.

---

## **Primary Actor**
- **WoL Service**: The system component responsible for performing validation.

---

## **Secondary Actors**
- **Admin**: Provides the input data (e.g., target computer name/IP, MAC address).
- **Proxy Computer**: May provide additional validation if it needs to check its reachability.

---

## **Preconditions**
- Admin has submitted the necessary inputs:
  - Target computer name or IP.
  - Target computer MAC address.
  - Proxy computer name or IP.
- WoL Service is operational.

---

## **Postconditions**
- **On Success**:
  - Validation is successful, and the system proceeds to the next step (e.g., generating the magic packet).
- **On Failure**:
  - The system logs the failure.
  - The admin is notified of the invalid input.
  - The use case terminates without further action.

---

## **Main Flow**
1. The WoL Service receives the target computer name/IP, MAC address, and proxy computer name from the Admin.
2. The WoL Service validates the target computer name or IP format:
   - Ensures the name is non-empty and adheres to naming conventions (e.g., hostname rules).
   - Checks if the IP address is valid (e.g., correct format, within acceptable ranges).
3. The WoL Service validates the MAC address format:
   - Ensures the MAC address is 6 octets in hexadecimal notation.
   - Verifies the MAC address is not a broadcast or reserved address.
4. The WoL Service verifies the proxy computer name or IP:
   - Ensures it is reachable on the network.
   - Optionally checks proxy permissions or configurations.
5. If all validations pass:
   - The system logs a successful validation event.
   - The main use case proceeds to the next step (e.g., generating the magic packet).

---

## **Alternate Flows**

### **Invalid Target Computer Name/IP**
- If the target computer name or IP is invalid:
  - The system logs the error (e.g., "Invalid IP format").
  - The admin is notified of the invalid input via an alert.
  - The use case terminates.

### **Invalid MAC Address**
- If the MAC address fails validation:
  - The system logs the error (e.g., "MAC address contains invalid characters").
  - The admin is notified of the issue.
  - The use case terminates.

### **Proxy Computer Unreachable**
- If the proxy computer cannot be reached:
  - The system logs the error (e.g., "Proxy unreachable").
  - The admin is notified to provide a reachable proxy.
  - The use case terminates.

---

## **Extensions**
- **Cached Validation**:
  - The WoL Service may cache previously validated target computer and proxy information to speed up future operations.
- **Validation Rules Configuration**:
  - The system should allow administrators to update validation rules (e.g., allowed IP ranges) via a configuration interface.

---

## **Special Requirements**
1. **Performance**:
   - Validation must complete within 2 seconds to ensure responsiveness.
2. **Security**:
   - Input data must be sanitized to prevent injection attacks.
3. **Scalability**:
   - The system should handle simultaneous validation requests for multiple target computers without degradation in performance.

---

## **Trigger**
- The use case starts when the Admin submits a wake-up request, and the WoL Service begins validating the input data.
