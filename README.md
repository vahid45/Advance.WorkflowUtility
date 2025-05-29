# Advanced Workflow Utility

This project contains a collection of custom workflow activities for Microsoft Dynamics CRM that extend the standard workflow capabilities.

## Workflow Activities

### 1. WFStartWorkflowFromWorkflow
Starts a workflow from within another workflow.
- **Inputs:**
  - Workflow: Reference to the workflow to start
  - Record URL: URL of the record to run the workflow on

### 2. WFQualifyLead
Qualifies a lead and creates related records.
- **Inputs:**
  - Lead: Reference to the lead to qualify
  - Create Account: Boolean to create account
  - Create Contact: Boolean to create contact
  - Create Opportunity: Boolean to create opportunity
  - Existing Account: Optional reference to existing account
  - LeadStatus: Status of the lead
- **Outputs:**
  - Created Account: Reference to created account
  - Created Contact: Reference to created contact
  - Created Opportunity: Reference to created opportunity

### 3. WFGetCurrentUser
Gets the current user running the workflow.
- **Outputs:**
  - Current User: Reference to the system user

### 4. WFExecuteWorkflowOnFetchXML
Executes a workflow on records returned by a FetchXML query.
- **Inputs:**
  - FetchXml: The FetchXML query
  - Workflow: Reference to the workflow to execute
  - Record URL: Dynamic record URL
- **Outputs:**
  - Have Error: Boolean indicating if there was an error
  - Message: Error message if any

### 5. WFCloneRecord
Clones a record with specified options.
- **Inputs:**
  - Clonning Record URL: URL of the record to clone
  - Prefix: Optional prefix for the cloned record
  - Fields to Ignore: List of fields to ignore during cloning
- **Outputs:**
  - Cloned Guid: ID of the cloned record

### 6. WFCancelRunningWorkflow
Cancels a running workflow instance.
- **Inputs:**
  - Current WorkflowName: Name of the workflow to cancel
  - Parent Record URL: URL of the record the workflow is running on

### 7. WFUserInRole
Checks if a user is in specified roles.
- **Inputs:**
  - User: Reference to the user to check
  - Is CurrentUser: Boolean to use current user
  - Roles: Comma-separated list of roles
- **Outputs:**
  - User In Role: Boolean indicating if user is in any of the roles

### 8. WFShareRecordWithUser
Shares a record with a specific user.
- **Inputs:**
  - Sharing Record URL: URL of the record to share
  - User: Reference to the user to share with
  - Read/Write/Delete/Append/AppendTo/Assign Permissions: Boolean flags for each permission

### 9. WFDuplicateDetection
Checks for duplicate records based on specified criteria.
- **Inputs:**
  - Entity: Entity name to check
  - Fetch XML: Query to find duplicates
  - Various filtering options
- **Outputs:**
  - IsExist: Boolean indicating if duplicates exist

### 10. WFNoteCheckAttachment
Checks if a note has attachments.
- **Inputs:**
  - Note To Check: Reference to the note
- **Outputs:**
  - Has Attachment: Boolean indicating if note has attachments

### 11. WFExecuteSQLJob
Executes a SQL Server job.
- **Inputs:**
  - SSIS SQL Connection String: Connection string to SQL Server
  - SQL Job Name: Name of the job to execute
- **Outputs:**
  - Message: Execution result message

### 12. WFSetStageProcess
Sets the stage of a business process flow.
- **Inputs:**
  - Record URL: URL of the record
  - Process: Reference to the process
  - Process Stage Name: Name of the stage to set

### 13. WFShareRecordWithTeam
Shares a record with a team.
- **Inputs:**
  - Sharing Record URL: URL of the record to share
  - Team: Reference to the team
  - Various permission flags (Read/Write/Delete/Append/AppendTo/Assign)

### 14. WFGetConfig
Retrieves configuration values from a custom entity.
- **Inputs:**
  - Entity Name: Name of the config entity
  - Key Field name: Name of the key field
  - Key: The key to look up
  - Value Field Name: Name of the value field
  - Value Field Type: Type of the value (1-String, 2-Lookup)
- **Outputs:**
  - Various value outputs based on type

### 15. WFGetLenghtofString
Gets the length of a string.
- **Inputs:**
  - Text: The string to measure
- **Outputs:**
  - Length: Integer length of the string

### 16. WFNoteDeleteAttachment
Deletes attachments from a note.
- **Inputs:**
  - Note With Attachments: Reference to the note
  - Size limits and extension filters
  - Append Notice: Boolean to add deletion notice
- **Outputs:**
  - Number Of Attachments Deleted: Count of deleted attachments

### 17. WFCreateOrderProduct
Creates a product line item in a sales order.
- **Inputs:**
  - Order: Reference to the sales order
  - Product: Reference to the product
  - Unit: Reference to the unit of measure
  - Quantity: Decimal quantity
  - Discount: Optional decimal discount
  - Tax: Optional decimal tax
- **Outputs:**
  - Sales Order Detail: Reference to created line item
  - Message: Operation result message

### 18. WFRollupQuery
Performs aggregate calculations on FetchXML query results.
- **Inputs:**
  - FetchXml: The query to execute
  - Record URL: Dynamic record URL
  - Sum Column: Column to sum
- **Outputs:**
  - Count: Number of records
  - Sum: Sum of values
  - Average: Average of values
  - Max: Maximum value
  - Min: Minimum value

### 19. WFConvertMiladiToShamsi
Converts Gregorian date to Persian (Shamsi) date.
- **Inputs:**
  - Miladi Date: Gregorian date to convert
  - Format: Output date format
- **Outputs:**
  - Shamsi Date: Converted Persian date string

### 20. WFConvertOrderToInvoice
Converts a sales order to an invoice.
- **Inputs:**
  - Order: Reference to the sales order
- **Outputs:**
  - Invoice: Reference to created invoice

### 21. ConcatString
Concatenates two strings.
- **Inputs:**
  - String1: First string
  - String2: Second string
- **Outputs:**
  - ConcatedString: Combined string

### 22. WFNoteUpdateTitle
Updates the title of a note.
- **Inputs:**
  - Note To Update: Reference to the note
  - New Title: New title text
- **Outputs:**
  - Updated Title: The new title

### 23. QuotRecalculation
Recalculates a quote's prices.
- **Inputs:**
  - Quote: Reference to the quote to recalculate

### 24. WFRollupFieldCalculation
Calculates a rollup field value for a specified record.
- **Inputs:**
  - Record URL: URL of the record to calculate the rollup field for
  - Rollup Field Name: Name of the rollup field to calculate
- **Outputs:**
  - Success: Boolean indicating if the calculation was successful
  - Message: Status or error message

## Installation

1. Clone the repo
2. build it
3. Registrer the Assembly by using Plugin registration Tool

## Requirements

- Microsoft Dynamics CRM 2016 or later
- .NET Framework 4.6.2 or later

## Support

For support and questions, please contact the development team. 