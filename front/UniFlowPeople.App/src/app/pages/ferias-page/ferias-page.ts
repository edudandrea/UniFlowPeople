import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-ferias-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './ferias-page.html',
})
export class FeriasPage {
  @Input({ required: true }) vm!: any;
}
