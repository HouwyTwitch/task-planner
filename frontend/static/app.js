// Task Planner SPA
const API = "/api";

const state = {
  token: localStorage.getItem("token") || "",
  me: null,
  users: [],
  projects: [],
  tasks: [],
  currentView: { kind: "all" }, // {kind:"all"|"today"|"project"|"assigned", id?}
  ws: null,
  wsConnected: false,
};

// ---------- HTTP ----------
async function api(path, opts = {}) {
  const headers = { "Content-Type": "application/json", ...(opts.headers || {}) };
  if (state.token) headers["Authorization"] = "Bearer " + state.token;
  const res = await fetch(API + path, { ...opts, headers });
  if (res.status === 401) { logout(); throw new Error("Не авторизовано"); }
  const ct = res.headers.get("content-type") || "";
  const data = ct.includes("application/json") ? await res.json() : await res.text();
  if (!res.ok) throw new Error((data && data.detail) || ("HTTP " + res.status));
  return data;
}

// ---------- Auth ----------
async function login(username, password) {
  const body = new URLSearchParams({ username, password });
  const res = await fetch(API + "/auth/login", {
    method: "POST",
    headers: { "Content-Type": "application/x-www-form-urlencoded" },
    body,
  });
  const data = await res.json();
  if (!res.ok) throw new Error(data.detail || "Ошибка входа");
  state.token = data.access_token;
  state.me = data.user;
  localStorage.setItem("token", state.token);
}

async function register(payload) {
  const data = await api("/auth/register", { method: "POST", body: JSON.stringify(payload) });
  state.token = data.access_token;
  state.me = data.user;
  localStorage.setItem("token", state.token);
}

function logout() {
  state.token = ""; state.me = null;
  localStorage.removeItem("token");
  if (state.ws) { try { state.ws.close(); } catch(e){} state.ws = null; }
  render();
}

// ---------- Data ----------
async function loadAll() {
  state.me = await api("/auth/me");
  state.users = await api("/auth/users");
  state.projects = await api("/projects");
  await loadTasks();
}

async function loadTasks() {
  const view = state.currentView;
  let path = "/tasks?include_done=true";
  if (view.kind === "project") path += "&project_id=" + view.id;
  state.tasks = await api(path);
}

// ---------- WebSocket ----------
function connectWs() {
  if (state.ws) { try { state.ws.close(); } catch (e){} }
  const proto = location.protocol === "https:" ? "wss:" : "ws:";
  const ws = new WebSocket(`${proto}//${location.host}/ws?token=${encodeURIComponent(state.token)}`);
  state.ws = ws;
  ws.addEventListener("open", () => { state.wsConnected = true; updateStatus(); });
  ws.addEventListener("close", () => {
    state.wsConnected = false; updateStatus();
    // reconnect
    setTimeout(() => { if (state.token) connectWs(); }, 2000);
  });
  ws.addEventListener("message", (ev) => {
    try {
      const msg = JSON.parse(ev.data);
      handleWsMessage(msg);
    } catch(e){ console.error(e); }
  });
}

function handleWsMessage(msg) {
  if (msg.type === "task.created" || msg.type === "task.updated") {
    const idx = state.tasks.findIndex(t => t.id === msg.task.id);
    if (idx >= 0) state.tasks[idx] = msg.task;
    else state.tasks.push(msg.task);
    renderContent();
  } else if (msg.type === "task.deleted") {
    state.tasks = state.tasks.filter(t => t.id !== msg.task_id);
    renderContent();
  } else if (msg.type === "reminder.fire") {
    const t = msg.task;
    // если у нас нет — подхватить
    const idx = state.tasks.findIndex(x => x.id === t.id);
    if (idx >= 0) state.tasks[idx] = t; else state.tasks.push(t);
    showReminderToast(t, msg.reminder_id);
    tryLocalNotify(t);
    renderContent();
  } else if (msg.type === "reminder.ack") {
    const idx = state.tasks.findIndex(x => x.id === msg.task.id);
    if (idx >= 0) state.tasks[idx] = msg.task;
    renderContent();
  }
}

// ---------- Local (foreground) notifications ----------
function tryLocalNotify(task) {
  if (!("Notification" in window)) return;
  if (Notification.permission !== "granted") return;
  const n = new Notification("Напоминание: " + task.title, {
    body: task.notes ? task.notes.slice(0, 200) : "Пора приступать",
    tag: "task-" + task.id,
    icon: "/static/icon.png",
  });
  n.onclick = () => { window.focus(); openTaskEditor(task.id); n.close(); };
}

