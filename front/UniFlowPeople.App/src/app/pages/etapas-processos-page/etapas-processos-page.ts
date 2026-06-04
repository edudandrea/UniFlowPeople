import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-etapas-processos-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './etapas-processos-page.html',
})
export class EtapasProcessosPage {
  @Input({ required: true }) vm!: any;
}
