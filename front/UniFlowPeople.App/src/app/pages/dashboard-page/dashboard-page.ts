import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ContratosPage } from '../contratos-page/contratos-page';

@Component({
  selector: 'app-dashboard-page',
  standalone: true,
  imports: [CommonModule, FormsModule, ContratosPage],
  templateUrl: './dashboard-page.html',
})
export class DashboardPage {
  @Input({ required: true }) vm!: any;
}
