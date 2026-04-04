import csv
import re
import sys
from abc import ABC, abstractmethod
from argparse import ArgumentParser
from pathlib import Path

import requests
from bs4 import BeautifulSoup

# Оператор А 95+ А 95 А 92 ДП Газ
# -1 означає що ціна не зазначена

URL = "https://index.minfin.com.ua/ua/markets/fuel/tm/"

COLUMN_KEYS = ("operator", "a95_plus", "a95", "a92", "dp", "gas")

RowDict = dict[str, str | float | int]


def _td_style_has_padding_3px(td) -> bool:
    style = td.get("style") or ""
    # Match padding:3px with optional spaces (e.g. "padding: 3px")
    return re.search(r"padding\s*:\s*3px", style, re.I) is not None


def _parse_cell_number(text: str) -> float | int:
    t = (text or "").strip()
    if not t:
        return -1
    normalized = t.replace(",", ".").replace("\xa0", "").replace(" ", "")
    try:
        return float(normalized)
    except ValueError:
        return -1


def scrape_fuel_prices(url: str) -> list[RowDict]:
    response = requests.get(url, timeout=30)
    response.raise_for_status()
    soup = BeautifulSoup(response.text, "html.parser")
    table = soup.find("table", class_=lambda c: c and "zebra" in c.split())
    if table is None:
        return []

    rows: list[RowDict] = []
    for tr in table.find_all("tr"):
        tds = tr.find_all("td", recursive=False)
        if not tds:
            continue
        cells = [td for td in tds if not _td_style_has_padding_3px(td)]
        if len(cells) != 6:
            continue
        operator = cells[0].get_text(strip=True)
        numbers = [_parse_cell_number(c.get_text()) for c in cells[1:]]
        rows.append(dict(zip(COLUMN_KEYS, (operator, *numbers), strict=True)))
    return rows


class FuelPriceWriter(ABC):
    """Serializes scraped fuel price rows to a file. Subclass for each format."""

    @property
    @abstractmethod
    def default_filename(self) -> str:
        ...

    @abstractmethod
    def write(self, rows: list[RowDict], path: Path) -> None:
        ...


class CsvFuelPriceWriter(FuelPriceWriter):
    default_filename = "fuel_prices.csv"

    def write(self, rows: list[RowDict], path: Path) -> None:
        path.parent.mkdir(parents=True, exist_ok=True)
        # utf-8-sig helps Excel on Windows recognize UTF-8
        with path.open("w", newline="", encoding="utf-8-sig") as f:
            w = csv.DictWriter(f, fieldnames=list(COLUMN_KEYS))
            w.writeheader()
            w.writerows(rows)


def get_fuel_price_writer(format_name: str) -> FuelPriceWriter:
    """Return a writer for the given format. Extend the registry when adding formats."""
    registry: dict[str, FuelPriceWriter] = {
        "csv": CsvFuelPriceWriter(),
    }
    key = format_name.lower().strip()
    if key not in registry:
        supported = ", ".join(sorted(registry))
        raise ValueError(f"Unknown output format {format_name!r}. Supported: {supported}")
    return registry[key]


def write_fuel_prices(
    rows: list[RowDict],
    output_dir: Path,
    *,
    format_name: str = "csv",
    filename: str | None = None,
) -> Path:
    writer = get_fuel_price_writer(format_name)
    name = filename or writer.default_filename
    path = output_dir.resolve() / name
    writer.write(rows, path)
    return path


if __name__ == "__main__":
    if hasattr(sys.stdout, "reconfigure"):
        try:
            sys.stdout.reconfigure(encoding="utf-8")
        except (OSError, ValueError):
            pass

    parser = ArgumentParser()
    parser.add_argument("--url", type=str, default=URL)
    parser.add_argument(
        "--output-dir",
        type=Path,
        default=Path.cwd(),
        help="Directory for the output file (default: current working directory)",
    )
    parser.add_argument(
        "--format",
        type=str,
        default="csv",
        help="Output format (default: csv). More formats can be registered later.",
    )
    args = parser.parse_args()
    data = scrape_fuel_prices(args.url)
    out_path = write_fuel_prices(data, args.output_dir, format_name=args.format)
    print(f"Wrote {len(data)} rows to {out_path}")
