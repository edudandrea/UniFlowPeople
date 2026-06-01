import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-treinamentos-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './treinamentos-page.html',
})
export class TreinamentosPage {
  @Input({ required: true }) vm!: any;
}
