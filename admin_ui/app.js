// Admin SPA — управление пользователями и рабочими пространствами (WebDAV).
const API = "/api";
const state = {
  token: localStorage.getItem("admin_token") || "",
  me: null,
  users: [],
  workspaces: [],
  myWorkspaces: [],
  view: "workspaces",
};

async function api(path, opts = {}) {
  const headers = { "Content-Type": "application/json", ...(opts.headers || {}) };
  if (state.token) headers["Authorization"] = "Bearer " + state.token;
  const res = await fetch(API + path, { ...opts, headers });
  if (res.status === 401) { logout(false); throw new Error("Не авторизовано"); }
  const ct = res.headers.get("content-type") || "";
  const data = ct.includes("application/json") ? await res.json() : await res.text();
  if (!res.ok) throw new Error((data && data.detail) || ("HTTP " + res.status));
  return data;
}

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
  localStorage.setItem("admin_token", state.token);
}

async function register(payload) {
  const data = await api("/auth/register", { method: "POST", body: JSON.stringify(payload) });
  state.token = data.access_token;
  state.me = data.user;
  localStorage.setItem("admin_token", state.token);
}

function logout(reload = true) {
  state.token = ""; state.me = null;
  localStorage.removeItem("admin_token");
  if (reload) render();
}

async function loadAll() {
  state.me = await api("/auth/me");
  state.myWorkspaces = await api("/admin/my-workspaces");
  if (state.me.is_admin) {
    state.users = await api("/admin/users");
    state.workspaces = await api("/admin/workspaces");
  } else {
    state.users = []; state.workspaces = [];
  }
}

// ---------- utils ----------
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
const $ = (sel, root = document) => root.querySelector(sel);
const uname = (id) => (state.users.find(u => u.id === id) || {}).username || ("id=" + id);

function serverOrigin() { return location.origin; }
function webdavUrl(slug) { return serverOrigin() + "/webdav/" + slug + "/"; }

async function copy(text) {
  try { await navigator.clipboard.writeText(text); }
  catch(e) {
    const ta = document.createElement("textarea");
    ta.value = text; document.body.appendChild(ta); ta.select();
    document.execCommand("copy"); ta.remove();
  }
}

// ---------- render ----------
function render() {
  const root = $("#app");
  root.innerHTML = "";
  root.appendChild(state.token ? renderShell() : renderLogin());
}

