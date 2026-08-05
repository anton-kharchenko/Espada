import { queryOptions, useQuery } from '@tanstack/react-query';
import { getConsoleSession } from '../api/sessionApi';

export const consoleSessionQueryOptions = queryOptions({
  queryKey: ['bff', 'session'] as const,
  queryFn: ({ signal }) => getConsoleSession(signal),
  retry: false,
  staleTime: 30_000,
});

export const useConsoleSession = () => useQuery(consoleSessionQueryOptions);
