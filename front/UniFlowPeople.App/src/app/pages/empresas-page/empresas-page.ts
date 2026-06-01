import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-empresas-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './empresas-page.html',
})
export class EmpresasPage {
  @Input({ required: true }) vm!: any;
}
