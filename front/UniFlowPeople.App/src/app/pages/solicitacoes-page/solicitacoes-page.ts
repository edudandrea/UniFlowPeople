import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-solicitacoes-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './solicitacoes-page.html',
})
export class SolicitacoesPage {
  @Input({ required: true }) vm!: any;
}
