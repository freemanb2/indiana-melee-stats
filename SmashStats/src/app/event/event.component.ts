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
  public sets = Array<any>();

  constructor(private http: HttpClient) { }

  ngOnInit(): void {
  }
}
