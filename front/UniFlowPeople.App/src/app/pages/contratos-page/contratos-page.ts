import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-contratos-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './contratos-page.html',
})
export class ContratosPage {
  @Input({ required: true }) vm!: any;
}
