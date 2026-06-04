import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-epis-acessos-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './epis-acessos-page.html',
})
export class EpisAcessosPage {
  @Input({ required: true }) vm!: any;
}
