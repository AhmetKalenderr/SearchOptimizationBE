import { useEffect, useState } from 'react';
import { getDocumentTypes, searchDocuments } from '../api';
import type { DocumentListItem, DocumentType } from '../types';

export function SearchView() {
  const [query, setQuery] = useState('');
  const [typeId, setTypeId] = useState<number | undefined>(undefined);
  const [from, setFrom] = useState('');
  const [to, setTo] = useState('');
  const [page, setPage] = useState(1);
  const pageSize = 10;
  const todayISO = new Date().toISOString().slice(0, 10);

  const [types, setTypes] = useState<DocumentType[]>([]);
  const [items, setItems] = useState<DocumentListItem[]>([]);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [elapsedMs, setElapsedMs] = useState<number | null>(null);

  useEffect(() => {
    getDocumentTypes().then(setTypes).catch(() => setTypes([]));
  }, []);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    setError(null);
    const start = performance.now();
    searchDocuments({ q: query || undefined, typeId, from: from || undefined, to: to || undefined, page, pageSize })
      .then((result) => {
        if (cancelled) return;
        setItems(result.items);
        setTotal(result.total);
        setElapsedMs(Math.round(performance.now() - start));
      })
      .catch((e) => {
        if (!cancelled) setError(e instanceof Error ? e.message : 'Bilinmeyen hata');
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });
    return () => {
      cancelled = true;
    };
  }, [query, typeId, from, to, page]);

  const totalPages = Math.max(1, Math.ceil(total / pageSize));

  return (
    <section className="search-view">
      <div className="filters">
        <input
          className="search-input"
          type="search"
          placeholder="Doküman ara… (ör: fatura mart, sözleşme)"
          value={query}
          onChange={(e) => {
            setQuery(e.target.value);
            setPage(1);
          }}
        />
        <select
          value={typeId ?? ''}
          onChange={(e) => {
            setTypeId(e.target.value ? Number(e.target.value) : undefined);
            setPage(1);
          }}
        >
          <option value="">Tüm tipler</option>
          {types.map((t) => (
            <option key={t.id} value={t.id}>
              {t.name}
            </option>
          ))}
        </select>
        <input
          type="date"
          value={from}
          max={todayISO}
          onChange={(e) => {
            setFrom(e.target.value);
            setPage(1);
          }}
        />
        <input
          type="date"
          value={to}
          max={todayISO}
          min={from || undefined}
          onChange={(e) => {
            setTo(e.target.value);
            setPage(1);
          }}
        />
      </div>

      <div className="meta-line">
        {loading
          ? 'Yükleniyor…'
          : error
          ? `Hata: ${error}`
          : `${total} sonuç${query ? ` ("${query}" için)` : ''}${elapsedMs !== null ? ` • ${elapsedMs}ms` : ''}`}
      </div>

      {!loading && !error && items.length === 0 && (
        <div className="empty-state">
          <strong>Sonuç bulunamadı.</strong>
          <p>İpuçları: arama terimini kısaltın, filtreleri kaldırın, yazım hatalarını kontrol edin.</p>
        </div>
      )}

      <ul className="result-list">
        {items.map((d) => (
          <li key={d.id} className="result-item">
            <div className="result-title">
              <span className="badge">{d.documentTypeName}</span>
              <span>{d.title}</span>
              {d.relevanceScore !== null && (
                <span className="score">skor {d.relevanceScore} · {d.matchedTokenCount} eşleşme</span>
              )}
            </div>
            <div className="result-snippet">{d.snippet}</div>
            <div className="result-meta">
              {d.uploadedBy} · {new Date(d.uploadedAt).toLocaleDateString('tr-TR')} · {d.fileSizeKb} KB
            </div>
          </li>
        ))}
      </ul>

      {totalPages > 1 && (
        <div className="pager">
          <button disabled={page <= 1} onClick={() => setPage((p) => Math.max(1, p - 1))}>
            ← Önceki
          </button>
          <span>
            Sayfa {page} / {totalPages}
          </span>
          <button disabled={page >= totalPages} onClick={() => setPage((p) => Math.min(totalPages, p + 1))}>
            Sonraki →
          </button>
        </div>
      )}
    </section>
  );
}
