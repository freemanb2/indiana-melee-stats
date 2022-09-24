import { ObjectId } from "mongodb";
import Event from "./event";

export default class Tournament {
    constructor(public _id: string, public TournamentName: string, public Events: Array<string>) {}
}