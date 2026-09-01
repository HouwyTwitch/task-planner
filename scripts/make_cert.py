"""Генерирует self-signed сертификат для локальной инфраструктуры без домена.

Использует IP хоста + `localhost` в SAN. Работает на Windows/Linux без openssl.

Использование:
    python scripts/make_cert.py                # автоопределение IP
    python scripts/make_cert.py 192.168.1.10 host.local
"""
from __future__ import annotations
import datetime
import ipaddress
import socket
import sys
from pathlib import Path

from cryptography import x509
from cryptography.x509.oid import NameOID
from cryptography.hazmat.primitives import hashes, serialization
from cryptography.hazmat.primitives.asymmetric import rsa


def local_ips() -> list[str]:
    ips: set[str] = {"127.0.0.1"}
    try:
        hn = socket.gethostname()
        for info in socket.getaddrinfo(hn, None):
            ip = info[4][0]
            if ":" not in ip:  # skip ipv6 for simplicity
                ips.add(ip)
    except Exception:
        pass
    return sorted(ips)


def main() -> int:
    data_dir = Path(__file__).resolve().parent.parent / "data"
    data_dir.mkdir(parents=True, exist_ok=True)
    cert_path = data_dir / "cert.pem"
    key_path = data_dir / "key.pem"

    extra_names = sys.argv[1:]

    ip_names = local_ips()
    dns_names = ["localhost"] + [n for n in extra_names if not _is_ip(n)]
    ip_names += [n for n in extra_names if _is_ip(n)]

    key = rsa.generate_private_key(public_exponent=65537, key_size=2048)
    subject = issuer = x509.Name([
        x509.NameAttribute(NameOID.COMMON_NAME, "task-planner-local"),
        x509.NameAttribute(NameOID.ORGANIZATION_NAME, "Local Org"),
    ])
    san_entries = [x509.DNSName(n) for n in dns_names] + [x509.IPAddress(ipaddress.ip_address(ip)) for ip in ip_names]

    cert = (
        x509.CertificateBuilder()
        .subject_name(subject)
        .issuer_name(issuer)
        .public_key(key.public_key())
        .serial_number(x509.random_serial_number())
        .not_valid_before(datetime.datetime.utcnow() - datetime.timedelta(days=1))
        .not_valid_after(datetime.datetime.utcnow() + datetime.timedelta(days=365 * 5))
        .add_extension(x509.SubjectAlternativeName(san_entries), critical=False)
        .add_extension(x509.BasicConstraints(ca=True, path_length=None), critical=True)
        .sign(private_key=key, algorithm=hashes.SHA256())
    )

    key_path.write_bytes(
        key.private_bytes(
            encoding=serialization.Encoding.PEM,
            format=serialization.PrivateFormat.TraditionalOpenSSL,
            encryption_algorithm=serialization.NoEncryption(),
        )
    )
    cert_path.write_bytes(cert.public_bytes(serialization.Encoding.PEM))

    print(f"[OK] Сертификат:  {cert_path}")
    print(f"[OK] Ключ:        {key_path}")
    print(f"[INFO] DNS: {', '.join(dns_names)}")
    print(f"[INFO] IP:  {', '.join(ip_names)}")
    print()
    print("Установите cert.pem в 'Доверенные корневые центры сертификации' на всех клиентах,")
    print("чтобы браузер разрешил Web Push (иначе SW не зарегистрируется).")
    return 0


def _is_ip(s: str) -> bool:
    try:
        ipaddress.ip_address(s); return True
    except ValueError:
        return False


if __name__ == "__main__":
    raise SystemExit(main())
