import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-usuarios-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './usuarios-page.html',
})
export class UsuariosPage {
  @Input({ required: true }) vm!: any;
}
