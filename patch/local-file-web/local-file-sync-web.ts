import {
  LocalFileSyncBase,
  type LocalFileSyncBaseDeps,
} from '@sp/sync-providers/local-file';
import { OP_LOG_SYNC_LOGGER } from '../../../core/sync-logger.adapter';
import { SyncCredentialStore } from '../../credential-store.service';
import { SyncProviderId } from '../../provider.const';
import { createWebDirectoryFileAdapter } from './web-directory-file-adapter';
import {
  clearDirectoryHandle,
  hasWritePermission,
  loadDirectoryHandle,
  requestDirectoryPermission,
  saveDirectoryHandle,
} from './directory-handle-store';

/**
 * Провайдер синхронизации через папку, выбранную в браузере.
 *
 * В апстриме LocalFile собирается только для Electron и Android. Этот вариант
 * закрывает веб: File System Access API даёт постоянный доступ к папке на
 * диске, в том числе к сетевому каталогу, подключённому как диск. Благодаря
 * этому статический сайт синхронизируется через общий файл без сервера.
 *
 * Поддерживается в Chrome и Edge. В браузерах без File System Access API
 * провайдер не регистрируется — см. `IS_WEB_LOCAL_FILE_SYNC_SUPPORTED`.
 */

export const IS_WEB_LOCAL_FILE_SYNC_SUPPORTED =
  typeof window !== 'undefined' && 'showDirectoryPicker' in window;

export class LocalFileSyncWeb extends LocalFileSyncBase {
  private static readonly L = 'LocalFileSyncWeb';

  constructor(deps: LocalFileSyncBaseDeps) {
    super(deps);
  }

  /**
   * Готовы, только если папка выбрана И право на запись уже выдано.
   * После перезапуска браузера право нужно подтвердить кликом — это делает
   * `pickDirectory()`, вызываемый из настроек синхронизации.
   */
  async isReady(): Promise<boolean> {
    if (!IS_WEB_LOCAL_FILE_SYNC_SUPPORTED) return false;
    const handle = await loadDirectoryHandle().catch(() => null);
    if (!handle) return false;
    return hasWritePermission(handle);
  }

  /** Путь внутри выбранной папки; ведущий слэш убираем. */
  async getFilePath(targetPath: string): Promise<string> {
    return targetPath.startsWith('/') ? targetPath.substring(1) : targetPath;
  }

  /**
   * Открывает выбор папки. Если папка уже выбиралась и нужно лишь освежить
   * право доступа — сначала пробуем подтвердить его без повторного выбора,
   * чтобы пользователь не искал каталог заново после каждого перезапуска.
   */
  async pickDirectory(): Promise<string | void> {
    if (!IS_WEB_LOCAL_FILE_SYNC_SUPPORTED) {
      throw new Error('Браузер не поддерживает выбор папки. Используйте Chrome или Edge.');
    }

    const existing = await loadDirectoryHandle().catch(() => null);
    if (existing && (await requestDirectoryPermission(existing).catch(() => false))) {
      this.logger.normal(`${LocalFileSyncWeb.L}.pickDirectory(): доступ подтверждён`);
      return existing.name;
    }

    try {
      const handle = await (
        window as unknown as {
          showDirectoryPicker(opts?: {
            mode?: 'read' | 'readwrite';
            id?: string;
          }): Promise<FileSystemDirectoryHandle>;
        }
      ).showDirectoryPicker({ mode: 'readwrite', id: 'sp-sync-folder' });

      if (!(await requestDirectoryPermission(handle))) {
        throw new Error('Нет разрешения на запись в выбранную папку');
      }

      await saveDirectoryHandle(handle);
      this.logger.normal(`${LocalFileSyncWeb.L}.pickDirectory(): выбрана «${handle.name}»`);
      return handle.name;
    } catch (e) {
      // Пользователь закрыл диалог — это не ошибка.
      if ((e as DOMException)?.name === 'AbortError') return;
      throw e;
    }
  }

  /** Отвязывает папку — синхронизация остановится до нового выбора. */
  async forgetDirectory(): Promise<void> {
    await clearDirectoryHandle();
  }
}

const buildLocalFileSyncWebDeps = (): LocalFileSyncBaseDeps => ({
  logger: OP_LOG_SYNC_LOGGER,
  fileAdapter: createWebDirectoryFileAdapter(),
  credentialStore: new SyncCredentialStore(
    SyncProviderId.LocalFile,
  ) as LocalFileSyncBaseDeps['credentialStore'],
});

export const createLocalFileSyncWeb = (): LocalFileSyncWeb =>
  new LocalFileSyncWeb(buildLocalFileSyncWebDeps());
