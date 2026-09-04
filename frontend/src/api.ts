import type {
  ActivityLogEntry,
  Project,
  SmartFilter,
  TodoItem,
  TodoUpsertPayload
} from "./types";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5009/api";
const ACTOR = "web-user";

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...init,
    headers: {
      "Content-Type": "application/json",
      "X-User": ACTOR,
      ...(init?.headers ?? {})
    }
  });

  if (!response.ok) {
    throw new Error(`Request failed (${response.status}): ${await response.text()}`);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

export async function listProjects(): Promise<Project[]> {
  return request<Project[]>("/projects");
}

export async function createProject(payload: {
  name: string;
  description?: string | null;
  colorHex?: string | null;
}): Promise<Project> {
  return request<Project>("/projects", {
    method: "POST",
    body: JSON.stringify(payload)
  });
}

export async function listTasks(query: {
  projectId?: string;
  smartFilter?: SmartFilter;
  tag?: string;
}): Promise<TodoItem[]> {
  const params = new URLSearchParams();
  if (query.projectId) params.set("projectId", query.projectId);
  if (query.smartFilter && query.smartFilter !== "None") params.set("smartFilter", query.smartFilter);
  if (query.tag) params.set("tag", query.tag);

  const suffix = params.size > 0 ? `?${params.toString()}` : "";
  return request<TodoItem[]>(`/tasks${suffix}`);
}

export async function createTask(payload: {
  projectId?: string;
  title: string;
  description?: string | null;
  status: TodoItem["status"];
  priority: TodoItem["priority"];
  tags: string[];
  dueDateUtc?: string | null;
  remindersUtc: string[];
  recurrence?: TodoItem["recurrence"];
}): Promise<TodoItem> {
  return request<TodoItem>("/tasks", {
    method: "POST",
    body: JSON.stringify(payload)
  });
}

export async function updateTask(id: string, payload: TodoUpsertPayload): Promise<TodoItem> {
  return request<TodoItem>(`/tasks/${id}`, {
    method: "PUT",
    body: JSON.stringify(payload)
  });
}

export async function deleteTask(id: string): Promise<void> {
  await request<void>(`/tasks/${id}`, {
    method: "DELETE"
  });
}

export async function listActivity(limit = 100): Promise<ActivityLogEntry[]> {
  return request<ActivityLogEntry[]>(`/activity?limit=${limit}`);
}
