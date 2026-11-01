import { CurrencyPipe } from '@angular/common';
import { Component, computed, inject } from '@angular/core';
import { ConfirmationService, MessageService } from 'primeng/api';
import { Button } from 'primeng/button';
import { Message } from 'primeng/message';
import { TableLazyLoadEvent, TableModule } from 'primeng/table';
import { BookStore } from '../../application/book-store';
import { Book } from '../../domain/book';

@Component({
  selector: 'app-body',
  imports: [CurrencyPipe, TableModule, Button, Message],
  templateUrl: './body.html',
})
export class Body {
  private readonly confirmation = inject(ConfirmationService);
  private readonly messages = inject(MessageService);

  protected readonly store = inject(BookStore);
  protected readonly first = computed(() => (this.store.page() - 1) * this.store.pageSize());
  protected readonly initialSortField = this.store.sortField();
  protected readonly initialSortOrder = this.store.sortDesc() ? -1 : 1;

  protected onLazyLoad(event: TableLazyLoadEvent): void {
    const rows = event.rows ?? this.store.pageSize();
    const page = Math.floor((event.first ?? 0) / rows) + 1;
    const sortField = (event.sortField as string | undefined) ?? this.store.sortField();
    this.store.setTable(page, rows, sortField, event.sortOrder === -1);
  }

  protected confirmDelete(book: Book): void {
    this.confirmation.confirm({
      header: 'Delete book',
      message: `Delete "${book.title}"?`,
      acceptLabel: 'Delete',
      rejectLabel: 'Cancel',
      accept: () => this.remove(book),
    });
  }

  private remove(book: Book): void {
    this.store.remove(book.id).subscribe({
      next: () =>
        this.messages.add({ severity: 'success', summary: 'Book deleted', detail: book.title }),
      error: (error: Error) =>
        this.messages.add({ severity: 'error', summary: 'Delete failed', detail: error.message }),
    });
  }
}
