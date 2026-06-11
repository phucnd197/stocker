import type { components, operations } from '@stocker/api-contracts';

// Type exports from API contracts
export type RankingRequest =
  operations['StockerFeaturesStockStockRankingRankStocksEndpoint']['parameters']['query'];
export type RankingResponse =
  components['schemas']['StockerFeaturesStockStockRankingRankingResponse'];
export type Stock =
  components['schemas']['StockerFeaturesStockStockRankingCompanyData'];

// Form values type (extends RankingRequest but with optional numberOfStocks for form)
export interface RankingFormValues {
  minimumMarketcap?: number | null;
  numberOfStocks: number;
  refresh: boolean;
}

// Default form values
export const DEFAULT_FORM_VALUES: RankingFormValues = {
  minimumMarketcap: null,
  numberOfStocks: 50,
  refresh: false,
};
