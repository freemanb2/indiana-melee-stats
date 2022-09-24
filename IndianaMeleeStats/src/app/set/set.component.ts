import { Component, Input, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import Set from '../../models/set';

@Component({
  selector: 'set',
  templateUrl: './set.component.html',
  styleUrls: ['./set.component.css']
})
export class SetComponent implements OnInit {
  @Input() id: string = "";

  public displayScore = "";

  constructor(private http: HttpClient) { }

  ngOnInit(): void {
    this.getSet(this.id);
  }

  getSet(id: string): void {
    var apiRoute = "http://localhost:8080/sets/" + id;
    this.http.get<Set>(apiRoute).subscribe((result: Set) => {
      this.displayScore = result.DisplayScore;
    });
  }

}
