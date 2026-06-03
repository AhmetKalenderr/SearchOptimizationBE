import { useEffect, useState } from 'react';
import { deleteDocument, getDocument } from '../api';
import type { DocumentDetail } from '../types';

interface Props {
  documentId: string;
  onBack: () => void;
  onDeleted: (id: string) => void;
}

export function DocumentDetailView({ documentId, onBack, onDeleted }: Props) {
  const [doc, setDoc] = useState<DocumentDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [deleting, setDeleting] = useState(false);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    setError(null);
    getDocument(documentId)
      .then((d) => {
        if (!cancelled) setDoc(d);
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
  }, [documentId]);

  async function handleDelete() {
    if (!doc) return;
    if (!window.confirm(`"${doc.title}" dokümanını silmek istediğinize emin misiniz? Bu işlem geri alınamaz.`)) {
      return;
    }
    setDeleting(true);
    setError(null);
    try {
      await deleteDocument(doc.id);
      onDeleted(doc.id);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Silme başarısız');
      setDeleting(false);
    }
  }

  return (
    <section className="detail-view">
      <button className="back-button" onClick={onBack}>
        ← Aramaya dön
      </button>

      {loading && <div className="meta-line">Yükleniyor…</div>}
      {error && <div className="meta-line">Hata: {error}</div>}

      {doc && (
        <article className="detail-card">
          <div className="detail-header">
            <span className="badge">{doc.documentTypeName}</span>
            <h2>{doc.title}</h2>
          </div>
          <div className="result-meta">
            {doc.uploadedBy} · {new Date(doc.uploadedAt).toLocaleDateString('tr-TR')} · {doc.fileSizeKb} KB
          </div>
          <div className="detail-content">{doc.content}</div>
          <div className="detail-actions">
            <button className="danger-button" onClick={handleDelete} disabled={deleting}>
              {deleting ? 'Siliniyor…' : 'Dokümanı Sil'}
            </button>
          </div>
        </article>
      )}
    </section>
  );
}
