// Service Worker для Web Push (работает при закрытой вкладке)

self.addEventListener("install", (e) => { self.skipWaiting(); });
self.addEventListener("activate", (e) => { e.waitUntil(self.clients.claim()); });

self.addEventListener("push", (event) => {
  let data = {};
  try { data = event.data ? event.data.json() : {}; } catch (e) { data = { title: "Напоминание" }; }
  const title = data.title || "Напоминание";
  const body = data.notes || "Пора приступать к задаче";
  const tag = "task-" + (data.task_id || Math.random());
  event.waitUntil(
    self.registration.showNotification("🔔 " + title, {
      body,
      tag,
      renotify: true,
      requireInteraction: true,
      data,
    })
  );
});

self.addEventListener("notificationclick", (event) => {
  event.notification.close();
  const url = "/#task-" + (event.notification.data && event.notification.data.task_id || "");
  event.waitUntil(
    self.clients.matchAll({ type: "window", includeUncontrolled: true }).then((wins) => {
      for (const w of wins) {
        if ("focus" in w) { w.focus(); return w.navigate ? w.navigate(url) : null; }
      }
      if (self.clients.openWindow) return self.clients.openWindow(url);
    })
  );
});