function renderLogin() {
  const shell = el("div", { class: "login-shell" });
  const card = el("div", { class: "card login-card" });
  card.appendChild(el("div", { class: "brand" },
    el("div", { class: "brand-mark" }, "✓"),
    el("div", { class: "brand-name" }, "SP Team Sync")
  ));
  const tabs = el("div", { class: "tabs" });
  const tLogin = el("button", { class: "tab active" }, "Вход");
  const tReg = el("button", { class: "tab" }, "Регистрация");
  tabs.append(tLogin, tReg);
  card.appendChild(tabs);

  const errBox = el("div", { class: "err" });

  const loginForm = el("form", {},
    el("label", {}, "Логин", el("input", { name: "username", required: true, autocomplete: "username" })),
    el("label", {}, "Пароль", el("input", { name: "password", type: "password", required: true, autocomplete: "current-password" })),
    el("button", { class: "btn primary", type: "submit" }, "Войти"),
    el("div", { class: "hint" }, "Первый зарегистрированный станет администратором."),
  );
  loginForm.addEventListener("submit", async (e) => {
    e.preventDefault(); errBox.textContent = "";
    try {
      const d = Object.fromEntries(new FormData(loginForm));
      await login(d.username, d.password);
      await loadAll(); render();
    } catch (ex) { errBox.textContent = ex.message; }
  });

  const regForm = el("form", { class: "hidden" },
    el("label", {}, "Логин", el("input", { name: "username", required: true })),
    el("label", {}, "Отображаемое имя", el("input", { name: "display_name" })),
    el("label", {}, "Пароль (≥6 символов)", el("input", { name: "password", type: "password", required: true, minlength: "6" })),
    el("button", { class: "btn primary", type: "submit" }, "Зарегистрироваться"),
  );
  regForm.addEventListener("submit", async (e) => {
    e.preventDefault(); errBox.textContent = "";
    try {
      const d = Object.fromEntries(new FormData(regForm));
      await register({ username: d.username, password: d.password, display_name: d.display_name });
      await loadAll(); render();
    } catch (ex) { errBox.textContent = ex.message; }
  });

  card.append(loginForm, regForm, errBox);

  tLogin.addEventListener("click", () => {
    tLogin.classList.add("active"); tReg.classList.remove("active");
    loginForm.classList.remove("hidden"); regForm.classList.add("hidden"); errBox.textContent = "";
  });
  tReg.addEventListener("click", () => {
    tReg.classList.add("active"); tLogin.classList.remove("active");
    regForm.classList.remove("hidden"); loginForm.classList.add("hidden"); errBox.textContent = "";
  });

  shell.appendChild(card);
  return shell;
}

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
    el("div", { class: "brand-name" }, "SP Team Sync"),
  ));
  const nav = el("ul", { class: "nav-list" });
  const items = [
    { id: "workspaces", name: "Рабочие пространства" },
    { id: "my", name: "Мои пространства" },
  ];
  if (state.me && state.me.is_admin) items.push({ id: "users", name: "Пользователи" });
  items.push({ id: "howto", name: "Как настроить SP" });
  for (const it of items) {
    if (it.id === "workspaces" && !state.me.is_admin) continue;
    const item = el("li", {
      class: "nav-item" + (state.view === it.id ? " active" : ""),
      onclick: () => { state.view = it.id; render(); },
    }, it.name);
    nav.appendChild(item);
  }
  side.appendChild(nav);

  const me = state.me || {};
  const initial = (me.display_name || me.username || "?").charAt(0).toUpperCase();
  side.appendChild(el("div", { class: "sidebar-foot" },
    el("div", { class: "avatar" }, initial),
    el("div", { class: "user-info" },
      el("div", { class: "name" }, me.display_name || me.username),
      el("div", { class: "role" }, me.is_admin ? "администратор" : "пользователь"),
    ),
    el("button", { class: "btn ghost", title: "Открыть Super Productivity", onclick: () => window.open("/app/", "_blank") }, "→SP"),
    el("button", { class: "btn ghost", title: "Выйти", onclick: () => { logout(); } }, "⇥"),
  ));
  return side;
}

function renderMain() {
  const main = el("div", { class: "main" });
  main.appendChild(el("div", { class: "topbar" }, el("h1", {}, viewTitle())));
  const content = el("div", { class: "content" });
  main.appendChild(content);
  if (state.view === "workspaces") renderWorkspaces(content);
  else if (state.view === "my") renderMyWorkspaces(content);
  else if (state.view === "users") renderUsers(content);
  else if (state.view === "howto") renderHowto(content);
  return main;
}

function viewTitle() {
  return {
    workspaces: "Рабочие пространства",
    my: "Мои пространства",
    users: "Пользователи",
    howto: "Как подключить Super Productivity",
  }[state.view] || "";
}

// ---------- Workspaces (admin) ----------
function renderWorkspaces(root) {
  if (!state.me.is_admin) { root.textContent = "Только для администратора"; return; }
  const head = el("div", { class: "section-head" },
    el("h2", {}, "Все рабочие пространства"),
    el("div", { class: "spacer" }),
    el("button", { class: "btn primary", onclick: () => openWorkspaceEditor(null) }, "+ Создать"),
  );
  root.appendChild(head);
  const list = el("div", { class: "row-list" });
  for (const w of state.workspaces) list.appendChild(renderWorkspaceCard(w));
  if (!state.workspaces.length) list.appendChild(el("div", { class: "hint" }, "Пока нет пространств. Создайте первое."));
  root.appendChild(list);
}

