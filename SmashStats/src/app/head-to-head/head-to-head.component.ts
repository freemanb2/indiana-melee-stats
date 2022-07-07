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
  public sets = Array<any>();
  public uniqueTournaments = Array<any>();
  playerOne = '';
  playerTwo = '';

  constructor(private http: HttpClient) { }

  ngOnInit(): void {
  }

  getHeadToHeadResults(playerOne: any, playerTwo: any): void {
    this.sets = Array<any>();
    this.getPlayerId(playerOne).subscribe((player1) => {
      this.getPlayerId(playerTwo).subscribe((player2) => {
        var apiRoute = "http://localhost:8080/sets/" + player1._id + "/" + player2._id;
        this.http.get<any>(apiRoute).subscribe((results: Array<Set>) => {
          results.forEach((set: any) => {
              var getTournamentDetails = "http://localhost:8080/tournaments/set/" + set._id;
              this.http.get<any>(getTournamentDetails).subscribe((results) => {
                set.TournamentName = results.TournamentName; 
                this.sets.push(set);
                if(this.uniqueTournaments.indexOf(results.TournamentName == -1)){
                  this.uniqueTournaments.push(results.TournamnetName);
                }
              });
            })
          });
        });
      });
    };

  getPlayerId(player: Player) {
    var apiRoute = "http://localhost:8080/players/gamerTag/" + player;
    return this.http.get<Player>(apiRoute);
  }
}
