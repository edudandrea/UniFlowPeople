import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-documentos-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './documentos-page.html',
})
export class DocumentosPage {
  @Input({ required: true }) vm!: any;
}
