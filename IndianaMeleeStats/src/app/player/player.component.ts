import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Component({
  selector: 'app-player',
  templateUrl: './player.component.html',
  styleUrls: ['./player.component.css']
})
export class PlayerComponent implements OnInit {
  public tournaments = Array<any>();
  public headToHeads = Array<any>();
  public loadingResults = false;
  public loadingHeadToHeads = false;

  constructor(private http: HttpClient) { }

  ngOnInit(): void {
  }

  getSets(gamerTag: any): void {
    this.tournaments = Array<any>();
    this.headToHeads = Array<any>();
    this.loadingResults = true;
    var apiRoute = "http://localhost:8080/sets/" + gamerTag;
    this.http.get<any>(apiRoute).subscribe((results: Array<any>) => {
      this.tournaments = results;
      this.loadingResults = false;
    });

    this.getHeadToHeads(gamerTag);
  }

  getHeadToHeads(gamerTag: any): void {
    var apiRoute = "http://localhost:8080/players/" + gamerTag + "/headToHeads";
    this.loadingHeadToHeads = true;
    this.http.get<any>(apiRoute).subscribe((results: Array<any>) => {
      this.headToHeads = results;
      this.loadingHeadToHeads = false;
    });
  }
}
