import { useState } from 'react';
import { Box, Typography, Paper, Alert, Snackbar } from '@mui/material';
import { z } from 'zod';
import type { RankingFormValues } from '../types/stockRanking';
import { DEFAULT_FORM_VALUES } from '../types/stockRanking';
import { useStockRanking } from '../hooks/useStockRanking';
import { RankingFilterForm } from './RankingFilterForm';
import { RankedStocksTable } from './RankedStocksTable';

interface FieldErrors {
  minimumMarketcap?: string;
  numberOfStocks?: string;
}

// Zod schema for form validation
const rankingFormSchema = z.object({
  minimumMarketcap: z.number().nullable().optional(),
  numberOfStocks: z
    .number()
    .min(1, 'Must be at least 1')
    .max(500, 'Cannot exceed 500'),
});

export function StockRankingPage() {
  const [values, setValues] = useState<RankingFormValues>(DEFAULT_FORM_VALUES);
  const [fieldErrors, setFieldErrors] = useState<FieldErrors>({});
  const [submittedParams, setSubmittedParams] =
    useState<RankingFormValues | null>(null);
  const [successOpen, setSuccessOpen] = useState(false);

  const { data, isLoading, isError, error } = useStockRanking(submittedParams);

  function handleFieldChange(field: keyof RankingFormValues, value: string) {
    setValues(prev => ({
      ...prev,
      [field]:
        field === 'minimumMarketcap' && value === ''
          ? null
          : field === 'minimumMarketcap' || field === 'numberOfStocks'
            ? parseFloat(value) || 0
            : value,
    }));
    setFieldErrors(prev => ({ ...prev, [field]: undefined }));
  }

  function handleSubmit() {
    setFieldErrors({});

    // Validate using Zod
    const result = rankingFormSchema.safeParse(values);
    if (!result.success) {
      const errors: FieldErrors = {};
      for (const issue of result.error.issues) {
        const field = issue.path[0] as keyof FieldErrors;
        errors[field] = issue.message;
      }
      setFieldErrors(errors);
      return;
    }

    // Submit the query
    setSubmittedParams(result.data);
    setSuccessOpen(true);
  }

  return (
    <Box sx={{ maxWidth: 1400, mx: 'auto', py: 4, px: 2 }}>
      <Typography variant="h4" gutterBottom>
        Stock Rankings
      </Typography>

      <Typography variant="body1" color="text.secondary" sx={{ mb: 2 }}>
        Analyze and rank stocks based on PE ratio and ROIC metrics. Use the
        filters below to customize your rankings.
      </Typography>

      <Paper sx={{ p: 3, mt: 3 }}>
        <Typography variant="h6" gutterBottom>
          Filters
        </Typography>

        <RankingFilterForm
          values={values}
          errors={fieldErrors}
          onChange={handleFieldChange}
          onSubmit={handleSubmit}
          isLoading={isLoading}
        />
      </Paper>

      {isError && (
        <Alert severity="error" sx={{ mt: 3 }}>
          {error instanceof Error
            ? error.message
            : 'Failed to load stock rankings. Please try again.'}
        </Alert>
      )}

      {data && (
        <Box sx={{ mt: 3 }}>
          <RankedStocksTable
            stocks={data.rankedStocks ?? []}
            totalRanked={data.totalRanked ?? 0}
            totalMissingCap={data.totalMissingCap ?? 0}
            isLoading={isLoading}
          />
        </Box>
      )}

      <Snackbar
        open={successOpen}
        autoHideDuration={3000}
        onClose={() => setSuccessOpen(false)}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      >
        <Alert severity="info" onClose={() => setSuccessOpen(false)}>
          Rankings updated successfully
        </Alert>
      </Snackbar>
    </Box>
  );
}