// ---------- Web Push subscribe ----------
async function ensurePushSubscription() {
  if (!("serviceWorker" in navigator) || !("PushManager" in window)) return;
  try {
    const reg = await navigator.serviceWorker.register("/sw.js");
    const perm = await Notification.requestPermission();
    if (perm !== "granted") return;
    let sub = await reg.pushManager.getSubscription();
    const { key } = await api("/push/vapid-public-key");
    if (!sub) {
      sub = await reg.pushManager.subscribe({
        userVisibleOnly: true,
        applicationServerKey: urlBase64ToUint8Array(key),
      });
    }
    const raw = sub.toJSON();
    await api("/push/subscribe", { method: "POST", body: JSON.stringify({
      endpoint: raw.endpoint,
      keys: raw.keys,
      user_agent: navigator.userAgent,
    })});
  } catch (e) {
    console.warn("push subscribe failed:", e);
  }
}

function urlBase64ToUint8Array(b64) {
  const padding = "=".repeat((4 - b64.length % 4) % 4);
  const base64 = (b64 + padding).replace(/-/g, "+").replace(/_/g, "/");
  const raw = atob(base64);
  const out = new Uint8Array(raw.length);
  for (let i = 0; i < raw.length; ++i) out[i] = raw.charCodeAt(i);
  return out;
}

// ---------- Rendering ----------
const $ = (sel, el = document) => el.querySelector(sel);
const $$ = (sel, el = document) => Array.from(el.querySelectorAll(sel));
const el = (tag, attrs = {}, ...children) => {
  const n = document.createElement(tag);
  for (const [k, v] of Object.entries(attrs)) {
    if (k === "class") n.className = v;
    else if (k === "html") n.innerHTML = v;
    else if (k.startsWith("on")) n.addEventListener(k.slice(2).toLowerCase(), v);
    else if (v !== false && v != null) n.setAttribute(k, v);
  }
  for (const c of children.flat()) {
    if (c == null || c === false) continue;
    n.appendChild(typeof c === "string" ? document.createTextNode(c) : c);
  }
  return n;
};

function render() {
  const root = $("#app");
  root.innerHTML = "";
  if (!state.token) {
    root.appendChild(renderLogin());
  } else {
    root.appendChild(renderShell());
  }
}

// ---------- Login ----------
function renderLogin() {
  const tpl = $("#tpl-login").content.cloneNode(true);
  const shell = tpl.firstElementChild;
  const tabs = $$(".tab", shell);
  const formLogin = $("[data-form=login]", shell);
  const formReg = $("[data-form=register]", shell);
  tabs.forEach(t => t.addEventListener("click", () => {
    tabs.forEach(x => x.classList.toggle("active", x === t));
    const which = t.dataset.tab;
    formLogin.classList.toggle("hidden", which !== "login");
    formReg.classList.toggle("hidden", which !== "register");
  }));
  formLogin.addEventListener("submit", async (e) => {
    e.preventDefault();
    const data = Object.fromEntries(new FormData(formLogin));
    const err = $("[data-err]", formLogin);
    err.textContent = "";
    try {
      await login(data.username, data.password);
      await afterLogin();
    } catch (ex) { err.textContent = ex.message; }
  });
  formReg.addEventListener("submit", async (e) => {
    e.preventDefault();
    const data = Object.fromEntries(new FormData(formReg));
    const err = $("[data-err]", formReg);
    err.textContent = "";
    try {
      await register({ username: data.username, password: data.password, display_name: data.display_name });
      await afterLogin();
    } catch (ex) { err.textContent = ex.message; }
  });
  return shell;
}

async function afterLogin() {
  await loadAll();
  connectWs();
  ensurePushSubscription();
  render();
  checkMissed();
}

// ---------- App shell ----------
function renderShell() {
  const shell = el("div", { class: "app-shell" });
  shell.appendChild(renderSidebar());
  shell.appendChild(renderMain());
  return shell;
}

