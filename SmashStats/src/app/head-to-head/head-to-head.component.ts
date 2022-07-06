import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import Set from 'src/models/set';
import Player from 'src/models/player';
import Tournament from 'src/models/tournament';
import Event from 'src/models/event';

@Component({
  selector: 'app-head-to-head',
  templateUrl: './head-to-head.component.html',
  styleUrls: ['./head-to-head.component.css']
})
export class HeadToHeadComponent implements OnInit {
  public tournaments = Array<Tournament>();
  playerOne = '';
  playerTwo = '';

  constructor(private http: HttpClient) { }

  ngOnInit(): void {
  }

  getHeadToHeadResults(playerOne: any, playerTwo: any): void {
    this.getPlayerId(playerOne).subscribe((player1) => {
      this.getPlayerId(playerTwo).subscribe((player2) => {
        var apiRoute = "http://localhost:8080/tournaments/" + player1._id + "/" + player2._id;
        this.http.get<any>(apiRoute).subscribe((results: Array<Tournament>) => {
          results.forEach((tournament: Tournament) => {
            tournament.Events.forEach((event: Event) => {
              console.log(event.Sets);
              event.Sets = event.Sets.filter((set) => { (set.Players.find((player) => { player._id == player1._id }) && set.Players.find((player) => { player._id == player2._id }))});
            })
            this.tournaments.push(tournament);
          });
        });
      });
    });
  }

  getPlayerId(player: Player) {
    var apiRoute = "http://localhost:8080/players/gamerTag/" + player;
    return this.http.get<Player>(apiRoute);
  }
}
