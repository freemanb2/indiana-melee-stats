import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import Tournament from 'src/models/tournament';

@Component({
  selector: 'app-tournament-list',
  templateUrl: './tournament-list.component.html',
  styleUrls: ['./tournament-list.component.css']
})
export class TournamentListComponent implements OnInit {
  public tournamentIds = Array<string>();

  constructor(private http: HttpClient) { }

  ngOnInit(): void {
    this.getRecentTournamentIds();
  }

  getRecentTournamentIds(): void {
    var apiRoute = "http://localhost:8080/tournaments/recent/10";
    var tournamentIds = Array<string>();
    this.http.get<Array<Tournament>>(apiRoute).subscribe((results: Array<Tournament>) => {
      results.forEach(result => {
        tournamentIds.push(result._id);
      });
    });
    this.tournamentIds = tournamentIds;
  }
}