function renderSidebar() {
  const side = el("div", { class: "sidebar" });
  side.appendChild(el("div", { class: "sidebar-head" },
    el("div", { class: "brand-mark" }, "✓"),
    el("div", { class: "brand-name" }, "Планировщик")
  ));

  side.appendChild(el("div", { class: "sidebar-section-title" }, "Виды"));
  const nav = el("ul", { class: "nav-list" });
  const views = [
    { kind: "today", name: "Сегодня", icon: "•", color: "#5b8def" },
    { kind: "assigned", name: "Мне назначено", icon: "•", color: "#f2b658" },
    { kind: "all", name: "Все задачи", icon: "•", color: "#9aa1b1" },
  ];
  for (const v of views) {
    const item = el("li", {
      class: "nav-item" + (state.currentView.kind === v.kind ? " active" : ""),
      onclick: () => { state.currentView = { kind: v.kind }; loadTasks().then(render); },
    },
      el("span", { class: "dot", style: `background:${v.color}` }),
      el("span", { class: "name" }, v.name),
    );
    nav.appendChild(item);
  }
  side.appendChild(nav);

  const projHead = el("div", { class: "sidebar-section-title", style: "display:flex; align-items:center; gap:8px" },
    el("span", { style: "flex:1" }, "Проекты"),
    el("button", { class: "btn ghost", style: "padding:2px 6px; font-size:12px", onclick: openProjectEditor }, "+"),
  );
  side.appendChild(projHead);
  const projList = el("ul", { class: "nav-list" });
  for (const p of state.projects) {
    const item = el("li", {
      class: "nav-item" + (state.currentView.kind === "project" && state.currentView.id === p.id ? " active" : ""),
      onclick: () => { state.currentView = { kind: "project", id: p.id }; loadTasks().then(render); },
    },
      el("span", { class: "dot", style: `background:${p.color}` }),
      el("span", { class: "name" }, p.name),
      el("button", { class: "btn ghost", style: "padding:0 4px; font-size:11px",
        onclick: (e) => { e.stopPropagation(); openProjectEditor(p); } }, "⚙"),
    );
    projList.appendChild(item);
  }
  side.appendChild(projList);

  const me = state.me || {};
  const initial = (me.display_name || me.username || "?").charAt(0).toUpperCase();
  side.appendChild(el("div", { class: "sidebar-foot" },
    el("div", { class: "avatar" }, initial),
    el("div", { class: "user-info" },
      el("div", { class: "name" }, me.display_name || me.username || ""),
      el("div", { class: "role" }, me.is_admin ? "администратор" : "пользователь"),
    ),
    el("button", { class: "btn ghost", title: "Выйти", onclick: logout }, "⇥"),
  ));
  return side;
}

function renderMain() {
  const main = el("div", { class: "main" });
  const bar = el("div", { class: "topbar" },
    el("h1", {}, currentViewTitle()),
    el("div", { class: "spacer" }),
    el("div", { class: "status" },
      el("span", { class: "ws-dot" + (state.wsConnected ? " on" : "") }),
      state.wsConnected ? "онлайн" : "оффлайн",
    ),
  );
  main.appendChild(bar);

  const content = el("div", { class: "content", id: "content" });
  main.appendChild(content);
  renderContentInto(content);
  return main;
}

function currentViewTitle() {
  const v = state.currentView;
  if (v.kind === "today") return "Сегодня";
  if (v.kind === "assigned") return "Мне назначено";
  if (v.kind === "project") {
    const p = state.projects.find(x => x.id === v.id);
    return p ? p.name : "Проект";
  }
  return "Все задачи";
}

function updateStatus() {
  const bar = document.querySelector(".topbar .status");
  if (!bar) return;
  bar.innerHTML = "";
  bar.appendChild(el("span", { class: "ws-dot" + (state.wsConnected ? " on" : "") }));
  bar.appendChild(document.createTextNode(state.wsConnected ? "онлайн" : "оффлайн"));
}

function renderContent() {
  const c = $("#content");
  if (c) renderContentInto(c);
}

function renderContentInto(root) {
  root.innerHTML = "";

  // missed banner
  if (state._missed && state._missed.length) {
    const banner = el("div", { class: "missed-banner" },
      el("div", { class: "txt" },
        el("div", { class: "b-title" }, `Пропущенные напоминания: ${state._missed.length}`),
        el("div", { class: "b-desc" }, "Их время уже прошло — подтвердите или откройте задачу"),
      ),
      el("button", { class: "btn", onclick: showMissedModal }, "Посмотреть"),
    );
    root.appendChild(banner);
  }

  // Быстрое добавление
  const addBox = el("div", { class: "add-task" });
  const projId = state.currentView.kind === "project" ? state.currentView.id : null;
  const input = el("input", { class: "title", placeholder: "Добавить задачу…", onkeydown: async (e) => {
    if (e.key === "Enter" && input.value.trim()) {
      await createTaskQuick(input.value.trim(), projId);
      input.value = "";
    }
  }});
  addBox.appendChild(input);
  addBox.appendChild(el("button", { class: "btn primary", onclick: () => openTaskEditor(null, projId) }, "Настроить"));
  root.appendChild(addBox);

  // Список
  const tasks = filterTasksForView();
  const groups = groupTasks(tasks);
  for (const [title, list] of groups) {
    root.appendChild(el("div", { class: "task-group-title" }, `${title} · ${list.length}`));
    const container = el("div", { class: "task-list" });
    for (const t of list) container.appendChild(renderTaskCard(t));
    root.appendChild(container);
  }
  if (!tasks.length) {
    root.appendChild(el("div", { class: "empty" }, "Нет задач в этом виде. Добавьте первую сверху."));
  }
}

