import { useEffect, useState } from 'react';
import { checkDuplicate, createDocumentType, getDocumentTypes, uploadDocument } from '../api';
import type { CreateTypeError } from '../api';
import type { DocumentType, DuplicateCheckResult } from '../types';

const SEED_USERS = [
  { id: 1, fullName: 'Ayşe Yılmaz' },
  { id: 2, fullName: 'Mehmet Demir' },
  { id: 3, fullName: 'Fatma Kaya' },
  { id: 4, fullName: 'Can Öztürk' },
  { id: 5, fullName: 'Zeynep Aydın' },
  { id: 6, fullName: 'Burak Şahin' },
  { id: 7, fullName: 'Selin Çelik' },
  { id: 8, fullName: 'Emre Aslan' },
];

type Status =
  | { kind: 'idle' }
  | { kind: 'checking' }
  | { kind: 'duplicates_found'; result: DuplicateCheckResult }
  | { kind: 'uploading' }
  | { kind: 'success'; id: string }
  | { kind: 'error'; message: string };

export function UploadView() {
  const [title, setTitle] = useState('');
  const [content, setContent] = useState('');
  const [typeId, setTypeId] = useState<number>(1);
  const [userId, setUserId] = useState<number>(1);
  const [types, setTypes] = useState<DocumentType[]>([]);
  const [status, setStatus] = useState<Status>({ kind: 'idle' });

  const [creatingType, setCreatingType] = useState(false);
  const [newTypeName, setNewTypeName] = useState('');
  const [typeBusy, setTypeBusy] = useState(false);
  const [typeError, setTypeError] = useState<string | null>(null);

  useEffect(() => {
    getDocumentTypes().then(setTypes).catch(() => setTypes([]));
  }, []);

  async function handleCreateType() {
    const name = newTypeName.trim();
    if (!name) {
      setTypeError('Tip adı boş olamaz.');
      return;
    }
    setTypeBusy(true);
    setTypeError(null);
    try {
      const created = await createDocumentType(name);
      const refreshed = await getDocumentTypes();
      setTypes(refreshed);
      setTypeId(created.id);
      setCreatingType(false);
      setNewTypeName('');
    } catch (e) {
      const err = e as CreateTypeError;
      if (err.status === 409 && err.existingId) {
        const refreshed = await getDocumentTypes();
        setTypes(refreshed);
        setTypeId(err.existingId);
        setCreatingType(false);
        setNewTypeName('');
        setTypeError(`"${name}" zaten var, mevcut tip seçildi.`);
      } else {
        setTypeError(err.message ?? 'Tip oluşturulamadı.');
      }
    } finally {
      setTypeBusy(false);
    }
  }

  function cancelCreateType() {
    setCreatingType(false);
    setNewTypeName('');
    setTypeError(null);
  }

  function reset() {
    setTitle('');
    setContent('');
    setStatus({ kind: 'idle' });
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!title.trim() || !content.trim()) {
      setStatus({ kind: 'error', message: 'Başlık ve içerik boş olamaz.' });
      return;
    }

    setStatus({ kind: 'checking' });
    try {
      const dupResult = await checkDuplicate(title, content);
      if (dupResult.exactMatches.length > 0 || dupResult.nearTitleMatches.length > 0) {
        setStatus({ kind: 'duplicates_found', result: dupResult });
        return;
      }
      await doUpload(false);
    } catch (e) {
      setStatus({ kind: 'error', message: e instanceof Error ? e.message : 'Bilinmeyen hata' });
    }
  }

  async function doUpload(confirmDuplicate: boolean) {
    setStatus({ kind: 'uploading' });
    try {
      const result = await uploadDocument({
        title: title.trim(),
        content: content.trim(),
        documentTypeId: typeId,
        uploadedById: userId,
        confirmDuplicate,
      });
      if (result.success && result.documentId) {
        setStatus({ kind: 'success', id: result.documentId });
      } else {
        setStatus({
          kind: 'error',
          message: result.errorMessage ?? 'Yükleme başarısız.',
        });
      }
    } catch (e) {
      setStatus({ kind: 'error', message: e instanceof Error ? e.message : 'Bilinmeyen hata' });
    }
  }

  return (
    <section className="upload-view">
      <form onSubmit={handleSubmit} className="upload-form">
        <label>
          Başlık
          <input value={title} onChange={(e) => setTitle(e.target.value)} required />
        </label>
        <label>
          Tip
          {creatingType ? (
            <div className="type-create-row">
              <input
                value={newTypeName}
                onChange={(e) => setNewTypeName(e.target.value)}
                placeholder="Yeni tip adı"
                disabled={typeBusy}
                autoFocus
                onKeyDown={(e) => {
                  if (e.key === 'Enter') {
                    e.preventDefault();
                    handleCreateType();
                  } else if (e.key === 'Escape') {
                    cancelCreateType();
                  }
                }}
              />
              <button
                type="button"
                className="type-action primary"
                onClick={handleCreateType}
                disabled={typeBusy}
              >
                {typeBusy ? '…' : 'Kaydet'}
              </button>
              <button
                type="button"
                className="type-action"
                onClick={cancelCreateType}
                disabled={typeBusy}
              >
                İptal
              </button>
            </div>
          ) : (
            <div className="type-select-row">
              <select value={typeId} onChange={(e) => setTypeId(Number(e.target.value))}>
                {types.map((t) => (
                  <option key={t.id} value={t.id}>
                    {t.name}
                  </option>
                ))}
              </select>
              <button
                type="button"
                className="type-action"
                onClick={() => {
                  setCreatingType(true);
                  setTypeError(null);
                }}
                title="Yeni tip ekle"
              >
                + Yeni
              </button>
            </div>
          )}
          {typeError && <small className="type-error">{typeError}</small>}
        </label>
        <label>
          Yükleyen
          <select value={userId} onChange={(e) => setUserId(Number(e.target.value))}>
            {SEED_USERS.map((u) => (
              <option key={u.id} value={u.id}>
                {u.fullName}
              </option>
            ))}
          </select>
        </label>
        <label>
          İçerik
          <textarea
            value={content}
            onChange={(e) => setContent(e.target.value)}
            rows={8}
            required
          />
        </label>
        <button type="submit" disabled={status.kind === 'checking' || status.kind === 'uploading'}>
          {status.kind === 'checking' ? 'Kontrol ediliyor…' : status.kind === 'uploading' ? 'Yükleniyor…' : 'Yükle'}
        </button>
      </form>

      {status.kind === 'duplicates_found' && (
        <div className="duplicate-warning">
          <h3>Olası mükerrer kayıt bulundu</h3>
          {status.result.exactMatches.length > 0 && (
            <>
              <p><strong>Aynı içerik daha önce yüklenmiş:</strong></p>
              <ul>
                {status.result.exactMatches.map((m) => (
                  <li key={m.documentId}>
                    {m.title} — {m.uploadedBy}, {new Date(m.uploadedAt).toLocaleDateString('tr-TR')}
                  </li>
                ))}
              </ul>
            </>
          )}
          {status.result.nearTitleMatches.length > 0 && (
            <>
              <p><strong>Benzer başlıklı dokümanlar:</strong></p>
              <ul>
                {status.result.nearTitleMatches.map((m) => (
                  <li key={m.documentId}>
                    {m.title} — {m.uploadedBy}, {new Date(m.uploadedAt).toLocaleDateString('tr-TR')}
                  </li>
                ))}
              </ul>
            </>
          )}
          <div className="duplicate-actions">
            <button type="button" onClick={() => doUpload(true)}>Yine de yükle</button>
            <button type="button" onClick={reset}>İptal</button>
          </div>
        </div>
      )}

      {status.kind === 'success' && (
        <div className="success-banner">
          Doküman başarıyla yüklendi. ID: <code>{status.id}</code>
          <button type="button" onClick={reset}>Yeni doküman</button>
        </div>
      )}

      {status.kind === 'error' && (
        <div className="error-banner">
          Hata: {status.message}
          <button type="button" onClick={() => setStatus({ kind: 'idle' })}>Kapat</button>
        </div>
      )}
    </section>
  );
}
