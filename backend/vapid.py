"""VAPID key generation and loading for Web Push."""
import base64
from pathlib import Path

from cryptography.hazmat.primitives import serialization
from cryptography.hazmat.primitives.asymmetric import ec

from .config import settings


def _b64url(data: bytes) -> str:
    return base64.urlsafe_b64encode(data).rstrip(b"=").decode("ascii")


def ensure_vapid_keys() -> tuple[str, str, str]:
    """Гарантирует наличие VAPID-ключей на диске. Возвращает (priv_pem_path, pub_pem_path, pub_b64)."""
    priv_path = Path(settings.vapid_private_key_path)
    pub_path = Path(settings.vapid_public_key_path)
    pub_b64_path = Path(settings.vapid_public_key_b64_path)

    if not priv_path.exists() or not pub_path.exists() or not pub_b64_path.exists():
        priv = ec.generate_private_key(ec.SECP256R1())
        priv_path.parent.mkdir(parents=True, exist_ok=True)
        priv_path.write_bytes(
            priv.private_bytes(
                encoding=serialization.Encoding.PEM,
                format=serialization.PrivateFormat.PKCS8,
                encryption_algorithm=serialization.NoEncryption(),
            )
        )
        pub = priv.public_key()
        pub_path.write_bytes(
            pub.public_bytes(
                encoding=serialization.Encoding.PEM,
                format=serialization.PublicFormat.SubjectPublicKeyInfo,
            )
        )
        raw = pub.public_bytes(
            encoding=serialization.Encoding.X962,
            format=serialization.PublicFormat.UncompressedPoint,
        )
        pub_b64_path.write_text(_b64url(raw))

    return str(priv_path), str(pub_path), pub_b64_path.read_text().strip()


def public_key_b64() -> str:
    _, _, b64 = ensure_vapid_keys()
    return b64


def private_key_pem_path() -> str:
    p, _, _ = ensure_vapid_keys()
    return p