function filterTasksForView() {
  const v = state.currentView;
  const now = new Date();
  const soon = new Date(now); soon.setHours(23, 59, 59);
  return state.tasks.filter(t => {
    if (v.kind === "project" && t.project_id !== v.id) return false;
    if (v.kind === "assigned" && t.assignee_id !== state.me.id) return false;
    if (v.kind === "today") {
      const dueSoon = t.scheduled_at && new Date(t.scheduled_at) <= soon;
      const overdue = t.scheduled_at && new Date(t.scheduled_at) < now && !t.is_done;
      return dueSoon || overdue || (!t.scheduled_at && !t.is_done && t.assignee_id === state.me.id);
    }
    return true;
  });
}

function groupTasks(tasks) {
  const active = tasks.filter(t => !t.is_done).sort((a,b) => (b.priority - a.priority) || ((a.scheduled_at||"z") > (b.scheduled_at||"z") ? 1 : -1));
  const done = tasks.filter(t => t.is_done).sort((a,b) => (b.done_at||"").localeCompare(a.done_at||""));
  const out = [];
  if (active.length) out.push(["Активные", active]);
  if (done.length) out.push(["Выполненные", done]);
  return out;
}

function renderTaskCard(t) {
  const card = el("div", { class: "task-card" + (t.is_done ? " done" : ""), onclick: () => openTaskEditor(t.id) },
    el("div", { class: "prio-bar prio-" + (t.priority || 0) }),
  );
  const check = el("button", { class: "check" + (t.is_done ? " done" : ""),
    title: t.is_done ? "Вернуть в работу" : "Завершить",
    onclick: (e) => { e.stopPropagation(); toggleDone(t); },
  }, t.is_done ? "✓" : "");
  card.insertBefore(check, card.firstChild);

  const body = el("div", { class: "task-body" });
  body.appendChild(el("div", { class: "title" }, t.title));
  const meta = el("div", { class: "meta" });
  if (t.project_id) {
    const p = state.projects.find(x => x.id === t.project_id);
    if (p) meta.appendChild(el("span", { class: "chip", style: `background:${p.color}22; color:${p.color}` }, p.name));
  }
  if (t.assignee_id) {
    const u = state.users.find(x => x.id === t.assignee_id);
    if (u) meta.appendChild(el("span", { class: "chip" }, "@" + (u.display_name || u.username)));
  }
  if (t.scheduled_at) {
    const d = new Date(t.scheduled_at);
    const overdue = d < new Date() && !t.is_done;
    const cls = overdue ? "chip overdue" : (isSameDay(d, new Date()) ? "chip today" : "chip");
    meta.appendChild(el("span", { class: cls }, formatDate(d)));
  }
  if (t.recurrence && t.recurrence.kind && t.recurrence.kind !== "none") {
    meta.appendChild(el("span", { class: "chip recurring" }, "↻ " + recurrenceLabel(t.recurrence)));
  }
  if (t.reminders && t.reminders.length) {
    meta.appendChild(el("span", { class: "chip bell" }, `🔔 ${t.reminders.length}`));
  }
  if (t.estimate_minutes) {
    meta.appendChild(el("span", { class: "chip" }, humanMinutes(t.estimate_minutes)));
  }
  body.appendChild(meta);
  card.appendChild(body);

  card.appendChild(el("div", { class: "task-right" },
    el("button", { class: "btn ghost", title: "Удалить", onclick: (e) => { e.stopPropagation(); if (confirm("Удалить задачу?")) deleteTask(t); }}, "×"),
  ));
  return card;
}

function isSameDay(a, b) { return a.toDateString() === b.toDateString(); }
function formatDate(d) {
  const pad = n => String(n).padStart(2, "0");
  const today = new Date();
  const sameDay = isSameDay(d, today);
  const time = `${pad(d.getHours())}:${pad(d.getMinutes())}`;
  if (sameDay) return "сегодня " + time;
  const t2 = new Date(today); t2.setDate(t2.getDate() + 1);
  if (isSameDay(d, t2)) return "завтра " + time;
  return `${pad(d.getDate())}.${pad(d.getMonth()+1)} ${time}`;
}
function humanMinutes(n) {
  if (n < 60) return `${n} мин`;
  const h = Math.floor(n/60), m = n%60;
  return m ? `${h}ч ${m}м` : `${h}ч`;
}
function recurrenceLabel(r) {
  const map = { daily: "ежедневно", weekly: "еженедельно", monthly: "ежемесячно", workdays: "по будням" };
  return map[r.kind] || r.kind;
}

