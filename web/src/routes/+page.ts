import { api } from '$lib/api';

export const load = async () => {
  const health = await api<{ status: string; time: string }>('/healthz');
  return { health };
};
