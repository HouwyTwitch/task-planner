#!/usr/bin/env node
/**
 * Крошечный статический сервер для папки site/.
 *
 * Нужен по одной причине: File System Access API (доступ к общей папке)
 * работает только в защищённом контексте — это HTTPS либо localhost.
 * Открытый двойным кликом файл (file://) такого контекста не даёт, поэтому
 * сайт отдаём с 127.0.0.1.
 */

import { createServer } from 'node:http';
import { createReadStream } from 'node:fs';
import { stat } from 'node:fs/promises';
import { extname, join, normalize, resolve } from 'node:path';
import { dirname } from 'node:path';
import { fileURLToPath } from 'node:url';

const КОРЕНЬ = resolve(join(dirname(fileURLToPath(import.meta.url)), '..', 'site'));
const ПОРТ = Number(process.env.PORT || 4321);

const ТИПЫ = {
  '.html': 'text/html; charset=utf-8',
  '.js': 'text/javascript; charset=utf-8',
  '.mjs': 'text/javascript; charset=utf-8',
  '.css': 'text/css; charset=utf-8',
  '.json': 'application/json; charset=utf-8',
  '.svg': 'image/svg+xml',
  '.png': 'image/png',
  '.jpg': 'image/jpeg',
  '.webp': 'image/webp',
  '.ico': 'image/x-icon',
  '.woff': 'font/woff',
  '.woff2': 'font/woff2',
  '.ttf': 'font/ttf',
  '.webmanifest': 'application/manifest+json',
};

const отдать = (ответ, путь) => {
  ответ.writeHead(200, {
    'Content-Type': ТИПЫ[extname(путь).toLowerCase()] || 'application/octet-stream',
  });
  createReadStream(путь).pipe(ответ);
};

const сервер = createServer(async (запрос, ответ) => {
  try {
    const путьЗапроса = decodeURIComponent((запрос.url || '/').split('?')[0]);
    // normalize + проверка префикса не дают выйти за пределы site/
    const цель = resolve(join(КОРЕНЬ, normalize(путьЗапроса)));
    if (!цель.startsWith(КОРЕНЬ)) {
      ответ.writeHead(403).end('Доступ запрещён');
      return;
    }

    const сведения = await stat(цель).catch(() => null);
    if (сведения?.isFile()) return отдать(ответ, цель);
    if (сведения?.isDirectory()) {
      const индекс = join(цель, 'index.html');
      if ((await stat(индекс).catch(() => null))?.isFile()) return отдать(ответ, индекс);
    }

    // Одностраничное приложение: любой неизвестный путь отдаём index.html
    const индекс = join(КОРЕНЬ, 'index.html');
    if ((await stat(индекс).catch(() => null))?.isFile()) return отдать(ответ, индекс);

    ответ.writeHead(404).end('Не найдено');
  } catch (ошибка) {
    ответ.writeHead(500).end('Ошибка сервера: ' + ошибка.message);
  }
});

const сведенияКорня = await stat(КОРЕНЬ).catch(() => null);
if (!сведенияКорня?.isDirectory()) {
  console.error(`\nНет папки site/. Сначала соберите сайт: scripts/build.sh (или build.bat)\n`);
  process.exit(1);
}

сервер.listen(ПОРТ, '127.0.0.1', () => {
  console.log(`\n  Общие задачи запущены: http://localhost:${ПОРТ}\n`);
  console.log(`  Остановить — Ctrl+C\n`);
});
