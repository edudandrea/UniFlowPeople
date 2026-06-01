import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-cargos-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './cargos-page.html',
})
export class CargosPage {
  @Input({ required: true }) vm!: any;
}
