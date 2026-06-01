import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-departamentos-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './departamentos-page.html',
})
export class DepartamentosPage {
  @Input({ required: true }) vm!: any;
}
