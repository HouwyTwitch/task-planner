from pathlib import Path
from pydantic_settings import BaseSettings, SettingsConfigDict


BASE_DIR = Path(__file__).resolve().parent.parent


class Settings(BaseSettings):
    model_config = SettingsConfigDict(env_file=str(BASE_DIR / ".env"), extra="ignore")

    app_name: str = "Task Planner"
    secret_key: str = "change-me-in-production-please-32-chars-min"
    algorithm: str = "HS256"
    access_token_ttl_minutes: int = 60 * 24 * 7  # неделя

    database_url: str = f"sqlite:///{BASE_DIR / 'data' / 'app.db'}"

    vapid_private_key_path: str = str(BASE_DIR / "data" / "vapid_private.pem")
    vapid_public_key_path: str = str(BASE_DIR / "data" / "vapid_public.pem")
    vapid_public_key_b64_path: str = str(BASE_DIR / "data" / "vapid_public.b64")
    vapid_subject: str = "mailto:admin@localhost"

    scheduler_tick_seconds: int = 20


settings = Settings()
(BASE_DIR / "data").mkdir(parents=True, exist_ok=True)
