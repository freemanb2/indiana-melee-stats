import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import Set from 'src/models/set';
import Player from 'src/models/player';

@Component({
  selector: 'app-head-to-head',
  templateUrl: './head-to-head.component.html',
  styleUrls: ['./head-to-head.component.css']
})
export class HeadToHeadComponent implements OnInit {
  public setIds = Array<string>();
  playerOne = '';
  playerTwo = '';

  constructor(private http: HttpClient) { }

  ngOnInit(): void {
  }

  getHeadToHeadResults(playerOne: any, playerTwo: any): void {
    this.getPlayerId(playerOne).subscribe((player1) => {
      this.getPlayerId(playerTwo).subscribe((player2) => {
        var playerTwoId = this.getPlayerId(playerTwo);
        var apiRoute = "http://localhost:8080/sets/" + player1._id + "/" + player2._id;
        var setIds = Array<string>();
        this.http.get<any>(apiRoute).subscribe((results: Array<Set>) => {
          results.forEach((result: any) => {
            setIds.push(result._id);
          });
        });
        this.setIds = setIds;
      });
    });
  }

  getPlayerId(player: any) {
    var apiRoute = "http://localhost:8080/players/gamerTag/" + player;
    return this.http.get<Player>(apiRoute);
  }
}
