import { TextField, Button, Box, Alert } from '@mui/material';
import type { RankingFormValues } from '../types/stockRanking';

interface FieldErrors {
  minimumMarketcap?: string;
  numberOfStocks?: string;
}

interface RankingFilterFormProps {
  values: RankingFormValues;
  errors: FieldErrors;
  onChange: (field: keyof RankingFormValues, value: string) => void;
  onSubmit: () => void;
  isLoading?: boolean;
}

export function RankingFilterForm({
  values,
  errors,
  onChange,
  onSubmit,
  isLoading = false,
}: RankingFilterFormProps) {
  const handleFieldChange =
    (field: keyof RankingFormValues) =>
    (e: React.ChangeEvent<HTMLInputElement>) => {
      const value = e.target.value;
      onChange(field, value);
    };

  const isFormValid =
    values.numberOfStocks > 0 &&
    !errors.minimumMarketcap &&
    !errors.numberOfStocks;

  return (
    <Box sx={{ mb: 3 }}>
      <Box sx={{ display: 'flex', gap: 2, alignItems: 'flex-start' }}>
        <TextField
          label="Minimum Market Cap (Optional)"
          value={values.minimumMarketcap ?? ''}
          onChange={handleFieldChange('minimumMarketcap')}
          error={!!errors.minimumMarketcap}
          helperText={
            errors.minimumMarketcap || 'Filter by minimum market capitalization'
          }
          type="number"
          fullWidth
          slotProps={{ htmlInput: { min: 0, step: 1000000 } }}
          placeholder="e.g., 1000000"
        />
        <TextField
          label="Number of Stocks"
          value={values.numberOfStocks}
          onChange={handleFieldChange('numberOfStocks')}
          error={!!errors.numberOfStocks}
          helperText={
            errors.numberOfStocks || 'Number of top-ranked stocks to return'
          }
          type="number"
          sx={{ width: 200 }}
          slotProps={{ htmlInput: { min: 1, max: 500 } }}
          required
        />
        <Button
          variant="contained"
          onClick={onSubmit}
          disabled={!isFormValid || isLoading}
          sx={{ mt: 1, height: 56 }}
        >
          {isLoading ? 'Loading...' : 'Get Rankings'}
        </Button>
      </Box>
      {values.minimumMarketcap && values.minimumMarketcap > 0 && (
        <Alert severity="info" sx={{ mt: 2 }}>
          Showing stocks with market cap ≥ $
          {Number(values.minimumMarketcap).toLocaleString()}
        </Alert>
      )}
    </Box>
  );
}
