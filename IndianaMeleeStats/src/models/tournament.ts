import Event from './event';

export default class Tournament {
    constructor(public _id: string, public TournamentName: string, public Date: Date, public Events: Array<Event>) {}
}