function renderWorkspaceCard(w) {
  const membersTxt = (w.member_ids || []).map(uname).join(", ") || "—";
  return el("div", { class: "row-card" },
    el("div", {},
      el("div", { class: "title" }, w.name, " ", el("code", {}, w.slug)),
      el("div", { class: "meta" }, "Участники: " + membersTxt),
      el("div", { class: "meta" }, "URL: ", el("code", {}, webdavUrl(w.slug))),
    ),
    el("div", { class: "row-actions" },
      el("button", { class: "btn", onclick: () => showConnectInfo(w) }, "Как подключить"),
      el("button", { class: "btn", onclick: () => openWorkspaceEditor(w) }, "Изменить"),
      el("button", { class: "btn danger", onclick: async () => {
        if (!confirm("Удалить пространство? Файлы синхронизации на сервере останутся в data/webdav/" + w.slug + ".")) return;
        await api("/admin/workspaces/" + w.id, { method: "DELETE" });
        state.workspaces = state.workspaces.filter(x => x.id !== w.id);
        render();
      }}, "×"),
    ),
  );
}

function openWorkspaceEditor(w) {
  const draft = w ? { ...w, member_ids: [...(w.member_ids || [])] } :
    { slug: "", name: "", member_ids: [state.me.id] };
  showModal((close) => {
    const modal = el("div", { class: "modal" });
    modal.appendChild(el("div", { class: "modal-head" },
      el("h2", {}, w ? "Пространство" : "Новое пространство"),
      el("div", { class: "spacer" }),
      el("button", { class: "btn ghost", onclick: close }, "×"),
    ));
    const err = el("div", { class: "err" });
    const body = el("div", { class: "modal-body" });
    const slugI = el("input", { value: draft.slug, disabled: !!w, placeholder: "team-alpha" });
    slugI.addEventListener("input", () => draft.slug = slugI.value.trim().toLowerCase());
    body.appendChild(el("label", {}, "Slug (для URL)", slugI));
    const nameI = el("input", { value: draft.name, placeholder: "Команда Альфа" });
    nameI.addEventListener("input", () => draft.name = nameI.value);
    body.appendChild(el("label", {}, "Название", nameI));

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
      }, "@" + u.username);
      members.appendChild(chip);
    }
    body.appendChild(el("label", {}, "Участники (все они смогут синкать сюда через WebDAV)", members));
    body.appendChild(err);
    modal.appendChild(body);
    const foot = el("div", { class: "modal-foot" },
      el("button", { class: "btn", onclick: close }, "Отмена"),
      el("button", { class: "btn primary", onclick: async () => {
        err.textContent = "";
        try {
          if (w) {
            const upd = await api("/admin/workspaces/" + w.id, { method: "PUT", body: JSON.stringify(draft) });
            const idx = state.workspaces.findIndex(x => x.id === upd.id);
            if (idx >= 0) state.workspaces[idx] = upd;
          } else {
            const created = await api("/admin/workspaces", { method: "POST", body: JSON.stringify(draft) });
            state.workspaces.push(created);
          }
          render(); close();
        } catch (ex) { err.textContent = ex.message; }
      }}, w ? "Сохранить" : "Создать"),
    );
    modal.appendChild(foot);
    return modal;
  });
}

