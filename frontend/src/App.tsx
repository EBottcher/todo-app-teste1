import { FormEvent, useCallback, useEffect, useMemo, useState } from "react";
import {
  createProject,
  createTask,
  deleteTask,
  listActivity,
  listProjects,
  listTasks,
  updateTask
} from "./api";
import type {
  ActivityLogEntry,
  Project,
  RecurrenceFrequency,
  SmartFilter,
  TodoItem,
  TodoPriority,
  TodoStatus
} from "./types";

type TaskFormState = {
  projectId: string;
  title: string;
  description: string;
  status: TodoStatus;
  priority: TodoPriority;
  tagsCsv: string;
  dueDateLocal: string;
  remindersLocalCsv: string;
  recurrenceFrequency: RecurrenceFrequency;
  recurrenceInterval: number;
};

const statusOptions: TodoStatus[] = ["Todo", "InProgress", "Done", "Blocked"];
const priorityOptions: TodoPriority[] = ["Low", "Medium", "High", "Critical"];
const recurrenceOptions: RecurrenceFrequency[] = ["None", "Daily", "Weekly", "Monthly", "Yearly"];
const smartFilterOptions: SmartFilter[] = ["None", "Today", "Overdue", "Upcoming"];

type Theme = "light" | "dark";

const THEME_STORAGE_KEY = "todo-app-theme";

function getInitialTheme(): Theme {
  if (typeof window === "undefined") {
    return "light";
  }

  const stored = window.localStorage.getItem(THEME_STORAGE_KEY);
  if (stored === "light" || stored === "dark") {
    return stored;
  }

  return window.matchMedia && window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
}

const defaultTaskForm: TaskFormState = {
  projectId: "",
  title: "",
  description: "",
  status: "Todo",
  priority: "Medium",
  tagsCsv: "",
  dueDateLocal: "",
  remindersLocalCsv: "",
  recurrenceFrequency: "None",
  recurrenceInterval: 1
};

function toLocalInputValue(utcValue?: string | null): string {
  if (!utcValue) {
    return "";
  }

  const date = new Date(utcValue);
  const localDate = new Date(date.getTime() - date.getTimezoneOffset() * 60 * 1000);
  return localDate.toISOString().slice(0, 16);
}

function toUtcIso(localValue: string): string | null {
  if (!localValue.trim()) {
    return null;
  }

  const date = new Date(localValue);
  if (Number.isNaN(date.getTime())) {
    throw new Error(`Invalid date value: ${localValue}`);
  }

  return date.toISOString();
}

function normalizeCsv(values: string): string[] {
  return values
    .split(",")
    .map((value) => value.trim())
    .filter((value) => value.length > 0);
}

function mapTaskToForm(task: TodoItem): TaskFormState {
  return {
    projectId: task.projectId,
    title: task.title,
    description: task.description ?? "",
    status: task.status,
    priority: task.priority,
    tagsCsv: task.tags.join(", "),
    dueDateLocal: toLocalInputValue(task.dueDateUtc),
    remindersLocalCsv: task.remindersUtc.map(toLocalInputValue).filter(Boolean).join(", "),
    recurrenceFrequency: task.recurrence?.frequency ?? "None",
    recurrenceInterval: task.recurrence?.interval ?? 1
  };
}

