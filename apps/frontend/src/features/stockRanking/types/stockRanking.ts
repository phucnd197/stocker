import type { components, operations } from '@stocker/api-contracts';

// Type exports from API contracts
export type RankingRequest =
  operations['StockerFeaturesStockStockRankingRankStocksEndpoint']['parameters']['query'];
export type RankingResponse =
  components['schemas']['StockerFeaturesStockStockRankingRankingResponse'];
export type Stock =
  components['schemas']['StockerFeaturesStockStockRankingStock'];

// Form values type (extends RankingRequest but with optional numberOfStocks for form)
export interface RankingFormValues {
  minimumMarketcap?: number | null;
  numberOfStocks: number;
}

// Default form values
export const DEFAULT_FORM_VALUES: RankingFormValues = {
  minimumMarketcap: null,
  numberOfStocks: 50,
};
