import React from "react";
import {
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TableSortLabel,
  Paper,
  Box,
  Typography,
  Chip,
  CircularProgress,
  Alert,
  Tooltip,
} from "@mui/material";
import type { Stock } from "../types/stockRanking";

type SortKey = keyof Stock | "combinedRank";

interface SortConfig {
  key: SortKey;
  direction: "asc" | "desc";
}

interface RankedStocksTableProps {
  stocks: Stock[];
  totalRanked: number;
  totalMissingCap: number;
  isLoading?: boolean;
}

// Format number as currency
const formatCurrency = (value: number | null | undefined): string => {
  if (value == null) return "N/A";
  return `$${value.toLocaleString()}`;
};

// Format percentage
const formatPercentage = (value: number | null | undefined): string => {
  if (value == null) return "N/A";
  return `${value.toFixed(2)}%`;
};

// Format number with commas
const formatNumber = (value: number | null | undefined): string => {
  if (value == null) return "N/A";
  return value.toLocaleString();
};

// Rank badge component
const RankBadge = ({ rank }: { rank: number | null | undefined }) => {
  if (rank == null) return <Typography color="text.secondary">N/A</Typography>;

  let color: "success" | "info" | "default" = "default";
  let label = `#${rank}`;

  if (rank === 1) {
    color = "success";
    label = "🥇 1";
  } else if (rank === 2) {
    color = "info";
    label = "🥈 2";
  } else if (rank === 3) {
    color = "info";
    label = "🥉 3";
  }

  return <Chip label={label} color={color} size="small" />;
};

