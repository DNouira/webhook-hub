import type { PageLoad } from './$types';
import { api } from '$lib/api';

type Health = { status: string; time: string };

export const load: PageLoad = async () => {
  const health = await api<Health>('/api/health');
  return { health };
};
