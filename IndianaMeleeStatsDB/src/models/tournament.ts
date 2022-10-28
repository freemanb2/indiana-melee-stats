import Event from "./event";

export default class Tournament {
    constructor(public _id: string, public TournamentName: string, public Link: string, public Events: Array<Event>) {}
}