// ---------- Task ops ----------
async function createTaskQuick(title, project_id) {
  const created = await api("/tasks", { method: "POST", body: JSON.stringify({ title, project_id, reminders: [] })});
  state.tasks.push(created);
  renderContent();
}

async function toggleDone(t) {
  const upd = await api("/tasks/" + t.id, { method: "PATCH", body: JSON.stringify({ is_done: !t.is_done })});
  const idx = state.tasks.findIndex(x => x.id === t.id);
  if (idx >= 0) state.tasks[idx] = upd;
  renderContent();
}

async function deleteTask(t) {
  await api("/tasks/" + t.id, { method: "DELETE" });
  state.tasks = state.tasks.filter(x => x.id !== t.id);
  renderContent();
}

// ---------- Task editor ----------
function openTaskEditor(taskId, project_id) {
  const existing = taskId ? state.tasks.find(t => t.id === taskId) : null;
  const draft = existing ? JSON.parse(JSON.stringify(existing)) : {
    title: "", notes: "", project_id: project_id || null, assignee_id: null,
    estimate_minutes: 0, priority: 0, scheduled_at: null, due_at: null,
    recurrence: { kind: "none", interval: 1, weekdays: [], day_of_month: null },
    reminders: [],
  };
  if (!draft.recurrence || !draft.recurrence.kind) draft.recurrence = { kind: "none", interval: 1, weekdays: [], day_of_month: null };
  showModal((close) => renderTaskEditor(draft, existing, close));
}

