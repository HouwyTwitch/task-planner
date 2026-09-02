import type { FileAdapter } from '@sp/sync-providers';
import { loadDirectoryHandle } from './directory-handle-store';

/**
 * Реализация FileAdapter поверх File System Access API.
 *
 * Все пути — относительные внутри выбранной пользователем папки. Папка может
 * лежать в сетевом каталоге, подключённом как диск (например Z:\Общие\задачи),
 * тогда синхронизация идёт прямо через общий файл без какого-либо сервера.
 */

const NOT_FOUND = 'NotFoundError';

const stripLeadingSlash = (p: string): string => (p.startsWith('/') ? p.slice(1) : p);

const splitPath = (filePath: string): { dirs: string[]; name: string } => {
  const parts = stripLeadingSlash(filePath).split('/').filter(Boolean);
  const name = parts.pop() ?? '';
  return { dirs: parts, name };
};

const requireRoot = async (): Promise<FileSystemDirectoryHandle> => {
  const root = await loadDirectoryHandle();
  if (!root) {
    throw new Error('Папка синхронизации не выбрана');
  }
  return root;
};

/** Спускается по вложенным папкам; при create=true создаёт недостающие. */
const resolveDir = async (
  root: FileSystemDirectoryHandle,
  dirs: string[],
  create: boolean,
): Promise<FileSystemDirectoryHandle> => {
  let current = root;
  for (const dir of dirs) {
    current = await current.getDirectoryHandle(dir, { create });
  }
  return current;
};

export const createWebDirectoryFileAdapter = (): FileAdapter => ({
  async readFile(filePath: string): Promise<string> {
    const { dirs, name } = splitPath(filePath);
    const root = await requireRoot();
    const dir = await resolveDir(root, dirs, false);
    const fileHandle = await dir.getFileHandle(name);
    const file = await fileHandle.getFile();
    return file.text();
  },

  async writeFile(filePath: string, dataStr: string): Promise<void> {
    const { dirs, name } = splitPath(filePath);
    const root = await requireRoot();
    const dir = await resolveDir(root, dirs, true);
    const fileHandle = await dir.getFileHandle(name, { create: true });
    // createWritable пишет во временный файл и заменяет целевой при close(),
    // поэтому оборванная запись не оставит наполовину записанный файл.
    const writable = await fileHandle.createWritable();
    try {
      await writable.write(dataStr);
      await writable.close();
    } catch (e) {
      await writable.abort().catch(() => undefined);
      throw e;
    }
  },

  async deleteFile(filePath: string): Promise<void> {
    const { dirs, name } = splitPath(filePath);
    const root = await requireRoot();
    try {
      const dir = await resolveDir(root, dirs, false);
      await dir.removeEntry(name);
    } catch (e) {
      // Удаление отсутствующего файла — не ошибка для вызывающей стороны.
      if ((e as DOMException)?.name !== NOT_FOUND) throw e;
    }
  },

  async checkDirExists(dirPath: string): Promise<boolean> {
    const parts = stripLeadingSlash(dirPath).split('/').filter(Boolean);
    try {
      const root = await requireRoot();
      await resolveDir(root, parts, false);
      return true;
    } catch (e) {
      if ((e as DOMException)?.name === NOT_FOUND) return false;
      throw e;
    }
  },

  async listFiles(dirPath: string): Promise<string[]> {
    const parts = stripLeadingSlash(dirPath).split('/').filter(Boolean);
    const root = await requireRoot();
    const dir = await resolveDir(root, parts, false);
    const names: string[] = [];
    for await (const [name, handle] of (
      dir as unknown as {
        entries(): AsyncIterableIterator<[string, FileSystemHandle]>;
      }
    ).entries()) {
      if (handle.kind === 'file') names.push(name);
    }
    return names;
  },
});