function showConnectInfo(w) {
  const url = webdavUrl(w.slug);
  showModal((close) => {
    const modal = el("div", { class: "modal" });
    modal.appendChild(el("div", { class: "modal-head" },
      el("h2", {}, "Подключение к «" + w.name + "»"),
      el("div", { class: "spacer" }),
      el("button", { class: "btn ghost", onclick: close }, "×"),
    ));
    const body = el("div", { class: "modal-body" });
    body.appendChild(el("div", { class: "instructions" },
      el("div", {}, "Откройте Super Productivity → Settings → Sync & Export/Import → Sync provider: ", el("code", {}, "WebDAV")),
      el("div", { class: "kv" },
        el("div", { class: "hint" }, "Base URL:"),
        el("div", { class: "copyable" }, el("code", {}, url),
          el("button", { class: "btn ghost", onclick: () => copy(url) }, "копировать")),
      ),
      el("div", { class: "kv" },
        el("div", { class: "hint" }, "Sync folder path:"),
        el("div", { class: "copyable" }, el("code", {}, "sp"),
          el("button", { class: "btn ghost", onclick: () => copy("sp") }, "копировать")),
      ),
      el("div", { class: "kv" },
        el("div", { class: "hint" }, "Username:"),
        el("div", { class: "hint" }, "ваш логин на этом сервере"),
      ),
      el("div", { class: "kv" },
        el("div", { class: "hint" }, "Password:"),
        el("div", { class: "hint" }, "ваш пароль на этом сервере"),
      ),
      el("p", { class: "hint" }, "Все участники подключаются к одному и тому же URL — задачи, проекты и заметки становятся общими."),
    ));
    modal.appendChild(body);
    modal.appendChild(el("div", { class: "modal-foot" },
      el("button", { class: "btn primary", onclick: () => { window.open("/app/", "_blank"); } }, "Открыть SP"),
      el("button", { class: "btn", onclick: close }, "Закрыть"),
    ));
    return modal;
  });
}

// ---------- My workspaces ----------
function renderMyWorkspaces(root) {
  const list = el("div", { class: "row-list" });
  for (const w of state.myWorkspaces) {
    list.appendChild(el("div", { class: "row-card" },
      el("div", {},
        el("div", { class: "title" }, w.name, " ", el("code", {}, w.slug)),
        el("div", { class: "meta" }, "WebDAV: ", el("code", {}, webdavUrl(w.slug))),
      ),
      el("div", { class: "row-actions" },
        el("button", { class: "btn", onclick: () => showConnectInfo(w) }, "Как подключить"),
      ),
    ));
  }
  if (!state.myWorkspaces.length) list.appendChild(el("div", { class: "hint" }, "Вас пока не добавили ни в одно пространство. Обратитесь к администратору."));
  root.appendChild(list);
}

// ---------- Users (admin) ----------
function renderUsers(root) {
  if (!state.me.is_admin) { root.textContent = "Только для администратора"; return; }
  const head = el("div", { class: "section-head" },
    el("h2", {}, "Пользователи"),
    el("div", { class: "spacer" }),
    el("button", { class: "btn primary", onclick: openUserEditor }, "+ Создать"),
  );
  root.appendChild(head);
  const list = el("div", { class: "row-list" });
  for (const u of state.users) list.appendChild(renderUserCard(u));
  root.appendChild(list);
}

function renderUserCard(u) {
  return el("div", { class: "row-card" },
    el("div", {},
      el("div", { class: "title" }, u.display_name || u.username, " ", el("code", {}, u.username),
        u.is_admin ? el("span", { class: "badge admin" }, "admin") : null,
      ),
    ),
    el("div", { class: "row-actions" },
      el("button", { class: "btn", onclick: () => openResetPassword(u) }, "Пароль"),
      u.id === state.me.id ? null : el("button", { class: "btn danger", onclick: async () => {
        if (!confirm("Удалить пользователя " + u.username + "?")) return;
        await api("/admin/users/" + u.id, { method: "DELETE" });
        state.users = state.users.filter(x => x.id !== u.id);
        render();
      }}, "×"),
    ),
  );
}