function renderTaskEditor(draft, existing, close) {
  const modal = el("div", { class: "modal" });
  modal.appendChild(el("div", { class: "modal-head" },
    el("h2", {}, existing ? "Задача" : "Новая задача"),
    el("div", { class: "spacer" }),
    el("button", { class: "btn ghost", onclick: close }, "×"),
  ));
  const body = el("div", { class: "modal-body" });

  // title
  const titleI = el("input", { value: draft.title, placeholder: "Название" });
  titleI.addEventListener("input", () => draft.title = titleI.value);
  body.appendChild(el("div", { class: "row top" }, el("div", { class: "label" }, "Название"), titleI));

  // notes
  const notesI = el("textarea", { placeholder: "Заметки" });
  notesI.value = draft.notes || "";
  notesI.addEventListener("input", () => draft.notes = notesI.value);
  body.appendChild(el("div", { class: "row top" }, el("div", { class: "label" }, "Заметки"), notesI));

  // project
  const projSel = el("select", {});
  projSel.appendChild(el("option", { value: "" }, "— без проекта —"));
  for (const p of state.projects) {
    const opt = el("option", { value: String(p.id) }, p.name);
    if (draft.project_id === p.id) opt.selected = true;
    projSel.appendChild(opt);
  }
  projSel.addEventListener("change", () => draft.project_id = projSel.value ? parseInt(projSel.value) : null);
  body.appendChild(el("div", { class: "row" }, el("div", { class: "label" }, "Проект"), projSel));

  // assignee
  const asgSel = el("select", {});
  asgSel.appendChild(el("option", { value: "" }, "— не назначено —"));
  for (const u of state.users) {
    const opt = el("option", { value: String(u.id) }, "@" + (u.display_name || u.username));
    if (draft.assignee_id === u.id) opt.selected = true;
    asgSel.appendChild(opt);
  }
  asgSel.addEventListener("change", () => draft.assignee_id = asgSel.value ? parseInt(asgSel.value) : null);
  body.appendChild(el("div", { class: "row" }, el("div", { class: "label" }, "Исполнитель"), asgSel));

  // priority
  const prSel = el("select", {});
  for (const [v, name] of [[0,"низкий"],[1,"обычный"],[2,"высокий"],[3,"срочно"]]) {
    const opt = el("option", { value: String(v) }, name);
    if (draft.priority === v) opt.selected = true;
    prSel.appendChild(opt);
  }
  prSel.addEventListener("change", () => draft.priority = parseInt(prSel.value));
  body.appendChild(el("div", { class: "row" }, el("div", { class: "label" }, "Приоритет"), prSel));

  // scheduled_at
  const schedI = el("input", { type: "datetime-local", value: toLocalDT(draft.scheduled_at) });
  schedI.addEventListener("input", () => draft.scheduled_at = fromLocalDT(schedI.value));
  body.appendChild(el("div", { class: "row" }, el("div", { class: "label" }, "Запланировано на"), schedI));

  // estimate
  const estI = el("input", { type: "number", min: "0", value: String(draft.estimate_minutes || 0) });
  estI.addEventListener("input", () => draft.estimate_minutes = parseInt(estI.value || "0"));
  body.appendChild(el("div", { class: "row" }, el("div", { class: "label" }, "Оценка (мин)"), estI));

  // recurrence
  const recBox = el("div", { class: "rec-editor" });
  const recKind = el("select", {});
  for (const [v, name] of [["none","нет"],["daily","каждые N дней"],["workdays","по будням"],["weekly","каждые N недель"],["monthly","каждые N месяцев"]]) {
    const o = el("option", { value: v }, name);
    if (draft.recurrence.kind === v) o.selected = true;
    recKind.appendChild(o);
  }
  const intI = el("input", { type: "number", min: "1", value: String(draft.recurrence.interval || 1), style: "width:70px" });
  const weekBox = el("div", { class: "weekday-picker" });
  const domI = el("input", { type: "number", min: "1", max: "31", value: draft.recurrence.day_of_month || "", placeholder: "число" });
  const buildWeek = () => {
    weekBox.innerHTML = "";
    const names = ["Пн","Вт","Ср","Чт","Пт","Сб","Вс"];
    for (let i=0; i<7; i++) {
      const chip = el("button", { type: "button", class: "weekday-chip" + ((draft.recurrence.weekdays||[]).includes(i) ? " active" : ""),
        onclick: () => {
          const s = new Set(draft.recurrence.weekdays || []);
          s.has(i) ? s.delete(i) : s.add(i);
          draft.recurrence.weekdays = Array.from(s).sort();
          buildWeek();
        }
      }, names[i]);
      weekBox.appendChild(chip);
    }
  };
  buildWeek();
  const relayout = () => {
    intI.style.display = ["daily","weekly","monthly"].includes(draft.recurrence.kind) ? "" : "none";
    weekBox.style.display = draft.recurrence.kind === "weekly" ? "" : "none";
    domI.style.display = draft.recurrence.kind === "monthly" ? "" : "none";
  };
  recKind.addEventListener("change", () => { draft.recurrence.kind = recKind.value; relayout(); });
  intI.addEventListener("input", () => draft.recurrence.interval = parseInt(intI.value || "1"));
  domI.addEventListener("input", () => draft.recurrence.day_of_month = domI.value ? parseInt(domI.value) : null);
  const rl1 = el("div", { style: "display:flex; gap:8px; align-items:center; flex-wrap:wrap" }, recKind, intI, domI);
  recBox.appendChild(rl1);
  recBox.appendChild(weekBox);
  relayout();
  body.appendChild(el("div", { class: "row top" }, el("div", { class: "label" }, "Регулярность"), recBox));

  // reminders
  const remBox = el("div", { class: "reminders-list" });
  const buildReminders = () => {
    remBox.innerHTML = "";
    for (const [i, r] of draft.reminders.entries()) {
      const amt = el("input", { type: "number", min: "0", value: String(r.offset_amount || 0) });
      const unit = el("select", {});
      for (const [v, n] of [["minutes","мин"],["hours","час"],["days","дн"],["weeks","нед"]]) {
        const o = el("option", { value: v }, n);
        if (r.offset_unit === v) o.selected = true;
        unit.appendChild(o);
      }
      const snap = el("label", { class: "snap" });
      const snapC = el("input", { type: "checkbox" });
      snapC.checked = r.snap_to_workday !== false;
      snap.appendChild(snapC); snap.appendChild(document.createTextNode(" на рабочий день"));
      const preview = el("div", { class: "fire" }, computeFirePreview(draft, r) || "");
      const rm = el("button", { type: "button", class: "btn ghost", title: "Удалить", onclick: () => {
        draft.reminders.splice(i, 1); buildReminders();
      }}, "×");
      const update = () => {
        r.offset_amount = parseInt(amt.value || "0");
        r.offset_unit = unit.value;
        r.snap_to_workday = snapC.checked;
        preview.textContent = computeFirePreview(draft, r) || "";
      };
      amt.addEventListener("input", update);
      unit.addEventListener("change", update);
      snapC.addEventListener("change", update);
      remBox.appendChild(el("div", { class: "reminder-row" }, amt, unit, el("span", {}, "до"), preview, snap, rm));
    }
    if (!draft.reminders.length) remBox.appendChild(el("div", { class: "hint" }, "Без напоминаний"));
    remBox.appendChild(el("button", { type: "button", class: "btn", onclick: () => {
      draft.reminders.push({ offset_amount: 15, offset_unit: "minutes", snap_to_workday: true, target_user_id: null });
      buildReminders();
    }}, "+ Напоминание"));
  };
  buildReminders();
  body.appendChild(el("div", { class: "row top" }, el("div", { class: "label" }, "Напоминания"), remBox));

  modal.appendChild(body);

  const foot = el("div", { class: "modal-foot" });
  if (existing) {
    foot.appendChild(el("button", { class: "btn danger", onclick: async () => {
      if (!confirm("Удалить задачу?")) return;
      await deleteTask(existing); close();
    }}, "Удалить"));
    foot.appendChild(el("div", { style: "flex:1" }));
  }
  foot.appendChild(el("button", { class: "btn", onclick: close }, "Отмена"));
  foot.appendChild(el("button", { class: "btn primary", onclick: async () => {
    if (!draft.title.trim()) { alert("Введите название"); return; }
    if (existing) {
      const upd = await api("/tasks/" + existing.id, { method: "PATCH", body: JSON.stringify(draft) });
      const idx = state.tasks.findIndex(x => x.id === upd.id);
      if (idx >= 0) state.tasks[idx] = upd;
    } else {
      const created = await api("/tasks", { method: "POST", body: JSON.stringify(draft) });
      state.tasks.push(created);
    }
    renderContent();
    close();
  }}, existing ? "Сохранить" : "Создать"));
  modal.appendChild(foot);
  return modal;
}

