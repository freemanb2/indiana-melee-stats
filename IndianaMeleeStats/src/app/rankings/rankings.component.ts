import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import Player from 'src/models/player';

@Component({
  selector: 'app-rankings',
  templateUrl: './rankings.component.html',
  styleUrls: ['./rankings.component.css']
})
export class RankingsComponent implements OnInit {
  public rankedPlayers = Array<any>();
  public loading = true;

  constructor(private http: HttpClient) { }

  ngOnInit(): void {
    this.getRankedPlayersInOrder();
  }

  getRankedPlayersInOrder(): void {
    var apiRoute = "http://localhost:8080/players/rankings";
    this.http.get<any>(apiRoute).subscribe((results: Array<Player>) => {
      this.rankedPlayers = results;
      this.loading = false;
    });
  }

}
