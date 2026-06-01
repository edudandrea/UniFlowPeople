import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-demissao-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './demissao-page.html',
})
export class DemissaoPage {
  @Input({ required: true }) vm!: any;
}
