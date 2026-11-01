import { Component } from '@angular/core';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { Toast } from 'primeng/toast';
import { Body } from './ui/body/body';
import { Footer } from './ui/footer/footer';
import { Header } from './ui/header/header';

@Component({
  selector: 'app-root',
  imports: [Header, Body, Footer, Toast, ConfirmDialog],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {}
