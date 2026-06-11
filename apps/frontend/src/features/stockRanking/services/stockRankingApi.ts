import type { RankingRequest, RankingResponse } from '../types/stockRanking';

export function createStockRankingApi(
  fetcher: <T>(
    path: string,
    options?: RequestInit,
    defaultContentType?: boolean
  ) => Promise<T>
) {
  return {
    getRankings: (params: RankingRequest) => {
      // Build query string from params
      const queryParams = new URLSearchParams();

      if (
        params.minimumMarketcap !== null &&
        params.minimumMarketcap !== undefined
      ) {
        queryParams.append(
          'minimumMarketcap',
          params.minimumMarketcap.toString()
        );
      }
      queryParams.append('numberOfStocks', params.numberOfStocks.toString());

      const queryString = queryParams.toString();
      const url = `/api/stocks/ranking${queryString ? `?${queryString}` : ''}`;

      return fetcher<RankingResponse>(url, {
        method: 'GET',
      });
    },
  };
}