function openUserEditor() {
  const draft = { username: "", display_name: "", password: "", is_admin: false };
  showModal((close) => {
    const modal = el("div", { class: "modal" });
    modal.appendChild(el("div", { class: "modal-head" },
      el("h2", {}, "Новый пользователь"),
      el("div", { class: "spacer" }),
      el("button", { class: "btn ghost", onclick: close }, "×"),
    ));
    const err = el("div", { class: "err" });
    const body = el("div", { class: "modal-body" });
    const usernameI = el("input", {}); usernameI.addEventListener("input", () => draft.username = usernameI.value.trim());
    body.appendChild(el("label", {}, "Логин", usernameI));
    const displayI = el("input", {}); displayI.addEventListener("input", () => draft.display_name = displayI.value);
    body.appendChild(el("label", {}, "Отображаемое имя", displayI));
    const pwI = el("input", { type: "password", minlength: "6" }); pwI.addEventListener("input", () => draft.password = pwI.value);
    body.appendChild(el("label", {}, "Пароль", pwI));
    const adminC = el("input", { type: "checkbox" }); adminC.addEventListener("change", () => draft.is_admin = adminC.checked);
    const adminL = el("label", { style: "display:flex; flex-direction:row; align-items:center; gap:8px" }, adminC, "администратор");
    body.appendChild(adminL);
    body.appendChild(err);
    modal.appendChild(body);
    modal.appendChild(el("div", { class: "modal-foot" },
      el("button", { class: "btn", onclick: close }, "Отмена"),
      el("button", { class: "btn primary", onclick: async () => {
        err.textContent = "";
        try {
          const created = await api("/admin/users", { method: "POST", body: JSON.stringify(draft) });
          state.users.push(created); render(); close();
        } catch (ex) { err.textContent = ex.message; }
      }}, "Создать"),
    ));
    return modal;
  });
}

function openResetPassword(u) {
  showModal((close) => {
    const modal = el("div", { class: "modal" });
    modal.appendChild(el("div", { class: "modal-head" },
      el("h2", {}, "Новый пароль для " + u.username),
      el("div", { class: "spacer" }),
      el("button", { class: "btn ghost", onclick: close }, "×"),
    ));
    const err = el("div", { class: "err" });
    const body = el("div", { class: "modal-body" });
    const pwI = el("input", { type: "password", minlength: "6" });
    body.appendChild(el("label", {}, "Пароль", pwI));
    body.appendChild(err);
    modal.appendChild(body);
    modal.appendChild(el("div", { class: "modal-foot" },
      el("button", { class: "btn", onclick: close }, "Отмена"),
      el("button", { class: "btn primary", onclick: async () => {
        err.textContent = "";
        try {
          await api("/admin/users/" + u.id + "/password", { method: "POST", body: JSON.stringify({ password: pwI.value }) });
          close();
        } catch (ex) { err.textContent = ex.message; }
      }}, "Сменить"),
    ));
    return modal;
  });
}

// ---------- Howto ----------
function renderHowto(root) {
  root.appendChild(el("div", { class: "instructions" },
    el("h2", { style: "margin-top:0" }, "Как подключить Super Productivity к общему пространству"),
    el("ol", {},
      el("li", {}, "Откройте ", el("a", { href: "/app/", target: "_blank" }, "Super Productivity"), " (кнопка «→SP» слева внизу)."),
      el("li", {}, "В приложении: ", el("code", {}, "Settings → Sync & Export/Import"), "."),
      el("li", {}, "Sync provider: ", el("code", {}, "WebDAV"), "."),
      el("li", {}, "Base URL — адрес вашего пространства из раздела «Мои пространства» (например ", el("code", {}, webdavUrl("team-alpha")), ")."),
      el("li", {}, "Sync folder path: ", el("code", {}, "sp"), "."),
      el("li", {}, "Username / Password — ваш логин/пароль на этом сервере."),
      el("li", {}, "Нажмите ", el("code", {}, "Save & sync now"), "."),
    ),
    el("p", { class: "hint" }, "Синхронизация двусторонняя: все участники пространства видят одни и те же задачи, проекты, заметки, теги, тайминги. Конфликты одновременных правок разрешает сам SP по контенту (модель op-log)."),
  ));
}

// ---------- modal helper ----------
function showModal(builder) {
  const shell = el("div", { class: "modal-shell", onclick: (e) => { if (e.target === shell) close(); } });
  const close = () => shell.remove();
  shell.appendChild(builder(close));
  document.body.appendChild(shell);
}

// ---------- boot ----------
(async function boot() {
  if (state.token) {
    try { await loadAll(); render(); return; }
    catch (e) { logout(false); }
  }
  render();
})();
