export type TodoStatus = "Todo" | "InProgress" | "Done" | "Blocked";
export type TodoPriority = "Low" | "Medium" | "High" | "Critical";
export type RecurrenceFrequency = "None" | "Daily" | "Weekly" | "Monthly" | "Yearly";
export type SmartFilter = "None" | "Today" | "Overdue" | "Upcoming";

export interface RecurrencePattern {
  frequency: RecurrenceFrequency;
  interval: number;
}

export interface TodoItem {
  id: string;
  projectId: string;
  title: string;
  description?: string | null;
  status: TodoStatus;
  priority: TodoPriority;
  tags: string[];
  dueDateUtc?: string | null;
  remindersUtc: string[];
  recurrence?: RecurrencePattern | null;
  createdAtUtc: string;
  updatedAtUtc: string;
  createdBy: string;
  updatedBy: string;
}

export interface Project {
  id: string;
  name: string;
  description?: string | null;
  colorHex?: string | null;
  createdAtUtc: string;
  updatedAtUtc: string;
  createdBy: string;
  updatedBy: string;
}

export interface ActivityLogEntry {
  id: string;
  entityType: string;
  entityId: string;
  action: string;
  changedBy: string;
  changedAtUtc: string;
  details: string;
}

export interface TodoUpsertPayload {
  projectId: string;
  title: string;
  description?: string | null;
  status: TodoStatus;
  priority: TodoPriority;
  tags: string[];
  dueDateUtc?: string | null;
  remindersUtc: string[];
  recurrence?: RecurrencePattern | null;
}