export default function App() {
  const [projects, setProjects] = useState<Project[]>([]);
  const [tasks, setTasks] = useState<TodoItem[]>([]);
  const [activity, setActivity] = useState<ActivityLogEntry[]>([]);
  const [taskForm, setTaskForm] = useState<TaskFormState>(defaultTaskForm);
  const [projectName, setProjectName] = useState("");
  const [projectDescription, setProjectDescription] = useState("");
  const [projectColorHex, setProjectColorHex] = useState("#16a34a");
  const [editingTaskId, setEditingTaskId] = useState<string | null>(null);
  const [smartFilter, setSmartFilter] = useState<SmartFilter>("None");
  const [selectedProjectId, setSelectedProjectId] = useState("");
  const [tagFilter, setTagFilter] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [theme, setTheme] = useState<Theme>(getInitialTheme);

  useEffect(() => {
    document.documentElement.setAttribute("data-theme", theme);
    window.localStorage.setItem(THEME_STORAGE_KEY, theme);
  }, [theme]);

  function toggleTheme() {
    setTheme((previous) => (previous === "dark" ? "light" : "dark"));
  }

  const selectedProjectName = useMemo(() => {
    if (!selectedProjectId) {
      return "All projects";
    }

    return projects.find((project) => project.id === selectedProjectId)?.name ?? "Selected project";
  }, [projects, selectedProjectId]);

  const refreshData = useCallback(async () => {
    setLoading(true);
    setError(null);

    try {
      const [projectsResult, tasksResult, activityResult] = await Promise.all([
        listProjects(),
        listTasks({
          projectId: selectedProjectId || undefined,
          smartFilter,
          tag: tagFilter.trim() || undefined
        }),
        listActivity(80)
      ]);

      setProjects(projectsResult);
      setTasks(tasksResult);
      setActivity(activityResult);

      if (!taskForm.projectId && projectsResult.length > 0) {
        setTaskForm((previous) => ({
          ...previous,
          projectId: projectsResult[0].id
        }));
      }
    } catch (caughtError) {
      setError((caughtError as Error).message);
    } finally {
      setLoading(false);
    }
  }, [selectedProjectId, smartFilter, tagFilter, taskForm.projectId]);

  useEffect(() => {
    void refreshData();
  }, [refreshData]);

  function resetTaskForm(defaultProjectId: string) {
    setTaskForm({
      ...defaultTaskForm,
      projectId: defaultProjectId
    });
    setEditingTaskId(null);
  }

  async function handleCreateProject(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);

    try {
      const project = await createProject({
        name: projectName,
        description: projectDescription || null,
        colorHex: projectColorHex || null
      });

      setProjectName("");
      setProjectDescription("");
      setProjectColorHex("#16a34a");
      await refreshData();
      setTaskForm((previous) => ({
        ...previous,
        projectId: previous.projectId || project.id
      }));
    } catch (caughtError) {
      setError((caughtError as Error).message);
    }
  }

  async function handleSubmitTask(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);

    if (!taskForm.projectId) {
      setError("Choose a project before saving the task.");
      return;
    }

    try {
      const dueDateUtc = toUtcIso(taskForm.dueDateLocal);
      const remindersUtc = normalizeCsv(taskForm.remindersLocalCsv)
        .map(toUtcIso)
        .filter((value): value is string => value !== null);

      const payload = {
        projectId: taskForm.projectId,
        title: taskForm.title,
        description: taskForm.description || null,
        status: taskForm.status,
        priority: taskForm.priority,
        tags: normalizeCsv(taskForm.tagsCsv),
        dueDateUtc,
        remindersUtc,
        recurrence:
          taskForm.recurrenceFrequency === "None"
            ? null
            : {
                frequency: taskForm.recurrenceFrequency,
                interval: taskForm.recurrenceInterval
              }
      };

      if (editingTaskId) {
        await updateTask(editingTaskId, payload);
      } else {
        await createTask(payload);
      }

      await refreshData();
      resetTaskForm(payload.projectId);
    } catch (caughtError) {
      setError((caughtError as Error).message);
    }
  }

  async function handleDeleteTask(taskId: string) {
    setError(null);

    try {
      await deleteTask(taskId);
      await refreshData();
      if (editingTaskId === taskId) {
        const fallbackProjectId = projects[0]?.id ?? "";
        resetTaskForm(fallbackProjectId);
      }
    } catch (caughtError) {
      setError((caughtError as Error).message);
    }
  }

  return (
    <main className="layout">
      <section className="panel">
        <div className="row" style={{ justifyContent: "space-between" }}>
          <h1>Todo-list POC</h1>
          <button
            type="button"
            className="theme-toggle"
            onClick={toggleTheme}
            aria-pressed={theme === "dark"}
          >
            {theme === "dark" ? "☀️ Light mode" : "🌙 Dark mode"}
          </button>
        </div>
        <p className="subtitle">Project: {selectedProjectName}</p>
        {error ? <p className="error">{error}</p> : null}
        <p className="meta">
          {loading ? "Loading..." : `${tasks.length} task(s), ${activity.length} activity event(s)`}
        </p>
      </section>

      <section className="panel">
        <h2>Projects / Lists</h2>
        <form className="grid-form" onSubmit={handleCreateProject}>
          <label>
            Name
            <input value={projectName} onChange={(event) => setProjectName(event.target.value)} required />
          </label>
          <label>
            Description
            <input
              value={projectDescription}
              onChange={(event) => setProjectDescription(event.target.value)}
              placeholder="Optional"
            />
          </label>
          <label>
            Color
            <input
              type="color"
              value={projectColorHex}
              onChange={(event) => setProjectColorHex(event.target.value)}
            />
          </label>
          <button type="submit">Create project</button>
        </form>
      </section>

      <section className="panel">
        <h2>Smart filters</h2>
        <div className="row">
          <label>
            Filter
            <select value={smartFilter} onChange={(event) => setSmartFilter(event.target.value as SmartFilter)}>
              {smartFilterOptions.map((option) => (
                <option key={option} value={option}>
                  {option}
                </option>
              ))}
            </select>
          </label>
          <label>
            Project
            <select value={selectedProjectId} onChange={(event) => setSelectedProjectId(event.target.value)}>
              <option value="">All projects</option>
              {projects.map((project) => (
                <option key={project.id} value={project.id}>
                  {project.name}
                </option>
              ))}
            </select>
          </label>
          <label>
            Tag
            <input
              value={tagFilter}
              onChange={(event) => setTagFilter(event.target.value)}
              placeholder="e.g. backend"
            />
          </label>
          <button type="button" onClick={() => void refreshData()}>
            Apply
          </button>
        </div>
      </section>

      <section className="panel">
        <h2>{editingTaskId ? "Update task" : "Create task"}</h2>
        <form className="grid-form" onSubmit={handleSubmitTask}>
          <label>
            Project
            <select
              value={taskForm.projectId}
              onChange={(event) => setTaskForm((previous) => ({ ...previous, projectId: event.target.value }))}
              required
            >
              {projects.map((project) => (
                <option key={project.id} value={project.id}>
                  {project.name}
                </option>
              ))}
            </select>
          </label>

          <label>
            Title
            <input
              value={taskForm.title}
              onChange={(event) => setTaskForm((previous) => ({ ...previous, title: event.target.value }))}
              required
            />
          </label>

          <label>
            Description
            <textarea
              value={taskForm.description}
              onChange={(event) => setTaskForm((previous) => ({ ...previous, description: event.target.value }))}
              rows={3}
            />
          </label>

          <label>
            Status
            <select
              value={taskForm.status}
              onChange={(event) => setTaskForm((previous) => ({ ...previous, status: event.target.value as TodoStatus }))}
            >
              {statusOptions.map((status) => (
                <option key={status} value={status}>
                  {status}
                </option>
              ))}
            </select>
          </label>

          <label>
            Priority
            <select
              value={taskForm.priority}
              onChange={(event) =>
                setTaskForm((previous) => ({ ...previous, priority: event.target.value as TodoPriority }))
              }
            >
              {priorityOptions.map((priority) => (
                <option key={priority} value={priority}>
                  {priority}
                </option>
              ))}
            </select>
          </label>

          <label>
            Tags
            <input
              value={taskForm.tagsCsv}
              onChange={(event) => setTaskForm((previous) => ({ ...previous, tagsCsv: event.target.value }))}
              placeholder="work, urgent"
            />
          </label>

          <label>
            Due date (UTC)
            <input
              type="datetime-local"
              value={taskForm.dueDateLocal}
              onChange={(event) => setTaskForm((previous) => ({ ...previous, dueDateLocal: event.target.value }))}
            />
          </label>

          <label>
            Reminders (comma separated)
            <input
              value={taskForm.remindersLocalCsv}
              onChange={(event) =>
                setTaskForm((previous) => ({ ...previous, remindersLocalCsv: event.target.value }))
              }
              placeholder="2026-09-30T09:00, 2026-10-01T08:30"
            />
          </label>

          <label>
            Recurrence
            <select
              value={taskForm.recurrenceFrequency}
              onChange={(event) =>
                setTaskForm((previous) => ({
                  ...previous,
                  recurrenceFrequency: event.target.value as RecurrenceFrequency
                }))
              }
            >
              {recurrenceOptions.map((recurrence) => (
                <option key={recurrence} value={recurrence}>
                  {recurrence}
                </option>
              ))}
            </select>
          </label>

          <label>
            Recurrence interval
            <input
              type="number"
              min={1}
              value={taskForm.recurrenceInterval}
              onChange={(event) =>
                setTaskForm((previous) => ({
                  ...previous,
                  recurrenceInterval: Math.max(1, Number(event.target.value))
                }))
              }
            />
          </label>

          <div className="row">
            <button type="submit">{editingTaskId ? "Update task" : "Create task"}</button>
            {editingTaskId ? (
              <button
                type="button"
                className="secondary"
                onClick={() => resetTaskForm(projects[0]?.id ?? "")}
              >
                Cancel edit
              </button>
            ) : null}
          </div>
        </form>
      </section>

      <section className="panel">
        <h2>Tasks</h2>
        <div className="table-wrap">
          <table>
            <thead>
              <tr>
                <th>Title</th>
                <th>Status</th>
                <th>Priority</th>
                <th>Tags</th>
                <th>Due</th>
                <th>Reminders</th>
                <th>Recurrence</th>
                <th>Project</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {tasks.map((task) => {
                const projectName = projects.find((project) => project.id === task.projectId)?.name ?? task.projectId;

                return (
                  <tr key={task.id}>
                    <td>{task.title}</td>
                    <td>{task.status}</td>
                    <td>{task.priority}</td>
                    <td>{task.tags.join(", ") || "-"}</td>
                    <td>{task.dueDateUtc ? new Date(task.dueDateUtc).toLocaleString() : "-"}</td>
                    <td>
                      {task.remindersUtc.length > 0
                        ? task.remindersUtc.map((reminder) => new Date(reminder).toLocaleString()).join(" | ")
                        : "-"}
                    </td>
                    <td>
                      {task.recurrence
                        ? `${task.recurrence.frequency} every ${task.recurrence.interval}`
                        : "None"}
                    </td>
                    <td>{projectName}</td>
                    <td>
                      <div className="row">
                        <button
                          type="button"
                          className="small"
                          onClick={() => {
                            setEditingTaskId(task.id);
                            setTaskForm(mapTaskToForm(task));
                          }}
                        >
                          Edit
                        </button>
                        <button
                          type="button"
                          className="small danger"
                          onClick={() => void handleDeleteTask(task.id)}
                        >
                          Delete
                        </button>
                      </div>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      </section>

      <section className="panel">
        <h2>Activity log (who changed what)</h2>
        <ul className="activity-list">
          {activity.map((entry) => (
            <li key={entry.id}>
              <strong>{entry.changedBy}</strong> {entry.action} {entry.entityType}{" "}
              <span className="meta">{new Date(entry.changedAtUtc).toLocaleString()}</span>
              <div>{entry.details}</div>
            </li>
          ))}
        </ul>
      </section>
    </main>
  );
}
