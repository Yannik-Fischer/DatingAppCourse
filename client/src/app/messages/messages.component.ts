import { Component, inject, OnInit } from '@angular/core';
import { MessageService } from '../_services/message.service';
import { ButtonsModule } from 'ngx-bootstrap/buttons';
import { FormsModule } from '@angular/forms';
import { TimeagoModule } from 'ngx-timeago';
import { Message } from '../_models/message';
import { RouterLink } from '@angular/router';
import { PaginationModule } from 'ngx-bootstrap/pagination';
import { TitleCasePipe } from '@angular/common';

@Component({
  selector: 'app-messages',
  standalone: true,
  imports: [ButtonsModule, FormsModule, TimeagoModule, RouterLink, PaginationModule, TitleCasePipe],
  templateUrl: './messages.component.html',
  styleUrl: './messages.component.css'
})
export class MessagesComponent implements OnInit {
  messageService = inject(MessageService);
  container = 'Inbox';
  isOutbox = this.container === 'Outbox';
  pageNumber = 1;
  pageSize = 5;

  ngOnInit(): void {
    this.loadMessages();
  }

  deleteMessage(id: number) {
    this.messageService.deleteMessage(id).subscribe({
      next: () => {
        this.messageService.paginatedResult.update(prev => {
          if (prev && prev.items) {
            prev.items.splice(prev.items.findIndex(m => m.id === id), 1);
            return prev;
          }
          return prev;
        });
      }
    })
  }

  loadMessages() {
    this.messageService.getMessages(this.pageNumber, this.pageSize, this.container);
  }

  getRoute(message: Message) {
    if (this.container === 'Outbox') {
      return `/members/${message.recipientUsername}`;
    } else {
      return `/members/${message.senderUsername}`;
    }
  }

  pageChanged(event: any) {
    if (this.pageNumber !== event.page) {
      this.pageNumber = event.page;
      this.loadMessages();
    }
  }
}
