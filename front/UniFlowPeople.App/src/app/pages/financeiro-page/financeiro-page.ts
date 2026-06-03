import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-financeiro-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './financeiro-page.html',
})
export class FinanceiroPage {
  @Input({ required: true }) vm!: any;
}
