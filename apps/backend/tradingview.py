# import requests
import csv
import typing

import grequests


class CompanyRank(dict[str, typing.Any]):
    def __init__(self, rank: int, company_data: dict[str, typing.Any]) -> None:
        self.rank = rank
        self.company_data = company_data
        dict.__init__(self, rank=rank, company_data=company_data)  # type: ignore


def get_total_count() -> int:
    url = "https://scanner.tradingview.com/vietnam/scan?label-product=markets-screener"
    headers = {
        "accept": "application/json",
        "content-type": "application/json",
    }
    data: dict[str, typing.Any] = {
        "columns": [],
        "range": [0, 1],
        "preset": "all_stocks",
    }

    req = grequests.map(requests=[grequests.post(url, headers=headers, json=data)])[0]
    if req.status_code == 200:
        return req.json()["totalCount"]
    return 0


pe_columns = [
    "name",
    "description",
    "close",
    "change",
    "volume",
    "relative_volume_10d_calc",
    "market_cap_basic",
    "fundamental_currency_code",
    "price_earnings_ttm",
    "earnings_per_share_diluted_ttm",
    "earnings_per_share_diluted_yoy_growth_ttm",
    "dividends_yield_current",
    "sector.tr",
    "market",
    "sector",
]


def get_stock_sorted_by_pe(max: int) -> grequests.AsyncRequest:
    url = "https://scanner.tradingview.com/vietnam/scan?label-product=markets-screener"
    headers = {
        "accept": "application/json",
        "content-type": "application/json",
    }
    columns = pe_columns
    data: dict[str, typing.Any] = {
        "columns": columns,
        "ignore_unknown_fields": False,
        "options": {"lang": "en"},
        "range": [0, max],
        "sort": {"sortBy": "price_earnings_ttm", "sortOrder": "asc"},
        "preset": "all_stocks",
    }
    return grequests.post(url, json=data, headers=headers)


roa_columns = [
    "name",
    "description",
    "gross_margin_ttm",
    "operating_margin_ttm",
    "pre_tax_margin_ttm",
    "net_margin_ttm",
    "free_cash_flow_margin_ttm",
    "return_on_assets_fq",
    "return_on_equity_fq",
    "return_on_invested_capital_fq",
    "research_and_dev_ratio_ttm",
]


def get_stock_sort_by_roa(max: int) -> grequests.AsyncRequest:
    url = "https://scanner.tradingview.com/vietnam/scan?label-product=markets-screener"
    headers = {
        "accept": "application/json",
        "content-type": "application/json",
    }
    columns = roa_columns
    data: dict[str, typing.Any] = {
        "columns": columns,
        "ignore_unknown_fields": False,
        "options": {"lang": "en"},
        "range": [0, max],
        "sort": {"sortBy": "return_on_assets_fq", "sortOrder": "desc"},
        "preset": "all_stocks",
    }
    return grequests.post(url, json=data, headers=headers)


def extract_rank(res: typing.Any, columns: list[str]):
    ranking: dict[str, CompanyRank] = {}

    if res == None or res.status_code != 200:
        return ranking

    json_raw = res.json()
    data = json_raw["data"]
    for i in range(len(data)):
        record_data = data[i]["d"]
        company_data = {"stock_exchange": data[i]["s"].split(":")[0]}
        for j in range(len(columns)):
            company_data[columns[j]] = record_data[j]
        ranking[company_data["name"]] = CompanyRank(i, company_data)

    return ranking


max = get_total_count()
[pe_res, roa_res] = grequests.map(
    requests=[get_stock_sorted_by_pe(max), get_stock_sort_by_roa(max)]
)  # type: ignore
pe_rank = extract_rank(pe_res, pe_columns)
roa_rank = extract_rank(roa_res, roa_columns)


final_rank: dict[str, dict[str, typing.Any]] = {}
for company_name in pe_rank:
    combined_rank = pe_rank[company_name].rank + roa_rank[company_name].rank
    company_data = {}

    for key in pe_rank[company_name].company_data:
        company_data[key] = pe_rank[company_name].company_data[key]
    for key in roa_rank[company_name].company_data:
        company_data[key] = roa_rank[company_name].company_data[key]

    company_data["combined_rank"] = combined_rank
    company_data["pe_rank"] = pe_rank[company_name].rank
    company_data["roa_rank"] = roa_rank[company_name].rank

    final_rank[company_name] = company_data

missing_cap: list[dict[str, typing.Any]] = []
final_rank_sorted: list[dict[str, typing.Any]] = []
for k, v in sorted(final_rank.items(), key=lambda item: item[1]["combined_rank"]):
    market_cap: int | None = pe_rank[k].company_data["market_cap_basic"]

    # Only get company with more than 1T VND in market cap
    if market_cap == None:
        missing_cap.append(v)
        continue

    final_rank_sorted.append(v)


final_columns = list(
    map(lambda key: key.replace("_", " "), list(final_rank_sorted[0].keys()))
)

with open("trading_view_ranking.csv", "w+", newline="") as out_file:
    writer = csv.writer(
        out_file, quotechar="|", quoting=csv.QUOTE_MINIMAL, lineterminator="\n"
    )

    writer.writerow(final_columns)
    for record in final_rank_sorted:
        values: typing.Any = []
        for key in record:
            values.append(record[key])
        writer.writerow(values)


with open("trading_view_ranking_missing_cap.csv", "w+", newline="") as out_file:
    writer = csv.writer(
        out_file, quotechar="|", quoting=csv.QUOTE_MINIMAL, lineterminator="\n"
    )

    writer.writerow(final_columns)
    for record in missing_cap:
        values: typing.Any = []
        for key in record:
            values.append(record[key])
        writer.writerow(values)