export function RankedStocksTable({
  stocks,
  totalRanked,
  totalMissingCap,
  isLoading = false,
}: RankedStocksTableProps) {
  const [sortConfig, setSortConfig] = React.useState<SortConfig>({
    key: "combinedRank",
    direction: "asc",
  });

  const handleSort = (key: SortKey) => {
    setSortConfig((prev) => ({
      key,
      direction: prev.key === key && prev.direction === "asc" ? "desc" : "asc",
    }));
  };

  const sortedStocks = React.useMemo(() => {
    if (!stocks.length) return [];

    return [...stocks].sort((a, b) => {
      const aValue = a[sortConfig.key];
      const bValue = b[sortConfig.key];

      if (aValue == null && bValue == null) return 0;
      if (aValue == null) return 1;
      if (bValue == null) return -1;

      if (typeof aValue === "number" && typeof bValue === "number") {
        return sortConfig.direction === "asc"
          ? aValue - bValue
          : bValue - aValue;
      }

      return 0;
    });
  }, [stocks, sortConfig]);

  // Loading skeleton
  if (isLoading) {
    return (
      <Box sx={{ display: "flex", justifyContent: "center", py: 8 }}>
        <CircularProgress />
      </Box>
    );
  }

  // Empty state
  if (!stocks.length) {
    return (
      <Alert severity="info" sx={{ mt: 2 }}>
        No stocks found matching your criteria. Try adjusting your filters.
      </Alert>
    );
  }

  return (
    <Box>
      <Typography variant="h6" gutterBottom>
        Stock Rankings
      </Typography>

      <Typography variant="body2" color="text.secondary" gutterBottom>
        Showing {stocks.length} of {totalRanked} ranked stocks
        {totalMissingCap > 0 &&
          ` (${totalMissingCap} excluded due to missing market cap)`}
      </Typography>

      <TableContainer component={Paper} sx={{ mt: 2 }}>
        <Table sx={{ minWidth: 1200 }} size="small">
          <TableHead>
            <TableRow>
              <TableCell
                sortDirection={
                  sortConfig.key === "combinedRank"
                    ? sortConfig.direction
                    : false
                }
              >
                <TableSortLabel
                  active={sortConfig.key === "combinedRank"}
                  direction={sortConfig.direction}
                  onClick={() => handleSort("combinedRank")}
                >
                  <strong>Rank</strong>
                </TableSortLabel>
              </TableCell>
              <TableCell>
                <strong>Company</strong>
              </TableCell>
              <TableCell
                sortDirection={
                  sortConfig.key === "peRank" ? sortConfig.direction : false
                }
              >
                <TableSortLabel
                  active={sortConfig.key === "peRank"}
                  direction={sortConfig.direction}
                  onClick={() => handleSort("peRank")}
                >
                  <strong>PE Rank</strong>
                </TableSortLabel>
              </TableCell>
              <TableCell
                sortDirection={
                  sortConfig.key === "roicRank" ? sortConfig.direction : false
                }
              >
                <TableSortLabel
                  active={sortConfig.key === "roicRank"}
                  direction={sortConfig.direction}
                  onClick={() => handleSort("roicRank")}
                >
                  <strong>ROIC Rank</strong>
                </TableSortLabel>
              </TableCell>
              <TableCell
                sortDirection={
                  sortConfig.key === "marketCap" ? sortConfig.direction : false
                }
              >
                <TableSortLabel
                  active={sortConfig.key === "marketCap"}
                  direction={sortConfig.direction}
                  onClick={() => handleSort("marketCap")}
                >
                  <strong>Market Cap</strong>
                </TableSortLabel>
              </TableCell>
              <TableCell
                sortDirection={
                  sortConfig.key === "price" ? sortConfig.direction : false
                }
              >
                <TableSortLabel
                  active={sortConfig.key === "price"}
                  direction={sortConfig.direction}
                  onClick={() => handleSort("price")}
                >
                  <strong>Price</strong>
                </TableSortLabel>
              </TableCell>
              <TableCell
                sortDirection={
                  sortConfig.key === "change" ? sortConfig.direction : false
                }
              >
                <TableSortLabel
                  active={sortConfig.key === "change"}
                  direction={sortConfig.direction}
                  onClick={() => handleSort("change")}
                >
                  <strong>Change</strong>
                </TableSortLabel>
              </TableCell>
              <TableCell
                sortDirection={
                  sortConfig.key === "volume" ? sortConfig.direction : false
                }
              >
                <TableSortLabel
                  active={sortConfig.key === "volume"}
                  direction={sortConfig.direction}
                  onClick={() => handleSort("volume")}
                >
                  <strong>Volume</strong>
                </TableSortLabel>
              </TableCell>
              <TableCell
                sortDirection={
                  sortConfig.key === "peRatio" ? sortConfig.direction : false
                }
              >
                <TableSortLabel
                  active={sortConfig.key === "peRatio"}
                  direction={sortConfig.direction}
                  onClick={() => handleSort("peRatio")}
                >
                  <strong>PE Ratio</strong>
                </TableSortLabel>
              </TableCell>
              <TableCell
                sortDirection={
                  sortConfig.key === "eps" ? sortConfig.direction : false
                }
              >
                <TableSortLabel
                  active={sortConfig.key === "eps"}
                  direction={sortConfig.direction}
                  onClick={() => handleSort("eps")}
                >
                  <strong>EPS</strong>
                </TableSortLabel>
              </TableCell>
              <TableCell
                sortDirection={
                  sortConfig.key === "roic" ? sortConfig.direction : false
                }
              >
                <TableSortLabel
                  active={sortConfig.key === "roic"}
                  direction={sortConfig.direction}
                  onClick={() => handleSort("roic")}
                >
                  <strong>ROIC</strong>
                </TableSortLabel>
              </TableCell>
              <TableCell
                sortDirection={
                  sortConfig.key === "dividendsYield"
                    ? sortConfig.direction
                    : false
                }
              >
                <TableSortLabel
                  active={sortConfig.key === "dividendsYield"}
                  direction={sortConfig.direction}
                  onClick={() => handleSort("dividendsYield")}
                >
                  <strong>Div Yield</strong>
                </TableSortLabel>
              </TableCell>
              <TableCell>
                <strong>Sector</strong>
              </TableCell>
              <TableCell>
                <strong>Exchange</strong>
              </TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {sortedStocks.map((stock, index) => (
              <TableRow
                key={stock.name ?? index}
                sx={{ "&:last-child td, &:last-child th": { border: 0 } }}
              >
                <TableCell component="th" scope="row">
                  <RankBadge rank={stock.combinedRank} />
                </TableCell>
                <TableCell>
                  <Tooltip
                    title={
                      stock.description
                        ? `${stock.description}`
                        : "No description available"
                    }
                  >
                    <Typography
                      variant="body2"
                      sx={{
                        fontWeight: 500,
                        maxWidth: 200,
                        overflow: "hidden",
                        textOverflow: "ellipsis",
                        whiteSpace: "nowrap",
                      }}
                    >
                      {stock.name ?? "Unknown"}
                    </Typography>
                  </Tooltip>
                </TableCell>
                <TableCell align="right">{stock.peRank ?? "N/A"}</TableCell>
                <TableCell align="right">{stock.roicRank ?? "N/A"}</TableCell>
                <TableCell align="right">
                  {formatCurrency(stock.marketCap)}
                </TableCell>
                <TableCell align="right">
                  {stock.price != null ? `$${stock.price.toFixed(2)}` : "N/A"}
                </TableCell>
                <TableCell align="right">
                  {stock.change != null ? (
                    <Typography
                      color={stock.change >= 0 ? "success.main" : "error.main"}
                      sx={{ fontWeight: 500 }}
                    >
                      {stock.change >= 0 ? "+" : ""}
                      {stock.change.toFixed(2)}
                    </Typography>
                  ) : (
                    "N/A"
                  )}
                </TableCell>
                <TableCell align="right">
                  {formatNumber(stock.volume)}
                </TableCell>
                <TableCell align="right">
                  {stock.peRatio != null ? stock.peRatio.toFixed(2) : "N/A"}
                </TableCell>
                <TableCell align="right">
                  {stock.eps != null ? `$${stock.eps.toFixed(2)}` : "N/A"}
                </TableCell>
                <TableCell align="right">
                  {formatPercentage(stock.roic)}
                </TableCell>
                <TableCell align="right">
                  {formatPercentage(stock.dividendsYield)}
                </TableCell>
                <TableCell>
                  <Typography
                    variant="body2"
                    sx={{
                      maxWidth: 150,
                      overflow: "hidden",
                      textOverflow: "ellipsis",
                      whiteSpace: "nowrap",
                    }}
                  >
                    {stock.sector ?? "N/A"}
                  </Typography>
                </TableCell>
                <TableCell>
                  <Typography
                    variant="body2"
                    sx={{
                      maxWidth: 100,
                      overflow: "hidden",
                      textOverflow: "ellipsis",
                      whiteSpace: "nowrap",
                    }}
                  >
                    {stock.stockExchange ?? stock.market ?? "N/A"}
                  </Typography>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    </Box>
  );
}
