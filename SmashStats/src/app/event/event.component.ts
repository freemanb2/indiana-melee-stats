import { Component, Input, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import Event from '../../models/event';
import Set from '../../models/set';

@Component({
  selector: 'event',
  templateUrl: './event.component.html',
  styleUrls: ['./event.component.css']
})
export class EventComponent implements OnInit {
  @Input() id: string = "";

  public eventName = "";
  public sets = Array<string>();

  constructor(private http: HttpClient) { }

  ngOnInit(): void {
    this.getEvent(this.id);
  }

  getEvent(id: string): void {
    var apiRoute = "http://localhost:8080/events/" + id;
    this.http.get<Event>(apiRoute).subscribe((result: Event) => {
      this.eventName = result.EventName;
      this.sets = result.Sets;
    });
  }
}
