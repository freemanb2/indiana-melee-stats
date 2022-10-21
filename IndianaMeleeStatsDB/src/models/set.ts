import Player from './player';

export default class Set {
    constructor(public _id: string, public DisplayScore: string, public WinnerId: string, public LoserId: string, public Players: Array<Player>, public PlayerIds: Array<string>) {}
}