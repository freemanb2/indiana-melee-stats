import { Component, Injectable, Input, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import Tournament from 'src/models/tournament';
import Event from '../../models/event';

@Component({
  selector: 'tournament',
  templateUrl: './tournament.component.html',
  styleUrls: ['./tournament.component.css']
})
@Injectable()
export class TournamentComponent implements OnInit {
  @Input() id: string = "";

  public tournamentName = "";
  public events = Array();
  
  constructor(private http: HttpClient) { }

  ngOnInit(): void { 
    this.getTournament(this.id);
  }
  
  getTournament(id: string): void {
    var apiRoute = "http://localhost:8080/tournaments/" + id;
    this.http.get<Tournament>(apiRoute).subscribe((result: Tournament) => {
      this.tournamentName = result.TournamentName;
      this.events = result.Events;
    });
  }
}
