import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TournamentListComponent } from './tournament-list/tournament-list.component';
import { HeadToHeadComponent } from './head-to-head/head-to-head.component';
import { RankingsComponent } from './rankings/rankings.component';
import { PlayerComponent } from './player/player.component';

const routes: Routes = [
  { path: 'tournaments', component: TournamentListComponent },
  { path: 'head-to-head', component: HeadToHeadComponent },
  { path: 'rankings', component: RankingsComponent },
  { path: 'players', component: PlayerComponent }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
