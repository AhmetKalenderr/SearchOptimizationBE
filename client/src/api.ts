import type {
  DocumentListItem,
  DocumentType,
  DuplicateCheckResult,
  PaginatedResult,
  UploadResult,
} from './types';

export interface SearchParams {
  q?: string;
  typeId?: number;
  from?: string;
  to?: string;
  page?: number;
  pageSize?: number;
}

function toQueryString(params: Record<string, string | number | undefined>): string {
  const parts: string[] = [];
  for (const [key, value] of Object.entries(params)) {
    if (value === undefined || value === null || value === '') continue;
    parts.push(`${encodeURIComponent(key)}=${encodeURIComponent(String(value))}`);
  }
  return parts.length ? `?${parts.join('&')}` : '';
}

export async function searchDocuments(params: SearchParams): Promise<PaginatedResult<DocumentListItem>> {
  const qs = toQueryString({ ...params });
  const res = await fetch(`/api/documents${qs}`);
  if (!res.ok) throw new Error(`Arama başarısız: ${res.status}`);
  return res.json();
}

export async function getDocumentTypes(): Promise<DocumentType[]> {
  const res = await fetch('/api/documents/types');
  if (!res.ok) throw new Error('Tipler yüklenemedi');
  return res.json();
}

export interface CreateTypeError {
  status: number;
  message: string;
  existingId?: number;
}

export async function createDocumentType(name: string): Promise<DocumentType> {
  const res = await fetch('/api/documents/types', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ name }),
  });
  if (!res.ok) {
    const body = await res.json().catch(() => ({}));
    const err: CreateTypeError = {
      status: res.status,
      message: body.error ?? 'Tip oluşturulamadı.',
      existingId: body.existingId,
    };
    throw err;
  }
  return res.json();
}

export async function checkDuplicate(title: string, content: string): Promise<DuplicateCheckResult> {
  const res = await fetch('/api/documents/check-duplicate', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ title, content }),
  });
  if (!res.ok) throw new Error('Duplicate kontrolü başarısız');
  return res.json();
}

export async function uploadDocument(payload: {
  title: string;
  content: string;
  documentTypeId: number;
  uploadedById: number;
  confirmDuplicate?: boolean;
}): Promise<UploadResult> {
  const res = await fetch('/api/documents', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  });
  return res.json();
}
