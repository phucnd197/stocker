import { useQuery } from "@tanstack/react-query";
import { useApiFetcher } from "../../auth/hooks/useApiFetcher";
import { createStockRankingApi } from "../services/stockRankingApi";
import type { RankingRequest } from "../types/stockRanking";

export const STOCK_RANKING_QUERY_KEY = ["stockRanking"] as const;

export function useStockRanking(params: RankingRequest | null) {
  const fetcher = useApiFetcher();
  const api = createStockRankingApi(fetcher);

  return useQuery({
    queryKey: [...STOCK_RANKING_QUERY_KEY, params],
    queryFn: () => api.getRankings(params!),
    enabled: params !== null && params.numberOfStocks > 0,
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
}
