export const GENRES = [
  'Fiction',
  'SciFi',
  'Fantasy',
  'Mystery',
  'Biography',
  'NonFiction',
  'Children',
  'History',
] as const;

export type Genre = (typeof GENRES)[number];

export interface Book {
  id: string;
  title: string;
  author: string;
  isbn: string;
  publishedYear: number | null;
  coverUrl: string | null;
  openLibraryKey: string;
  description: string | null;
  genre: Genre;
  price: number;
  stock: number;
  createdAtUtc: string;
}

export interface CatalogBook {
  title: string;
  author: string;
  isbn: string | null;
  publishedYear: number | null;
  coverUrl: string | null;
  openLibraryKey: string;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface BookQuery {
  genre?: Genre;
  page: number;
  pageSize: number;
  sortField: string;
  sortDesc: boolean;
}

export interface NewBook {
  title: string;
  author: string;
  isbn: string | null;
  publishedYear: number | null;
  coverUrl: string | null;
  openLibraryKey: string;
  description: string | null;
  genre: Genre;
  price: number;
  stock: number;
}

export interface BookUpdate {
  genre: Genre;
  price: number;
  stock: number;
  description: string | null;
}
