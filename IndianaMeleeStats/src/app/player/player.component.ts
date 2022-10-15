import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import Set from 'src/models/set'; 

@Component({
  selector: 'app-player',
  templateUrl: './player.component.html',
  styleUrls: ['./player.component.css']
})
export class PlayerComponent implements OnInit {
  public sets = Array<any>();

  constructor(private http: HttpClient) { }

  ngOnInit(): void {
  }

  getSets(gamerTag: any): void {
    var apiRoute = "http://localhost:8080/players/sets/" + gamerTag;
    this.http.get<any>(apiRoute).subscribe((results: Array<Set>) => {
      this.sets = results;
    });
  }

}
