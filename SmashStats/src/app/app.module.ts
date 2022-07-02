import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClient, HttpClientModule } from '@angular/common/http';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { TournamentComponent } from './tournament/tournament.component';
import { TournamentListComponent } from './tournament-list/tournament-list.component';
import { RouterModule, Routes } from '@angular/router';
import { EventComponent } from './event/event.component';
import { SetComponent } from './set/set.component';
import { PlayerComponent } from './player/player.component';

const routes: Routes = [
  { path: 'tournaments', component: TournamentListComponent}
];

@NgModule({
  declarations: [
    AppComponent,
    TournamentComponent,
    TournamentListComponent,
    EventComponent,
    SetComponent,
    PlayerComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    RouterModule.forRoot(routes)
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
