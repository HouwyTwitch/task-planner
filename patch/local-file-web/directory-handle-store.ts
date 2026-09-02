/**
 * Хранит дескриптор выбранной папки синхронизации в IndexedDB.
 *
 * File System Access API отдаёт объект-дескриптор, который переживает
 * перезагрузку страницы, если его сохранить в IndexedDB (в localStorage он не
 * сериализуется). После перезапуска браузер может потребовать подтвердить
 * доступ — для этого нужен жест пользователя, см. `requestDirectoryPermission`.
 */

const DB_NAME = 'sp-local-file-web';
const STORE_NAME = 'handles';
const HANDLE_KEY = 'syncFolder';

const openDb = (): Promise<IDBDatabase> =>
  new Promise((resolve, reject) => {
    const request = indexedDB.open(DB_NAME, 1);
    request.onupgradeneeded = () => {
      if (!request.result.objectStoreNames.contains(STORE_NAME)) {
        request.result.createObjectStore(STORE_NAME);
      }
    };
    request.onsuccess = () => resolve(request.result);
    request.onerror = () => reject(request.error);
  });

export const saveDirectoryHandle = async (
  handle: FileSystemDirectoryHandle,
): Promise<void> => {
  const db = await openDb();
  await new Promise<void>((resolve, reject) => {
    const tx = db.transaction(STORE_NAME, 'readwrite');
    tx.objectStore(STORE_NAME).put(handle, HANDLE_KEY);
    tx.oncomplete = () => resolve();
    tx.onerror = () => reject(tx.error);
  });
  db.close();
};

export const loadDirectoryHandle =
  async (): Promise<FileSystemDirectoryHandle | null> => {
    const db = await openDb();
    const handle = await new Promise<FileSystemDirectoryHandle | null>(
      (resolve, reject) => {
        const tx = db.transaction(STORE_NAME, 'readonly');
        const request = tx.objectStore(STORE_NAME).get(HANDLE_KEY);
        request.onsuccess = () => resolve(request.result ?? null);
        request.onerror = () => reject(request.error);
      },
    );
    db.close();
    return handle;
  };

export const clearDirectoryHandle = async (): Promise<void> => {
  const db = await openDb();
  await new Promise<void>((resolve) => {
    const tx = db.transaction(STORE_NAME, 'readwrite');
    tx.objectStore(STORE_NAME).delete(HANDLE_KEY);
    tx.oncomplete = () => resolve();
    tx.onerror = () => resolve();
  });
  db.close();
};

/**
 * Методы разрешений File System Access API отсутствуют в стандартных типах
 * lib.dom, поэтому описываем их сами вместо подключения отдельного пакета
 * типов ради двух сигнатур.
 */
type ЗапросРазрешения = { mode?: 'read' | 'readwrite' };

type ДескрипторСРазрешениями = FileSystemDirectoryHandle & {
  queryPermission(opts?: ЗапросРазрешения): Promise<PermissionState>;
  requestPermission(opts?: ЗапросРазрешения): Promise<PermissionState>;
};

const сРазрешениями = (handle: FileSystemDirectoryHandle): ДескрипторСРазрешениями =>
  handle as ДескрипторСРазрешениями;

/** Есть ли уже выданное право на запись (без запроса — не требует жеста). */
export const hasWritePermission = async (
  handle: FileSystemDirectoryHandle,
): Promise<boolean> =>
  (await сРазрешениями(handle).queryPermission({ mode: 'readwrite' })) === 'granted';

/** Запрашивает право на запись. Требует жеста пользователя (клика). */
export const requestDirectoryPermission = async (
  handle: FileSystemDirectoryHandle,
): Promise<boolean> =>
  (await сРазрешениями(handle).requestPermission({ mode: 'readwrite' })) === 'granted';