function computeFirePreview(draft, r) {
  if (!draft.scheduled_at) return "укажите дату";
  const s = new Date(draft.scheduled_at);
  const mult = { minutes: 60000, hours: 3600000, days: 86400000, weeks: 7*86400000 }[r.offset_unit || "minutes"] || 60000;
  let d = new Date(s.getTime() - (r.offset_amount||0) * mult);
  if (r.snap_to_workday) {
    while (d.getDay() === 0 || d.getDay() === 6) d = new Date(d.getTime() + 86400000);
  }
  return "сработает: " + formatDate(d);
}

function toLocalDT(iso) {
  if (!iso) return "";
  const d = new Date(iso);
  const pad = n => String(n).padStart(2, "0");
  return `${d.getFullYear()}-${pad(d.getMonth()+1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
}
function fromLocalDT(s) {
  if (!s) return null;
  const d = new Date(s);
  return d.toISOString();
}

// ---------- Project editor ----------
function openProjectEditor(project) {
  const draft = project ? { ...project, member_ids: [...(project.member_ids||[])] } : {
    name: "", color: "#5b8def", member_ids: [state.me.id],
  };
  showModal((close) => {
    const modal = el("div", { class: "modal" });
    modal.appendChild(el("div", { class: "modal-head" },
      el("h2", {}, project ? "Проект" : "Новый проект"),
      el("div", { class: "spacer" }),
      el("button", { class: "btn ghost", onclick: close }, "×"),
    ));
    const body = el("div", { class: "modal-body" });
    const nameI = el("input", { value: draft.name, placeholder: "Название" });
    nameI.addEventListener("input", () => draft.name = nameI.value);
    body.appendChild(el("div", { class: "row" }, el("div", { class: "label" }, "Название"), nameI));

    const colorI = el("input", { type: "color", value: draft.color, style: "width:60px; height:36px; padding:0" });
    colorI.addEventListener("input", () => draft.color = colorI.value);
    body.appendChild(el("div", { class: "row" }, el("div", { class: "label" }, "Цвет"), colorI));

    const members = el("div", { class: "members-picker" });
    for (const u of state.users) {
      const active = draft.member_ids.includes(u.id);
      const chip = el("button", { type: "button", class: "member-chip" + (active ? " active" : ""),
        onclick: () => {
          const s = new Set(draft.member_ids);
          s.has(u.id) ? s.delete(u.id) : s.add(u.id);
          draft.member_ids = Array.from(s);
          chip.classList.toggle("active");
        }
      }, "@" + (u.display_name || u.username));
      members.appendChild(chip);
    }
    body.appendChild(el("div", { class: "row top" }, el("div", { class: "label" }, "Участники"), members));

    modal.appendChild(body);
    const foot = el("div", { class: "modal-foot" });
    if (project) {
      foot.appendChild(el("button", { class: "btn danger", onclick: async () => {
        if (!confirm("Удалить проект? Задачи останутся, но потеряют привязку.")) return;
        await api("/projects/" + project.id, { method: "DELETE" });
        state.projects = state.projects.filter(x => x.id !== project.id);
        if (state.currentView.kind === "project" && state.currentView.id === project.id) state.currentView = { kind: "all" };
        await loadTasks(); render(); close();
      }}, "Удалить"));
      foot.appendChild(el("div", { style: "flex:1" }));
    }
    foot.appendChild(el("button", { class: "btn", onclick: close }, "Отмена"));
    foot.appendChild(el("button", { class: "btn primary", onclick: async () => {
      if (!draft.name.trim()) { alert("Введите название"); return; }
      if (project) {
        const upd = await api("/projects/" + project.id, { method: "PUT", body: JSON.stringify(draft) });
        const idx = state.projects.findIndex(x => x.id === upd.id);
        if (idx >= 0) state.projects[idx] = upd;
      } else {
        const created = await api("/projects", { method: "POST", body: JSON.stringify(draft) });
        state.projects.push(created);
        state.currentView = { kind: "project", id: created.id };
        await loadTasks();
      }
      render(); close();
    }}, project ? "Сохранить" : "Создать"));
    modal.appendChild(foot);
    return modal;
  });
}

// ---------- Missed ----------
async function checkMissed() {
  try {
    const missed = await api("/tasks/missed/all");
    state._missed = missed;
    renderContent();
  } catch(e){ /* ignore */ }
}

function showMissedModal() {
  showModal((close) => {
    const modal = el("div", { class: "modal" });
    modal.appendChild(el("div", { class: "modal-head" },
      el("h2", {}, `Пропущенные напоминания (${(state._missed||[]).length})`),
      el("div", { class: "spacer" }),
      el("button", { class: "btn ghost", onclick: close }, "×"),
    ));
    const body = el("div", { class: "modal-body" });
    for (const m of (state._missed || [])) {
      const row = el("div", { class: "task-card", style: "grid-template-columns: 1fr auto" },
        el("div", { class: "task-body" },
          el("div", { class: "title" }, m.task.title),
          el("div", { class: "meta" },
            el("span", { class: "chip bell" }, "🔔 " + formatDate(new Date(m.reminder.fire_at))),
          ),
        ),
        el("div", { style: "display:flex; gap:6px" },
          el("button", { class: "btn", onclick: () => { close(); openTaskEditor(m.task.id); } }, "Открыть"),
          el("button", { class: "btn primary", onclick: async () => {
            await api(`/tasks/${m.task.id}/reminders/${m.reminder.id}/ack`, { method: "POST" });
            state._missed = state._missed.filter(x => x.reminder.id !== m.reminder.id);
            close(); renderContent();
          }}, "Подтвердить"),
        ),
      );
      body.appendChild(row);
    }
    if (!(state._missed||[]).length) body.appendChild(el("div", { class: "hint" }, "Ничего нет"));
    modal.appendChild(body);
    return modal;
  });
}

function showReminderToast(task, reminder_id) {
  let box = $(".toasts");
  if (!box) { box = el("div", { class: "toasts" }); document.body.appendChild(box); }
  const t = el("div", { class: "toast reminder" },
    el("div", { class: "t-title" }, "🔔 " + task.title),
    task.notes ? el("div", { class: "t-body" }, task.notes.slice(0, 200)) : null,
    el("div", { class: "t-actions" },
      el("button", { class: "btn", onclick: () => { openTaskEditor(task.id); t.remove(); } }, "Открыть"),
      el("button", { class: "btn primary", onclick: async () => {
        await api(`/tasks/${task.id}/reminders/${reminder_id}/ack`, { method: "POST" });
        t.remove();
      }}, "Ок"),
    ),
  );
  box.appendChild(t);
  setTimeout(() => t.remove(), 30000);
}

// ---------- Modal helper ----------
function showModal(builder) {
  const shell = el("div", { class: "modal-shell", onclick: (e) => { if (e.target === shell) close(); } });
  const close = () => shell.remove();
  const modal = builder(close);
  shell.appendChild(modal);
  document.body.appendChild(shell);
}

// ---------- Boot ----------
(async function boot() {
  if (state.token) {
    try {
      await loadAll();
      connectWs();
      ensurePushSubscription();
      render();
      checkMissed();
      // pull missed every few minutes
      setInterval(checkMissed, 5 * 60 * 1000);
      return;
    } catch (e) {
      console.warn(e);
      logout();
    }
  }
  render();
})();
