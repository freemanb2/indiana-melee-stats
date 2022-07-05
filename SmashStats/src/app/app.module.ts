import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClient, HttpClientModule } from '@angular/common/http';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { TournamentComponent } from './tournament/tournament.component';
import { TournamentListComponent } from './tournament-list/tournament-list.component';
import { EventComponent } from './event/event.component';
import { SetComponent } from './set/set.component';
import { PlayerComponent } from './player/player.component';
import { HeadToHeadComponent } from './head-to-head/head-to-head.component';

@NgModule({
  declarations: [
    AppComponent,
    TournamentComponent,
    TournamentListComponent,
    EventComponent,
    SetComponent,
    PlayerComponent,
    HeadToHeadComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
