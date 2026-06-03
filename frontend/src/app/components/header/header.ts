import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TourStateService } from '../../services/tour-state.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './header.html',
  styleUrl: './header.css',
})
export class Header {
  tourState = inject(TourStateService);

  updateSearch(term: string) {
    this.tourState.setSearchTerm(term);
  }
}
