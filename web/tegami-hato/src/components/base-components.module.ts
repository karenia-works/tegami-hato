import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { NavbarComponent } from "./navbar/navbar.component";
import { FooterComponent } from "./footer/footer.component";
import { RouterModule } from "@angular/router";
import { SearchbarComponent } from './searchbar/searchbar.component';
import { MatIconModule } from '@angular/material/icon';

@NgModule({
  declarations: [NavbarComponent, FooterComponent, SearchbarComponent],
  imports: [CommonModule, RouterModule, MatIconModule],
  exports: [NavbarComponent, FooterComponent, SearchbarComponent]
})
export class BaseComponentsModule {}
