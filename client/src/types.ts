export interface DocumentListItem {
  id: string;
  title: string;
  snippet: string;
  documentTypeName: string;
  documentTypeId: number;
  uploadedBy: string;
  uploadedAt: string;
  fileSizeKb: number;
  relevanceScore: number | null;
  matchedTokenCount: number | null;
}

export interface PaginatedResult<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
}

export interface DocumentType {
  id: number;
  name: string;
}

export interface DuplicateMatch {
  documentId: string;
  title: string;
  documentTypeName: string;
  uploadedBy: string;
  uploadedAt: string;
  matchKind: 'exact' | 'near-title';
}

export interface DuplicateCheckResult {
  exactMatches: DuplicateMatch[];
  nearTitleMatches: DuplicateMatch[];
}

export interface UploadResult {
  success: boolean;
  documentId: string | null;
  duplicates: DuplicateCheckResult | null;
  errorMessage: string | null;
}

export interface User {
  id: number;
  fullName: string;
  department: string;
}
