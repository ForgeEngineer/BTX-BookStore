import { Component, computed, effect, inject, input, model, signal, untracked } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { Button } from 'primeng/button';
import { Dialog } from 'primeng/dialog';
import { InputNumber } from 'primeng/inputnumber';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { Select } from 'primeng/select';
import { Textarea } from 'primeng/textarea';
import { Observable, Subscription } from 'rxjs';
import { BookStore } from '../../application/book-store';
import { Book, CatalogBook, GENRES, Genre } from '../../domain/book';
import { CatalogRepository } from '../../domain/catalog.repository';

@Component({
  selector: 'app-book-dialog',
  imports: [FormsModule, Button, Dialog, InputNumber, InputText, Message, Select, Textarea],
  templateUrl: './book-dialog.html',
})
export class BookDialog {
  private readonly store = inject(BookStore);
  private readonly catalog = inject(CatalogRepository);
  private readonly messages = inject(MessageService);
  private searchSubscription?: Subscription;

  readonly visible = model(false);
  readonly book = input<Book | null>(null);

  protected readonly genres = [...GENRES];
  protected readonly step = signal<'search' | 'details'>('search');
  protected readonly query = signal('');
  protected readonly results = signal<CatalogBook[] | null>(null);
  protected readonly searching = signal(false);
  protected readonly searchError = signal<string | null>(null);
  protected readonly selected = signal<CatalogBook | null>(null);
  protected readonly genre = signal<Genre | null>(null);
  protected readonly price = signal<number | null>(null);
  protected readonly stock = signal<number | null>(0);
  protected readonly description = signal('');
  protected readonly saving = signal(false);

  protected readonly header = computed(() => (this.book() ? 'Edit book' : 'Add book'));
  protected readonly current = computed(() => this.book() ?? this.selected());
  protected readonly canSave = computed(
    () => this.genre() !== null && this.price() !== null && !this.saving(),
  );

  constructor() {
    effect(() => {
      if (this.visible()) {
        const book = this.book();
        untracked(() => this.reset(book));
      }
    });
  }

  protected search(): void {
    const query = this.query().trim();
    if (!query) {
      return;
    }

    this.searchSubscription?.unsubscribe();
    this.searching.set(true);
    this.searchError.set(null);
    this.results.set(null);

    this.searchSubscription = this.catalog.search(query).subscribe({
      next: (results) => {
        this.results.set(results);
        this.searching.set(false);
      },
      error: (error: Error) => {
        this.searchError.set(error.message);
        this.searching.set(false);
      },
    });
  }

  protected select(result: CatalogBook): void {
    this.selected.set(result);
    this.step.set('details');
  }

  protected back(): void {
    this.step.set('search');
  }

  protected save(): void {
    const genre = this.genre();
    const price = this.price();
    if (genre === null || price === null) {
      return;
    }

    const details = {
      genre,
      price,
      stock: this.stock() ?? 0,
      description: this.description() || null,
    };
    const book = this.book();
    const selected = this.selected();

    if (book) {
      this.submit(this.store.update(book.id, details), 'Book updated');
    } else if (selected) {
      this.submit(this.store.add({ ...selected, ...details }), 'Book added');
    }
  }

  private submit(request: Observable<Book>, summary: string): void {
    this.saving.set(true);
    request.subscribe({
      next: (book) => {
        this.saving.set(false);
        this.messages.add({ severity: 'success', summary, detail: book.title });
        this.visible.set(false);
      },
      error: (error: Error) => {
        this.saving.set(false);
        this.messages.add({ severity: 'error', summary: 'Save failed', detail: error.message });
      },
    });
  }

  private reset(book: Book | null): void {
    this.searchSubscription?.unsubscribe();
    this.step.set(book ? 'details' : 'search');
    this.query.set('');
    this.results.set(null);
    this.searching.set(false);
    this.searchError.set(null);
    this.selected.set(null);
    this.genre.set(book?.genre ?? null);
    this.price.set(book?.price ?? null);
    this.stock.set(book?.stock ?? 0);
    this.description.set(book?.description ?? '');
    this.saving.set(false);
  }
}
