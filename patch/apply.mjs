#!/usr/bin/env node
/**
 * Вживляет наши изменения в свежий клон Super Productivity.
 *
 * Делает три вещи:
 *  1. кладёт веб-провайдер синхронизации через папку (в апстриме его нет);
 *  2. регистрирует провайдер в фабрике;
 *  3. встраивает плагин «Передача задач» как встроенный.
 *
 * Скрипт идемпотентный: повторный запуск ничего не ломает.
 */

import { cp, mkdir, readFile, writeFile } from 'node:fs/promises';
import { existsSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const КОРЕНЬ = join(dirname(fileURLToPath(import.meta.url)), '..');
const SP = join(КОРЕНЬ, 'sp-src');

const ЗЕЛЁНЫЙ = (т) => `\x1b[32m${т}\x1b[0m`;
const ЖЁЛТЫЙ = (т) => `\x1b[33m${т}\x1b[0m`;

const шаг = (текст) => console.log(`  ${текст}`);
const готово = (текст) => console.log(`  ${ЗЕЛЁНЫЙ('✓')} ${текст}`);
const пропуск = (текст) => console.log(`  ${ЖЁЛТЫЙ('•')} ${текст} — уже сделано`);

/** Заменяет в файле, только если замены ещё нет. */
async function вставитьОдинРаз(путь, признак, найти, заменить, описание) {
  const текст = await readFile(путь, 'utf8');
  if (текст.includes(признак)) {
    пропуск(описание);
    return false;
  }
  if (!текст.includes(найти)) {
    throw new Error(
      `Не нашли точку вставки в ${путь}.\n` +
      `Ожидали фрагмент:\n${найти}\n` +
      `Скорее всего апстрим изменился — патч нужно обновить.`,
    );
  }
  await writeFile(путь, текст.replace(найти, заменить), 'utf8');
  готово(описание);
  return true;
}

async function main() {
  if (!existsSync(SP)) {
    throw new Error(`Нет папки sp-src. Сначала склонируйте Super Productivity:\n` +
      `  git clone --depth 1 https://github.com/johannesjo/super-productivity.git sp-src`);
  }

  console.log('\nВживляем изменения в Super Productivity\n');

  // ---- 1. Провайдер синхронизации через папку ----
  const кудаПровайдер = join(
    SP, 'src/app/op-log/sync-providers/file-based/local-file',
  );
  await mkdir(кудаПровайдер, { recursive: true });
  for (const файл of [
    'local-file-sync-web.ts',
    'web-directory-file-adapter.ts',
    'directory-handle-store.ts',
  ]) {
    await cp(join(КОРЕНЬ, 'patch/local-file-web', файл), join(кудаПровайдер, файл));
  }
  готово('Веб-провайдер синхронизации скопирован');

  // ---- 2. Регистрация провайдера в фабрике ----
  const фабрика = join(SP, 'src/app/op-log/sync-providers/sync-providers.factory.ts');
  await вставитьОдинРаз(
    фабрика,
    'createLocalFileSyncWeb',
    `  if (IS_ANDROID_WEB_VIEW) {`,
    `  // Веб: синхронизация через папку, выбранную File System Access API.
  // Даёт работу с общим файлом в сетевом каталоге без сервера.
  if (!IS_ELECTRON && !IS_ANDROID_WEB_VIEW) {
    const { createLocalFileSyncWeb, IS_WEB_LOCAL_FILE_SYNC_SUPPORTED } =
      await import('./file-based/local-file/local-file-sync-web');
    if (IS_WEB_LOCAL_FILE_SYNC_SUPPORTED) {
      providers.push(createLocalFileSyncWeb() as SyncProviderBase<SyncProviderId>);
    }
  }

  if (IS_ANDROID_WEB_VIEW) {`,
    'Провайдер зарегистрирован в фабрике',
  );

  // ---- 3. Плагин «Передача задач» ----
  const кудаПлагин = join(SP, 'src/assets/bundled-plugins/task-handoff');
  await mkdir(кудаПлагин, { recursive: true });
  await cp(join(КОРЕНЬ, 'plugin'), кудаПлагин, { recursive: true });
  готово('Плагин скопирован в assets/bundled-plugins/task-handoff');

  const сервисПлагинов = join(SP, 'src/app/plugins/plugin.service.ts');
  await вставитьОдинРаз(
    сервисПлагинов,
    "'assets/bundled-plugins/task-handoff'",
    `const BUNDLED_PLUGIN_PATHS = [`,
    `const BUNDLED_PLUGIN_PATHS = [\n  'assets/bundled-plugins/task-handoff',`,
    'Плагин добавлен в список встроенных',
  );
  await вставитьОдинРаз(
    сервисПлагинов,
    `'task-handoff',`,
    `  'todoist-import',`,
    `  'task-handoff',\n  'todoist-import',`,
    'Плагин добавлен в реестр идентификаторов',
  );

  console.log(`\n${ЗЕЛЁНЫЙ('Готово.')} Теперь можно собирать: npm run buildFrontend:prodWeb\n`);
}

main().catch((ошибка) => {
  console.error(`\n\x1b[31mОшибка:\x1b[0m ${ошибка.message}\n`);
  process.exit(1);
});
