from pathlib import Path
from pydantic_settings import BaseSettings, SettingsConfigDict


BASE_DIR = Path(__file__).resolve().parent.parent


class Settings(BaseSettings):
    model_config = SettingsConfigDict(env_file=str(BASE_DIR / ".env"), extra="ignore")

    app_name: str = "SP Team Sync"
    secret_key: str = "change-me-in-production-please-32-chars-min"
    algorithm: str = "HS256"
    access_token_ttl_minutes: int = 60 * 24 * 7  # неделя

    database_url: str = f"sqlite:///{BASE_DIR / 'data' / 'app.db'}"
    webdav_root: str = str(BASE_DIR / "data" / "webdav")


settings = Settings()
(BASE_DIR / "data").mkdir(parents=True, exist_ok=True)
Path(settings.webdav_root).mkdir(parents=True, exist_ok=True)
