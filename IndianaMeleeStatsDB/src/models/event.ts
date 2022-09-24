import Set from './set';

export default class Event {
    constructor(public _id: string, public EventName: string, public EventType: string, public Sets: Array<string>) {}
}