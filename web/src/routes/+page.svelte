<script lang="ts">
  import { api } from '$lib/api';

  export let data: { health: { status: string; time: string } };
  let health = data.health;
  let refreshing = false;

  async function refresh() {
    refreshing = true;
    try {
      health = await api<{ status: string; time: string }>('/api/health');
    } finally {
      refreshing = false;
    }
  }
</script>

<div class="min-h-screen bg-gray-950 text-gray-100 grid place-items-center p-6">
  <div class="w-full max-w-md rounded-2xl border border-gray-800 bg-gray-900/60 shadow-xl">
    <div class="p-6">
      <h1 class="text-xl font-semibold">Webhook Hub</h1>
      <p class="mt-1 text-sm text-gray-400">.NET API → OpenTelemetry → Jaeger</p>

      <div class="mt-6 grid grid-cols-2 gap-4 text-sm">
        <div class="rounded-lg bg-gray-800/60 p-4">
          <div class="text-gray-400">Status</div>
          <div class="mt-1 font-medium">{health.status}</div>
        </div>
        <div class="rounded-lg bg-gray-800/60 p-4">
          <div class="text-gray-400">Time (UTC)</div>
          <div class="mt-1 font-mono text-xs">{new Date(health.time).toUTCString()}</div>
        </div>
      </div>

      <div class="mt-6 flex gap-3">
        <button
          class="inline-flex items-center rounded-lg bg-emerald-600 px-4 py-2 font-medium hover:bg-emerald-500 disabled:opacity-50"
          on:click|preventDefault={refresh}
          disabled={refreshing}
        >
          {#if refreshing}
            <svg class="mr-2 h-4 w-4 animate-spin" viewBox="0 0 24 24" fill="none">
              <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" />
              <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v4A4 4 0 008 12H4z" />
            </svg>
          {/if}
          Refresh
        </button>

        <a
          class="rounded-lg border border-gray-700 px-4 py-2 hover:bg-gray-800"
          href="http://localhost:16686" target="_blank" rel="noreferrer"
        >Open Jaeger</a>
      </div>
    </div>
  </div>
</div>
