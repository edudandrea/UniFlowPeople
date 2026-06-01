import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-colaboradores-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './colaboradores-page.html',
})
export class ColaboradoresPage {
  @Input({ required: true }) vm!: any;
}
