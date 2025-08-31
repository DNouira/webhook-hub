import { PUBLIC_API_BASE_URL } from '$env/static/public';
const BASE = PUBLIC_API_BASE_URL || 'http://localhost:8080';

export async function api<T>(path: string, init?: RequestInit): Promise<T> {
  const url = `${BASE}${path.startsWith('/') ? '' : '/'}${path}`;
  const res = await fetch(url, {
    headers: { 'Content-Type': 'application/json', ...(init?.headers ?? {}) },
    ...init
  });
  if (!res.ok) throw new Error(`${res.status} ${res.statusText}`);
  return res.json() as Promise<T>;
}
